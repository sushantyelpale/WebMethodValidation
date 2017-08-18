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


namespace DALOptimizer
{
    class AllPatterns
    {

        //conn.close() Pattern
        public InvocationExpression ConnCloseInvExpr()
        {
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

        //cmd.Dispose() Pattern
        public InvocationExpression CmdDispose()
        {
            var expr = new InvocationExpression {
                Target = new MemberReferenceExpression
                {
                    Target = new IdentifierExpression("conn"),
                    MemberName = "Dispose"
                }
            };
            return expr;
        }

        //conn.open()
        public InvocationExpression ConnOpenInvExpr()
        {
            var expr = new InvocationExpression
            {
                Target = new MemberReferenceExpression
                {
                    Target = new IdentifierExpression("conn"),
                    MemberName = "Open"
                }
            };
            return expr;
        }

        //da.Fill(dt);
        public ExpressionStatement FillExpr()
        {
            var Expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression { 
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "Fill"
                    },
                    Arguments = {
                        new IdentifierExpression(Pattern.AnyString)
                    }
                }
            };
            return Expr;
        }

        //ConnectionClass.connect()
        public InvocationExpression ConnectionClassconnectExpr()
        {
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

       
        //Replaced by cmd = new SqlCommand("InsertTicket", con);
        public AssignmentExpression sqlCmdstmt()
        {
            var expr = new AssignmentExpression
                {
                    Left = new IdentifierExpression(Pattern.AnyString),
                    Right = new ObjectCreateExpression
                    {
                        Type = new SimpleType("SqlCommand"),
                        Arguments = { new AnyNode("PrimitiveExpression"), new IdentifierExpression(Pattern.AnyString) }
                    }
            };
            return expr;
        }



        //conn = new SqlConnection(constr);
        public ExpressionStatement sqlConnstmt()
        {
            var expr = new ExpressionStatement{
                 Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(Pattern.AnyString),
                    Right = new ObjectCreateExpression
                    {
                        Type = new SimpleType("SqlConnection"),
                        Arguments = { new IdentifierExpression(Pattern.AnyString) }
                    }
                }
            };
            return expr;
        }

        //stored procedure statement inside try block
        //cmd.CommandType=CommandType.StoredProcedure;
        public ExpressionStatement StoredProc()
        {
            var expr = new ExpressionStatement{
                Expression =  new AssignmentExpression
                {
                    Left = new MemberReferenceExpression { 
                        Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "CommandType"
                    },
                    Right = new MemberReferenceExpression {
                        Target = new IdentifierExpression("CommandType"),
                        MemberName = "StoredProcedure"
                    }
                }
            };
            return expr;
        }


        public ParameterDeclaration methd()
        {
            var expr = new ParameterDeclaration { 
                Type = new AnyNode()
            };

            return expr;
        }

        //cmd.ExecuteNonQuery()
        public ExpressionStatement ExNonQuery()
        {
            var expr = new ExpressionStatement{ 
                Expression = new AssignmentExpression
                {
                    Left = new IdentifierExpression(Pattern.AnyString),
                    Right = new InvocationExpression
                    {
                        Target = new MemberReferenceExpression { 
                            Target = new IdentifierExpression(Pattern.AnyString),
                        MemberName = "ExecuteNonQuery"
                        }
                    }
                }
            };

            return expr;
        }

        //log.Error("TicketDAL:displayTicket : " + ex.Message);
        public ExpressionStatement logErr()
        {
            var expr = new ExpressionStatement
            {
                Expression = new InvocationExpression
                {
                    Target = new MemberReferenceExpression
                    {
                        Target = new IdentifierExpression("log"),
                        MemberName = "Error"
                    },
                    Arguments = { 
                        new BinaryOperatorExpression{
                            Operator = BinaryOperatorType.Add,
                            Left = new AnyNode("PrimitiveExpression"),
                            Right = new MemberReferenceExpression{
                                    Target = new IdentifierExpression(Pattern.AnyString),
                                    MemberName = Pattern.AnyString
                            }
                        }
                    }
                }
            };
            return expr;
        }

        //catch clause
        public CatchClause ctchclause()
        {
            var expr = new CatchClause
            {
                Type = new SimpleType("Exception"),
                VariableName = "ex",
                Body = new BlockStatement { 
                    new ExpressionStatement
                    {
                        Expression = new InvocationExpression
                        {
                            Target = new MemberReferenceExpression
                            {
                                Target = new ObjectCreateExpression { 
                                    Type = new SimpleType("LoggerProcessing")
                                },
                                MemberName = "write"
                            },
                            Arguments = { new IdentifierExpression("ex") }
                        }
                    }
                }
            };
            return expr;
        }


        //finally Block
        public BlockStatement FinalyBlck()
        {
            var Expr = new BlockStatement
            {

            };
            return Expr;
        }

        //DatabaseProcessing db = new DatabaseProcessing();
        public FieldDeclaration DbProcessing()
        {
            var Expr = new FieldDeclaration{
                ReturnType = new SimpleType("DatabaseProcessing"),
                Variables = {new VariableInitializer(
                    "db",  new ObjectCreateExpression
                        (type : new SimpleType("DatabaseProcessing")) )
                } 
            };
            return Expr;
        }

        public MethodDeclaration MthdDecl()
        {
            var expr = new MethodDeclaration
            {
                Modifiers = Modifiers.Any,
                ReturnType = new PrimitiveType(Pattern.AnyString),
                Name = Pattern.AnyString,
                Parameters = {
                    new ParameterDeclaration
                    {
                        Type = new MemberType
                        {
                            Target = new SimpleType(Pattern.AnyString),
                            MemberName = Pattern.AnyString
                        },
                        Name = Pattern.AnyString
                    },
                },
                Body = new AnyNode("BlockStatement")
            };
            return expr;
        }


        public VariableDeclarationStatement newPattern()
        {
            var pattern = new VariableDeclarationStatement
            {
                Type = new AnyNode("type"),
                Variables = {
                            new VariableInitializer {
                                Name = Pattern.AnyString,
                                Initializer = new ObjectCreateExpression {
                                    Type = new Backreference("type"),
                                    Arguments = { new Repeat(new AnyNode()) }
                                }
                            }
                    }

               };
            return pattern;
        }

    }
}

