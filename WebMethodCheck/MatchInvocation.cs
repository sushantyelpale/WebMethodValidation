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
        AllPatterns allPatterns = new AllPatterns();
        PrintFunction PrFun = new PrintFunction();

        public void FindInvocationTypeMethod(Solution solution, int count)
        {
            foreach (var file in solution.AllFiles)
            {
                if (Path.GetFileName(file.fileName).EndsWith("AssemblyInfo.cs"))
                    continue;
                if (Path.GetFileName(file.fileName).EndsWith("Validation.cs"))
                    continue;

                //if (!Path.GetFileName(file.fileName).EndsWith("RetailerDashboard.aspx.cs"))
                //    continue;
                var astResolver = new CSharpAstResolver(file.project.Compilation, file.syntaxTree, file.unresolvedTypeSystemForFile);
                foreach (var invocation in file.syntaxTree.Descendants.OfType<AstNode>())
                {
                    switch (count)
                    {
                        case 1: FindUsingAPIDecl(invocation, file);
                                FindWebMethod(invocation, file);
                                FindTryCatchInWebMethod(invocation, file);    
                            break;
                        case 2: FindValidationMethod(invocation, file);
                                FindIfElseInWebMethod(invocation, file);
                                FindClassOfWebMethods(invocation, file);
                            break;
                    }
                }
            }
        }
        public void FindUsingAPIDecl(AstNode invocation, CSharpFile file)
        {
            if (invocation.GetType().Name == "UsingDeclaration")
                file.IndexOfUsingDecl.Add((UsingDeclaration)invocation);
            
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

        public bool FoundWebMethodAttribute(AstNode invocation)
        {
            bool foundWebMethodAttribute = false;
            foreach (var attribute in invocation.Children.OfType<AttributeSection>())
            {
                if (attribute.GetText().Contains("WebMethod"))
                {
                    foundWebMethodAttribute = true;
                    break;
                }
            }
            return foundWebMethodAttribute;
        }

        public void FindValidationMethod(AstNode invocation, CSharpFile file)
        {
            if (invocation.NextSibling != null)
            {
                var next = invocation.NextSibling;
                if (invocation.GetType().Name == "MethodDeclaration" &&
                    next.GetType().Name == "MethodDeclaration")
                {
                    if (FoundWebMethodAttribute(next))
                        file.IndexOfWebMthdDecl.Add((MethodDeclaration)invocation);
                }
            }
        }
        public void FindIfElseInWebMethod(AstNode invocation, CSharpFile file) 
        {
            if (invocation.GetType().Name == "IfElseStatement" && 
                FoundWebMethodAttribute(invocation.GetParent<MethodDeclaration>()))
            {
                Expression childOfTypeRoleCondition = invocation.GetChildByRole(Roles.Condition);
                if (childOfTypeRoleCondition.GetType().Name == "UnaryOperatorExpression") {
                    string strToCheck = "Valid" + invocation.GetParent<MethodDeclaration>().Name;

                    if (allPatterns.IfElseValidMethodUnary(strToCheck).Match(childOfTypeRoleCondition).Success)
                        file.IndexOfIfElStmt.Add((IfElseStatement)invocation);

                    else if(allPatterns.IfElseValidMethodUnaryOld().Match(childOfTypeRoleCondition).Success)
                    {
                        string strToCheckforAlreadyDeclared = childOfTypeRoleCondition.Descendants.OfType<IdentifierExpression>().First().GetText();
                        if (strToCheckforAlreadyDeclared.IndexOf(("valid"), StringComparison.OrdinalIgnoreCase) > 0 )
                            file.IndexOfIfElStmtValidation.Add((IfElseStatement)invocation);
                    }
                    else if (allPatterns.IfElseValidMethodUnaryMemberRef().Match(childOfTypeRoleCondition).Success)
                    {
                        string strToCheckAlreadyDecare = childOfTypeRoleCondition.Descendants.OfType<MemberReferenceExpression>().First().LastChild.GetText();
                        if (strToCheckAlreadyDecare.IndexOf(("valid"), StringComparison.OrdinalIgnoreCase) > 0)
                            file.IndexOfIfElStmtValidation.Add((IfElseStatement)invocation);
                    }
                }
                else if (childOfTypeRoleCondition.GetType().Name == "BinaryOperatorExpression")
                {
                    if (allPatterns.IfElseValidMethodBinary().Match(childOfTypeRoleCondition).Success)
                    {
                        string strToCheck = childOfTypeRoleCondition.Descendants.OfType<IdentifierExpression>().First().GetText();
                        if (strToCheck.IndexOf("Valid", StringComparison.OrdinalIgnoreCase) > 0)
                            file.IndexOfIfElStmtValidation.Add((IfElseStatement)invocation);
                    }
                    else if (allPatterns.IfElseValidMethodBinaryMemberRef().Match(childOfTypeRoleCondition).Success)
                    {
                        string strToCheck = childOfTypeRoleCondition.Descendants.OfType<IdentifierExpression>().First().NextSibling.GetText();
                        if (strToCheck.IndexOf("Valid", StringComparison.OrdinalIgnoreCase) > 0)
                            file.IndexOfIfElStmtValidation.Add((IfElseStatement)invocation);
                    }
                }
                //else if ()
             //   if(childRole == "UnaryOperatorExpression" || childRole == "BinaryOperatorExpression")
               //     file.IndexOfIfElStmt.Add((IfElseStatement)invocation);
/*                file.IndexOfIfElStmt.Add((IfElseStatement)invocation);
                string strToCheck = null;
                try{
                    //strToCheck = invocation.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.GetText();
                    strToCheck = invocation.GetChildByRole(Roles.Condition).DescendantsAndSelf.OfType<InvocationExpression>().First().Children.OfType<IdentifierExpression>().First().GetText();
                }
                catch(Exception) {}
                if (strToCheck != null)
                {
                    if (strToCheck.IndexOf("Valid", StringComparison.OrdinalIgnoreCase) >= 0  && 
                        FoundWebMethodAttribute(invocation.GetParent<MethodDeclaration>()))
                    {
                        if (strToCheck == "Valid" + invocation.GetParent<MethodDeclaration>().Name)
                        {
                            file.IndexOfIfElStmt.Add((IfElseStatement)invocation);
                        }
                        else
                        {
                            invocation.Descendants.OfType<InvocationExpression>().First().GetNodeAt(invocation.StartLocation, null);
                            //foreach(var expr)
                        }
//                        else if()
                    }
                }
 */ 
            }
        }
        public void FindClassOfWebMethods(AstNode invocation, CSharpFile file)
        {
            if (invocation.GetType().Name == "TypeDeclaration" )
            {
                bool WebMethodPresent = false;
                foreach (var inv in invocation.Descendants.OfType<MethodDeclaration>())
                {
                    if (inv.FirstChild.GetText().Contains("WebMethod"))
                    {
                        WebMethodPresent = true;
                        break;
                    }
                }
                if (WebMethodPresent)
                    file.IndexOfClassDecl.Add((TypeDeclaration)invocation);
            }
        }
    }
}
