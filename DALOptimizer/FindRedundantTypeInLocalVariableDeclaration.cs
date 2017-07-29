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
