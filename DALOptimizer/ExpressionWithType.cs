using System;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace DALOptimizer
{
	/// <summary>
	/// Pattern that matches any C# expression with the specified type.
	/// </summary>
	public class ExpressionWithType : Pattern
	{
		/*
			Possible usage:
			var pattern = new InvocationExpression {
				Target = new MemberReferenceExpression {
					Target = new ExpressionWithType(astResolver, typeof(string)),
					MemberName = "IndexOf"
				},
				Arguments = {
					new Repeat(new AnyNode()) { MinCount = 1 }
				}
			};
		 */
		
		readonly CSharpAstResolver resolver;
		readonly IType expectedType;
		
		public ExpressionWithType(CSharpAstResolver resolver, IType expectedType)
		{
			this.resolver = resolver;
			this.expectedType = expectedType;
		}
		
		public ExpressionWithType(CSharpAstResolver resolver, Type expectedType)
		{
			this.resolver = resolver;
			// The FindType() extension method can be used to look up the IType corresponding to a given System.Type
			this.expectedType = resolver.Compilation.FindType(expectedType);
		}
		
		public override bool DoMatch(INode other, Match match)
		{
			Expression expr = other as Expression;
			if (expr == null)
				return false;
			ResolveResult result = resolver.Resolve(expr);
			return result.Type.Equals(expectedType);
		}
	}
}
