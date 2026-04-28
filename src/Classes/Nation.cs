using Minecraft.Server.FourKit.Entity;

namespace Respublica;

public class Nation {
	public string name { get; set; } = "";
	public LiteDB.ObjectId capital { get; set; } = LiteDB.ObjectId.Empty;
	public Guid king { get; set; } = Guid.Empty;
}

public class DBNation : Nation {
	public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId(); // if wrong func name fix
}

public static partial class DBInteract { // DBInteract class partition for nations
// TODO: eh fuck it i'll do the docs for this one a different time
	public static void initNation(Player king, string name) {
		var col = Database.Instance.GetCollection<DBNation>("nations");
		if (col.FindOne(LiteDB.Query.EQ("name", name)) != null) {
			Console.WriteLine("[RESPUBLICA] Tried to initialize nation that already exists!");
			return;
		}
		if (col.FindOne(LiteDB.Query.EQ("king", king.getUniqueId())) != null) {
			Console.WriteLine("[RESPUBLICA] King already has a nation!");
			return;
		}
		var id = LiteDB.ObjectId.NewObjectId();
       	var newnation = new DBNation
    	{
			id = id,
        	name = name,
        	king = king.getUniqueId(),
			capital = getTown(king).id
        };
		var t = getTown(king);
		t.nation = id;
		col.Insert(newnation);
		updateTown(t, t);
	}
	public static void remNation(DBNation nation) {
		var col = Database.Instance.GetCollection<DBNation>("nations");
		if (col.FindById(nation.id) == null) { // UNI - i have no idea if this is gonna work
			Console.WriteLine("[RESPUBLICA] Tried to delete non-existent nation!");
			return;
		}
		col.Delete(nation.id);
	}
	public static void updateNation(DBNation nation, Nation newnation) {
		var col = Database.Instance.GetCollection<DBNation>("nations");
		if (col.FindById(nation.id) == null) {
			Console.WriteLine("[RESPUBLICA] Tried to modify non-existent nation!");
			return;
		}

		var dbt = new DBNation();
		foreach (var prop in typeof(Nation).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbt, prop.GetValue(newnation));
		} // Convert Nation to DBNation

		dbt.id = nation.id;

		col.Update(dbt);
	}
	public static DBNation getNation(string name)
	{
		var col = Database.Instance.GetCollection<DBNation>("nations");
		if (col.FindOne(LiteDB.Query.EQ("name", name)) == null)
		{
			Console.WriteLine("[RESPUBLICA] Tried to get non-existent nation!");
			return new DBNation();
		} else
		{
			return col.Find(LiteDB.Query.EQ("name", name)).First();
		}
	}
	public static DBNation getNation(Player plr)
	{
		var getT = getTownById(getPlr(plr.getUniqueId()).town);
		if (getT == null) return new DBNation();
		var getN = getNationById(getT.nation); // UNI - total eyesore
		if (getN == null) return new DBNation();
		return getN;
	}
	public static DBNation? getNationById(LiteDB.ObjectId id) => Database.Instance.GetCollection<DBNation>("nations").FindById(id);
	// UNI - FindById bug fix, Claude Sonnet 4.6, applied to all functions with FindById (only getTownById was part of the prompt)
}
