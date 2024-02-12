using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Xml.Linq;


namespace Kompilyatory
{
    internal class MyVisitor : ExprBaseVisitor<IParseTree>
    {
        private object BodyWhile([NotNull] ExprParser.WhileContext context, bool Node = false)
        {
            List<object> whileBody = new List<object>();
            Stack<string> LeftExpr = new Stack<string>();
            Stack<string> RightExpr = new Stack<string>();
            foreach (var item in context.whileBody().stat())
            {
                if (item.print() != null)
                    whileBody.Add(BodyPrint(item.print()));
                else if (item.initialization() != null)
                    whileBody.Add(BodyInit(item.initialization()));
                else if (item.@if() != null)
                    whileBody.Add(BodyIf(item.@if()));
                else if (item.@while() != null)
                    whileBody.Add(BodyWhile(item.@while()));
                else if (item.changeValue() != null)
                    whileBody.Add(NewValue(item.changeValue()));
            }

            BodyExpr(ref LeftExpr, context.equation().expr()[0]);
            BodyExpr(ref RightExpr, context.equation().expr()[1]);

            var StateWhile = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "while",
                                new Dictionary<string, object>()
                                {
                                    { "left", LeftExpr },
                                    { "right", RightExpr },
                                    { "operator", context.equation().op.GetText() },
                                    { "body", whileBody },
                                }
                            },
                        }
                    }
                };

            if (Node) Program.InitNode.Add(StateWhile);
            else return StateWhile;
            return null;
        }
        private object BodyIf([NotNull] ExprParser.IfContext context, bool Node = false)
        {
            List<object> ifBody = new List<object>();
            List<object> elseBody = new List<object>();
            Stack<string> LeftExpr = new Stack<string>();
            Stack<string> RightExpr = new Stack<string>();
          
            foreach (var item in context.ifBody().stat())
            {
                if (item.print() != null)
                    ifBody.Add(BodyPrint(item.print()));
                else if (item.initialization() != null)
                    ifBody.Add(BodyInit(item.initialization()));
                else if (item.@if() != null)
                    ifBody.Add(BodyIf(item.@if()));
                else if (item.changeValue() != null)
                    ifBody.Add(NewValue(item.changeValue()));
            }
            if (context.elseBody() != null)
            {
                foreach (var item in context.elseBody().stat())
                {
                    if (item.print() != null)
                        elseBody.Add(BodyPrint(item.print()));
                    else if (item.initialization() != null)
                        elseBody.Add(BodyInit(item.initialization()));
                    else if (item.@if() != null)
                        elseBody.Add(BodyIf(item.@if()));
                    else if (item.changeValue() != null)
                        elseBody.Add(NewValue(item.changeValue()));
                }
            }
            BodyExpr(ref LeftExpr, context.equation().expr()[0]);
            BodyExpr(ref RightExpr, context.equation().expr()[1]);

            var StateIfElse = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "if",
                                new Dictionary<string, object>()
                                {
                                    { "left", LeftExpr },
                                    { "right", RightExpr },
                                    { "operator", context.equation().op.GetText() },
                                    { "body", ifBody },
                                    { "else",
                                        new Dictionary<string,object>()
                                        {
                                            { "body", elseBody}
                                        }
                                    }
                                }
                            },
                        }
                    }
                };

            if (Node) Program.InitNode.Add(StateIfElse);
            else return StateIfElse;
            return null;
        }
        private object BodyInit([NotNull] ExprParser.InitializationContext context, bool Node = false)
        {
            var name = context.ID() == null ? "" : context.ID().GetText();
            var type = context.TYPE() == null ? "" : context.TYPE().GetText();

            exprStack = new Stack<string>();

            this.VisitChildren(context);
            var StateInit = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "initialization",
                                new Dictionary<string, object>()
                                {
                                    {"type", type},
                                    {"ID", name},
                                    {"expr", exprStack },
                                    //{"END", context.END().ToString()}
                                }
                            }
                        }
                    }
                };
            if (Node)
                Program.InitNode.Add(StateInit);
            else
            {
                return StateInit;
            }

            return null;
        }
        private object BodyPrint([NotNull] ExprParser.PrintContext context, bool Node = false)
        {
            var StateWriteln = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "writeln",
                                new Dictionary<string, object>()
                                {
                                    {"value" , null}
                                }
                            }
                        }
                    }
                };

            List<object> op = new List<object>();
            foreach (var item in context.print_arguments())
            {
                if (item.expr() != null)
                {
                    exprStack = new Stack<string>();
                    this.VisitExpr(item.expr());
                    op.Add(new List<Dictionary<string, object>>() {
                        new Dictionary<string, object> {
                            { "expr", exprStack },
                            { "value", new List<string>(){ item.TYPE().GetText() } }
                        }
                    });
                }
                else if (item.NUMBER() != null) op.Add(new List<Dictionary<string, object>>() {
                    new Dictionary<string, object>{
                        {
                            "numeric", new List<string>() { item.NUMBER().GetText() }
                        }
                    }
                });
                else if (item.LINE() != null) op.Add(new List<Dictionary<string, object>>() {
                    new Dictionary<string, object>{
                        {
                            "line", new List<string>() { item.LINE().GetText().Trim('"') }
                        }
                    }
                });
                else if (item.ID() != null) op.Add(new List<Dictionary<string, object>>() {
                    new Dictionary<string, object>{
                        {
                            "variable", new List<string>() { item.ID().GetText().Trim('$') }
                        }
                    }
                });
            }
            StateWriteln["state"]["writeln"]["value"] = op;

            if (Node) Program.InitNode.Add(StateWriteln);
            else
            {
                return StateWriteln;
            }
            return null;
        }
        private void BodyExpr(ref Stack<string> Expr, [NotNull] ExprParser.ExprContext context)
        {
            exprStack = new Stack<string>();
            VisitExpr(context);
            Expr = exprStack;
        }
        private object NewValue([NotNull] ExprParser.ChangeValueContext context, bool Node = false)
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
                                    { "type", context.TYPE() == null ? null : context.TYPE().GetText() },
                                    {"expr", StackNewValue },
                                    //{"END", context.END().ToString()}
                                }
                            }
                        }
                    }
                };
            if (Node)
                Program.InitNode.Add(StateNewValue);
            else
            {
                return StateNewValue;
            }
            return null;
        }
        public override IParseTree VisitChangeValue([NotNull] ExprParser.ChangeValueContext context)
        {
            NewValue(context, true);
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
        //JsonConvert.SerializeObject(keys, Formatting.Indented
        static public Stack<string> exprStack;
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
                if (expression[0] == '$') expression = expression.Trim('$');
                exprStack.Push(expression);

            }
            return this.VisitChildren(context);
        }
        public override IParseTree VisitProg([NotNull] ExprParser.ProgContext context){ return this.VisitChildren(context); }
        public override IParseTree VisitStat([NotNull] ExprParser.StatContext context) { return this.VisitChildren(context); }
    }
}
