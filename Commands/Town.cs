namespace Respublica.Commands;

using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

public class TownCmd : CommandExecutor // Commands for managing towns
{
	public bool onCommand(CommandSender sender, Command command, string label, string[] args)
	{
		if (sender is not Player) return true;

		var t = DBInteract.getTown((Player)sender);
		
		// /t or /town
		if (args.Length == 0) {
			if (string.IsNullOrEmpty(t.name)) {
                        	sender.sendMessage("You don't have a town!");
                        	return true;
                	}
			sender.sendMessage($"--- {Town.formatName(t.name)} ---");
			sender.sendMessage($"Chunk count: {Database.Instance.GetCollection<DBChunk>("chunks").Count(LiteDB.Query.EQ("town", t.name))}"); // UNI - srry didn't feel like adding a function
			sender.sendMessage($"Mayor: {Plr.guidToUsrname(t.mayor)}");
			sender.sendMessage($"PVP: {t.perm.PVP} EXPLOSIONS: {t.perm.EXPLOSION} FIRE: {t.perm.FIRE} MOBS: {t.perm.MOBS}");
			return true;
		}

		if (args.Length > 0) {
			switch(args[0]) {
				case "new":
                case "create":
					if (args.Length < 2) { sender.sendMessage("Invalid set command."); break; }
					if (!string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You already have a town!");
                        break;
                	}
					var cc = Chunk.cToCC(((Player)sender).getLocation());
					var nc = Chunk.getChunk(cc.x, cc.z);
					
					if (nc != null) break;

					DBInteract.initTown((Player)sender, args[1]);
					sender.sendMessage($"Created town \"{Town.formatName(args[1])}\"!");
					break;
				case "set":
					if (args.Length < 2) { sender.sendMessage("Invalid set command."); break; }
					if (string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You don't have a town!");
                        break;
                	}

					if (t.mayor != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }

					switch (args[1]) // sub-sub command
					{
						case "name":
							if (args.Length < 3) { sender.sendMessage("Invalid set command."); break; }
							var newname = (MCTown)t;
							newname.name = args[2];
							DBInteract.updateTown(t, newname);
							sender.sendMessage($"Changed town name to \"{Town.formatName(args[2])}\"");
							break;
						case "mayor":
							if (args.Length < 3) { sender.sendMessage("Invalid set command."); break; }
							if (!t.residents.Exists(x => x == Plr.usrToGuid(args[2]))) break;
							var newmayor = Plr.usrToGuid(args[2]);

							var newt = (MCTown)t;
							newt.mayor = newmayor;
							DBInteract.updateTown(t, newt);
							sender.sendMessage($"Resigned mayor position to {newmayor}");
							break;
						case "homeblock":
							var newhome = (MCTown)t;
							var newcoord = Chunk.cToCC(((Player)sender).getLocation());
							var newchunk = Chunk.getChunk(newcoord.x, newcoord.z);
							if (newchunk == null)
							{
								sender.sendMessage("Chunk is not available.");
								break;
							}
							newhome.homeChunk = newchunk;
							DBInteract.updateTown(t, newhome);
							sender.sendMessage($"Changed home chunk to {{{newcoord.x}, {newcoord.z}}}.");
							break;
					}

					break;
				case "claim":
					if (string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You don't have a town!");
//						Console.WriteLine("bro dont had a town");
                        break;
                	}

					if (t.mayor != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }

					var newcoord2 = Chunk.cToCC(((Player)sender).getLocation());
					var newclaim = Chunk.initChunk(newcoord2.x, newcoord2.z, t.id); // UNI - to put a little less on the db
					var available = Chunk.chunkAvailable(newclaim);
					if (available != availabilityEnum.AVAILABLE)
					{
						switch (available)
						{
							case availabilityEnum.SELF_CLAIMED:
								sender.sendMessage("You already own this chunk!");
								break;
							case availabilityEnum.EX_CLAIMED:
								sender.sendMessage("Someone already owns this chunk!");
								break;
							case availabilityEnum.NOT_NEAR:
								sender.sendMessage("Chunk is not on border!");
								break;
						}
						break;
					}

					DBInteract.createChunk(newclaim);
					sender.sendMessage($"Claimed chunk {{{newcoord2.x}, {newcoord2.z}}}");
					break;
				case "invite":
					if (args.Length < 2) { sender.sendMessage("Invalid invite command."); break; }

					if (string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You don't have a town!");
                        break;
                	}

					switch (args[1]) // sub-sub command
					{
						case "add":
							if (args.Length < 3) { sender.sendMessage("Invalid invite command."); break; }
							if (t.residents.Exists(x => x == Plr.usrToGuid(args[2]))) {sender.sendMessage($"Player {args[2]} is already in this town!");break;}
							if (DBInteract.getPlr(Plr.usrToGuid(args[2])).town != LiteDB.ObjectId.Empty) {sender.sendMessage($"Player {args[2]} is already in another town!");break;}
							if (!DBInteract.isPlrReal(Plr.usrToGuid(args[2]))) {sender.sendMessage($"Player {args[2]} is not registered on this server.");break;}
							var newjoin = Plr.usrToGuid(args[2]);
							var newinv = new Invite {id=t.id,expiration=DateTime.UtcNow.AddDays(3)}; // UNI - setting expiration to 3 for now
							DBInteract.addInvite(newjoin, newinv);
							sender.sendMessage($"Invited {args[2]} to the town.");
							break;
					}

					break;
			}
		}
		return true;
	}
}
