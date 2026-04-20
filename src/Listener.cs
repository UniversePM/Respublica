namespace Respublica;

using Minecraft.Server.FourKit.Entity;
using Minecraft.Server.FourKit.Event;
using Minecraft.Server.FourKit.Event.Block;
using Minecraft.Server.FourKit.Event.Entity;
using Minecraft.Server.FourKit.Event.Player;
using Minecraft.Server.FourKit.Event.World;

public enum eventEnum {
	INTERACT_ERR,
	BREAK_ERR,
	PLACE_ERR,
	PVP_ERR,
	ATTK_ERR
}

public class RespublicaListener : Listener
{
	public readonly List<EntityType> MOBS = // list of bad mobs that the player should always be able to hit
	[
		EntityType.CAVE_SPIDER,
		EntityType.CREEPER,
		EntityType.ENDER_DRAGON, // UNI - who on gods green earth will have an ender dragon in their town??
		EntityType.ENDERMAN,
		EntityType.GHAST,
		EntityType.GIANT,
		EntityType.MAGMA_CUBE,
		EntityType.SILVERFISH,
		EntityType.SKELETON,
		EntityType.SLIME,
		EntityType.SNOWMAN,
		EntityType.SPIDER,
		EntityType.WITCH,
		EntityType.WITHER,
		EntityType.ZOMBIE
	];

	public static string toTex(eventEnum en)
	{
		switch (en) {
			case eventEnum.INTERACT_ERR:
				return "Sorry, you are not allowed to interact in this town.";
			case eventEnum.BREAK_ERR:
				return "Sorry, you are not allowed to destroy in this town.";
			case eventEnum.PLACE_ERR:
				return "Sorry, you are not allowed to place in this town.";
			case eventEnum.PVP_ERR:
				return "Sorry, you are not allowed to attack players in this town.";
			case eventEnum.ATTK_ERR:
				return "Sorry, you are not allowed to slaughter in this town.";
		}
		return "Sorry, you are not allowed to do that in this town."; // should rarely be needed
	}

	[EventHandler]
	public void onJoin(PlayerJoinEvent e)
	{
		if (!DBInteract.isPlrReal(e.getPlayer().getUniqueId())) {
			if (Plr.usrToGuid(e.getPlayer().getName()) != Guid.Empty) {
				e.getPlayer().sendMessage("[RESPUBLICA] Someone has already joined this server with your username!");
				e.getPlayer().sendMessage("Because of this, you won't be able to interact with Respublica until you have a different name.");
				return;
			}
			DBInteract.initPlr(e.getPlayer()); Console.WriteLine("Plr didn't exist, creating"); return; }

		Console.WriteLine($"[RESPUBLICA] Plr exists, name={e.getPlayer().getName()}, uid={e.getPlayer().getUniqueId()}");

		DBInteract.updatePlr(e.getPlayer().getUniqueId(), e.getPlayer().getName()); // UNI - no reason not to always do ig? would just be annoying to constantly check if the usrname is different
	}

	[EventHandler]
	public void onMove(PlayerMoveEvent e)
	{
		var cloc1 = Chunk.cToCC(e.getFrom());
		var cloc2 = Chunk.cToCC(e.getTo());
		var c1 = Chunk.getChunk(cloc1.x, cloc1.z);
		var c2 = Chunk.getChunk(cloc2.x, cloc2.z);

		if (c1?.town != c2?.town && c2?.town != LiteDB.ObjectId.Empty && c2 != null)
		{
			e.getPlayer().sendMessage($"Entering {DBInteract.getTownById(c2.town).name}!");
		}
		if (c2 == null && c2?.town == LiteDB.ObjectId.Empty && c1 != null && c1.town != LiteDB.ObjectId.Empty)
		{
			e.getPlayer().sendMessage("Entering the wild!");
		}
	}

	// interactions
	[EventHandler]
	public void onInteract(PlayerInteractEvent e)
	{
		if (e.getAction() != Minecraft.Server.FourKit.Block.Action.RIGHT_CLICK_BLOCK) return;
		var cloc = Chunk.cToCC(e.getPlayer().getLocation());
		var chunk = Chunk.getChunk(cloc.x, cloc.z);
		var town = DBInteract.getTownById(chunk?.town ?? LiteDB.ObjectId.Empty);

		if (chunk == null) return;

		if (town != DBInteract.getTown(e.getPlayer()))
		{
			e.getPlayer().sendMessage(toTex(eventEnum.INTERACT_ERR));
			e.setCancelled(true);
			return;
		}
	}

