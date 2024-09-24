namespace Obfuscator.Core;

public class CoreBase
{
    public static void DebugWrite(string message)
    {
        #if DEBUG
        Console.WriteLine(message);
        #endif
    }
}