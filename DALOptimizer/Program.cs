using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace DALOptimizer
{
	class Program
	{
		public static void Main(string[] args)
		{
            //Console.WriteLine(args);

			if (args.Length == 0) {
				Console.WriteLine("Please specify the path to a .sln file on the command line");
				
				Console.Write("Press any key to continue . . . ");
				Console.ReadKey(true);
				return;
			}
			
			Solution solution = new Solution(args[0]);
			foreach (var file in solution.AllFiles) {
				var astResolver = new CSharpAstResolver(file.Project.Compilation, file.SyntaxTree, file.UnresolvedTypeSystemForFile);
				foreach (var invocation in file.SyntaxTree.Descendants.OfType<InvocationExpression>()) {
					// Retrieve semantics for the invocation
					var rr = astResolver.Resolve(invocation) as InvocationResolveResult;
					if (rr == null) {
						// Not an invocation resolve result - e.g. could be a UnknownMemberResolveResult instead
						continue;
					}
					if (rr.Member.FullName != "System.String.IndexOf") {
						// Invocation isn't a string.IndexOf call
						continue;
					}
					if (rr.Member.Parameters.First().Type.FullName != "System.String") {
						// Ignore the overload that accepts a char, as that doesn't take a StringComparison.
						// (looking for a char always performs the expected ordinal comparison)
						continue;
					}
					if (rr.Member.Parameters.Last().Type.FullName == "System.StringComparison") {
						// Already using the overload that specifies a StringComparison
						continue;
					}
					Console.WriteLine(invocation.GetRegion() + ": " + invocation.GetText());
					file.IndexOfInvocations.Add(invocation);
				}
			}
			Console.WriteLine("Found {0} places to refactor in {1} files.",
			                  solution.AllFiles.Sum(f => f.IndexOfInvocations.Count),
			                  solution.AllFiles.Count(f => f.IndexOfInvocations.Count > 0));
			Console.Write("Apply refactorings? ");
			string answer = Console.ReadLine();
			if ("yes".Equals(answer, StringComparison.OrdinalIgnoreCase) || "y".Equals(answer, StringComparison.OrdinalIgnoreCase)) {
				foreach (var file in solution.AllFiles) {
					if (file.IndexOfInvocations.Count == 0)
						continue;
					// DocumentScript expects the the AST to stay unmodified (so that it fits
					// to the document state at the time of the DocumentScript constructor call),
					// so we call Freeze() to prevent accidental modifications (e.g. forgetting a Clone() call).
					file.SyntaxTree.Freeze();
					// AST resolver used to find context for System.StringComparison generation
					var compilation = file.Project.Compilation;
					var astResolver = new CSharpAstResolver(compilation, file.SyntaxTree, file.UnresolvedTypeSystemForFile);
					
					// Create a document containing the file content:
					var document = new StringBuilderDocument(file.OriginalText);
					var formattingOptions = FormattingOptionsFactory.CreateAllman();
					var options = new TextEditorOptions();
					using (var script = new DocumentScript(document, formattingOptions, options)) {
						foreach (InvocationExpression expr in file.IndexOfInvocations) {
							// Generate a reference to System.StringComparison in this context:
							var astBuilder = new TypeSystemAstBuilder(astResolver.GetResolverStateBefore(expr));
							IType stringComparison = compilation.FindType(typeof(StringComparison));
							AstType stringComparisonAst = astBuilder.ConvertType(stringComparison);
							
							// Alternative 1: clone a portion of the AST and modify it
							var copy = (InvocationExpression)expr.Clone();
							copy.Arguments.Add(stringComparisonAst.Member("Ordinal"));
							script.Replace(expr, copy);
							
//							// Alternative 2: perform direct text insertion
//							int offset = script.GetCurrentOffset(expr.RParToken.StartLocation);
//							script.InsertText(offset, ", " + stringComparisonAst.GetText() +  ".Ordinal");
						}
					}
					File.WriteAllText(Path.ChangeExtension(file.FileName, ".output.cs"), document.Text);
				}
			}
		}
	}
}