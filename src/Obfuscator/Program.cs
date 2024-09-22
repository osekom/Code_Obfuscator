using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Obfuscator.Enums;
using Obfuscator.Extensions;

namespace Obfuscator;

class Program
{
    static string slnName = string.Empty;
    static string pathProject = string.Empty;
    static MSBuildWorkspace workspace;
    static Project solution;
    static BuildOptionsEnums optionBuild = BuildOptionsEnums.Debug;
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Start Obfuscator \n");

        if (args.Length is <= 0 or 1)
        {
            Console.WriteLine("Usage: Obfuscator <PathProject .csproj> <OptionBuild Release | Debug>");
            return;
        }
        
        pathProject = args[0];
        optionBuild = BuildOptions.GetBuildOptions(args[1]);

        if (!Directory.Exists(pathProject))
        {
            Console.WriteLine("Project directory doesn't exist");
            return;
        }
        
        string? projectName = Directory.GetFiles(pathProject, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
        string[] directories = Directory.GetDirectories(pathProject, "*", SearchOption.AllDirectories);
        string[] pathFiles = Directory.GetFiles(pathProject, "*.cs", SearchOption.TopDirectoryOnly);

        if (string.IsNullOrEmpty(projectName))
        {
            Console.WriteLine("Project name doesn't exist");
            return;
        }
        
        slnName = projectName;
        workspace = MSBuildWorkspace.Create();
        solution = await workspace.OpenProjectAsync(slnName);
        
        Console.WriteLine($"{pathProject}:");
        foreach (var pathFile in pathFiles)
        {
            await InitProcessObfuscate(pathFile);
        }

        foreach (var directory in directories)
        {
            //remove unnecessary directories
            if (directory.Contains("bin") || 
                directory.Contains("obj") || 
                directory.Contains("Platforms") ||
                directory.Contains("Resources") ||
                directory.Contains("Properties")) continue;
            
            string[] files = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly);
            Console.WriteLine($"{directory}:");
            files.ToList().ForEach(x => Console.WriteLine($"\t - { x.Replace(directory, string.Empty)}"));
            
            //roslyn process
            foreach (var file in files)
            {
                await InitProcessObfuscate(file);
            }
        }
    }

    private static async Task InitProcessObfuscate(string filePath)
    {
        string code = File.ReadAllText(filePath);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = syntaxTree.GetRoot();
        
        IEnumerable<ClassDeclarationSyntax> classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        IEnumerable<VariableDeclarationSyntax> variableDeclarations = root.DescendantNodes().OfType<VariableDeclarationSyntax>();
        
        InspectCode(classDeclarations, variableDeclarations);
        await ObfuscateCode(filePath, variableDeclarations);
        
    }
    
    private static async Task ObfuscateCode(string filePath, IEnumerable<VariableDeclarationSyntax> variableDeclarations)
    {
        try
        {
            foreach (var variableDeclaration in variableDeclarations)
            {
                var variableType = variableDeclaration.Type;
                foreach (var variable in variableDeclaration.Variables)
                {
                    var variableName = variable.Identifier.Text;

                    var document = solution.Documents.FirstOrDefault(d => d.Name == filePath.Replace(pathProject, string.Empty));
                    var syntaxRoot = await document.GetSyntaxRootAsync();
                    var semanticModel = await document.GetSemanticModelAsync();
                    var newRoot = RenameVariable(syntaxRoot, semanticModel, workspace, variableName, StringExtensions.RandomName());

                    if (newRoot != syntaxRoot)
                    {
                        var newDocument = document.WithSyntaxRoot(newRoot);
                        var updatedSolution = newDocument.Project.Solution;
                        workspace.TryApplyChanges(updatedSolution);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    static SyntaxNode RenameVariable(SyntaxNode root, SemanticModel semanticModel, Workspace workspace, string originalName, string newName)
    {
        var variableNodes = root.DescendantNodes().OfType<VariableDeclaratorSyntax>()
            .Where(v => v.Identifier.Text == originalName);
        
        if (!variableNodes.Any())
        {
            return root;
        }
        
        var editor = new SyntaxEditor(root, workspace);

        foreach (var variable in variableNodes)
        {
            var symbol = semanticModel.GetDeclaredSymbol(variable);
            if (symbol != null)
            {
                var references = root.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(id => id.Identifier.Text == originalName);

                foreach (var reference in references)
                {
                    editor.ReplaceNode(reference, reference.WithIdentifier(SyntaxFactory.Identifier(newName)));
                }
                
                editor.ReplaceNode(variable, variable.WithIdentifier(SyntaxFactory.Identifier(newName)));
            }
        }
        
        return editor.GetChangedRoot();
    }

    private static void InspectCode(IEnumerable<ClassDeclarationSyntax> classDeclarations, IEnumerable<VariableDeclarationSyntax> variableDeclarations)
    {
        foreach (var classDeclaration in classDeclarations)
        {
            Console.WriteLine($"Class: {classDeclaration.Identifier.Text}");
            var classAttributes = classDeclaration.AttributeLists;
            foreach (var attributeList in classAttributes)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    Console.WriteLine($"  Attribute: {attribute.Name}");
                }
            }
            
            var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                Console.WriteLine($"  Method: {method.Identifier.Text}");

                var methodAttributes = method.AttributeLists;
                foreach (var attributeList in methodAttributes)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        Console.WriteLine($"    Attribute: {attribute.Name}");
                    }
                }
            }
            
            var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
            foreach (var property in properties)
            {
                Console.WriteLine($"  Property: {property.Identifier.Text}");

                var propertyAttributes = property.AttributeLists;
                foreach (var attributeList in propertyAttributes)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        Console.WriteLine($"    Attribute: {attribute.Name}");
                    }
                }
            }
        }
        
        foreach (var variableDeclaration in variableDeclarations)
        {
            var variableType = variableDeclaration.Type;
            foreach (var variable in variableDeclaration.Variables)
            {
                var variableName = variable.Identifier.Text;
                Console.WriteLine($"Variable: {variableName}, Tipo: {variableType}");
            }
        }
    }
}