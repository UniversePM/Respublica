namespace Respublica.Commands;

using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

internal sealed class NationCmd : CommandExecutor // Commands for managing nations
{
	public bool onCommand(CommandSender sender, Command command, string label, string[] args)
	{
		if (sender is not Player) return true;

		var n = DBInteract.getNation((Player)sender);
		
		// /t or /town
		if (args.Length == 0) {
			if (string.IsNullOrEmpty(n.name)) {
                sender.sendMessage("You don't have a nation!");
                return true;
            }
			sender.sendMessage($"--- {StringManager.formatName(n.name)} ---");
			sender.sendMessage($"Towns: {Database.Instance.GetCollection<DBTown>("towns").Count(LiteDB.Query.EQ("nation", n.id))}"); // UNI - srry didn't feel like adding a function
			sender.sendMessage($"Leader: {PlrRegister.guidToUsrname(n.king)}");
			return true;
		}

		if (args.Length > 0) {
			if (string.IsNullOrEmpty(PlrRegister.guidToUsrname(((Player)sender).getUniqueId()))) return true;

			var getregfunc = (Respublica.getInstance()?.extRegisterFunc ?? []).Find(x => x.type == ExternalType.SubNation && x.cmd == args[0]);
            if (getregfunc != null)
			{
				getregfunc.func.DynamicInvoke(sender, command, label, args);
				return true;
			}

			switch(args[0]) {
				case "new":
                case "create":
					if (args.Length < 2) { sender.sendMessage("Invalid set command."); break; }
                    if (string.IsNullOrEmpty(DBInteract.getTown((Player)sender).name)) {
                        sender.sendMessage("You don't have a town!");
                        break;
                	}

					if (DBInteract.getTown((Player)sender).mayor != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }
					if (!string.IsNullOrEmpty(n.name)) {
                        sender.sendMessage("You are already in a nation!");
                        break;
                	}
                    // TODO: add more checks, refer to initNation()

                    var fixedName = StringManager.RemoveSpecialCharacters(args[1]);

					DBInteract.initNation((Player)sender, fixedName);
					sender.sendMessage($"Created nation \"{StringManager.formatName(fixedName)}\"!");
					break;
				case "set":
					if (args.Length < 2) { sender.sendMessage("Invalid set command."); break; }
					if (string.IsNullOrEmpty(n.name)) {
                        sender.sendMessage("You don't have a nation!");
                        break;
                	}

					if (n.king != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }

					switch (args[1]) // sub-sub command
					{
						case "name":
							if (args.Length < 3) { sender.sendMessage("Invalid set command."); break; }
                            var fixedName1 = StringManager.RemoveSpecialCharacters(args[2]);
							n.name = fixedName1;
							DBInteract.updateNation(n, n);
							sender.sendMessage($"Changed nation name to \"{StringManager.formatName(fixedName1)}\"");
							break;
						case "leader":
							if (args.Length < 3) { sender.sendMessage("Invalid set command."); break; }
							if (DBInteract.getPlr(PlrRegister.usrToGuid(args[2])).town != n.capital) break;
							var newleader = PlrRegister.usrToGuid(args[2]);

							n.king = newleader;
							DBInteract.updateNation(n, n);
							sender.sendMessage($"Resigned leader position to {args[2]}");
							break;
					}

					break;
				// case "invite":
				// 	if (args.Length < 2) { sender.sendMessage("Invalid invite command."); break; }

				// 	if (string.IsNullOrEmpty(n.name)) {
                //         sender.sendMessage("You don't have a town!");
                //         break;
                // 	}

				// 	switch (args[1]) // sub-sub command
				// 	{
				// 		case "add":
				// 			if (args.Length < 3) { sender.sendMessage("Invalid invite command."); break; }
				// 			if (DBInteract.getPlr(PlrInteract.usrToGuid(args[2])).town == n.id) {sender.sendMessage($"Player {args[2]} is already in this town!");break;}
				// 			if (DBInteract.getPlr(PlrInteract.usrToGuid(args[2])).town != LiteDB.ObjectId.Empty) {sender.sendMessage($"Player {args[2]} is already in another town!");break;}
				// 			if (!DBInteract.isPlrReal(PlrInteract.usrToGuid(args[2]))) {sender.sendMessage($"Player {args[2]} is not registered on this server.");break;}
				// 			var newjoin = PlrInteract.usrToGuid(args[2]);
				// 			var newinv = new Invite {id=n.id,expiration=DateTime.UtcNow.AddDays(3)}; // UNI - setting expiration to 3 for now
				// 			DBInteract.addInvite(newjoin, newinv);
				// 			sender.sendMessage($"Invited {args[2]} to the town.");
				// 			break;
				// 	}

				// 	break;
				// case "trust":
				// 	if (args.Length < 2) { sender.sendMessage("Invalid trust command."); break; }

				// 	if (string.IsNullOrEmpty(n.name)) {
                //         sender.sendMessage("You don't have a town!");
                //         break;
                // 	}

				// 	switch (args[1]) // sub-sub command
				// 	{
				// 		case "add":
				// 			if (n.mayor != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }
				// 			if (args.Length < 3) { sender.sendMessage("Invalid trust command."); break; }
				// 			if (n.trusted.Contains(PlrInteract.usrToGuid(args[2]))) { sender.sendMessage($"{args[2]} is already trusted in this town!"); break; }
				// 			if (!DBInteract.isPlrReal(PlrInteract.usrToGuid(args[2]))) {sender.sendMessage($"Player {args[2]} is not registered on this server.");break;}
				// 			n.trusted.Add(PlrInteract.usrToGuid(args[2]));
				// 			DBInteract.updateTown(n, n);
				// 			sender.sendMessage($"Trusted {args[2]} in this town.");
				// 			break;
				// 		case "remove":
				// 			if (n.mayor != ((Player)sender).getUniqueId()) { sender.sendMessage("You do not have permission to use this command."); break; }
				// 			if (args.Length < 3) { sender.sendMessage("Invalid trust command."); break; }
				// 			if (!n.trusted.Contains(PlrInteract.usrToGuid(args[2]))) { sender.sendMessage($"{args[2]} already isn't trusted!"); break; }
				// 			// no need to check if usr is real
				// 			n.trusted.Remove(PlrInteract.usrToGuid(args[2]));
				// 			DBInteract.updateTown(n, n);
				// 			sender.sendMessage($"Untrusted {args[2]} in this town.");
				// 			break;
				// 		case "list":
				// 			sender.sendMessage("--- Trusted ---");
            	// 			foreach (var tlp in n.trusted) sender.sendMessage(PlrInteract.guidToUsrname(tlp) ?? "? (Invalid user)");
				// 			break;
				// 	}
				// 	break;
			}
		}
		return true;
	}
}
