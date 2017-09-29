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
        AllPatterns allPatterns = new AllPatterns();

        public void initializeExpr(Solution solution, int choice)
        {
            string CheckAccessMethodName = "";
            if (choice == 1)
            {
                do
                {
                    Console.WriteLine("Please Enter the CheckAccess Method Name: ");
                    CheckAccessMethodName = Console.ReadLine();
                } while (CheckAccessMethodName == null);
            }
            foreach (var file in solution.AllFiles)
            {
                
                if (file.IndexOfWebMthdDecl.Count == 0 &&    
                    file.IndexOfIfElStmt.Count == 0 &&     
                    file.IndexOfTryCatchStmt.Count == 0 &&  
                    file.IndexOfClassDecl.Count == 0 &&
                    file.IndexOfUsingDecl.Count == 0)
                {
                    continue;
                }
                
                file.syntaxTree.Freeze();

                // Create a document containing the file content:
                var document = new StringBuilderDocument(file.originalText);
                var formattingOptions = FormattingOptionsFactory.CreateAllman();
                var options = new TextEditorOptions();
                
                using (var script = new DocumentScript(document, formattingOptions, options))
                {
                    switch (choice)
                    {
                        case 1: //AddUsingAPIDecl(file, script);
                                WriteValidationMethodStructure(file, script);
                                WriteIfElseStructureInWebmethodTry(file, script);
                                WriteAccessControlStmtInTryCatch(file, script, CheckAccessMethodName);
                                break;
                        case 2: WriteValidationMethodBody(file, script);
                                InsertIfElseInWebmethodTry(file, script);
                                AddPageNameGlobalinClass(file, script);
                                CheckTryCatchInWebMethodBody(file, script);
                                break;
                    }
                }
                //File.WriteAllText(Path.ChangeExtension(file.fileName, ".output.cs"), document.Text);
                File.WriteAllText(Path.ChangeExtension(file.fileName, ".cs"), document.Text);
                
            }
            Console.WriteLine("Done. Press Any Key to Exit..............");
        }

        //Add Using ORP; Decl in file
        public void AddUsingAPIDecl(CSharpFile file, DocumentScript script)
        {
            var firstUsingExpr = file.IndexOfUsingDecl.First();
            var copy = (UsingDeclaration)firstUsingExpr.Clone();
            var parentDecl = firstUsingExpr.Parent;
            bool foundWebMethod = false;
            bool founORPDecl = false;
            foreach(var method in parentDecl.Descendants.OfType<MethodDeclaration>())
            {
                if (method.FirstChild.GetText().Contains("WebMethod"))
                {
                    foundWebMethod = true;
                    break;
                }
            }
            foreach(var otherUsingExpr in firstUsingExpr.Parent.Children.OfType<UsingDeclaration>()) 
            {
                if (otherUsingExpr.Match(allPatterns.ORPUsingDecl()).Success)
                {
                    founORPDecl = true;
                    break;
                }
            }
            if (foundWebMethod && !founORPDecl)
                script.InsertBefore(file.IndexOfUsingDecl.Last(), allPatterns.ORPUsingDecl());
//            if (!(Path.GetDirectoryName(file.fileName).EndsWith("ORP")) && foundWebMethod && !founORPDecl)
//                script.InsertBefore(file.IndexOfUsingDecl.Last().NextSibling, allPatterns.ORPUsingDecl());

        }
        // Writing validation Methhod strucutre for webmethod 
        public void WriteValidationMethodStructure(CSharpFile file, DocumentScript script)
        {
            foreach (MethodDeclaration expr in file.IndexOfWebMthdDecl)
            {
                // for adding method before the webmethod
                var copy = (MethodDeclaration)expr.Clone();
                var chldOfTypPar = expr.GetChildByRole(Roles.Parameter);
                var mtdhName = expr.Name;
                var chdMtdhName = "Valid" + mtdhName;
                
                var validationMthd = allPatterns.ValidationMthd(chdMtdhName);
                bool validMethodPresent = false;

                if (chldOfTypPar != null)
                {
                    if (expr.PrevSibling!=null && expr.PrevSibling.GetType().Name == "MethodDeclaration")
                    {
                        if (expr.PrevSibling.GetText().Contains(chdMtdhName))
                            validMethodPresent = true;
                    }
                    if(!validMethodPresent)
                        script.InsertBefore(expr, validationMthd);
                }
            }
        }

        // Adding call to validation method inside try catch statement
        public void WriteIfElseStructureInWebmethodTry(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfTryCatchStmt)
            {
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
                    if (!foundIfElseDecl)
                        script.InsertBefore(trySecondChild, allPatterns.IfElseTryCall(methodName));
                }
            }
        }

        // To write access control statement in try block of webmethod
        public void WriteAccessControlStmtInTryCatch(CSharpFile file, DocumentScript script, string checkAccessMethodName)
        {
            foreach (var expr in file.IndexOfTryCatchStmt)
            {
                bool foundCheckAccessControl = false;
                var copy = (TryCatchStatement)expr.Clone();
                foreach (var expression in expr.FirstChild.NextSibling.Children.OfType<ExpressionStatement>())
                {
                    if (expression.Match(allPatterns.AccessControlExpression(checkAccessMethodName)).Success)
                        foundCheckAccessControl = true;
                }
                if (!foundCheckAccessControl)
                    script.InsertBefore(expr.FirstChild.NextSibling.FirstChild.NextSibling, allPatterns.AccessControlExpression(checkAccessMethodName));
            }
        }


        public void WriteValidationMethodBody(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfWebMthdDecl)
            {
                // if parameters are present, go to next iteration without doing anything.
                if (expr.Parameters.Count != 0)
                    continue;

                if (expr.Name != "Valid" + (expr.NextSibling.GetChildByRole(Roles.Identifier).GetText()))
                    continue;

                // logic to insert parameters to validation Method.
                var copy = (MethodDeclaration)expr.Clone();
                string str =  string.Join(", ", from parameter 
                                                 in expr.NextSibling.Descendants.OfType<ParameterDeclaration>() 
                                                 select parameter.GetText());
                var chldOfTypPar = expr.GetChildByRole(Roles.RPar);
                int offset = script.GetCurrentOffset(chldOfTypPar.StartLocation);
                script.InsertText(offset, str);

                // Insert Static keyword to validation method.
                script.InsertText(script.GetCurrentOffset(expr.ReturnType.StartLocation), "static ");

                // logic to insert if else statements inside validation Method body
                int offset1 = script.GetCurrentOffset(expr.LastChild.FirstChild.EndLocation);
                
                var locationToInsert = expr.LastChild.LastChild.PrevSibling;
                script.InsertBefore(locationToInsert, allPatterns.ValidationFieldDecl());
                foreach (var inv in expr.NextSibling.Children.OfType<ParameterDeclaration>())
                {
                    string dataType = inv.FirstChild.GetText();
                    string varName = inv.LastChild.GetText();
                    if (dataType.Contains("int"))
                        script.InsertBefore(locationToInsert, allPatterns.IfElStmtInt(varName));
                    else if (dataType.Contains("string") || dataType.Contains("String"))
                        script.InsertBefore(locationToInsert, allPatterns.IfElStmtStr(varName));
                    else if (dataType.Contains("float") || dataType.Contains("decimal"))
                        script.InsertBefore(locationToInsert, allPatterns.IfElseFloatDecimal(varName));
                    else
                        script.InsertText(script.GetCurrentOffset(locationToInsert.StartLocation),"DummyText_DatatypeIsDifferent ");
                }
                
            }
        }

        public void InsertIfElseInWebmethodTry(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfIfElStmt)
            {
                //check if parameters are passed before, go to next iteration 
                if (!(expr.Descendants.OfType<InvocationExpression>().First().GetText().Split("()".ToCharArray())[1] == ""))
                    continue;

                // logic to add parameters in if block (in try catch of webmethod) to Validation Method.
                var copy = (IfElseStatement)expr.Clone();
                var ParentExpr = expr.GetParent<MethodDeclaration>();
                string str = string.Join(", ", from parameter
                                               in ParentExpr.Descendants.OfType<ParameterDeclaration>()
                                               select parameter.Name);
               
                int parameterOffset = script.GetCurrentOffset(expr.GetChildByRole(Roles.RPar).StartLocation) - 1;
                script.InsertText(parameterOffset, str);
                if(expr.GetParent<MethodDeclaration>().ReturnType.GetText()!= "void")
                {
                    var returnStmt = expr.GetParent<MethodDeclaration>().Body.LastChild.PrevSibling.FirstChild;
                    int retValueOffset = script.GetCurrentOffset(expr.LastChild.LastChild.StartLocation);
                    if (returnStmt.GetText() == "return")
                    {
                        string retString = returnStmt.NextSibling.GetText();
                        script.InsertText(retValueOffset, " " + retString);
                    }
                    else
                        script.InsertText(retValueOffset, " DummyText");
                }
            }
        }
        // Add pageName parameter in clas global declaration
        public void AddPageNameGlobalinClass(CSharpFile file, DocumentScript script)
        {
            foreach (var expr in file.IndexOfClassDecl)
            {
                bool foundPageNameGlobalInClass = false;
                var copy = (TypeDeclaration)expr.Clone();
                foreach (var TypeMember in expr.Members.OfType<FieldDeclaration>())
                {
                    if (TypeMember.Match(allPatterns.PageNameGlobalFieldDecl(expr.Name + ".aspx")).Success)
                    {
                        foundPageNameGlobalInClass = true;
                        //break;
                    }
                    if (TypeMember.Match(allPatterns.PageNameGlobalFieldDecl1(expr.Name + ".aspx")).Success)
                        script.Remove(TypeMember, true);
                }
                if(!foundPageNameGlobalInClass)
                    script.InsertBefore(expr.Members.First(), allPatterns.PageNameGlobalFieldDecl(expr.Name + ".aspx"));
            }
        }

        // Checking Whether Try Catch is Present in WebMethod
        public void CheckTryCatchInWebMethodBody(CSharpFile file, DocumentScript script)
        {
            foreach (MethodDeclaration expr in file.IndexOfWebMthdDecl)
            {
                var copy = (MethodDeclaration)expr.NextSibling.Clone();
                if (expr.NextSibling.FirstChild.GetText().Contains("WebMethod") && 
                    !expr.NextSibling.Descendants.OfType<TryCatchStatement>().Any())
                    script.InsertText(script.GetCurrentOffset(expr.NextSibling.StartLocation), "DummyText_TryCatchNotPresent");
            }
        }
    }
}
