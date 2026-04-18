namespace Respublica.Commands;

using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

public class InvCmd : CommandExecutor // Commands for managing invites
{
    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        if (sender is not Player) return true;

        if (args.Length == 0)
        {
            sender.sendMessage("--- Town invites ---");
            foreach (var al0town in DBInteract.getPlr(((Player)sender).getUniqueId()).invites) sender.sendMessage(DBInteract.getTownById(al0town.id).name);
            sender.sendMessage("Do /invite accept [name] to join a town!");
        }

        if (args.Length > 1)
        {
            switch (args[0])
            {
                case "accept":
                    if (args.Length < 2) break;

                    var act = DBInteract.getTown(args[1]);

                    if (act == null) break;

                    var acplr = DBInteract.getPlr(((Player)sender).getUniqueId());
                    if (!acplr.invites.Any(x => x.id == act.id)) break;

                    acplr.invites.RemoveAll(x => x.id == act.id);
                    acplr.town = act.id;

                    DBInteract.updatePlr(DBInteract.getPlr(((Player)sender).getUniqueId()), acplr);
                    var actown = DBInteract.getTown(args[1]);
                    actown.residents.Add(((Player)sender).getUniqueId());
                    DBInteract.updateTown(DBInteract.getTown(args[1]), actown);

                    sender.sendMessage($"You have joined {args[1]}!");

                    break;
                case "decline":
                    if (args.Length < 2) break;
                    var dct = DBInteract.getTown(args[1]);
                    if (dct == null) break;
                    var dcplr = DBInteract.getPlr(((Player)sender).getUniqueId());
                    if (args[1] == "all") { dcplr.invites.Clear(); break; }
                    if (!dcplr.invites.Any(x => x.id == dct.id)) break;
                    dcplr.invites.RemoveAll(x => x.id == dct.id);

                    DBInteract.updatePlr(DBInteract.getPlr(((Player)sender).getUniqueId()), dcplr);

                    sender.sendMessage($"You have declined {args[1]}.");
                    break;
            }
        }
        return true;
    }
}