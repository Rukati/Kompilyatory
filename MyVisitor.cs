using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;


namespace Kompilyatory
{
    internal class MyVisitor : ExprBaseVisitor<IParseTree>
    {
        private void BuildBody(ref List<object> body, [NotNull] ExprParser.StatContext[] context)
        {
            foreach (var item in context)
            {
                if (item.print() != null)
                    body.Add(BodyPrint(item.print()));
                else if (item.initialization() != null)
                    body.Add(BodyInit(item.initialization()));
                else if (item.@if() != null)
                    body.Add(BodyIf(item.@if()));
                else if (item.@while() != null)
                    body.Add(BodyWhile(item.@while()));
                else if (item.changeValue() != null)
                    body.Add(ChangeValue(item.changeValue()));
                else if (item.@for() != null)
                    body.Add(BodyFor(item.@for()));
                else if (item.callFunc() != null)
                    body.Add(CallFunc(item.callFunc()));
                else if (item.doWhile() != null)
                    body.Add(BodyDoWhile(item.doWhile()));
            }
        }
        private void BuildArgcCallFunc(ref List<object> args, [NotNull] ExprParser.ArgcContext[] contexts)
        {
            if (contexts != null)
            {
                foreach (var VARIABLE in contexts)
                {
                    if (VARIABLE.arguments().expr() != null)
                    {
                        Stack<string> argg = new Stack<string>();
                        BodyExpr(ref argg, VARIABLE.arguments().expr());
                        args.Add(argg);
                    }
                    else args.Add(new List<string>(){VARIABLE.arguments().GetText()});
                    if (VARIABLE.argc() != null) BuildArgcCallFunc(ref args, VARIABLE.argc());
                }
            }
            
        }
        private object CallFunc([NotNull] ExprParser.CallFuncContext context, bool node = false)
        {
            List<object> args = new List<object>();
            BuildArgcCallFunc(ref args, context.argc());
            var stateCallFunc = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                { "state",
                    new Dictionary<string, Dictionary<string, object>>()
                    {
                        {
                            "callFunc",
                            new Dictionary<string, object>()
                            {
                                { "ID", context.funcID().GetText() },
                                { "argc", args },
                            }
                        },
                    }
                }
            };
            if (node) Program.InitNode.Add(stateCallFunc);
            else return stateCallFunc;
            return null;
        }
        private object BodyWhile([NotNull] ExprParser.WhileContext context, bool node = false)
        {
            List<object> whileBody = new List<object>();
            Stack<string> leftExpr = new Stack<string>();
            Stack<string> rightExpr = new Stack<string>();

            BuildBody(ref whileBody, context.whileBody().stat());

            BodyExpr(ref leftExpr, context.equation().expr()[0]);
            BodyExpr(ref rightExpr, context.equation().expr()[1]);

