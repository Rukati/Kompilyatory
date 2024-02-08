using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Kompilyatory
{
    internal class MyVisitor : ExprBaseVisitor<IParseTree>
    {
        static public dynamic InitNode = new List<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>();
        public override IParseTree VisitInitialization([NotNull] ExprParser.InitializationContext context)
        {
            var name = context.ID() == null ? "" : context.ID().GetText();
            var type = context.TYPE() == null ? "" : context.TYPE().GetText();
            var value = context.expr() == null ? "" : context.expr().GetText();

            InitNode.Add(
            new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
                 {
                    {
                        "stat", // Ключ "stat"
                        new Dictionary<string, Dictionary<string, string>>()
                        {
                            {
                                "Initialization",
                                new Dictionary<string, string>()
                                {
                                    {"type", type},
                                    {"ID", name},
                                    {"expr", value },
                                    {"END", context.END().ToString()}
                                }
                            }
                        }
                    }
                 }
             );

            Console.WriteLine($"+-- Initialization (type: {type}, ID: {name})\n|\t|\n|\t+-- value ({value})");
            return this.VisitChildren(context);
        }
        //JsonConvert.SerializeObject(keys, Formatting.Indented
        public override IParseTree VisitExpr([NotNull] ExprParser.ExprContext context)
        {
            var expression = context.children.Count == 3 ? context.children[1].ToString() : context.GetText();
            Console.WriteLine($"|\t\t|+-- expr ---  {expression}");



            return this.VisitChildren(context);
        }

        public override IParseTree VisitProg([NotNull] ExprParser.ProgContext context)
{
            Console.WriteLine("Prog\n|");
            return this.VisitChildren(context);
        }

        public override IParseTree VisitStat([NotNull] ExprParser.StatContext context)
        {


            return this.VisitChildren(context);
        }
    }
}
