namespace Respublica;

using Minecraft.Server.FourKit.Entity;

public class MCTown { // Class for non-DB towns
	public string name { get; set; } = "";
//  public int bal; // TODO: implement once eco plugins become a thing with apis n shit
    public bool DEFAULT_FIRE_PERM { get; set; } = false;
    public bool DEFAULT_BREAK_PERM { get; set; } = false;
    public bool DEFAULT_PLACE_PERM { get; set; } = false;
    public bool DEFAULT_MOB_PERM { get; set; } = false; // TODO: make these perms configurable in the future
    public Guid mayor { get; set; } = Guid.Empty;
    public List<Guid> residents { get; set; } = new List<Guid>();
	public MCChunk homeChunk { get; set; } = Chunk.initChunk(0, 0, ""); // blank nothing chunk
}

public class DBTown : MCTown { // Class for DB towns
	public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId();
}

public static class Town // Class for processing towns
{
	public static string formatName(string name) => name.Replace("_", " ");
}

public static partial class DBInteract { // DBInteract class partition for towns
	public static void initTown(Player mayor, string name) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindOne(LiteDB.Query.EQ("name", name)) != null) {
			Console.WriteLine("[RESPUBLICA] Tried to initialize town that already exists!");
			return;
		}
		if (col.FindOne(LiteDB.Query.EQ("mayor", mayor.getUniqueId())) != null) {
			Console.WriteLine("[RESPUBLICA] Mayor already has a town!");
			return;
		}
		var ccoord = Chunk.cToCC(mayor.getLocation());
		var newc = Chunk.initChunk(ccoord.x, ccoord.z, name);
       		var newtown = new DBTown
        	{
            	name = name,
            	mayor = mayor.getUniqueId(),
				homeChunk = newc
        	};
        	newtown.residents.Add(mayor.getUniqueId());
		
		createChunk(newc); // create the chunk
		col.Insert(newtown);
	}
	public static void remTown(DBTown town) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindOne(x => x == town) == null) { // UNI - i have no idea if this is gonna work
			Console.WriteLine("[RESPUBLICA] Tried to delete non-existent town!");
			return;
		}
		col.Delete(town.id);
	}
	public static void updateTown(DBTown town, MCTown newtown) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindOne(x => x.id == town.id) == null) {
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
			mayor = newtown.mayor,
			residents = newtown.residents,
			homeChunk = newtown.homeChunk
		}); // UNI - there has to be a better way of doing this omfg
	}
	public static DBTown getTown(string name)
	{
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindOne(LiteDB.Query.EQ("name", name)) == null)
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
	public static DBTown getTownById(LiteDB.ObjectId id) => Database.Instance.GetCollection<DBTown>("towns").Find(x => x.id == id).FirstOrDefault() ?? new DBTown();
}