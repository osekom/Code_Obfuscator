using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using Obfuscator.Extensions;

namespace Obfuscator.Core;

/// <summary>
/// Start process obfuscate code
/// </summary>
/// <param name="pathProject"/>
/// <param name="inspectCode"/>
/// <remarks>
/// inspectCode print information class, vars, interfaces, list, etc…
/// </remarks>
//TODO: Make print all files or file to process obfuscate "inspectCode var" (under evaluation)
public class ObfuscatorCode : CoreBase
{
    #region Properties
    
    private string _pathSolution { get; set; }

    private MSBuildWorkspace _workspace { get; set; }
    private Project _solution { get; set; }

    #endregion

    public ObfuscatorCode(string solutionFilePath)
    {
        _pathSolution = solutionFilePath;
    }
    
    public async Task<bool> Start_Obfuscate()
    {
        try
        {
            await LoadProject(_pathSolution);
            IEnumerable<Document> documents = _solution.Documents;
            foreach (var document in documents)
            {
                SyntaxNode? syntaxRoot = await document.GetSyntaxRootAsync();
                if (syntaxRoot == null) continue;
                SemanticModel? semanticModel = await document.GetSemanticModelAsync();
                if(semanticModel == null) continue;
                DebugWrite($"Obfuscating {document.Name}");
                ObfuscateSyntaxDeclarator(syntaxRoot, semanticModel, document);
                await LoadProject(_pathSolution);
            }
            
            CloseProject();
            return true;

        }
        catch (Exception e)
        {
            DebugWrite(e.ToString());
            throw;
        }
    }

    private async Task LoadProject(string solutionFilePath)
    {
        _workspace = MSBuildWorkspace.Create();
        _solution = await _workspace.OpenProjectAsync(solutionFilePath);
    }

    private void CloseProject()
    {
        _workspace.CloseSolution();
        _workspace.Dispose();
    }

    private void ObfuscateSyntaxDeclarator(SyntaxNode syntaxRoot, 
                                                     SemanticModel semanticModel, 
                                                     Document document)
    {
        string newVariableName = StringExtensions.RandomName();
        SyntaxNode newRoot = RenameVariable(syntaxRoot, semanticModel, _workspace, newVariableName);
        WriteNewSyntax(newRoot, syntaxRoot, document);
    }

    private void WriteNewSyntax(SyntaxNode newSyntax, SyntaxNode oldSyntax, Document document)
    {
        try
        {
            if (newSyntax == oldSyntax) return;
            var newDocument = document.WithSyntaxRoot(newSyntax);
            var updatedSolution = newDocument.Project.Solution;
            bool state = _workspace.TryApplyChanges(updatedSolution);
            if (state)
            {
                DebugWrite("Obfuscate complete!");
            }
            else
            {
                DebugWrite("Obfuscate failed, Force update cs file...");
                Directories.WriteFileCS(newSyntax.ToString(), newDocument.FilePath?? string.Empty);
            }
        }
        catch (Exception e)
        {
            DebugWrite(e.ToString());
        }
        
    }
    
    private SyntaxNode RenameVariable(SyntaxNode root, SemanticModel semanticModel, Workspace workspace, string newName)
    {
        var variableNodes = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();
        var variableDeclaratorSyntaxes = variableNodes as VariableDeclaratorSyntax[] ?? variableNodes.ToArray();
        if (!variableDeclaratorSyntaxes.Any())
        {
            return root;
        }
        
        var editor = new SyntaxEditor(root, workspace.Services);

        foreach (var variable in variableDeclaratorSyntaxes)
        {
            var symbol = ModelExtensions.GetDeclaredSymbol(semanticModel, variable);
            if (symbol != null)
            {
                var references = root.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(id => id.Identifier.Text == variable.Identifier.Text);

                foreach (var reference in references)
                {
                    editor.ReplaceNode(reference, reference.WithIdentifier(SyntaxFactory.Identifier(newName)));
                }
                
                editor.ReplaceNode(variable, variable.WithIdentifier(SyntaxFactory.Identifier(newName)));
                DebugWrite($"Obfuscating variable {variable.Identifier.ValueText} to {newName}");
            }
        }
        
        return editor.GetChangedRoot();
    }
}