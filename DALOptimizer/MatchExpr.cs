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

        //Match Assignment Expression
        //conn = new SqlConnection(constr);
        public void MatchAssExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr1 = Pat.sqlCmdstmt();
            var expr2 = Pat.sqlConnstmt();
            
           

            Match m1 = expr1.Match(invocation);
            Match m2 = expr2.Match(invocation);
           
            if (m1.Success)
            {
                file.IndexOfAssExpr.Add((AssignmentExpression)invocation);
                Console.WriteLine("new_Line:" + invocation.GetRegion().Begin.Line + "  Invocation: " + invocation.GetText().ToString());
            }
            if (m2.Success)
            {
                file.IndexOfAssExpr.Add((AssignmentExpression)invocation);
                Console.WriteLine("new_Line1:" + invocation.GetRegion().Begin.Line + "  Invocation: " + invocation.GetText().ToString());
            }
           

        }

        //Match Invocation Expression
        public void MatchInvocationExpr(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            
            var rr = astResolver.Resolve(invocation) as InvocationResolveResult;
            file.IndexOfInvocations.Add((InvocationExpression)invocation);
            Console.WriteLine("Line:" + invocation.GetRegion().Begin.Line + "  Invocation Expresson: " + invocation.GetText().ToString());        
        }

        //Match Expression Statement  //da.Fill(dt);
        public void MatchExprStmt(AstNode invocation, CSharpFile file, CSharpAstResolver astResolver)
        {
            var expr1 = Pat.FillExpr();
            var expr2 = Pat.logErr();
            var expr3 = Pat.ExNonQuery();
            var expr4 = Pat.StoredProc();
            
            Match FillExpr = expr1.Match(invocation);
            Match logErr = expr2.Match(invocation);
            Match ExNonQuery = expr3.Match(invocation);
            Match m4 = expr4.Match(invocation);
            
            if (FillExpr.Success)
            {
                file.IndexOfExprStmt.Add((ExpressionStatement)invocation);
            }
            if (logErr.Success)
            {
                file.IndexOfExprStmt.Add((ExpressionStatement)invocation);
            }
            if (ExNonQuery.Success)
            {
                file.IndexOfExprStmt.Add((ExpressionStatement)invocation);
            }
            if (m4.Success)
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
            //invocation.

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
