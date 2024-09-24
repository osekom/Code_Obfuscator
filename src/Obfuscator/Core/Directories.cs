using Obfuscator.Enums;

namespace Obfuscator.Core;

public class Directories : CoreBase
{
    #region Constant

    private const string WRONG_DIRECTORY = "Directory does not exist.";
    private const string WRONG_FILE = "File does not exist.";
    
    #endregion

    #region Properties

    private static readonly string[] DirectoriesToIgnore = new[]
    {
        "bin",
        "obj",
        "Resources",
        "Platforms",
        "Properties"
    };

    #endregion

    public Directories()
    {
        
    }
    
    private static bool FileExists(string nameFile) => File.Exists(nameFile);
    
    private static bool DirectoryExists(string? path) => Directory.Exists(path);
    
    public static string[] GetDirectories(string path, SearchOption option) => Directory.GetDirectories(Path.GetDirectoryName(path)?? string.Empty, "*", option);
    
    public static string[] GetFiles(string path, SearchOption option) => Directory.GetFiles(Path.GetDirectoryName(path)?? string.Empty, "*", option);

    public static ResponsePath CheckPathAndFileExists(string filePath)
    {
        ResponsePath responsePath = new ResponsePath();

        if (!DirectoryExists(Path.GetDirectoryName(filePath.Trim())))
        {
            responsePath.Message = WRONG_DIRECTORY;
        }

        if (!FileExists(filePath))
        {
            responsePath.Message = WRONG_FILE;   
        }

        return responsePath;

    }

    public static void WriteFileCS(string content, string filePath)
    {
        if (!CheckPathAndFileExists(filePath).IsOk) return;

        File.ReadAllText(filePath);
        File.WriteAllText(filePath, content);
        DebugWrite($@"Update file: {Path.GetFileName(filePath)}");
    }

    /// <summary>
    /// Return files in especefic directory.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="option"></param>
    /// <returns><cref="FilesInPath"></cref></returns>
    /// <remarks>
    /// Project = return files only project.
    /// Directory = return all files in the directory.
    /// </remarks>
    public static List<FilesInPath> GetFilesInPath(string path, TypeSearch option)
    {
        List<FilesInPath> filesInPath = [];

        if (CheckPathAndFileExists(path).IsOk)
        {
            var directories = GetDirectories(path, SearchOption.AllDirectories);
            
            //root directory project
            GetFiles(path, SearchOption.TopDirectoryOnly)
                .ToList()
                .ForEach(f => filesInPath.Add(
                    new FilesInPath
                    {
                        DirectoryName = Path.GetDirectoryName(path)?? string.Empty, 
                        FileName = Path.GetFileName(f),
                    }));
            
            //sub directories
            foreach (var directory in directories)
            {
                if (option == TypeSearch.Project && DirectoriesToIgnore.Any(x => directory.Contains($"{Path.GetDirectoryName(path)}/{x}")))
                {
                    continue;
                }
                
                string[] files = GetFiles(directory, SearchOption.TopDirectoryOnly);
                files.ToList().ForEach(f => filesInPath.Add(new FilesInPath{ DirectoryName = directory, FileName = Path.GetFileName(f) }));
            }
        }

        return filesInPath;
    }
    
}

public class FilesInPath
{
    public string FileName { get; set; } = string.Empty;
    public string DirectoryName { get; set; } = string.Empty;
}

public class ResponsePath
{
    public bool IsOk => string.IsNullOrEmpty(Message);
    public string Message { get; set; } = string.Empty;
}