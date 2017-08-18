using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

            //int count = 0;
            MatchExpr Mr = new MatchExpr();
            
			Solution solution = new Solution(args[0]);
			foreach (var file in solution.AllFiles) {
                if(file.FileName!="D:\\DALProject\\DAL\\AddCustomerDAL.cs")
                {
                    continue;
                }
                var astResolver = new CSharpAstResolver(file.Project.Compilation, file.SyntaxTree, file.UnresolvedTypeSystemForFile);
				foreach (var invocation in file.SyntaxTree.Descendants.OfType<AstNode>()) {
					
                    // all expressions
                    if (invocation.GetType().Name == "ExpressionStatement")
                    {
                        Mr.MatchExprStmt(invocation, file, astResolver);
                        continue;
                    }

                    //catch clause
                    if (invocation.GetType().Name == "CatchClause")
                    {
                        Mr.MatchCatchClause(invocation, file, astResolver);
                    }

                    // For All Global Field Declarations
                    if (invocation.GetType().Name == "FieldDeclaration")
                    {
                        Mr.MatchFieldDecl(invocation, file, astResolver);
                        continue;
                    }

                    if (invocation.GetType().Name == "AssignmentExpression")
                    {
                        Mr.MatchAssExpr(invocation, file, astResolver);
                        continue;
                    }

                    //check Global Property Declaration
                    if (invocation.GetType().Name == "PropertyDeclaration")
                    {
                        Mr.MatchPropDecl(invocation, file, astResolver);
                        continue;
                    }

                    //check Finally Block
                    if (invocation.GetType().Name == "BlockStatement")
                    {
                        Mr.MatchBlock(invocation, file, astResolver);
                        continue;
                    }

                    if (invocation.GetType().Name == "ParameterDeclaration")
                    {
                        if (invocation.Parent.GetType().Name == "MethodDeclaration")
                        {
                            Mr.MatchMethodDecl(invocation, file, astResolver);
                            continue;
                        }
                    }

                    if (invocation.GetType().Name == "MethodDeclaration")
                    {
                        Mr.MatchMethodDecl(invocation, file, astResolver);
                        continue;
                    }

				}
			}

            Console.WriteLine("Found {0} places to refactor Invocation Expression in {1} files.",
			      solution.AllFiles.Sum(f => f.IndexOfInvocations.Count),
			      solution.AllFiles.Count(f => f.IndexOfInvocations.Count > 0));
            Console.WriteLine("Found {0} places to refactor Field Declaration in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfFieldDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfFieldDecl.Count > 0));
            Console.WriteLine("Found {0} places to refactor Property Declaration in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfPropDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfPropDecl.Count > 0));
            Console.WriteLine("Found {0} places to refactor Finally Block in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfBlockStmt.Count),
                  solution.AllFiles.Count(f => f.IndexOfBlockStmt.Count > 0));
            Console.WriteLine("Found {0} places to refactor assignment Expressions in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfAssExpr.Count),
                  solution.AllFiles.Count(f => f.IndexOfAssExpr.Count > 0));

            Console.Write("Apply refactorings? ");
			string answer = Console.ReadLine();
			if ("yes".Equals(answer, StringComparison.OrdinalIgnoreCase) || "y".Equals(answer, StringComparison.OrdinalIgnoreCase)) {
				foreach (var file in solution.AllFiles) {
                    if (file.IndexOfFieldDecl.Count == 0 && file.IndexOfInvocations.Count == 0 && file.IndexOfPropDecl.Count==0 && file.IndexOfAssExpr.Count==00 && file.IndexOfBlockStmt.Count==0)
						continue;
					file.SyntaxTree.Freeze();

					var compilation = file.Project.Compilation;
					var astResolver = new CSharpAstResolver(compilation, file.SyntaxTree, file.UnresolvedTypeSystemForFile);
					
					// Create a document containing the file content:
					var document = new StringBuilderDocument(file.OriginalText);
					var formattingOptions = FormattingOptionsFactory.CreateAllman();
					var options = new TextEditorOptions();
					using (var script = new DocumentScript(document, formattingOptions, options)) {


                        // for global declarations
						foreach (FieldDeclaration expr in file.IndexOfFieldDecl) {
							// Generate a reference to System.StringComparison in this context:
							var astBuilder = new TypeSystemAstBuilder(astResolver.GetResolverStateBefore(expr));
							IType stringComparison = compilation.FindType(typeof(StringComparison));
							AstType stringComparisonAst = astBuilder.ConvertType(stringComparison);

                            var copy = (FieldDeclaration)expr.Clone();
							
                            AllPatterns pat = new AllPatterns();
                            var pattern = pat.ConnectionClassconnectExpr();

                            if (expr.GetText().Contains(pattern.GetText()))   // Replace ConnectionClass.connect() with DatabaseProcessing stmt
                            {
                                script.Replace(expr, pat.DbProcessing());
                                //script.Replace(offset, expr.GetRegion().ToString().Length, "DatabaseProcessing db = new DatabaseProcessing();\r\n");                                
                            }
                            else       // rest of all global declarations are removed
                            {
                                script.Remove(expr, true);
                            }
						}

                        foreach (var expr in file.IndexOfBlockStmt)
                        {
                            var astBuilder = new TypeSystemAstBuilder(astResolver.GetResolverStateBefore(expr));
                            IType stringComparison = compilation.FindType(typeof(StringComparison));
                            AstType stringComparisonAst = astBuilder.ConvertType(stringComparison);

                            var copy = (BlockStatement)expr.Clone();
                            AllPatterns Pat = new AllPatterns();
                            script.Replace(expr, Pat.FinalyBlck());                             
                        }

                        foreach (var expr in file.IndexOfCtchClause)
                        {
                            var astBuilder = new TypeSystemAstBuilder(astResolver.GetResolverStateBefore(expr));
                            IType stringComparison = compilation.FindType(typeof(StringComparison));
                            AstType stringComparisonAst = astBuilder.ConvertType(stringComparison);

                            var copy = (CatchClause)expr.Clone();
                            AllPatterns Pat = new AllPatterns();
                            script.Replace(expr, Pat.ctchclause());
                        }

                        foreach (var expr in file.IndexOfExprStmt)
                        {
                            var astBuilder = new TypeSystemAstBuilder(astResolver.GetResolverStateBefore(expr));
                            IType stringComparison = compilation.FindType(typeof(StringComparison));
                            AstType stringComparisonAst = astBuilder.ConvertType(stringComparison);
                            var copy = (ExpressionStatement)expr.Clone();
                            int offset = script.GetCurrentOffset(expr.StartLocation);

                            AllPatterns Pat = new AllPatterns();
                            var expr3 = Pat.StoredProc();
                            var expr4 = Pat.sqlConnstmt();

                            ICSharpCode.NRefactory.PatternMatching.Match StoredProc = expr3.Match(expr);
                            ICSharpCode.NRefactory.PatternMatching.Match sqlConnstmt = expr4.Match(expr);

                            if (StoredProc.Success || sqlConnstmt.Success)
                            {
                                script.Remove(expr, true);
                            }
                        }

                        foreach (var expr in file.IndexOfAssExpr)
                        {
                            var astBuilder = new TypeSystemAstBuilder(astResolver.GetResolverStateBefore(expr));
                            IType stringComparison = compilation.FindType(typeof(StringComparison));
                            AstType stringComparisonAst = astBuilder.ConvertType(stringComparison);
                            var copy = (AssignmentExpression)expr.Clone();
                            //int offset = script.GetCurrentOffset(expr.StartLocation);

                            AllPatterns Pat = new AllPatterns();
                            var expr3 = Pat.sqlCmdstmt();
                            ICSharpCode.NRefactory.PatternMatching.Match sqlCmdstmt = expr3.Match(expr);

                            if (sqlCmdstmt.Success)
                            {
                                int start = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.PrevSibling.StartLocation);
                                int end = script.GetCurrentOffset(expr.LastChild.LastChild.PrevSibling.EndLocation);
                                int length = end - start;
                                script.RemoveText(start,length);
                            }
                        }

                        // for changing API.RegistrationAPI objAPI in method declaration
                        foreach (var expr in file.IndexOfMthdDecl)
                        {
                            var astBuilder = new TypeSystemAstBuilder(astResolver.GetResolverStateBefore(expr));
                            IType stringComparison = compilation.FindType(typeof(StringComparison));
                            AstType stringComparisonAst = astBuilder.ConvertType(stringComparison);

                            var copy = (MethodDeclaration)expr.Clone();
                            AllPatterns Pat = new AllPatterns();

                            var chldOfTypPar = expr.GetChildByRole(Roles.Parameter);
                            string input = Regex.Replace(chldOfTypPar.GetText(), @"\w+\.\b","");
                            int offset = script.GetCurrentOffset(chldOfTypPar.StartLocation);
                            script.RemoveText(offset,chldOfTypPar.GetText().Length);
                            script.InsertText(offset,input);
                        }
  					}
					File.WriteAllText(Path.ChangeExtension(file.FileName, ".output.cs"), document.Text);
				}
			}
		}
	}
}