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
       
        // For All Global Declarations
        public void MatchFieldDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == "TypeDeclaration")
            {
                file.IndexOfFieldDecl.Add((FieldDeclaration)invocation);
                Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  Field Declaration1: " + invocation.GetText().ToString());
            }
        }

        // For All Global Property Declarations
        public void MatchPropDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.Parent.GetType().Name.ToString() == "TypeDeclaration")
            {
                file.IndexOfPropDecl.Add((PropertyDeclaration)invocation);
                Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  Field Declaration1: " + invocation.GetText().ToString());
            }
        }

        public void MatchCatchClause(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            file.IndexOfCtchClause.Add((CatchClause)invocation);
        }

        //Match Invocation Expression
        public void MatchInvocationExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            
            var rr = astResolver.Resolve(invocation) as InvocationResolveResult;
            file.IndexOfInvocations.Add((InvocationExpression)invocation);
            Console.WriteLine("Line:" + invocation.GetRegion().Begin.Line + "  Invocation Expresson: " + invocation.GetText().ToString());        
        }

        public void MatchAssExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr6 = Pat.sqlCmdstmt();
            Match sqlCmdstmt = expr6.Match(invocation);

            if (sqlCmdstmt.Success)
            {
                file.IndexOfAssExpr.Add((AssignmentExpression)invocation);
                Console.WriteLine("new_Line:" + invocation.GetRegion().Begin.Line + "  Invocation: " + invocation.GetText().ToString());
            }           
        }


        //Match Expression Statement  //da.Fill(dt);
        public void MatchExprStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr1 = Pat.FillExpr();
            var expr2 = Pat.logErr();
            var expr3 = Pat.ExNonQuery();
            var expr4 = Pat.StoredProc();
            var expr5 = Pat.sqlConnstmt();
            
            Match FillExpr = expr1.Match(invocation);
            Match logErr = expr2.Match(invocation);
            Match ExNonQuery = expr3.Match(invocation);
            Match StoredProc = expr4.Match(invocation);
            Match sqlConnstmt = expr5.Match(invocation);
           

            if (FillExpr.Success || logErr.Success || ExNonQuery.Success || StoredProc.Success || sqlConnstmt.Success)
            {
                file.IndexOfExprStmt.Add((ExpressionStatement)invocation);
                Console.WriteLine("new_Line:" + invocation.GetRegion().Begin.Line + "  Invocation: " + invocation.GetText().ToString());
            }
        }

        public void MatchVarDeclStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var newPat = Pat.newPattern();
            Console.WriteLine("Line.........:" + invocation.GetRegion().Begin.Line + "  Invocation Expresson: " + invocation.GetText().ToString());
        }

        public void MatchMethodDecl(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr1 = Pat.MthdDecl();
            Match MthdDecl = expr1.Match(invocation);
            if (MthdDecl.Success)
            {
                file.IndexOfMthdDecl.Add((MethodDeclaration)invocation);
            }
        }

        public void MatchBlock(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            if (invocation.PrevSibling.GetText() == "finally")
            {
                file.IndexOfBlockStmt.Add((BlockStatement)invocation);
                Console.WriteLine("Line:" + invocation.GetRegion().Begin.Line + "  Fnally Block: " + invocation.GetText().ToString());
            }
            
            else if (invocation.PrevSibling.GetText() == "try")
            {
                Console.WriteLine("Try Line:" + invocation.GetRegion().Begin.Line);
            }
        }


        //con.close()    and con.open()
        //            var expr1 = Pat.ConnCloseInvExpr();
        /*            var expr2 = Pat.ConnOpenInvExpr();
                    if (invocation.GetText() == expr1.GetText())
                    {
                        file.IndexOfInvocations.Add((InvocationExpression)invocation);
                        Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  "+invocation.GetType().Name+ "  " +invocation.GetText() + "\n");
                    }

                    if (invocation.GetText() == expr2.GetText())
                    {
                        file.IndexOfInvocations.Add((InvocationExpression)invocation);
                        Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  " + invocation.GetType().Name + "  " + invocation.GetText() + "\n");
                    }
  
                    if (invocation.GetText().ToString().Contains(expr1.GetText().ToString()))
                    {
                        if (invocation.Parent.GetType().Name.ToString() == "TypeDeclaration")
                        {
                            file.IndexOfFieldDecl.Add((FieldDeclaration)invocation);
                            Console.WriteLine("Line:" + invocation.GetRegion().Begin + "  Field Declaration: " + invocation.GetText().ToString());
                        }
                    }
           */

    }
}
