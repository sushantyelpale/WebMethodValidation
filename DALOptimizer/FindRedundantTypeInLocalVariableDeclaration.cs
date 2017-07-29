// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;

namespace DALOptimizer
{
	/// <summary>
	/// The pattern matching example from the article; not actually used in this demo application.
	/// </summary>
	public class SimplyLocalVariableDeclarationsUsingVar : DepthFirstAstVisitor
	{
		VariableDeclarationStatement pattern = new VariableDeclarationStatement {
			Type = new AnyNode("type"),
			Variables = {
				new VariableInitializer {
					Name = Pattern.AnyString,
					Initializer = new ObjectCreateExpression {
						Type = new Backreference("type"),
						Arguments = { new Repeat(new AnyNode()) },
						Initializer = new OptionalNode(new AnyNode()) // this line was "forgotten" in the article
					}
				}
			}};
		
		public override void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
		{
//			if (CanBeSimplified(varDecl)) {
//				varDecl.Type = new SimpleType("var");
//			}
			// recurse into the statement (there might be a lambda with additional variable declaration statements inside there)
			base.VisitVariableDeclarationStatement(varDecl);
		}
		
		bool CanBeSimplified(VariableDeclarationStatement varDecl)
		{
			if (varDecl.Variables.Count != 1)
				return false;
			if (varDecl.Modifiers != Modifiers.None) // this line was "forgotten" in the article
				return false;
			VariableInitializer v = varDecl.Variables.Single();
			ObjectCreateExpression oce = v.Initializer as ObjectCreateExpression;
			if (oce == null)
				return false;
			//return ?AreEqualTypes?(varDecl.Type, oce.Type);
			return varDecl.Type.IsMatch(oce.Type);
		}
	}
}
