using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;

namespace WebMethodCheck
{
    class ModifyInvocations
    {
        public void initializeExpr(Solution solution, int choice)
        {
            foreach (var file in solution.AllFiles)
            {
                if (file.IndexOfWebMthdDecl.Count == 0 && file.IndexOfWebMthdDecl.Count == 0 && file.IndexOfIfElStmt.Count == 0)
                    continue;
                file.syntaxTree.Freeze();

                // Create a document containing the file content:
                var document = new StringBuilderDocument(file.originalText);
                var formattingOptions = FormattingOptionsFactory.CreateAllman();
                var options = new TextEditorOptions();
                using (var script = new DocumentScript(document, formattingOptions, options))
                {
                    switch (choice)
                    {
                        case 1: WriteValidationMethodStructure(file, script);
                                WriteIfElseStructureInWebmethodTry(file, script);
                                WriteAccessControlStmtInTryCatch(file, script);
                                break;
                        case 2: WriteValidationMethodBody(file, script);
                                InsertIfElseInWebmethodTry(file, script);
                                AddPageNameGlobalinClass(file, script);
                                break;
                    }
                }
                File.WriteAllText(Path.ChangeExtension(file.fileName, ".output.cs"), document.Text);
                //File.WriteAllText(Path.ChangeExtension(file.fileName, ".cs"), document.Text);
            }
        }

        public void WriteValidationMethodStructure(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfWebMthdDecl)
            {
                // for adding method before the webmethod
                var copy = (MethodDeclaration)expr.Clone();
                var chldOfTypPar = expr.GetChildByRole(Roles.Parameter);
                var mtdhName = expr.Name;
                var chdMtdhName = "Valid" + mtdhName;
                AllPatterns allPatterns = new AllPatterns();
                var expr1 = allPatterns.ValidationMthd(chdMtdhName);
                bool validMethodPresent = false;

                if (chldOfTypPar != null)
                {
                    //string input = Regex.Replace(chldOfTypPar.GetText(), @"\w+\.\b", "");
                    if (expr.PrevSibling!=null && expr.PrevSibling.GetType().Name == "MethodDeclaration")
                    {
                        if (expr.PrevSibling.GetText().Contains(expr.Name))
                            validMethodPresent = true;
                    }
                    if(validMethodPresent == false)
                        script.InsertBefore(expr, expr1);
                }
            }
        }

        public void WriteIfElseStructureInWebmethodTry(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfTryCatchStmt)
            {
                AllPatterns allPatterns = new AllPatterns();
                var copy = (TryCatchStatement)expr.Clone();
                var parentMethod = expr.GetParent<MethodDeclaration>();
                var methodName = "Valid" + parentMethod.Name;
                var chldOfTypPar = parentMethod.GetChildByRole(Roles.Parameter);
                if (chldOfTypPar != null)
                {

                    bool foundIfElseDecl = false;
                    var trySecondChild = expr.FirstChild.NextSibling.FirstChild.NextSibling;
                    foreach (var expression in trySecondChild.Parent.Children.OfType<IfElseStatement>())
                    {
                        if (expression.GetText().Contains(methodName))
                            foundIfElseDecl = true;
                    }
                    if (foundIfElseDecl == false)
                        script.InsertBefore(trySecondChild, allPatterns.IfElseTryCall(methodName));
                }
            }
        }

        public void WriteAccessControlStmtInTryCatch(CSharpFile file, DocumentScript script)
        {
            AllPatterns allPatterns = new AllPatterns();
            foreach (var expr in file.IndexOfTryCatchStmt)
            {
                bool foundCheckAccessControl = false;
                var copy = (TryCatchStatement)expr.Clone();
                foreach (var expression in expr.FirstChild.NextSibling.Children.OfType<ExpressionStatement>())
                {
                    if (expression.Match(allPatterns.AccessControlExpression()).Success)
                        foundCheckAccessControl = true;
                }
                if (foundCheckAccessControl == false)
                    script.InsertBefore(expr.FirstChild.NextSibling.FirstChild.NextSibling, allPatterns.AccessControlExpression());
            }
        }

        public void WriteValidationMethodBody(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfWebMthdDecl)
            {
                // logic to insert parameters to validation Method.
                var copy = (MethodDeclaration)expr.Clone();
                string str = expr.NextSibling.GetText().Split("()".ToCharArray())[1];
                var chldOfTypPar = expr.GetChildByRole(Roles.RPar);
                int offset = script.GetCurrentOffset(chldOfTypPar.StartLocation);
                script.InsertText(offset, str);

                // logic to insert if else statements inside validation Method body
                AllPatterns AP = new AllPatterns();
                int offset1 = script.GetCurrentOffset(expr.LastChild.FirstChild.EndLocation);
                foreach (var inv in expr.NextSibling.Descendants.OfType<ParameterDeclaration>())
                {
                    if (inv.FirstChild.GetText().Contains("int"))
                    {
                        script.InsertBefore(expr.LastChild.LastChild.PrevSibling, AP.IfElStmtInt(inv.LastChild.GetText()));
                    }
                    else if (inv.FirstChild.GetText().Contains("string"))
                    {
                        script.InsertBefore(expr.LastChild.LastChild.PrevSibling, AP.IfElStmtStr(inv.LastChild.GetText()));
                    }
                }
            }
        }

        public void InsertIfElseInWebmethodTry(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfIfElStmt)
            {
                // logic to add parameters in if block (in try catch of webmethod) to Validation Method.
                var copy = (IfElseStatement)expr.Clone();
                var ParentExpr = expr.GetParent<MethodDeclaration>();
                StringBuilder str = new StringBuilder();
                str.Append(ParentExpr.GetText().Split("()".ToCharArray())[1]);
                str.Replace("int", "");
                str.Replace("string", "");
                str.Replace("String", "");
                str.Replace("  ", "");
                int parameterOffset = script.GetCurrentOffset(expr.GetChildByRole(Roles.RPar).StartLocation) - 1;
                script.InsertText(parameterOffset, str.ToString());

                string retString = expr.GetParent<MethodDeclaration>().Body.LastChild.PrevSibling.FirstChild.NextSibling.GetText();
                int retValueOffset = script.GetCurrentOffset(expr.LastChild.LastChild.StartLocation);
                script.InsertText(retValueOffset, " "+ retString);
            }
        }

        public void AddPageNameGlobalinClass(CSharpFile file, DocumentScript script)
        {
            AllPatterns allPatterns = new AllPatterns();
            foreach (var expr in file.IndexOfClassDecl)
            {
                bool foundPageNameGlobalinClass = false;
                var copy = (TypeDeclaration)expr.Clone();
                foreach (var TypeMember in expr.Members.OfType<FieldDeclaration>())
                {
                    if (TypeMember.Match(allPatterns.PageNameGlobalFieldDecl(expr.Name + ".aspx")).Success)
                    {
                        foundPageNameGlobalinClass = true;
                        break;
                    } 
                }
                if(!foundPageNameGlobalinClass)
                    script.InsertBefore(expr.Members.First(), allPatterns.PageNameGlobalFieldDecl(expr.Name + ".aspx"));
            }
        }
    }
}
