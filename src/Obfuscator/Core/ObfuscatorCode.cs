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
    public ObfuscatorCode(){}
    
    public async Task<bool> Start_Obfuscate(string pathProject, bool inspectCode = false)
    {
        try
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Project solution = await workspace.OpenProjectAsync(pathProject);
            
            IEnumerable<Document> documents = solution.Documents;
            foreach (var document in documents)
            {
                SyntaxNode? syntaxRoot = await document.GetSyntaxRootAsync();
                if (syntaxRoot == null) continue;
                SemanticModel? semanticModel = await document.GetSemanticModelAsync();
                if(semanticModel == null) continue;
                DebugWrite($"Obfuscating {document.Name}");
                ObfuscateSyntaxDeclarator(syntaxRoot, semanticModel, workspace, document);
            }

            return true;

        }
        catch (Exception e)
        {
            DebugWrite(e.ToString());
            throw;
        }
    }

    private void ObfuscateSyntaxDeclarator(SyntaxNode syntaxRoot, 
                                                     SemanticModel semanticModel, 
                                                     MSBuildWorkspace workspace,
                                                     Document document)
    {
        string newVariableName = StringExtensions.RandomName();
        SyntaxNode newRoot = RenameVariable(syntaxRoot, semanticModel, workspace, newVariableName);
        WriteNewSyntax(newRoot, syntaxRoot, document, workspace);
    }

    private void WriteNewSyntax(SyntaxNode newSyntax, SyntaxNode oldSyntax, Document document, MSBuildWorkspace workspace)
    {
        try
        {
            if (newSyntax == oldSyntax) return;
            var newDocument = document.WithSyntaxRoot(newSyntax);
            var updatedSolution = newDocument.Project.Solution;
            workspace.TryApplyChanges(updatedSolution);
        }
        catch (Exception e)
        {
            DebugWrite(e.ToString());
        }
        
    }
    
    private SyntaxNode RenameVariable(SyntaxNode root, SemanticModel semanticModel, Workspace workspace, string newName)
    {
        var variableNodes = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();
        
        if (!variableNodes.Any())
        {
            return root;
        }
        
        var editor = new SyntaxEditor(root, workspace);

        foreach (var variable in variableNodes)
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
            }
            DebugWrite($"Obfuscating variable {variable.Identifier.ValueText} to {newName}");
        }
        
        return editor.GetChangedRoot();
    }
}