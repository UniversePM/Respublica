namespace Respublica.Commands;

using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

internal sealed class PlotCmd : CommandExecutor // Commands for managing invites
{
    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        if (sender is not Player) return true;

        var pcoord = ChunkInteract.cToCC(((Player)sender).getLocation());
        var chunk = ChunkInteract.getChunk(pcoord.x, pcoord.z) ?? new();
        var t = DBInteract.getTownById(chunk.town);
        var plot = chunk.plot;
        if (t == null) // UNI - for not showing how bad the plot code is
        {
            plot.perm.PVP = true; plot.perm.EXPLOSION = true; plot.perm.FIRE = true; plot.perm.MOBS = true;
        }
        else if (string.IsNullOrEmpty(PlrRegister.guidToUsrname(plot.owner)))
        {
            plot.perm.PVP = t.perm.PVP; plot.perm.EXPLOSION = t.perm.EXPLOSION; plot.perm.FIRE = t.perm.FIRE; plot.perm.MOBS = t.perm.MOBS;
        }

        if (args.Length == 0)
        {
            sender.sendMessage($"--- Plot ({pcoord.x}, {pcoord.z}) ---");
            sender.sendMessage(string.Format("Town: {0}", string.IsNullOrEmpty(t?.name) ? "None" : StringManager.formatName(t.name))); // UNI - don't question the string.Format use
            sender.sendMessage(string.Format("Owner: {0}", string.IsNullOrEmpty(PlrRegister.guidToUsrname(plot.owner)) ? "None" : PlrRegister.guidToUsrname(plot.owner)));
            sender.sendMessage($"PVP: {plot.perm.PVP} EXPLOSIONS: {plot.perm.EXPLOSION} FIRE: {plot.perm.FIRE} MOBS: {plot.perm.MOBS}");
            sender.sendMessage(plot.forsale ? "Plot for sale! Do [/plot claim] to claim this plot." : "Plot not for sale.");
        }

        if (args.Length > 0)
        {
            if (string.IsNullOrEmpty(PlrRegister.guidToUsrname(((Player)sender).getUniqueId()))) return true;

            var getregfunc = (Respublica.getInstance()?.extRegisterFunc ?? []).Find(x => x.type == ExternalType.SubPlot && x.cmd == args[0]);
            if (getregfunc != null)
			{
				getregfunc.func.DynamicInvoke(sender, command, label, args);
				return true;
			}

            switch (args[0])
            {
                case "claim":
                    if (chunk == null) { sender.sendMessage("No town has claimed this chunk!"); break; }
                    if (plot.owner == ((Player)sender).getUniqueId()) { sender.sendMessage("You already own this plot!"); break; }
                    if (plot.owner != Guid.Empty) { sender.sendMessage("Plot is already claimed!"); break; }
                    if (!plot.forsale) { sender.sendMessage("Plot not for sale!"); break; }
                    chunk.plot.owner = ((Player)sender).getUniqueId();
                    DBInteract.updateChunk(chunk, chunk); // UNI - how did i never think of doing this
                    sender.sendMessage($"Claimed plot {{{pcoord.x}, {pcoord.z}}}");
                    break;
                case "trust":
					if (args.Length < 2) { sender.sendMessage("Invalid trust command."); break; }

					switch (args[1]) // sub-sub command
					{
						case "add":
							if (plot.owner != ((Player)sender).getUniqueId()) { sender.sendMessage("You don't own this plot!"); break; }
							if (args.Length < 3) { sender.sendMessage("Invalid trust command."); break; }
							if (plot.trusted.Contains(PlrRegister.usrToGuid(args[2]))) { sender.sendMessage($"{args[2]} is already trusted in this plot!"); break; }
							if (!DBInteract.isPlrReal(PlrRegister.usrToGuid(args[2]))) {sender.sendMessage($"Player {args[2]} is not registered on this server.");break;}
							chunk.plot.trusted.Add(PlrRegister.usrToGuid(args[2]));
							DBInteract.updateChunk(chunk, chunk);
							sender.sendMessage($"Trusted {args[2]} in this plot.");
							break;
						case "remove":
							if (plot.owner != ((Player)sender).getUniqueId()) { sender.sendMessage("You don't own this plot!"); break; }
							if (args.Length < 3) { sender.sendMessage("Invalid trust command."); break; }
							if (!plot.trusted.Contains(PlrRegister.usrToGuid(args[2]))) { sender.sendMessage($"{args[2]} already isn't trusted!"); break; }
							// no need to check if usr is real
							chunk.plot.trusted.Remove(PlrRegister.usrToGuid(args[2]));
							DBInteract.updateChunk(chunk, chunk);
							sender.sendMessage($"Untrusted {args[2]} in this plot.");
							break;
						case "list":
							sender.sendMessage("--- Trusted ---");
            				foreach (var tlp in plot.trusted) sender.sendMessage(PlrRegister.guidToUsrname(tlp) ?? "? (Invalid user)");
							break;
					}
					break;
            }
        }

        return true;
    }
}