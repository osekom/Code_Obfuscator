using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Obfuscator.Core;
using Obfuscator.Enums;
using Obfuscator.Extensions;

namespace Obfuscator;

class Program
{
    #region Properties

    private static string _pathProject = string.Empty;
    private static MSBuildWorkspace? _workspace;
    private static Project? _solution;
    private static BuildOptionsEnums _optionBuild = BuildOptionsEnums.Debug;
    
    #endregion
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Start Obfuscator \n");

        if (args.Length is <= 0 or 1)
        {
            Console.WriteLine("Usage: Obfuscator <PathProject.csproj> <OptionBuild Release | Debug>");
            return;
        }
        
        _pathProject = args[0];
        _optionBuild = BuildOptions.GetBuildOptions(args[1]);

        ResponsePath existFile = Directories.CheckPathAndFileExists(_pathProject);
        if (!existFile.IsOk) 
        {
            Console.WriteLine(existFile.Message);
            return;
        }

        try
        {
            _workspace = MSBuildWorkspace.Create();
            _solution = await _workspace.OpenProjectAsync(_pathProject);
        
            Console.WriteLine($"Working in {_pathProject} project \n");
        
            //TODO: PrintFiles it's flag?
            bool flagShowFiles = true;
            if (flagShowFiles)
            {
                List<FilesInPath> filesDirectories = Directories.GetFilesInPath(_pathProject, TypeSearch.Project);
                filesDirectories.ForEach(x => Console.WriteLine($"Directory: {x.DirectoryName} - File: {x.FileName}"));
            }
        
            Console.WriteLine("Starting obfuscate code...");
            bool status = await new ObfuscatorCode().Start_Obfuscate(_pathProject);
        
            Console.WriteLine( status? "Obfuscated complete!" : "Obfuscated failed!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}