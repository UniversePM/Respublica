namespace Respublica;

using Minecraft.Server.FourKit.Entity;

public class MCTown {
	public string name { get; set; } = "";
//      public int bal; // TODO: implement once eco plugins become a thing with apis n shit
        public bool DEFAULT_FIRE_PERM { get; set; } = false;
        public bool DEFAULT_BREAK_PERM { get; set; } = false;
        public bool DEFAULT_PLACE_PERM { get; set; } = false;
        public bool DEFAULT_MOB_PERM { get; set; } = false; // TODO: make these perms configurable in the future
	public string mayorName { get; set; } = "";
        public Guid mayor { get; set; } = Guid.Empty;
        public List<Guid> residents { get; set; } = new List<Guid>();
	public MCChunk homeChunk { get; set; } = Chunk.initChunk(0, 0, ""); // blank nothing chunk
}

public class DBTown : MCTown {
	public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId();
}

public static class Town
{
	public static string formatName(string name) => name.Replace("_", " ");
}

public static partial class DBInteract {
	public static void initTown(Player mayor, string name) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.Exists(LiteDB.Query.EQ("name", name))) {
			Console.WriteLine("[RESPUBLICA] Tried to initialize town that already exists!");
			return;
		}
		if (col.Exists(LiteDB.Query.EQ("mayor", mayor.getUniqueId()))) {
			Console.WriteLine("[RESPUBLICA] Mayor already has a town!");
			return;
		}
		var ccoord = Chunk.cToCC(mayor.getLocation());
		var newc = Chunk.initChunk(ccoord.x, ccoord.z, name);
       		var newtown = new DBTown
        	{
            		name = name,
			mayorName = mayor.getName(),
            		mayor = mayor.getUniqueId(),
			homeChunk = newc
        	};
        	newtown.residents.Add(mayor.getUniqueId());
		
		createChunk(newc); // create the chunk
		col.Insert(newtown);
	}
	public static void remTown(DBTown town) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (!col.Exists(x => x == town)) { // UNI - i have no idea if this is gonna work
			Console.WriteLine("[RESPUBLICA] Tried to delete non-existent town!");
			return;
		}
		col.Delete(town.id);
	}
	public static void updateTown(DBTown town, MCTown newtown) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (!col.Exists(x => x == town)) {
			Console.WriteLine("[RESPUBLICA] Tried to modify non-existent town!");
			return;
		}
		col.Update(new DBTown {
			id = town.id,
			name = newtown.name,
			DEFAULT_FIRE_PERM = newtown.DEFAULT_FIRE_PERM,
			DEFAULT_BREAK_PERM = newtown.DEFAULT_BREAK_PERM,
			DEFAULT_PLACE_PERM = newtown.DEFAULT_PLACE_PERM,
			DEFAULT_MOB_PERM = newtown.DEFAULT_MOB_PERM,
			mayorName = newtown.mayorName,
			mayor = newtown.mayor,
			residents = newtown.residents,
			homeChunk = newtown.homeChunk
		}); // UNI - there has to be a better way of doing this omfg
	}
	public static DBTown getTown(string name)
	{
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (!col.Exists(LiteDB.Query.EQ("name", name)))
		{
			Console.WriteLine("[RESPUBLICA] Tried to get non-existent town!");
			return new DBTown();
		} else
		{
			return col.Find(LiteDB.Query.EQ("name", name)).First();
		}
	}
	public static DBTown getTown(Player plr)
	{
		var col = Database.Instance.GetCollection<DBTown>("towns");
		var getT = col.Find(x => x.residents.Contains(plr.getUniqueId()))?.FirstOrDefault(); // UNI - total eyesore
		if (getT == null) return new DBTown();
		else return getT;
	}
}
