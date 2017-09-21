using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace WebMethodCheck
{
    class MatchInvocation
    {
        AllPatterns Pat = new AllPatterns();
        PrintFunction PrFun = new PrintFunction();

        public void FindInvocationTypeMethod(Solution solution, int count)
        {
            foreach (var file in solution.AllFiles)
            {
                if (!Path.GetFileName(file.fileName).EndsWith("AddCustomer.aspx.cs"))
                    continue;

                var astResolver = new CSharpAstResolver(file.project.Compilation, file.syntaxTree, file.unresolvedTypeSystemForFile);
                foreach (var invocation in file.syntaxTree.Descendants.OfType<AstNode>())
                {
                    switch (count)
                    {
                        case 1: FindWebMethod(invocation, file);
                                FindTryCatchInWebMethod(invocation, file);    
                            break;
                        case 2: FindValidationMethod(invocation, file);
                                FindIfElseInWebMethod(invocation, file);
                                break;
                    }
                }
            }
        }
        public void FindWebMethod(AstNode invocation, CSharpFile file)
        {
            if (invocation.GetType().Name == "MethodDeclaration" && invocation.FirstChild.GetText().Contains("WebMethod"))
                    file.IndexOfWebMthdDecl.Add((MethodDeclaration)invocation);
            
        }
        public void FindTryCatchInWebMethod(AstNode invocation, CSharpFile file)
        {
            if (invocation.GetType().Name == "TryCatchStatement" && 
                invocation.GetParent<MethodDeclaration>().FirstChild.GetText().Contains("WebMethod"))
                    file.IndexOfTryCatchStmt.Add((TryCatchStatement)invocation);
        }
        public void FindValidationMethod(AstNode invocation, CSharpFile file)
        {
            if (invocation.NextSibling != null)
            {
                if (invocation.GetType().Name == "MethodDeclaration" &&
                    invocation.NextSibling.GetType().Name == "MethodDeclaration")
                {
                    var next = invocation.NextSibling;
                    if (next.GetText().Contains("WebMethod"))
                    {
                        file.IndexOfWebMthdDecl.Add((MethodDeclaration)invocation);
                    }
                }
            }

            /*            if (invocation.GetType().Name == "IfElseStatement")
                        {
                            if (invocation.GetChildByRole(Roles.Condition).GetText().Contains("valid" + invocation.GetParent<MethodDeclaration>().Name))
                            {
                                file.IndexOfIfElStmt.Add((IfElseStatement)invocation);
                            }
                        }
             */
        }
        public void FindIfElseInWebMethod(AstNode invocation, CSharpFile file) 
        { 
            if(invocation.GetType().Name == "IfElseStatement" && invocation.Parent.Parent.GetType().Name == "TryCatchStatement")
            {
                string strToCheck = "null";
                try{
                    strToCheck = invocation.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.GetText();
                }
                catch(Exception e) {}
                if(strToCheck.Contains(invocation.GetParent<MethodDeclaration>().Name))  {
                    file.IndexOfIfElStmt.Add((IfElseStatement)invocation);
                }
            }
        }
    }
}
