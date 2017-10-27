using System;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.PatternMatching;

namespace ORPWebMethod
{
    internal class MatchInvocation
    {
        private AllPatterns allPatterns = new AllPatterns();
        private PrintFunction PrintFun = new PrintFunction();

        public void FindInvocationTypeMethod(Solution solution, int choice, string checkAccessMethodName)
        {
            foreach (var file in solution.AllFiles)
            {
                string fileName = Path.GetFileName(file.fileName);
                if (fileName.EndsWith("AssemblyInfo.cs") || fileName.EndsWith("Validation.cs"))
                    continue;
                if (!Path.GetFileName(file.fileName).EndsWith(".aspx.cs"))
                    continue;

                //if (!Path.GetFileName(file.fileName).EndsWith("RetailerDashboard.aspx.cs"))
                //    continue;

                var astResolver = new CSharpAstResolver(file.project.Compilation, file.syntaxTree, file.unresolvedTypeSystemForFile);
                foreach (var invocation in file.syntaxTree.Descendants.OfType<AstNode>())
                {
                    switch (choice)
                    {
                        case 1: FindCheckAccess(invocation, file, checkAccessMethodName);
                            FindUsingAPIDecl(invocation, file);
                            FindWebMethod(invocation, file);
                            FindFirstTryCatchInWebMethod(invocation, file);
                            break;

                        case 2: FindValidationMethod(invocation, file);
                            FindFirstTryCatchInWebMethod(invocation, file);
                            FindIfElseInWebMethod(invocation, file);
                            FindClassOfWebMethods(invocation, file);
                            break;
                    }
                }
            }
        }

        public void FindCheckAccess(AstNode invocation, CSharpFile file, string checkAccessMethodName)
        {
            if (allPatterns.AccessControlExpression(checkAccessMethodName).Match(invocation).Success
                && FindWebMethod(invocation.GetParent<MethodDeclaration>()))
                file.IndexofExprStmtCheckAccess.Add((ExpressionStatement)invocation);
        }

        public void FindUsingAPIDecl(AstNode invocation, CSharpFile file)
        {
            if (invocation.GetType().Name == "UsingDeclaration")
                file.IndexOfUsingDecl.Add((UsingDeclaration)invocation);
        }

        public void FindWebMethod(AstNode invocation, CSharpFile file)
        {
            if (invocation.GetType().Name == "MethodDeclaration" &&
                FindWebMethod(invocation))
                file.IndexOfWebMthdDecl.Add((MethodDeclaration)invocation);
        }

        public void FindFirstTryCatchInWebMethod(AstNode invocation, CSharpFile file)
        {
            var methodName = invocation.GetParent<MethodDeclaration>();
            if (invocation.GetType().Name == "MethodDeclaration" && FindWebMethod(invocation))
            {
                try
                {
                    var tryCatchStmt = invocation.Descendants.OfType<TryCatchStatement>().First();
                    file.IndexOfTryCatchStmt.Add((TryCatchStatement)tryCatchStmt);
                }
                catch (Exception) { }
            }
        }

        public bool FindWebMethod(AstNode methodNode)
        {
            bool foundWebMethodAttribute = false;
            foreach (var attribute in methodNode.Children.OfType<AttributeSection>())
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
                    if (FindWebMethod(next))
                        file.IndexOfWebMthdDecl.Add((MethodDeclaration)invocation);
                }
            }
        }

        public void FindIfElseInWebMethod(AstNode invocation, CSharpFile file)
        {
            if (invocation.GetType().Name == "IfElseStatement" &&
                FindWebMethod(invocation.GetParent<MethodDeclaration>()))
            {
                Expression childOfTypeRoleCondition = invocation.GetChildByRole(Roles.Condition);
                if (childOfTypeRoleCondition.GetType().Name == "UnaryOperatorExpression")
                {
                    string strToCheck = "Valid" + invocation.GetParent<MethodDeclaration>().Name;

                    if (allPatterns.IfElseValidMethodUnary(strToCheck).Match(childOfTypeRoleCondition).Success)
                        file.IndexOfIfElStmt.Add((IfElseStatement)invocation);
                    else if (allPatterns.IfElseValidMethodUnaryOld().Match(childOfTypeRoleCondition).Success)
                    {
                        string strToCheckforAlreadyDeclared = childOfTypeRoleCondition.Descendants.OfType<IdentifierExpression>().First().GetText();
                        if (strToCheckforAlreadyDeclared.IndexOf(("valid"), StringComparison.OrdinalIgnoreCase) > 0)
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
            if (invocation.GetType().Name == "TypeDeclaration")
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