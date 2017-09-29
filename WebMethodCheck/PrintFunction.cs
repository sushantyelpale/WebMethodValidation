using System;
using System.Linq;

using ICSharpCode.NRefactory.CSharp;

namespace WebMethodCheck
{
    class PrintFunction
    {
        public void PrintMethod(Solution solution)
        {
            Console.WriteLine("Found {0} places to refactor method Declaration Web Mtdh in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfWebMthdDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfWebMthdDecl.Count > 0));
           
            Console.WriteLine("Found {0} places to refactor if else statement Web Mtdh in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfIfElStmt.Count),
                  solution.AllFiles.Count(f => f.IndexOfIfElStmt.Count > 0));

            Console.WriteLine("Found {0} places to refactor method Declaration Web Mtdh in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfTryCatchStmt.Count),
                  solution.AllFiles.Count(f => f.IndexOfTryCatchStmt.Count > 0));

            Console.WriteLine("Found {0} places to refactor method Declaration Web Mtdh in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfClassDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfClassDecl.Count > 0));

            Console.WriteLine("Found {0} places to refactor method Declaration Web Mtdh in {1} files.",
                  solution.AllFiles.Sum(f => f.IndexOfUsingDecl.Count),
                  solution.AllFiles.Count(f => f.IndexOfUsingDecl.Count > 0));
            
        }

        public void PrintInvocation(AstNode invocation)
        {
            Console.WriteLine("new_Line:" + invocation.GetRegion().Begin.Line + "  Invocation: " + invocation.GetText().ToString());
        }
    }
}
