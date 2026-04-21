namespace Respublica;

using Minecraft.Server.FourKit;

public class ChunkCoord { // Class for quick-storing chunk coordinates
	public int x;
	public int z;
}

public enum availabilityEnum { // Enum for easier availability notice
	AVAILABLE,
	SELF_CLAIMED, // already claimed by the town trying to claim
	EX_CLAIMED, // already claimed by a different town
	NOT_NEAR // not near any town chunks
}

public class MCChunk { // Class for Non-DB chunks
	public int x { get; set; }
    public int z { get; set; }
    public LiteDB.ObjectId town { get; set; } = LiteDB.ObjectId.Empty; // id of town that owns the land
	public MCPlot plot = new();
}

public class DBChunk : MCChunk { // Class (of MCChunk) for DB chunks
	public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId();
}

public static class Chunk { // Class for processing chunks in certain ways
	public static MCChunk initChunk(int x, int z, LiteDB.ObjectId town) => new() { x=x, z=z, town=town};

	public static DBChunk initDBc(MCChunk chunk)
	{
		var dbc = new DBChunk();
		foreach (var prop in typeof(MCChunk).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbc, prop.GetValue(chunk));
		}
		return dbc;
	}
	
	public static DBChunk? getChunk(int x, int z) {
		var col = Database.Instance.GetCollection<DBChunk>("chunks");
		return col.Find(LiteDB.Query.EQ("x", x)).FirstOrDefault(e => e.z == z);
	}

	public static List<DBChunk> nearChunks(MCChunk chunk) {
		var result = new List<DBChunk>();
		for (int i=0;i<4;i++) {
			int cx = 0;
			int cz = 0;
			if (i==0) {cx=0;cz=1;} // north
			if (i==1) {cx=0;cz=-1;} // south
			if (i==2) {cx=1;cz=0;} // east
			if (i==3) {cx=-1;cz=0;} // west
			// UNI - replaced erroneous enum 4/5/26
			int newx = chunk.x+cx;
			int newz = chunk.z+cz;
			var nc = getChunk(newx, newz);
			if (nc == null) continue;
			result.Add(nc);
		}
		return result;
	}

	public static availabilityEnum chunkAvailable(MCChunk chunk) { // chunk format: x is the chunk x, z is the chunk z, town is the town that is requesting availability
		var tcheck = getChunk(chunk.x, chunk.z)?.town; // gets the REAL town in that chunk, null if unclaimed
		if (tcheck != null) {
			if (tcheck == chunk.town) return availabilityEnum.SELF_CLAIMED;
			return availabilityEnum.EX_CLAIMED;
		}
		var near = nearChunks(chunk);
		foreach (DBChunk nearchunk in near) {
			if (nearchunk.town == tcheck) return availabilityEnum.AVAILABLE;
		}
		return availabilityEnum.NOT_NEAR;
	}

	public static ChunkCoord cToCC(Location loc) => new() { x=(int)Math.Floor(loc.getX()/16), z=(int)Math.Floor(loc.getZ()/16) };
}

public static partial class DBInteract { // DB class partition for chunks
	public static void createChunk(MCChunk chunk) {
		var col = Database.Instance.GetCollection<DBChunk>("chunks");
		if (Chunk.getChunk(chunk.x, chunk.z) != null) {
			Console.WriteLine("[RESPUBLICA] Tried to initialize DB chunk that already exists!");
			return;
		}
		col.Insert(Chunk.initDBc(chunk));
	}
	public static void remChunk(DBChunk chunk) {
		var col = Database.Instance.GetCollection<DBChunk>("chunks");
		if (Chunk.getChunk(chunk.x, chunk.z) == null) {
			Console.WriteLine("[RESPUBLICA] Tried to unclaim DB chunk that doesn't exist!");
			return;
		}
		col.Delete(chunk.id);
	}
	public static void updateChunk(DBChunk chunk, MCChunk newchunk) {
		var col = Database.Instance.GetCollection<DBChunk>("chunks");
		if (Chunk.getChunk(chunk.x, chunk.z) == null) {
			Console.WriteLine("[RESPUBLICA] Tried to modify non-existent chunk!");
			return;
		}
		col.Update(new DBChunk {
			id = chunk.id,
			x = chunk.x,
			z = chunk.z,
			town = newchunk.town // TODO: implement perms when added
		});
	}
}
