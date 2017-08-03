using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;


namespace DALOptimizer
{
    class AllPatterns
    {
        
        public InvocationExpression ConnCloseInvExpr()
        {
            //conn.close()
            var expr = new InvocationExpression
            {
                Target = new MemberReferenceExpression
                {
                    Target = new IdentifierExpression("conn"),
                    MemberName = "Close"
                }
            };
            return expr;
        }

        public InvocationExpression ConnectionClassconnectExpr()
        {
            //ConnectionClass.connect()
            var expr = new InvocationExpression
            {
                Target = new MemberReferenceExpression
                {
                    Target = new IdentifierExpression("ConnectionClass"),
                    MemberName = "connect"
                }
            };
            return expr;
        }

        
    }
}
