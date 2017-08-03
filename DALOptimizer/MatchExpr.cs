using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace DALOptimizer
{
    class MatchExpr
    {
        AllPatterns Pat = new AllPatterns();
        public void MatchTypeDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr1 = Pat.ConnCloseInvExpr();

            if (invocation.GetText() == expr1.GetText())
            {
                file.IndexOfInvocations.Add((InvocationExpression)invocation);
                Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  "+invocation.GetType().Name+ "  " +invocation.GetText() + "\n");
            }

            
            if (invocation.GetText().ToString().Contains(expr1.GetText().ToString()))
            {
                if (invocation.Parent.GetType().Name.ToString() == "TypeDeclaration")
                {
                    file.IndexOfFieldDecl.Add((FieldDeclaration)invocation);
                    Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  Field Declaration: " + invocation.GetText().ToString());
                }
            }
        }


        public void MatchFieldDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            
            // For All Global Declarations
            if (invocation.Parent.GetType().Name.ToString() == "TypeDeclaration")
            {
                file.IndexOfFieldDecl.Add((FieldDeclaration)invocation);
                Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  Field Declaration1: " + invocation.GetText().ToString());
            }

            /*var pattern = new VariableDeclarationStatement
            {
                Type = new AnyNode("type"),
                Variables = {
                            new VariableInitializer {
                                Name = Pattern.AnyString,
                                Initializer = new ObjectCreateExpression {
                                    Type = new Backreference("type"),
                                    Arguments = { new Repeat(new AnyNode()) }
                                }
                            }
                        }
            };

            Match m = pattern.Match(invocation);

            if (m.Success)
            {
                Console.WriteLine("Matched Node is: " + invocation.GetText());
            }*/



        }

        public void MatchInvocationExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var rr = astResolver.Resolve(invocation) as InvocationResolveResult;
            file.IndexOfInvocations.Add((InvocationExpression)invocation);
            Console.WriteLine("Line:" + invocation.GetRegion().Begin.Line + "  Invocation Expresson: " + invocation.GetText().ToString());        
        }

    }
}
