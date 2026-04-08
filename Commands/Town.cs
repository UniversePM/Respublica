namespace Respublica.Commands;

using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

public class TownCmd : CommandExecutor
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
			sender.sendMessage($"---{Town.formatName(t.name)}---");
			sender.sendMessage($"Mayor: {t.mayorName}");
			sender.sendMessage($"PLACE: {t.DEFAULT_PLACE_PERM} BREAK: {t.DEFAULT_BREAK_PERM} FIRE: {t.DEFAULT_FIRE_PERM} MOBS: {t.DEFAULT_MOB_PERM}");
			return true;
		}

		if (args.Length > 1) {
			switch(args[0]) {
				case "new":
                case "create":
//					Console.WriteLine("create worked");
					if (!string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You already have a town!");
//						Console.WriteLine("bro had a town");
                        break;
                	}
					var cc = Chunk.cToCC(((Player)sender).getLocation());
					var nc = Chunk.getChunk(cc.x, cc.z);
					
					if (nc != null) { //Console.WriteLine("shit its not null??");
					break; }

					DBInteract.initTown((Player)sender, args[1]);
					sender.sendMessage($"Created town \"{Town.formatName(args[1])}\"!");
					break;
				case "set":
					if (args.Length < 3) { sender.sendMessage("Invalid set command."); break; }
					
					if (string.IsNullOrEmpty(t.name)) {
                        sender.sendMessage("You don't have a town!");
//						Console.WriteLine("bro dont had a town");
                        break;
                	}

					switch (args[1]) // sub-sub command
					{
						case "name":
							var newname = (MCTown)t;
							t.name = args[2];
							DBInteract.updateTown(t, newname);
							sender.sendMessage($"Changed town name to \"{Town.formatName(args[2])}\"");
							break;
					}

					break;
			}
		}
		return true;
	}
}
