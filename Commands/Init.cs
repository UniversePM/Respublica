using Minecraft.Server.FourKit;

namespace Respublica.Commands;

public static class Init
{
    public static void InitCmd() {
        FourKit.getCommand("town").setExecutor(new TownCmd());
        FourKit.getCommand("t").setExecutor(new TownCmd());
    }
}