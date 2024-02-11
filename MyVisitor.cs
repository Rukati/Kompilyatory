using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Xml.Linq;


namespace Kompilyatory
{
    internal class MyVisitor : ExprBaseVisitor<IParseTree>
    {
        public override IParseTree VisitPrint ([NotNull] ExprParser.PrintContext context)
        {
            exprStack = new Stack<string>();

            //Console.WriteLine($"+-- Initialization (type: {type}, ID: {name})\n|\t|\n|\t+-- value  {value}");
            this.VisitChildren(context);

            Kompilyatory.Program.InitNode.Add(
                new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    { "state",
                        new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                "writeln",
                                new Dictionary<string, object>()
                                {
                                   // {"type", type},
                                    //{"ID", name},
                                    {"value", exprStack },
                                    //{"END", context.END().ToString()}
                                }
                            }
                        }
                    }
                }
           );
          return null;
        }
        public override IParseTree VisitInitialization([NotNull] ExprParser.InitializationContext context)
        {
            var name = context.ID() == null ? "" : context.ID().GetText();
            var type = context.TYPE() == null ? "" : context.TYPE().GetText();
            var value = context.expr() == null ? "" : context.expr().GetText();

            exprStack = new Stack<string>();

            //Console.WriteLine($"+-- Initialization (type: {type}, ID: {name})\n|\t|\n|\t+-- value  {value}");
            this.VisitChildren(context);
            
            Kompilyatory.Program.InitNode.Add(
                new Dictionary<string,Dictionary<string, Dictionary<string, object>>> (){
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
                                    {"END", context.END().ToString()}
                                }
                            }
                        } 
                    }
                }
             );

            return null;
        }
        //JsonConvert.SerializeObject(keys, Formatting.Indented
        static public Stack<string> exprStack;
        public override IParseTree VisitExpr([NotNull] ExprParser.ExprContext context)
        {
            //Console.WriteLine($"|\t\t|+-- expr --- ");
            /*foreach (var item in context.children)
            {
                Console.WriteLine(item);
            }*/
            //Console.WriteLine("---------------");
            var expression = "";
            if (context.children.Count == 3 && context.children[0].ToString() != "(" ) expression = context.children[1].ToString();
            else
            {
                if (context.children[0].ToString() != "(") expression = context.GetText();
            }
            if (expression != "")
            {
                exprStack.Push(expression);

            }
            return this.VisitChildren(context);
        }

        public override IParseTree VisitProg([NotNull] ExprParser.ProgContext context)
{
            //Console.WriteLine("Prog\n|");
            return this.VisitChildren(context);
        }

        public override IParseTree VisitStat([NotNull] ExprParser.StatContext context)
        {


            return this.VisitChildren(context);
        }
    }
}
