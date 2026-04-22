namespace Respublica;

using System.Text;
using Minecraft.Server.FourKit.Entity;

public class MCTown { // Class for non-DB towns
	public string name { get; set; } = "";
//  public int bal; // TODO: implement once eco plugins become a thing with apis n shit
    public PlotPerm perm { get; set; } = new();
    public Guid mayor { get; set; } = Guid.Empty;
    public List<Guid> residents { get; set; } = [];
	public ChunkCoord homeChunk { get; set; } = new(); // blank nothing chunk
}

public class DBTown : MCTown { // Class for DB towns
	public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId();
}

public static class Town // Class for processing towns
{
	public static string formatName(string name) => name.Replace("_", " ");
	public static string RemoveSpecialCharacters(this string str) {
    	StringBuilder sb = new();
    	foreach (char c in str) {
        	if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-' || c == '_' || c == '\'') { // UNI - upon a users request (through an issue or pr) you can always add more conditions to this :>
    			sb.Append(c);
        	}
    	}
    	return sb.ToString();
	}
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
		var id = LiteDB.ObjectId.NewObjectId();
       	var newtown = new DBTown
    	{
			id = id,
        	name = name,
        	mayor = mayor.getUniqueId(),
			homeChunk = ccoord
        };
		var newc = Chunk.initChunk(ccoord.x, ccoord.z, id);
        newtown.residents.Add(mayor.getUniqueId());
		
		createChunk(newc); // create the chunk
		col.Insert(newtown);
	}
	public static void remTown(DBTown town) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindById(town.id) == null) { // UNI - i have no idea if this is gonna work
			Console.WriteLine("[RESPUBLICA] Tried to delete non-existent town!");
			return;
		}
		col.Delete(town.id);
	}
	public static void updateTown(DBTown town, MCTown newtown) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindById(town.id) == null) {
			Console.WriteLine("[RESPUBLICA] Tried to modify non-existent town!");
			return;
		}

		var dbt = new DBTown();
		foreach (var prop in typeof(MCTown).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbt, prop.GetValue(newtown));
		} // Convert MCTown to DBTown

		dbt.id = town.id;

		col.Update(dbt);
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
	public static DBTown getTownById(LiteDB.ObjectId id) => Database.Instance.GetCollection<DBTown>("towns").FindById(id) ?? new();
	// UNI - FindById bug fix, Claude Sonnet 4.6, applied to all functions with FindById (only getTownById was part of the prompt)
}