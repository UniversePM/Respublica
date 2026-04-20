namespace Respublica.Commands;

using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

public class PlotCmd : CommandExecutor // Commands for managing invites
{
    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        if (sender is not Player) return true;

        var pcoord = Chunk.cToCC(((Player)sender).getLocation());
        var plot = DBInteract.getPlot(pcoord.x, pcoord.z) ?? new();
        var chunk = Chunk.getChunk(pcoord.x, pcoord.z) ?? new();
        if (DBInteract.getTownById(plot.town) == new DBTown()) // UNI - for not showing how bad the plot code is
        {
            plot.perm.PVP = true; plot.perm.EXPLOSION = true; plot.perm.FIRE = true; plot.perm.MOBS = true;
        }
        else if (string.IsNullOrEmpty(Plr.guidToUsrname(plot.owner)))
        {
            var t = DBInteract.getTownById(plot.town);
            plot.perm.PVP = t.perm.PVP; plot.perm.EXPLOSION = t.perm.EXPLOSION; plot.perm.FIRE = t.perm.FIRE; plot.perm.MOBS = t.perm.MOBS;
        }

        if (args.Length == 0)
        {
            sender.sendMessage($"--- Plot ({pcoord.x}, {pcoord.z}) ---");
            var tname = DBInteract.getTownById(plot.town).name;
            if (string.IsNullOrEmpty(tname)) tname = DBInteract.getTownById(chunk.town).name;
            sender.sendMessage(string.Format("Town: {0}", string.IsNullOrEmpty(tname) ? "None" : Town.formatName(tname))); // UNI - don't question the string.Format use
            sender.sendMessage(string.Format("Owner: {0}", string.IsNullOrEmpty(Plr.guidToUsrname(plot.owner)) ? "None" : Plr.guidToUsrname(plot.owner)));
            sender.sendMessage($"PVP: {plot.perm.PVP} EXPLOSIONS: {plot.perm.EXPLOSION} FIRE: {plot.perm.FIRE} MOBS: {plot.perm.MOBS}");
            sender.sendMessage(plot.forsale ? "Plot for sale! Do [/plot claim] to claim this plot." : "Plot not for sale.");
        }

        if (args.Length > 0)
        {
            if (string.IsNullOrEmpty(Plr.guidToUsrname(((Player)sender).getUniqueId()))) return true;
            switch (args[0])
            {
                case "claim":
                    if (chunk == null) { sender.sendMessage("No town has claimed this chunk!"); break; }
                    if (plot.owner == ((Player)sender).getUniqueId()) { sender.sendMessage("You already own this plot!"); break; }
                    if (plot.owner != Guid.Empty) { sender.sendMessage("Plot is already claimed!"); break; }
                    if (!plot.forsale) { sender.sendMessage("Plot not for sale!"); break; }

                    if (DBInteract.getPlot(pcoord.x, pcoord.z) == null) DBInteract.initPlot(chunk);
                    var claimplot = plot;
                    claimplot.owner = ((Player)sender).getUniqueId();
                    DBInteract.updatePlot(plot, claimplot);
                    sender.sendMessage($"Claimed plot {{{pcoord.x}, {pcoord.z}}}");
                    break;
            }
        }

        return true;
    }
}