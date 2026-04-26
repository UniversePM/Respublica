using System.Reflection;

namespace Respublica;

public class RespublicaAPI
{
    private static RespublicaAPI? _instance;
    public static RespublicaAPI getInstance() => _instance ??= new RespublicaAPI();

    internal static Respublica? getRespublica()
    {
        if (Respublica.getInstance() == null) Respublica.setInstance(new Respublica());
        return Respublica.getInstance();
    }

    public static void registerExternal(string type, string name, string cmd, Delegate func)
    {
        var exex = new ExternalFunc
        {
            type = type,
            name = name,
            cmd = cmd,
            func = func
        };
        getRespublica()?.extRegisterFunc.Add(exex);
    }
}