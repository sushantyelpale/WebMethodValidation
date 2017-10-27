using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;

namespace ORPWebMethod
{
    internal class AllPatterns
    {
        public UnaryOperatorExpression IfElseValidMethodUnary(string varName)
        {
            return new UnaryOperatorExpression
            {
                Operator = UnaryOperatorType.Not,
                Expression = new InvocationExpression
                {
                    Target = new IdentifierExpression(varName)
                }
            };
        }

        public UnaryOperatorExpression IfElseValidMethodUnaryOld()
        {
            return new UnaryOperatorExpression
            {
                Operator = UnaryOperatorType.Not,
                Expression = new InvocationExpression
                {
                    Target = new IdentifierExpression(Pattern.AnyString),
                    Arguments = { new Repeat(new AnyNode("IdentifierExpression")) }
                }
            };
        }

        public UnaryOperatorExpression IfElseValidMethodUnaryMemberRef()
        {
            return new UnaryOperatorExpression
            {
                Operator = UnaryOperatorType.Not,
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = Pattern.AnyString,
                    },
                    Arguments = { new Repeat(new AnyNode("IdentifierExpression")) }
                }
            };
        }

        public BinaryOperatorExpression IfElseValidMethodBinary()
        {
            return new BinaryOperatorExpression
            {
                Left = new InvocationExpression
                {
                    Target = new IdentifierExpression(Pattern.AnyString),
                    Arguments = { new Repeat(new AnyNode("IdentifierExpression")) }
                },
                Operator = BinaryOperatorType.Equality,
                Right = new AnyNode("PrimitiveExpression")
            };
        }

        public BinaryOperatorExpression IfElseValidMethodBinaryMemberRef()
        {
            return new BinaryOperatorExpression
            {
                Left = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = Pattern.AnyString
                    },

                    Arguments = { new Repeat(new AnyNode("IdentifierExpression")) }
                },
                Operator = BinaryOperatorType.Equality,
                Right = new AnyNode("PrimitiveExpression")
            };
        }

        public IfElseStatement IfElStmtInt(string varName)
        {
            return new IfElseStatement
            {
                Condition = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("validation"),
                        MemberName = "IsValidInt"
                    },
                    Arguments = { new IdentifierExpression(varName) }
                },
                TrueStatement = new ReturnStatement
                {
                    Expression = new PrimitiveExpression(true)
                }
            };
        }

        public IfElseStatement IfElseFloatDecimal(string varName)
        {
            return new IfElseStatement
            {
                Condition = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("validation"),
                        MemberName = "IsValidDecimal"
                    },
                    Arguments = { new IdentifierExpression(varName) }
                },
                TrueStatement = new ReturnStatement
                {
                    Expression = new PrimitiveExpression(true)
                }
            };
        }

        public IfElseStatement IfElStmtStr(string varName)
        {
            return new IfElseStatement
            {
                Condition = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("validation"),
                        MemberName = "IsValidString"
                    },
                    Arguments = { new IdentifierExpression(varName) }
                },
                TrueStatement = new ReturnStatement
                {
                    Expression = new PrimitiveExpression(true)
                }
            };
        }

        public ParameterDeclaration parDecl(string varName, string val)
        {
            return new ParameterDeclaration(new PrimitiveType(varName), val);
        }

        public IfElseStatement IfElseTryCall(string mtdhName)
        {
            return new IfElseStatement
            {
                Condition = new UnaryOperatorExpression
                {
                    Operator = UnaryOperatorType.Not,
                    Expression = new InvocationExpression
                    {
                        Target = new IdentifierExpression(mtdhName)
                    }
                },
                TrueStatement = new ReturnStatement
                {
                }
            };
        }

        public FieldDeclaration PageNameGlobalFieldDecl(string className)
        {
            return new FieldDeclaration
            {
                Modifiers = Modifiers.Static,
                ReturnType = new PrimitiveType("string"),
                Variables = {
                      new VariableInitializer(
                          "pageName",
                          new PrimitiveExpression(className))
                  }
            };
        }

        public FieldDeclaration PageNameGlobalFieldDecl1(string className)
        {
            return new FieldDeclaration
            {
                Modifiers = Modifiers.Static | Modifiers.Public,
                ReturnType = new PrimitiveType("string"),
                Variables = {
                      new VariableInitializer(
                          "pageName",
                          new PrimitiveExpression(className))
                  }
            };
        }

        public ExpressionStatement AccessControlExpression(string MethodName)
        {
            return new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("AccessControl"),
                        MemberName = MethodName
                    },
                    Arguments = { new IdentifierExpression("pageName") }
                }
            };
        }

        // Using ORP Decl
        public UsingDeclaration ORPUsingDecl()
        {
            return new UsingDeclaration
            {
                Import = new SimpleType("ORP")
            };
        }

        // web method to add
        public MethodDeclaration ValidationMthd(string str)
        {
            return new MethodDeclaration
            {
                Modifiers = Modifiers.Public,
                //ModifierTokens = ,
                //ModifierTokens = Static,
                ReturnType = new PrimitiveType("bool"),
                Name = str,
                Body = new BlockStatement{
                    new ReturnStatement {
                        Expression = new PrimitiveExpression(true)
                    }
                }
            };
        }

        // Validation validation = new Validation();
        public FieldDeclaration ValidationFieldDecl()
        {
            return new FieldDeclaration
            {
                ReturnType = new SimpleType("Validation"),
                Variables = {
                    new VariableInitializer("validation",
                        new ObjectCreateExpression(new SimpleType("Validation")))
                }
            };
        }
    }
}