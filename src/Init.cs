using System.Text;
using Minecraft.Server.FourKit;

namespace Respublica;

internal static class Init // Function/class for initializing all the commands (for cleanliness)
{
    public static string centerSpaces(string version)
    {
        var sb = new StringBuilder();
        int vc = 0;
        int sc = 20; // UNI - amount of spaces to the middle, magic value
        foreach (char c in version) vc++;
        for (var i = 0; i <= sc-vc/2; i++) sb.Append(' ');
        return sb.ToString();
    } // UNI - all this literally for a funny print
    static readonly string[] boot =
    [
        "         _____________",
        "        /             \\_",
        "        \\               \\",
        "         |_              |",
        "           \\             \\    _",
        "            \\             \\__/ \\",
        "             \\                 /___",
        "              |_^                  \\__",
        "                 |__              ___ \\",
        "                    \\           /   \\_/",
        "                     |         /",
        "                      \\__     |",
        "                          \\    \\",
        "                           |    \\",
        "                           |    /",
        "       ___        _______ /____/",
        "      /   \\______/       /",
        "      \\__              /",
        "         \\__           \\",
        "            \\_____      |",
        "                  \\_____/",
        "                RESPUBLICA",
        string.Format("{0}{1}", [centerSpaces($"v{Respublica.getInstance()?.version}"), $"v{Respublica.getInstance()?.version}"])
    ];

    public static void InitCmd() {
        FourKit.getCommand("town").setExecutor(new Commands.TownCmd());
        FourKit.getCommand("t").setExecutor(new Commands.TownCmd());
        FourKit.getCommand("invite").setExecutor(new Commands.InvCmd());
        FourKit.getCommand("inv").setExecutor(new Commands.InvCmd());
        FourKit.getCommand("plot").setExecutor(new Commands.PlotCmd());
        FourKit.getCommand("p").setExecutor(new Commands.PlotCmd());
        FourKit.getCommand("respublica").setExecutor(new Commands.RespublicaCmd());
        FourKit.getCommand("rpba").setExecutor(new Commands.RespublicaCmd());
    }
    public static void InitPnt()
    {
        foreach (string line in boot) Console.WriteLine(line);
    }
}