	[EventHandler]
	public void onInteractEntity(PlayerInteractEntityEvent e)
	{
		var cloc = Chunk.cToCC(e.getPlayer().getLocation());
		var chunk = Chunk.getChunk(cloc.x, cloc.z);
		var town = DBInteract.getTownById(chunk?.town ?? LiteDB.ObjectId.Empty);

		if (chunk == null) return;

		if (town != DBInteract.getTown(e.getPlayer()))
		{
			e.getPlayer().sendMessage(toTex(eventEnum.INTERACT_ERR));
			e.setCancelled(true);
			return;
		}
	}

	[EventHandler]
	public void onGrowForce(StructureGrowEvent e)
	{
		var plr = e.getPlayer();
		if (plr == null) return;

		var cloc = Chunk.cToCC(plr.getLocation());
		var chunk = Chunk.getChunk(cloc.x, cloc.z);
		var town = DBInteract.getTownById(chunk?.town ?? LiteDB.ObjectId.Empty);

		if (chunk == null) return;

		if (town != DBInteract.getTown(plr))
		{
			plr.sendMessage(toTex(eventEnum.INTERACT_ERR));
			e.setCancelled(true);
			return;
		}
	}

	// block manipulation
	[EventHandler]
	public void onBreak(BlockBreakEvent e)
	{
		var cloc = Chunk.cToCC(e.getPlayer().getLocation());
		var chunk = Chunk.getChunk(cloc.x, cloc.z);
		var town = DBInteract.getTownById(chunk?.town ?? LiteDB.ObjectId.Empty);

		if (chunk == null) return;

		if (town != DBInteract.getTown(e.getPlayer()))
		{
			e.getPlayer().sendMessage(toTex(eventEnum.BREAK_ERR));
			e.setCancelled(true);
			return;
		}
	}

	[EventHandler]
	public void onPlace(BlockPlaceEvent e)
	{
		var cloc = Chunk.cToCC(e.getPlayer().getLocation());
		var chunk = Chunk.getChunk(cloc.x, cloc.z);
		var town = DBInteract.getTownById(chunk?.town ?? LiteDB.ObjectId.Empty);

		if (chunk == null) return;

		if (town != DBInteract.getTown(e.getPlayer()))
		{
			e.getPlayer().sendMessage(toTex(eventEnum.PLACE_ERR));
			e.setCancelled(true);
			return;
		}
	}

	[EventHandler]
	public void onLiquidFlow(BlockFromToEvent e)
	{
		var from = e.getBlock();
		var to = e.getToBlock();

		if (
		Chunk.getChunk(from.getChunk().getX(), from.getChunk().getZ())?.town !=
		 Chunk.getChunk(to.getChunk().getX(), to.getChunk().getZ())?.town &&
		Chunk.getChunk(to.getChunk().getX(), to.getChunk().getZ())?.town !=
		 LiteDB.ObjectId.Empty
		) // only trigger when the var from block's chunk isn't the same town as var to's chunk and only do that if to's town isn't empty
		{
			e.setCancelled(true);
			return;
		}
	}

	// pvp
	[EventHandler]
	public void onPVP(EntityDamageByEntityEvent e)
	{
		if (e.getDamager().getType() != EntityType.PLAYER) return;
		if (MOBS.Contains(e.getEntity().getType())) return; // if hostile, don't trigger this

		var cloc = Chunk.cToCC(((Player)e.getDamager()).getLocation());
		var chunk = Chunk.getChunk(cloc.x, cloc.z);
		var town = DBInteract.getTownById(chunk?.town ?? LiteDB.ObjectId.Empty);

		if (chunk == null) return;

		if (town != DBInteract.getTown((Player)e.getDamager()))
		{
			if (e.getEntity().getType() == EntityType.PLAYER) ((Player)e.getDamager()).sendMessage(toTex(eventEnum.PVP_ERR));
			else ((Player)e.getDamager()).sendMessage(toTex(eventEnum.ATTK_ERR));
			
			e.setCancelled(true);
			return;
		}
	}
}
