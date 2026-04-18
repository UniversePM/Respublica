namespace Respublica;

public class PlotPerm
{
    public bool PVP { get; set; } = false;
    public bool EXPLOSION { get; set; } = false;
    public bool FIRE { get; set; } = false;
    public bool MOBS { get; set; } = false;
}

// UNI - decided to separate plots from general chunk information, might change at some later time
public class MCPlot : MCChunk // Class for processing plots
{
    public Guid owner { get; set; } = Guid.Empty;
    // configs
    public string name { get; set; } = "";
    public string district { get; set; } = "";
//  public int price { get; set; }
    public bool forsale { get; set; }
    public PlotPerm perm { get; set; } = new();
}

public class DBPlot : MCPlot // Class for DB Plots
{
    public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId();
}

public static partial class DBInteract
{
    public static void initPlot(MCChunk chunk)
    {
        var col = Database.Instance.GetCollection<DBPlot>("plots");
        var dbpl = new DBPlot();
		foreach (var prop in typeof(MCChunk).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbpl, prop.GetValue(chunk));
		} // Convert MCChunk to DBPlot
        col.Insert(dbpl);
    }
    public static void updatePlot(DBPlot plot, MCPlot nplot)
    {
        var col = Database.Instance.GetCollection<DBPlot>("plots");
        var dbpl = new DBPlot();
		foreach (var prop in typeof(MCPlot).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbpl, prop.GetValue(nplot));
		} // Convert MCPlot to DBPlot
        dbpl.id = plot.id;
        col.Insert(dbpl);
    }
    public static DBPlot? getPlot(int x, int z) {
		var col = Database.Instance.GetCollection<DBPlot>("plots");
		return col.Find(LiteDB.Query.EQ("x", x)).FirstOrDefault(e => e.z == z);
	}
}