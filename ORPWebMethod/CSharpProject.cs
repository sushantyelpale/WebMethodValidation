using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace ORPWebMethod
{
    /// <summary>
    /// Represents a C# project (.csproj file)
    /// </summary>

    public class CSharpProject : IDisposable
    {
        /// <summary>
        /// Parent solution.
        /// </summary>
        public Solution Solution;

        /// <summary>
        /// Title is the project name as specified in the .sln file.
        /// </summary>
        public string Title;

        /// <summary>
        /// Name of the output assembly.
        /// </summary>
        public string AssemblyName;

        /// <summary>
        /// Full path to the .csproj file.
        /// </summary>
        public string FileName;

        public List<CSharpFile> Files = new List<CSharpFile>();

        public CompilerSettings CompilerSettings = new CompilerSettings();

        public Microsoft.Build.Evaluation.Project msbuildProject;

        /// <summary>
        /// The unresolved type system for this project.
        /// </summary>
        public IProjectContent ProjectContent;

        /// <summary>
        /// The resolved type system for this project.
        /// This field is initialized once all projects have been loaded (in Solution constructor).
        /// </summary>
        public ICompilation Compilation;

        public CSharpProject()
        { }

        public Solution returnSolution()
        {
            return Solution;
        }

        //public

        public CSharpProject(Solution solution, string title, string fileName)
        {
            // Normalize the file name
            fileName = Path.GetFullPath(fileName);

            this.Solution = solution;
            this.Title = title;
            this.FileName = fileName;

            msbuildProject = new Microsoft.Build.Evaluation.Project(fileName);

            this.AssemblyName = msbuildProject.GetPropertyValue("AssemblyName");
            this.CompilerSettings.AllowUnsafeBlocks = GetBoolProperty(msbuildProject, "AllowUnsafeBlocks") ?? false;
            this.CompilerSettings.CheckForOverflow = GetBoolProperty(msbuildProject, "CheckForOverflowUnderflow") ?? false;
            string defineConstants = msbuildProject.GetPropertyValue("DefineConstants");

            foreach (string symbol in defineConstants.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                this.CompilerSettings.ConditionalSymbols.Add(symbol.Trim());

            // Initialize the unresolved type system
            IProjectContent pc = new CSharpProjectContent();
            pc = pc.SetAssemblyName(this.AssemblyName);
            pc = pc.SetProjectFileName(fileName);
            pc = pc.SetCompilerSettings(this.CompilerSettings);
            // Parse the C# code files

            // BuildSyntaxTree buildSyntaxTree = new BuildSyntaxTree();
            // buildSyntaxTree.BuildSyntTree();

            BuildSyntTree();

            // Add parsed files to the type system
            pc = pc.AddOrUpdateFiles(Files.Select(f => f.unresolvedTypeSystemForFile));

            // Add referenced assemblies:
            foreach (string assemblyFile in ResolveAssemblyReferences(msbuildProject))
            {
                IUnresolvedAssembly assembly = solution.LoadAssembly(assemblyFile);
                pc = pc.AddAssemblyReferences(new[] { assembly });
            }

            // Add project references:
            foreach (var item in msbuildProject.GetItems("ProjectReference"))
            {
                string referencedFileName = Path.Combine(msbuildProject.DirectoryPath, item.EvaluatedInclude);
                // Normalize the path; this is required to match the name with the referenced project's file name
                referencedFileName = Path.GetFullPath(referencedFileName);
                pc = pc.AddAssemblyReferences(new[] { new ProjectReference(referencedFileName) });
            }
            this.ProjectContent = pc;
        }

        public void BuildSyntTree()
        {
            foreach (var item in msbuildProject.GetItems("Compile"))
            {
                var file = new CSharpFile(this, Path.Combine(msbuildProject.DirectoryPath, item.EvaluatedInclude));
                Files.Add(file);
            }
        }

        private IEnumerable<string> ResolveAssemblyReferences(Microsoft.Build.Evaluation.Project project)
        {
            // Use MSBuild to figure out the full path of the referenced assemblies
            var projectInstance = project.CreateProjectInstance();
            projectInstance.SetProperty("BuildingProject", "false");
            project.SetProperty("DesignTimeBuild", "true");

            projectInstance.Build("ResolveAssemblyReferences", new[] { new ConsoleLogger(LoggerVerbosity.Minimal) });
            var items = projectInstance.GetItems("_ResolveAssemblyReferenceResolvedFiles");
            string baseDirectory = Path.GetDirectoryName(this.FileName);
            return items.Select(i => Path.Combine(baseDirectory, i.GetMetadataValue("Identity")));
        }

        private static bool? GetBoolProperty(Microsoft.Build.Evaluation.Project p, string propertyName)
        {
            string val = p.GetPropertyValue(propertyName);
            bool result;
            if (bool.TryParse(val, out result))
                return result;
            else
                return null;
        }

        public override string ToString()
        {
            return string.Format("[CSharpProject AssemblyName={0}]", AssemblyName);
        }

        public void Dispose()
        {
            this.Solution = null;
            this.Title = null;
            this.AssemblyName = null;
            this.FileName = null;
            this.Files.Clear();
            this.CompilerSettings = null;
            this.ProjectContent = null;
            this.Compilation = null;
            //throw new NotImplementedException();
        }

        
    }
}