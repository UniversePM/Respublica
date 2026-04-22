namespace Respublica.Commands;

using System.Text;
using Minecraft.Server.FourKit;
using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

public class RespublicaCmd : CommandExecutor // Commands for managing invites
{
    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        if (sender is not Player) return true;

        if (args.Length == 0)
        {
            sender.sendMessage(["._--[Respublica]--_.",
             $"Version: {Respublica.getInstance()?.version ?? "Unknown"}",
             "RespublikLCE/Respublica on Github!"]);
            return true;
        }

        if (args.Length > 0)
        {
            if (string.IsNullOrEmpty(Plr.guidToUsrname(((Player)sender).getUniqueId()))) return true;
            switch (args[0])
            {
                case "map":
                    var mapcoord = Chunk.cToCC(((Player)sender).getLocation());
                    var mapfinal = new List<string>(); // gets converted to string[] for sendMessage
                    for (var my = 0; my < 7; my++)
                    {
                        var ns = new StringBuilder();
                        for (var mx = 0; mx < 27; mx++)
                        {
                            var mch = Chunk.getChunk(mapcoord.x+mx-13, mapcoord.z+my-4);
                            if (mx == 13 && my == 4) {ns.Append($"{ChatColor.WHITE}^"); continue;}
                            ns.Append(mch?.town != null ? $"{ChatColor.GREEN}O" : $"{ChatColor.GRAY}-");
                        }
                        mapfinal.Add(ns.ToString());
                    }
                    mapfinal.Add($"Town: {Town.formatName(DBInteract.getTownById(Chunk.getChunk(mapcoord.x, mapcoord.z)?.town ?? LiteDB.ObjectId.Empty).name)}");
                    sender.sendMessage([.. mapfinal]);
                    break;
            }
        }
        return true;
    }
}