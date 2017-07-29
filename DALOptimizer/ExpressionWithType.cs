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
