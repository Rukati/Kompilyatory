using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;

namespace Kompilyatory
{
    internal class MyVisitor : ExprBaseVisitor<IParseTree>
    {
        public override IParseTree VisitInitialization([NotNull] ExprParser.InitializationContext context)
        {
            var name = context.ID().GetText();
            var type = context.TYPE().GetText();
            var value = context.NUMBER() == null ? null : context.NUMBER().GetText();

            Console.WriteLine($"+-- Announcement (type: {type}, ID: {name})\n|\t|\n|\t+-- value ({value})");
            return this.VisitChildren(context);
        }

    /*    public override IParseTree ([NotNull] ExprParser.InitializationContext context)
        {
            Console.WriteLine($"|\t\t|+-- type--- {context.GetText()}");
            return this.VisitChildren(context);
        }*/

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
