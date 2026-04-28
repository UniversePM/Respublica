namespace Respublica;

using System.Text;
using Minecraft.Server.FourKit.Entity;

public class Town { // Class for non-DB towns
	public string name { get; set; } = "";
//  public int bal; // TODO: implement once eco plugins become a thing with apis n shit
    public PlotPerm perm { get; set; } = new();
    public Guid mayor { get; set; } = Guid.Empty;
    public List<Guid> trusted { get; set; } = [];
	public ChunkCoord homeChunk { get; set; } = new(); // blank nothing chunk
	public LiteDB.ObjectId nation { get; set; } = LiteDB.ObjectId.Empty;
	public Dictionary<string, object> attributes {get; set; } = [];
}

public class DBTown : Town { // Class for DB towns
	public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId();
}

public static class StringManager // Class for processing strings
{
	/// <summary>
	/// Formats the name to remove underscores.
	/// </summary>
	/// <param name="name">Name that you wish to format.</param>
	/// <returns>Formatted name. (Hello_World -> Hello World)<returns>
	public static string formatName(string name) => name.Replace("_", " ");

	/// <summary>
	/// Removes odd characters from a string.
	/// </summary>
	/// <param name="str">The string to process.</param>
	/// <returns>Processed string.</returns>
	public static string RemoveSpecialCharacters(string str) {
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

	/// <summary>
	/// Initializes a town in the Database.
	/// </summary>
	/// <param name="mayor">The mayor for the town.</param>
	/// <param name="name">The name of the town.</param>
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
		var ccoord = ChunkInteract.cToCC(mayor.getLocation());
		var id = LiteDB.ObjectId.NewObjectId();
       	var newtown = new DBTown
    	{
			id = id,
        	name = name,
        	mayor = mayor.getUniqueId(),
			homeChunk = ccoord
        };
		var newc = ChunkInteract.initChunk(ccoord.x, ccoord.z, id);
		var newp = getPlr(mayor.getUniqueId());
		newp.town = id;
        updatePlr(newp, newp);
		
		createChunk(newc); // create the chunk
		col.Insert(newtown);
	}

	/// <summary>
	/// Deletes a town in the Database.
	/// </summary>
	/// <param name="town">The town to delete.</param>
	public static void remTown(DBTown town) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindById(town.id) == null) { // UNI - i have no idea if this is gonna work
			Console.WriteLine("[RESPUBLICA] Tried to delete non-existent town!");
			return;
		}
		col.Delete(town.id);
	}

	/// <summary>
	/// Deletes a town in the Database.
	/// </summary>
	/// <param name="id">The town ID to delete.</param>
	public static void remTown(LiteDB.ObjectId id) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindById(id) == null) { // UNI - i have no idea if this is gonna work
			Console.WriteLine("[RESPUBLICA] Tried to delete non-existent town!");
			return;
		}
		col.Delete(id);
	}

	/// <summary>
	/// Updates a town in the Database.
	/// </summary>
	/// <param name="town">The old town.</param>
	/// <param name="newtown">The updated town.</param>
	public static void updateTown(DBTown town, Town newtown) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindById(town.id) == null) {
			Console.WriteLine("[RESPUBLICA] Tried to modify non-existent town!");
			return;
		}

		var dbt = new DBTown();
		foreach (var prop in typeof(Town).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbt, prop.GetValue(newtown));
		} // Convert MCTown to DBTown

		dbt.id = town.id;

		col.Update(dbt);
	}

	/// <summary>
	/// Updates a town in the Database.
	/// </summary>
	/// <param name="id">The town ID to update.</param>
	/// <param name="newtown">The updated town.</param>
	public static void updateTown(LiteDB.ObjectId id, Town newtown) {
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindById(id) == null) {
			Console.WriteLine("[RESPUBLICA] Tried to modify non-existent town!");
			return;
		}

		var dbt = new DBTown();
		foreach (var prop in typeof(Town).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbt, prop.GetValue(newtown));
		} // Convert MCTown to DBTown

		dbt.id = id;

		col.Update(dbt);
	}

	/// <summary>
	/// Gets a town by name.
	/// </summary>
	/// <param name="name">The town name to query.</param>
	/// <returns>The queried Database town.</returns>
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

	/// <summary>
	/// Gets a town by player.
	/// </summary>
	/// <param name="plr">The player to query.</param>
	/// <returns>The queried Database town.</returns>
	public static DBTown getTown(Player plr) => getTownById(getPlr(plr.getUniqueId()).town) ?? new DBTown();

	/// <summary>
	/// Gets a town ID by name.
	/// </summary>
	/// <param name="name">The town name to query.</param>
	/// <returns>The queried Town ID.</returns>
	public static LiteDB.ObjectId getTownId(string name)
	{
		var col = Database.Instance.GetCollection<DBTown>("towns");
		if (col.FindOne(LiteDB.Query.EQ("name", name)) == null)
		{
			Console.WriteLine("[RESPUBLICA] Tried to get non-existent town ID!");
			return LiteDB.ObjectId.Empty;
		} else return col.Find(LiteDB.Query.EQ("name", name)).First().id;
	}

	/// <summary>
	/// Gets a town ID by player.
	/// </summary>
	/// <param name="plr"></param>
	/// <returns></returns>
	public static LiteDB.ObjectId getTownId(Player plr) => getPlr(plr.getUniqueId()).town;

	/// <summary>
	/// Gets a town by ID.
	/// </summary>
	/// <param name="id">Town ID to query.</param>
	/// <returns>The queried Database town.</returns>
	public static DBTown? getTownById(LiteDB.ObjectId id) => Database.Instance.GetCollection<DBTown>("towns").FindById(id);
	// UNI - FindById bug fix, Claude Sonnet 4.6, applied to all functions with FindById (only getTownById was part of the prompt)
}
