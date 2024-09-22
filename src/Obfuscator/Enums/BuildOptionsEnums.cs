namespace Obfuscator.Enums;

public enum BuildOptionsEnums
{
    Debug,
    Release
}

public static class BuildOptions
{
    public static BuildOptionsEnums GetBuildOptions(string option)
    {
        try
        {
            BuildOptionsEnums defaultOption = BuildOptionsEnums.Debug;
            if (Enum.TryParse(option, true, out defaultOption))
                return defaultOption;
            return defaultOption;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BuildOptionsEnums.Debug;
        }
    }
}