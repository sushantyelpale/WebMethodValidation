using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;

namespace WebMethodCheck
{
    public class CSharpFile
    {
        public readonly CSharpProject project;
        public readonly string fileName;
        public readonly string originalText;

        public SyntaxTree syntaxTree;
        public CSharpUnresolvedFile unresolvedTypeSystemForFile;

        public CSharpFile(CSharpProject project, string fileName)
        {
            this.project = project;
            this.fileName = fileName;

            CSharpParser cSharpParser = new CSharpParser(project.CompilerSettings);
            
            // Keep the original text around; we might use it for a refactoring later
            this.originalText = File.ReadAllText(fileName);
            this.syntaxTree = cSharpParser.Parse(this.originalText, fileName);

            if (cSharpParser.HasErrors)
            {
                Console.WriteLine("Error parsing " + fileName + ":");
                foreach (var error in cSharpParser.ErrorsAndWarnings)
                {
                    Console.WriteLine("  " + error.Region + " " + error.Message);
                }
            }
            this.unresolvedTypeSystemForFile = this.syntaxTree.ToTypeSystem();
        }

        public CSharpAstResolver CreateResolver()
        {
            return new CSharpAstResolver(project.Compilation, syntaxTree, unresolvedTypeSystemForFile);
        }

        public List<MethodDeclaration> IndexOfWebMthdDecl = new List<MethodDeclaration>();
        public List<IfElseStatement> IndexOfIfElStmt = new List<IfElseStatement>();
        public List<TryCatchStatement> IndexOfTryCatchStmt = new List<TryCatchStatement>();
    }
}
