using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;


namespace WebMethodCheck
{
    class AllPatterns
    {
        public IfElseStatement IfElStmtInt(string varName)
        {
            var ifElStmt = new IfElseStatement{
                Condition = new BinaryOperatorExpression{
                    Operator = BinaryOperatorType.LessThanOrEqual,
                    Left = new IdentifierExpression(varName),
                    Right = new PrimitiveExpression(0)
                },
                TrueStatement = new ReturnStatement { 
                    Expression = new PrimitiveExpression(false)
                }
            };
            return ifElStmt;
        }

        public IfElseStatement IfElStmtStr(string varName)
        {
            var ifElStmt = new IfElseStatement
            {
                Condition = new BinaryOperatorExpression
                {
                    Operator = BinaryOperatorType.ConditionalOr,
                    Left = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new TypeReferenceExpression(new PrimitiveType("string")),
                            MemberName = "isEmptyOrNull"
                        },
                        Arguments = { new IdentifierExpression(varName) }
                    },
                    Right = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression
                        {
                            Target = new TypeReferenceExpression(new PrimitiveType("string")),
                            MemberName = "IsNullOrWhiteSpace"
                        },
                        Arguments = { new IdentifierExpression(varName) }
                    },
                },
                TrueStatement = new ReturnStatement {
                    Expression = new PrimitiveExpression(false)
                }
            };
            return ifElStmt;
        }

        public ParameterDeclaration parDecl(string varName, string val)
        {
            var expr = new ParameterDeclaration( new PrimitiveType(varName), val);
            return expr;
        }

        public IfElseStatement IfElseTryCall(string mtdhName)
        {
            var expr = new IfElseStatement
            {
                Condition = new UnaryOperatorExpression
                {
                    Operator = UnaryOperatorType.Not,
                    Expression = new InvocationExpression
                    {
                        Target = new IdentifierExpression(mtdhName)
                    }
                },
                TrueStatement = new ReturnStatement { 
                }
            };
            return expr;
        }

        public FieldDeclaration PageNameGlobalFieldDecl(string className)
        {
            var expr = new FieldDeclaration{
                  ReturnType =  new PrimitiveType("string"),
                  Variables = {
                      new VariableInitializer(
                          "pageName",
                          new PrimitiveExpression(className))
                  }
            };
            return expr;
        }

        public ExpressionStatement AccessControlExpression()
        {
            var Expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("accessControl"),
                        MemberName = "Check"
                    },
                    Arguments = { new IdentifierExpression("pageName") }
                }
            };
            return Expr;
        }

        // web method to add
        public MethodDeclaration ValidationMthd(string str)
        {
            var expr = new MethodDeclaration
            {
                Modifiers = Modifiers.Public,
                //ModifierTokens = Static,
                ReturnType = new PrimitiveType("bool"),
                Name = str,
                Body = new BlockStatement{
                    new ReturnStatement {
                        Expression = new PrimitiveExpression(true)
                    }
                }
            };
            return expr;
        }

    }
}