using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace ORPWebMethod
{
    public partial class Form1 : Form
    {
        string CheckAccessMethodName = "";
        string filename = "";

        MatchInvocation matchInvocation = new MatchInvocation();
        PrintFunction printFunction = new PrintFunction();
        ModifyInvocations ModifyInvocations = new ModifyInvocations();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button6.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Solution solution = new Solution(filename);
            solution.ChooseCSProjFile(filename);
            CheckAccessMethodName = textBox1.Text.Trim();

            if (textBox1.Text.Any())
            {
                matchInvocation.FindInvocationTypeMethod(solution, 1, CheckAccessMethodName);
                //printFunction.PrintMethod(solution);
                ModifyInvocations.initializeExpr(solution, 1, CheckAccessMethodName);
             //   button2.Enabled = true;
                MessageBox.Show("Pass 1 is Completed. \nApplication will Now Exit. \nRun the application once again for Pass 2");
                Environment.Exit(0);
            }
            else
            {
                MessageBox.Show("Inter CheckAccess Method Name");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Solution solution = new Solution(filename);
            solution.ChooseCSProjFile(filename);
            CheckAccessMethodName = textBox1.Text.Trim();

            if (textBox1.Text.Any())
            {
                matchInvocation.FindInvocationTypeMethod(solution, 2, CheckAccessMethodName);
                //printFunction.PrintMethod(solution);
                ModifyInvocations.initializeExpr(solution, 2, CheckAccessMethodName);
            }

            button6.Enabled = true;
            MessageBox.Show("Pass 2 is Completed");
        }

        private void label1_Click(object sender, EventArgs e)
        {
        
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!(textBox2.Text.Trim().EndsWith(".sln") || textBox2.Text.Trim().EndsWith(".csproj")))
            {
                MessageBox.Show("Choose File with extenstion .csproj or .sln");
                textBox2.Clear();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfd = new OpenFileDialog();
            openfd.ShowDialog();
            filename = openfd.FileName;
            textBox2.Text = filename;
            waterMarkTextBox1.Text = filename;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            StringBuilder stringToPrint = new StringBuilder();

            foreach (var className in ModifyInvocations.AccessControlPageNames)
            {
                stringToPrint.Append("insert into UserAccessControl (pageName, Role, isActive) values ('" + className + ".aspx" + "',-#, 1);\n");
            }

            if (stringToPrint.Length == 0)
            {
                MessageBox.Show("Nothing To Write");
            }
            else
            {
                try
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.ShowDialog();
                    System.IO.File.WriteAllText(ofd.FileName, stringToPrint.ToString());
                    MessageBox.Show("Written to " + ofd.FileName + "\nApplication Will now Exit");
                    Environment.Exit(0);
                }
                catch (Exception)
                {
                    MessageBox.Show("Path is Empty");
                }
            }
        }

        private void sushant(object sender, EventArgs e)
        {

        }

        private void waterMarkTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
