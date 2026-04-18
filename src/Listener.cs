namespace Respublica;

using Minecraft.Server.FourKit.Event;
using Minecraft.Server.FourKit.Event.Player;

public enum eventEnum {
	INTERACT_ERR,
	BREAK_ERR,
	PLACE_ERR
}

public class RespublicaListener : Listener
{
	public static string toTex(eventEnum en)
	{
		if (en == eventEnum.INTERACT_ERR) return "Sorry, you are not allowed to interact in this town.";
		if (en == eventEnum.BREAK_ERR) return "Sorry, you are not allowed to destroy in this town.";
		if (en == eventEnum.PLACE_ERR) return "Sorry, you are not allowed to place in this town.";
		return "";
	}

	[EventHandler]
	public void onJoin(PlayerJoinEvent e)
	{
		if (!DBInteract.isPlrReal(e.getPlayer().getUniqueId())) { DBInteract.initPlr(e.getPlayer()); Console.WriteLine("Plr didn't exist, creating"); return; }

		Console.WriteLine($"Plr exists, name={e.getPlayer().getName()}, uid={e.getPlayer().getUniqueId()}");

		DBInteract.updatePlr(e.getPlayer().getUniqueId(), e.getPlayer().getName()); // UNI - no reason not to always do ig? would just be annoying to constantly check if the usrname is different
	}

	// interactions
	[EventHandler]
	public void onInteract(PlayerInteractEvent e)
	{
		var cloc = Chunk.cToCC(e.getPlayer().getLocation());
		var chunk = Chunk.getChunk(cloc.x, cloc.z);

		if (chunk == null) return;

		if (DBInteract.getTownById(chunk.town) != DBInteract.getTown(e.getPlayer()))
		{
			e.getPlayer().sendMessage(toTex(eventEnum.INTERACT_ERR));
			e.setCancelled(true);
			return;
		}
	}
}
