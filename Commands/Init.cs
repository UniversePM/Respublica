using Minecraft.Server.FourKit;

namespace Respublica.Commands;

public static class Init // Function/class for initializing all the commands (for cleanliness)
{
    public static void InitCmd() {
        FourKit.getCommand("town").setExecutor(new TownCmd());
        FourKit.getCommand("t").setExecutor(new TownCmd());
        FourKit.getCommand("invite").setExecutor(new InvCmd());
        FourKit.getCommand("inv").setExecutor(new InvCmd());
    }
}