namespace Respublica;

using LiteDB;
using Minecraft.Server.FourKit;

internal class Database : IDisposable
{
    private static Database? _instance;
    private readonly LiteDatabase _db;
    private const string path = @"./plugindb";
    private Database()
    {
        if (!Directory.Exists(path)) {
			try {
				DirectoryInfo newpath = Directory.CreateDirectory(path);
				Console.WriteLine($"[RESPUBLICA] Created new plugindb folder instance in {path}");
			} catch
			{
				Console.WriteLine("[RESPUBLICA] Failed to create plugindb instance");
                var inst = Respublica.getInstance();
                if (inst == null) return;
                FourKit.disablePlugin(inst);
				return;
			}
		}
        _db = new LiteDatabase($@"{path}/RESPUBLICA_DB.db");
    }

    public static Database Instance => _instance ??= new Database();

    public ILiteCollection<T> GetCollection<T>(string name)
        => _db.GetCollection<T>(name);

    public void Dispose()
    {
        _db.Dispose();
        _instance = null;
    }
}
