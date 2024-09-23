namespace Obfuscator.Core;

public partial class CoreBase
{
    private const bool debugMode = true;
    
    public static void DebugWrite(string message)
    {
        if (debugMode) 
            Console.WriteLine(message);
    }
}