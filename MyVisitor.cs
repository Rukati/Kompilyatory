﻿using Antlr4.Runtime;
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
            }
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
            if (context.elseBody() != null)
                BuildBody(ref elseBody, context.elseBody().stat());

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
                                            { "body", elseBody}
                                        }
                                    }
                                }
                            },
                        }
                    }
                };

            if (node) Program.InitNode.Add(stateIfElse);
            else return stateIfElse;
            return null;
        }
        private object BodyInit([NotNull] ExprParser.InitializationContext context, bool node = false)
        {
            var name = context.ID() == null ? "" : context.ID().GetText();
            var type = context.TYPE() == null ? "" : context.TYPE().GetText();

            _exprStack = new Stack<string>();

            this.VisitChildren(context);
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
                                    {"expr", _exprStack },
                                    //{"END", context.END().ToString()}
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
        private readonly List<Dictionary<string, object>> _args = new List<Dictionary<string, object>>();
        private object BodyInit([NotNull] ExprParser.ParameterContext[] context) 
        {
            foreach (var item in context)
            {
             
                var name = item.ID() == null ? "" : item.ID().GetText();
                var type = item.paramType() == null ? "" : item.paramType().GetText();
                _exprStack = new Stack<string>();

                if (item.expr() != null) BodyExpr(ref _exprStack, item.expr());

                _args.Add(new Dictionary<string, object>()
                {
                    {"type",type},
                    {"ID",name},
                    {"expr", _exprStack},
                });
                if (item.parameter() != null) BodyInit(item.parameter());
            }
            return _args;
        }
        private object BodyPrint([NotNull] ExprParser.PrintContext context, bool node = false)
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
                    _exprStack = new Stack<string>();
                    this.VisitExpr(item.expr());
                    op.Add(new List<Dictionary<string, object>>() {
                        new Dictionary<string, object> {
                            { "expr", _exprStack },
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

            if (node) Program.InitNode.Add(StateWriteln);
            else
            {
                return StateWriteln;
            }
            return null;
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
            List<object> DoWhileBody = new List<object>();
            Stack<string> LeftExpr = new Stack<string>();
            Stack<string> RightExpr = new Stack<string>();
            
            BuildBody(ref DoWhileBody, context.whileBody().stat());

            BodyExpr(ref LeftExpr, context.equation().expr()[0]);
            BodyExpr(ref RightExpr, context.equation().expr()[1]);

            var StateDoWhile = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>(){
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "DoWhile",
                                new Dictionary<string, object>()
                                {
                                    { "left", LeftExpr },
                                    { "right", RightExpr },
                                    { "operator", context.equation().op.Text },
                                    { "body", DoWhileBody },
                                }
                            },
                        }
                    }
                };

            if (node) Program.InitNode.Add(StateDoWhile);
            else return StateDoWhile;
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
                if (expression[0] == '$') expression = expression.Trim('$');
                _exprStack.Push(expression);

            }
            return this.VisitChildren(context);
        }
        private void BuildFunction([NotNull] ExprParser.FunctionContext[] context)
        {
            foreach (var item in context)
            {
                List<object> body = new List<object>();
                BuildBody(ref body,item.stat());   
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
                                    {"args", item.parameter() != null? BodyInit(item.parameter()) : null},
                                    {"type", item.funType().GetText()},
                                    {"body", body},
                                }
                            }
                        }
                    }
                };
                Program.InitNode.Add(stateFunction);
            }
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
            }

            return null;
        }
        public override IParseTree VisitStat([NotNull] ExprParser.StatContext context) { return this.VisitChildren(context); }
    }
}
