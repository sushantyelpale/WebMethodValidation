using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace WebMethodCheck
{
    public class myDict1
    {
        public string dataType { get; set; }
        public string Value { get; set; }
    }

    class Program
    {
       // public SyntaxTree SyntaxTree;
       // public readonly string OriginalText;

        [STAThread]
        public static void Main(string[] args)
        {
            int choice;
            Console.Write("1. Generate validation method structure\n2. Generate Validation Method Body \nEnter 1 or 2: ");
            choice = Convert.ToInt32(Console.ReadLine());
            
            MatchInvocation matchInvocation = new MatchInvocation();
            PrintFunction printFunction = new PrintFunction();
            ModifyInvocations matchExpr = new ModifyInvocations();

            OpenFileDialog openfd = new OpenFileDialog();
            openfd.ShowDialog();
            string filename = openfd.FileName;

            if (string.IsNullOrWhiteSpace(filename))
            {
                MessageBox.Show("Please Select one file ");
                return;
            }

            Solution solution = new Solution(filename);
            solution.ChooseCSProjFile(filename);
            matchInvocation.FindInvocationTypeMethod(solution, choice);
            printFunction.PrintMethod(solution);
            matchExpr.initializeExpr(solution, choice);
            Console.ReadKey();
        }
    }
}