            var stateWhile = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "while",
                                new Dictionary<string, object>()
                                {
                                    { "left", leftExpr },
                                    { "right", rightExpr },
                                    { "operator", context.equation().op.Text },
                                    { "body", whileBody },
                                }
                            },
                        }
                    }
                };

            if (node) Program.InitNode.Add(stateWhile);
            else return stateWhile;
            return null;
        }
        private object BodyIf([NotNull] ExprParser.IfContext context, bool node = false)
        {
            List<object> ifBody = new List<object>();
            List<object> elseBody = new List<object>();
            Stack<string> leftExpr = new Stack<string>();
            Stack<string> rightExpr = new Stack<string>();

            BuildBody(ref ifBody, context.ifBody().stat());
            if (context.ifBody().@return() != null)
                ifBody.Add(new Dictionary<string,object>()
                {
                    {
                        "return", new List<object>(){ReturnBody(context.ifBody().@return())}
                    }                    
                });
            if (context.elseBody() != null)
            {
                BuildBody(ref elseBody, context.elseBody().stat());
                if (context.elseBody().@return() != null)
                {
                    elseBody.Add(new Dictionary<string,object>()
                    {
                        {
                            "return", ReturnBody(context.elseBody().@return())
                        }                    
                    });
                }
            }

            BodyExpr(ref leftExpr, context.equation().expr()[0]);
            BodyExpr(ref rightExpr, context.equation().expr()[1]);

            var stateIfElse = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "if",
                                new Dictionary<string, object>()
                                {
                                    { "left", leftExpr },
                                    { "right", rightExpr },
                                    { "operator", context.equation().op.Text },
                                    { "body", ifBody },
                                    { "else",
                                        new Dictionary<string,object>()
                                        {
                                            { "body", elseBody},
                                        }
                                    },
                                }
                            },
                        }
                    }
                };

            if (node) Program.InitNode.Add(stateIfElse);
            else return stateIfElse;
            return null;
        }
        private object ReturnBody([NotNull] ExprParser.ReturnContext item)
        {
            List<object> bodyReturn = new List<object>();
            Stack<string> returnStack = new Stack<string>();

            if (item != null)
            {
                if (item.expr() != null)
                {
                    BodyExpr(ref returnStack, item.expr());
                    bodyReturn.Add(returnStack);
                }
                else
                {
                    bodyReturn.Add(item.GetText());
                }
            }

            return bodyReturn;
        }
        private void BuildFunction([NotNull] ExprParser.FunctionContext[] context)
        {
            foreach (var item in context)
            {
                List<object> body = new List<object>();
                BuildBody(ref body,item.stat());
                
                List<Dictionary<string, string>> argc = new List<Dictionary<string, string>>();
                FuncArgc(item.parameter(), ref argc);
                var stateFunction = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    {
                        "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "function",
                                new Dictionary<string, object>()
                                {
                                    { "ID", item.ID().GetText()},
                                    { "args", item.parameter() != null? argc : null},
                                    { "type", item.funType().GetText()},
                                    { "body", body},
                                    { "return", new List<object>(){ReturnBody(item.@return())}},
                                }
                            }
                        }
                    }
                };
                Program.InitNode.Add(stateFunction);
            }
        }
        private object BodyInit([NotNull] ExprParser.InitializationContext context, bool node = false)
        {
            var name = context.ID() == null ? "" : context.ID().GetText();
            var type = context.TYPE() == null ? "" : context.TYPE().GetText();

            Stack<string> exprBody = new Stack<string>();
            if (context.expr() != null)
            {
                BodyExpr(ref exprBody, context.expr());
            }

            List<object> args = new List<object>();
            BuildArgcCallFunc(ref args, context.callFunc()?.argc());
            var stateInit = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "initialization",
                                new Dictionary<string, object>()
                                {
                                    {"type", type},
                                    {"ID", name},
                                    {"expr", exprBody },
                                    {
                                        "func", new Dictionary<string,object>()
                                        {
                                            {"ID",context.callFunc()?.funcID().GetText()},
                                            {"argc", args}
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            if (node)
                Program.InitNode.Add(stateInit);
            else
            {
                return stateInit;
            }

            return null;
        }
        private void FuncArgc([NotNull] ExprParser.ParameterContext context, ref List<Dictionary<string, string>> argc)
        {
            if (context.parameter().Length > 0)
            {
                argc.Add(
                    new Dictionary<string, string>()
                    {
                        { "type", context.TYPE().GetText() },
                        { "ID", context.ID().GetText() },
                    });
                FuncArgc(context.parameter()[0],ref argc);
            }
            else
            {
                if (context.ChildCount > 0)
                argc.Add(
                    new Dictionary<string, string>()
                    {
                        { "type", context.TYPE().GetText() },
                        { "ID", context.ID().GetText() },
                    });
            }
        }
        private object BodyPrint([NotNull] ExprParser.PrintContext context, bool node = false)
        {
            var StateWriteln = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    { "state",
                        new Dictionary<string, Dictionary<string,object>>()
                        {
                            {
                                "writeln",
                                new Dictionary<string, object>()
                                {
                                    {
                                        "Arguments" , new List<object>()
                                    },
                                    {
                                        "ln", context.PRINT().GetText() == "write" ? false : true
                                    }
                                }
                            }
                        }
                    }
                };

            List<object> argc = new List<object>();
            if (context.print_arguments() != null)
            {
                PrintBody(context.print_arguments(), ref argc);
                StateWriteln["state"]["writeln"]["Arguments"] = argc;
            }

            if (node) Program.InitNode.Add(StateWriteln);
            else
            {
                return StateWriteln;
            }
            return null;
        }
        private void PrintBody([NotNull]ExprParser.Print_argumentsContext item, ref List<object> argc)
        {
            if (item.expr() != null)
            {
                _exprStack = new Stack<string>();
                this.VisitExpr(item.expr());
                var exprValue = new Dictionary<string, object>()
                {
                    { "value", _exprStack.ToList() },
                    { "type", item.TYPE() == null ? "int" : item.TYPE().GetText() }
                };
                argc.Add(new Dictionary<string, object>()
                {
                    { "Expr", exprValue }
                });
            }
            else if (item.NUMBER() != null)
            {
                var numericValue = new Dictionary<string, object>();
                numericValue.Add("numeric", item.NUMBER().GetText());
                argc.Add(numericValue);
            }
            else if (item.LINE() != null)
            {
                var lineValue = item.LINE().GetText().Trim('"');
                argc.Add(new Dictionary<string, string>()
                {
                    { "line", lineValue }
                });
            }
            else if (item.ID() != null)
            {
                var variableValue = item.ID().GetText().Trim('$');
                argc.Add(new Dictionary<string, string>()
                {
                    { "variable", variableValue }
                });
            }
            if (item.print_arguments().Length > 0) PrintBody(item.print_arguments()[0], ref argc);
        }
        private void BodyExpr(ref Stack<string> expr, [NotNull] ExprParser.ExprContext context)
        {
            _exprStack = new Stack<string>();
            VisitExpr(context);
            expr = _exprStack;
        }
        private object ChangeValue([NotNull] ExprParser.ChangeValueContext context, bool node = false)
        {
            Stack<string> StackNewValue = new Stack<string>();
            BodyExpr(ref StackNewValue, context.expr());
            
            var StateNewValue = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "changeValue",
                                new Dictionary<string, object>()
                                {
                                    {"ID", context.ID().GetText().Trim('$')},
                                    {"expr", StackNewValue },
                                    //{"END", context.END().ToString()}
                                }
                            }
                        }
                    }
                };
            if (node) Program.InitNode.Add(StateNewValue);
            else return StateNewValue;
            return null;
        }
        private object BodyDoWhile([NotNull] ExprParser.DoWhileContext context, bool node = false)
        {
            List<object> doWhileBody = new List<object>();
            Stack<string> leftExpr = new Stack<string>();
            Stack<string> rightExpr = new Stack<string>();
            
            BuildBody(ref doWhileBody, context.whileBody().stat());

            BodyExpr(ref leftExpr, context.equation().expr()[0]);
            BodyExpr(ref rightExpr, context.equation().expr()[1]);

            var stateDoWhile = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "DoWhile",
                                new Dictionary<string, object>()
                                {
                                    { "left", leftExpr },
                                    { "right", rightExpr },
                                    { "operator", context.equation().op.Text },
                                    { "body", doWhileBody },
                                }
                            },
                        }
                    }
                };

            if (node) Program.InitNode.Add(stateDoWhile);
            else return stateDoWhile;
            return null;
        }
        private object BodyFor([NotNull] ExprParser.ForContext context, bool node = false)
        {
            Stack<string> LeftExpr = new Stack<string>();
            Stack<string> RightExpr = new Stack<string>();

            BodyExpr(ref LeftExpr, context.equation().expr()[0]);
            BodyExpr(ref RightExpr, context.equation().expr()[1]);

            string name = "";
            string type = "";
            Stack<string> expr = new Stack<string>();
            if (context.for_init() != null)
            {
                name = context.for_init().ID() == null ? "" : context.for_init().ID().GetText();
                type = context.for_init().TYPE() == null ? "" : context.for_init().TYPE().GetText();
                BodyExpr(ref expr, context.for_init().expr());
            }
            
            Stack<string> StackNewValue = new Stack<string>();
            if(context.changeValue() != null) BodyExpr(ref StackNewValue, context.changeValue().expr());

            List<object> forBody = new List<object>();

            BuildBody(ref forBody,context.for_body().stat());
            var StateWhile = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "for",
                                new Dictionary<string, object>()
                                {
                                    {
                                        "equation",
                                        new Dictionary<string,object>
                                        {
                                            { "left", LeftExpr },
                                            { "right", RightExpr },
                                            { "operator", context.equation().op.Text }
                                        }
                                    },
                                    {
                                        "init",
                                        new Dictionary<string, object>()
                                        {
                                            {"type", type},
                                            {"ID", name},
                                            {"expr", expr }
                                        }
                                    },
                                    {
                                        "changeValue",
                                        new Dictionary<string, object>()
                                        {
                                            {"ID", context.changeValue() == null ? null : context.changeValue().ID().GetText()},
                                            {"expr", StackNewValue},
                                        }
                                    },
                                    {
                                        "body",forBody
                                    }
                                }
                            },
                        }
                    }
                };

            if (node) Program.InitNode.Add(StateWhile);
            else return StateWhile;
            return null;
        }
        public override IParseTree VisitFor([NotNull] ExprParser.ForContext context)
        {
            BodyFor(context, true);
            return null;
        }
        public override IParseTree VisitDoWhile([NotNull] ExprParser.DoWhileContext context)
        {
            BodyDoWhile(context, true);
            return null;
        }
        public override IParseTree VisitChangeValue([NotNull] ExprParser.ChangeValueContext context)
        {
            ChangeValue(context, true);
            return null;
        }
        public override IParseTree VisitChildren([NotNull] IRuleNode node)
        {
            var result = DefaultResult;
            int n = node.ChildCount;
            for (int i = 0; i < n; i++)
            {
                if (!ShouldVisitNextChild(node, result))
                    return result;

                var child = node.GetChild(i);
                var childResult = child.Accept(this);
                result = AggregateResult(result, childResult);
            }
            return result;
        }
        public override IParseTree VisitIf([NotNull] ExprParser.IfContext context)
        {
            BodyIf(context, true);
            return null;
        }
        public override IParseTree VisitWhile([NotNull] ExprParser.WhileContext context)
        {
            BodyWhile(context, true);
            return null;
        }
        public override IParseTree VisitPrint ([NotNull] ExprParser.PrintContext context)
        {
          BodyPrint(context,true);
          return null;
        }
        public override IParseTree VisitInitialization([NotNull] ExprParser.InitializationContext context)
        {
            BodyInit(context,true);
            return null;
        }
        private Stack<string> _exprStack;
        public override IParseTree VisitExpr([NotNull] ExprParser.ExprContext context)
        {
            var expression = "";
            if (context.children.Count == 3 && context.children[0].ToString() != "(" ) expression = context.children[1].ToString();
            else
            {
                if (context.children[0].ToString() != "(") expression = context.GetText();
            }
            if (expression != "")
            {
                _exprStack.Push(expression);

            }
            return this.VisitChildren(context);
        }
        public override IParseTree VisitProg([NotNull] ExprParser.ProgContext context)
        {
            if (context.function() != null)
                BuildFunction(context.function());
            foreach (var item in context.stat())
            {
                if (item.print() != null)
                    BodyPrint(item.print(),true);
                else if (item.initialization() != null)
                    BodyInit(item.initialization(),true);
                else if (item.@if() != null)
                    BodyIf(item.@if(),true);
                else if (item.@while() != null)
                    BodyWhile(item.@while(),true);
                else if (item.changeValue() != null)
                    ChangeValue(item.changeValue(),true);
                else if (item.@for() != null)
                    BodyFor(item.@for(),true);
                else if (item.callFunc() != null)
                    CallFunc(item.callFunc(), true);
                else if (item.doWhile() != null)
                    BodyDoWhile(item.doWhile(), true);
            }

            return null;
        }
        public override IParseTree VisitStat([NotNull] ExprParser.StatContext context) { return this.VisitChildren(context); }
    }
}
