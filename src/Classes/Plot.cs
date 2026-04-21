namespace Respublica;

public class PlotPerm
{
    public bool PVP { get; set; } = false;
    public bool EXPLOSION { get; set; } = false;
    public bool FIRE { get; set; } = false;
    public bool MOBS { get; set; } = false;
}

// UNI - decided to separate plots from general chunk information, might change at some later time
// UNI - not changing, but i did make MCPlot technically part of MCChunk the same way i did with PlotPerm
public class MCPlot // Class for processing plots
{
    public Guid owner { get; set; } = Guid.Empty;
    // configs
    public string name { get; set; } = "";
    public string district { get; set; } = "";
//  public int price { get; set; }
    public bool forsale { get; set; }
    public PlotPerm perm { get; set; } = new();
}