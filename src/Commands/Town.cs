namespace Respublica.Commands;

using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

internal sealed class TownCmd : CommandExecutor // Commands for managing towns
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
			sender.sendMessage($"--- {StringManager.formatName(t.name)} ---");
			sender.sendMessage($"Townplots: {Database.Instance.GetCollection<DBChunk>("chunks").Count(LiteDB.Query.EQ("town", t.id))}"); // UNI - srry didn't feel like adding a function
			sender.sendMessage($"Mayor: {PlrRegister.guidToUsrname(t.mayor)}");
			sender.sendMessage($"PVP: {t.perm.PVP} EXPLOSIONS: {t.perm.EXPLOSION} FIRE: {t.perm.FIRE} MOBS: {t.perm.MOBS}");
			return true;
		}

		if (args.Length > 0) {
			if (string.IsNullOrEmpty(PlrRegister.guidToUsrname(((Player)sender).getUniqueId()))) return true;

			var getregfunc = (Respublica.getInstance()?.extRegisterFunc ?? []).Find(x => x.type == ExternalType.SubTown && x.cmd == args[0]);
            if (getregfunc != null)
			{
				getregfunc.func.DynamicInvoke(sender, command, label, args);
				return true;
			}

			switch(args[0]) {
				case "new":
                case "create":
					if (args.Length < 2) { sender.sendMessage("Invalid set command."); break; }
					if (!string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You already have a town!");
                        break;
                	}
					var cc = ChunkInteract.cToCC(((Player)sender).getLocation());
					var nc = ChunkInteract.getChunk(cc.x, cc.z);
					
					if (nc != null) break;

					var fixedName = StringManager.RemoveSpecialCharacters(args[1]);

					DBInteract.initTown((Player)sender, fixedName);
					sender.sendMessage($"Created town \"{StringManager.formatName(fixedName)}\"!");
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
							var newname = (Town)t;
							var fixedName1 = StringManager.RemoveSpecialCharacters(args[2]);
							newname.name = fixedName1;
							DBInteract.updateTown(t, newname);
							sender.sendMessage($"Changed town name to \"{StringManager.formatName(fixedName1)}\"");
							break;
						case "mayor":
							if (args.Length < 3) { sender.sendMessage("Invalid set command."); break; }
							if (DBInteract.getPlr(PlrRegister.usrToGuid(args[2])).town != t.id) break;
							var newmayor = PlrRegister.usrToGuid(args[2]);

							var newt = (Town)t;
							newt.mayor = newmayor;
							DBInteract.updateTown(t, newt);
							sender.sendMessage($"Resigned mayor position to {args[2]}");
							break;
						case "home":
						case "block":
						case "hb":
						case "homeblock":
							var newhome = (Town)t;
							var newcoord = ChunkInteract.cToCC(((Player)sender).getLocation());
							if (ChunkInteract.getChunk(newcoord.x, newcoord.z) == null)
							{
								sender.sendMessage("Chunk is not available.");
								break;
							}
							newhome.homeChunk = newcoord;
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

					var newcoord2 = ChunkInteract.cToCC(((Player)sender).getLocation());
					var newclaim = ChunkInteract.initChunk(newcoord2.x, newcoord2.z, t.id); // UNI - to put a little less on the db
					var available = ChunkInteract.chunkAvailable(newclaim, t.id);
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
							if (DBInteract.getPlr(PlrRegister.usrToGuid(args[2])).town == t.id) {sender.sendMessage($"Player {args[2]} is already in this town!");break;}
							if (DBInteract.getPlr(PlrRegister.usrToGuid(args[2])).town != LiteDB.ObjectId.Empty) {sender.sendMessage($"Player {args[2]} is already in another town!");break;}
							if (!DBInteract.isPlrReal(PlrRegister.usrToGuid(args[2]))) {sender.sendMessage($"Player {args[2]} is not registered on this server.");break;}
							var newjoin = PlrRegister.usrToGuid(args[2]);
							var newinv = new Invite {id=t.id,expiration=DateTime.UtcNow.AddDays(3)}; // UNI - setting expiration to 3 for now
							DBInteract.addInvite(newjoin, newinv);
							sender.sendMessage($"Invited {args[2]} to the town.");
							break;
					}

					break;
				case "trust":
					if (args.Length < 2) { sender.sendMessage("Invalid trust command."); break; }

					if (string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You don't have a town!");
                        break;
                	}

					switch (args[1]) // sub-sub command
					{
						case "add":
							if (t.mayor != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }
							if (args.Length < 3) { sender.sendMessage("Invalid trust command."); break; }
							if (t.trusted.Contains(PlrRegister.usrToGuid(args[2]))) { sender.sendMessage($"{args[2]} is already trusted in this town!"); break; }
							if (!DBInteract.isPlrReal(PlrRegister.usrToGuid(args[2]))) {sender.sendMessage($"Player {args[2]} is not registered on this server.");break;}
							t.trusted.Add(PlrRegister.usrToGuid(args[2]));
							DBInteract.updateTown(t, t);
							sender.sendMessage($"Trusted {args[2]} in this town.");
							break;
						case "remove":
							if (t.mayor != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }
							if (args.Length < 3) { sender.sendMessage("Invalid trust command."); break; }
							if (!t.trusted.Contains(PlrRegister.usrToGuid(args[2]))) { sender.sendMessage($"{args[2]} already isn't trusted!"); break; }
							// no need to check if usr is real
							t.trusted.Remove(PlrRegister.usrToGuid(args[2]));
							DBInteract.updateTown(t, t);
							sender.sendMessage($"Untrusted {args[2]} in this town.");
							break;
						case "list":
							sender.sendMessage("--- Trusted ---");
            				foreach (var tlp in t.trusted) sender.sendMessage(PlrRegister.guidToUsrname(tlp) ?? "? (Invalid user)");
							break;
					}
					break;
			}
		}
		return true;
	}
}
