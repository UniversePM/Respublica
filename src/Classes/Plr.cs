using Minecraft.Server.FourKit.Entity;

namespace Respublica;

public class Invite // Class for processing invites
{
    public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.Empty; // UNI - don't use this like a db object, this is just for storing town ids
    public DateTime expiration { get; set; } = DateTime.UtcNow;
}

public class Plr // Class for players and for default plot permissions
{
    public string name { get; set; } = "";
    public Guid uid { get; set; } = Guid.Empty;
    public LiteDB.ObjectId town { get; set; } = LiteDB.ObjectId.Empty;
    public PlotPerm perm { get; set; } = new();
    public List<Invite> invites { get; set; } = [];
}

public class DBPlr : Plr // DB class for player
{
    public LiteDB.ObjectId id { get; set; } = LiteDB.ObjectId.NewObjectId();
}
// UNI - literally only made this for guid to username LOL
public static class PlrRegister // Class for processing player classes
{
    /// <summary>
    /// Converts a player GUID to a username.
    /// </summary>
    /// <param name="uid">The player GUID to convert.</param>
    /// <returns>A player username.</returns>
    public static string guidToUsrname(Guid uid)
    {
        var col = Database.Instance.GetCollection<DBPlr>("plr");

        var plr = col.Find(LiteDB.Query.EQ("uid", uid)).FirstOrDefault();

        if (plr == null)
        {
            Console.WriteLine("[RESPUBLICA] No GUID match for guidToUsrname!");
            return string.Empty;
        }
        return plr.name;
    }

    /// <summary>
    /// Converts a username to a player GUID.
    /// </summary>
    /// <param name="name">The player username to convert.</param>
    /// <returns>A player GUID.</returns>
    public static Guid usrToGuid(string name)
    {
        var col = Database.Instance.GetCollection<DBPlr>("plr");

        var plr = col.Find(LiteDB.Query.EQ("name", name)).FirstOrDefault();

        if (plr == null)
        {
            Console.WriteLine("[RESPUBLICA] No GUID match for guidToUsrname!");
            return Guid.Empty;
        }
        return plr.uid;
    }
}

public static partial class DBInteract // DBInteract class partition for players
{
    /// <summary>
    /// Initializes a player in the Database.
    /// </summary>
    /// <param name="uid">The GUID of the player.</param>
    /// <param name="name">The username of the player.</param>
    public static void initPlr(Guid uid, string name)
    {
        var col = Database.Instance.GetCollection<DBPlr>("plr");

        if (isPlrReal(uid))
        {
            Console.WriteLine("[RESPUBLICA] User already exists!");
            return;
        }

        var plr = new DBPlr
        {
            name = name,
            uid = uid
        };

        Console.WriteLine($"[RESPUBLICA] Created new user {name} of uid {uid}");

        col.Insert(plr);
    }

    /// <summary>
    /// Initializes a player in the Database.
    /// </summary>
    /// <param name="plr">The player.</param>
    public static void initPlr(Player plr)
    {
        var col = Database.Instance.GetCollection<DBPlr>("plr");

        if (isPlrReal(plr.getUniqueId()))
        {
            Console.WriteLine("[RESPUBLICA] User already exists!");
            return;
        }

        var dbplr = new DBPlr
        {
            name = plr.getName(),
            uid = plr.getUniqueId()
        };

        Console.WriteLine($"[RESPUBLICA] Created new user {plr.getName()} of uid {plr.getUniqueId()}");

        col.Insert(dbplr);
    }

    /// <summary>
    /// Updates a player in the Database.
    /// </summary>
    /// <param name="plr">The Database player to update.</param>
    /// <param name="nplr">The updated player.</param>
    public static void updatePlr(DBPlr plr, Plr nplr)
    {
        var col = Database.Instance.GetCollection<DBPlr>("plr");
		if (col.FindOne(x => x.uid == plr.uid) == null) {
			Console.WriteLine("[RESPUBLICA] Tried to modify non-registered player!");
			return;
		}

        var dbp = new DBPlr();
		foreach (var prop in typeof(Plr).GetProperties())
		{
			if (prop.CanWrite) prop.SetValue(dbp, prop.GetValue(nplr));
		} // Convert MCPlr to DBPlr

        dbp.id = plr.id;

		col.Update(dbp);
    }

    /// <summary>
    /// Updates a player's name in the Database.
    /// </summary>
    /// <param name="uid">The player GUID to update.</param>
    /// <param name="name">The updated name of the player.</param>
    public static void updatePlr(Guid uid, string name)
    {
        var col = Database.Instance.GetCollection<DBPlr>("plr");

        if (!isPlrReal(uid))
        {
            Console.WriteLine("[RESPUBLICA] User doesn't exist!");
            return;
        }

        var newplr = col.Find(LiteDB.Query.EQ("uid", uid)).First();
        newplr.name = name;

        col.Update(newplr);
    }

    /// <summary>
    /// Invites a player to a town.
    /// </summary>
    /// <param name="uid">The player GUID.</param>
    /// <param name="invite">The invite object.</param>
    public static void addInvite(Guid uid, Invite invite)
    {
        var col = Database.Instance.GetCollection<DBPlr>("plr");

        if (!isPlrReal(uid))
        {
            Console.WriteLine("[RESPUBLICA] User doesn't exist!");
            return;
        }

        var newplr = getPlr(uid);
        if (newplr.town != LiteDB.ObjectId.Empty)
        {
            Console.WriteLine("[RESPUBLICA] User already in town!");
            return;
        }

        if (newplr.invites.Contains(invite))
        {
            Console.WriteLine("[RESPUBLICA] User already invited!");
        }

        newplr.invites.Add(invite);
        col.Update(newplr);
    }

    /// <summary>
    /// Gets if the player is in the Database.
    /// </summary>
    /// <param name="uid">The player GUID.</param>
    /// <returns>True if in the database, false if not.</returns>
    public static bool isPlrReal(Guid uid) => Database.Instance.GetCollection<DBPlr>("plr").FindOne(LiteDB.Query.EQ("uid", uid)) != null;

    /// <summary>
    /// Gets a player from the Database.
    /// </summary>
    /// <param name="uid">The player GUID.</param>
    /// <returns>Player if it exists, empty player if not.</returns>
    public static DBPlr getPlr(Guid uid) => Database.Instance.GetCollection<DBPlr>("plr").FindOne(LiteDB.Query.EQ("uid", uid)) ?? new();
}