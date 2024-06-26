//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6.6
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Z:\C#\Kompilyatory\bin\Debug\Expr.g4 by ANTLR 4.6.6

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Kompilyatory {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="ExprParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
[System.CLSCompliant(false)]
public interface IExprVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.prog"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProg([NotNull] ExprParser.ProgContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStat([NotNull] ExprParser.StatContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.callFunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCallFunc([NotNull] ExprParser.CallFuncContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.funcID"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncID([NotNull] ExprParser.FuncIDContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.argc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgc([NotNull] ExprParser.ArgcContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.arguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArguments([NotNull] ExprParser.ArgumentsContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.initialization"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInitialization([NotNull] ExprParser.InitializationContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.function"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunction([NotNull] ExprParser.FunctionContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter([NotNull] ExprParser.ParameterContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.funType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunType([NotNull] ExprParser.FunTypeContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.return"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturn([NotNull] ExprParser.ReturnContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.for"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFor([NotNull] ExprParser.ForContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.for_init"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFor_init([NotNull] ExprParser.For_initContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.for_body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFor_body([NotNull] ExprParser.For_bodyContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.if"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIf([NotNull] ExprParser.IfContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.ifBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfBody([NotNull] ExprParser.IfBodyContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.elseBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElseBody([NotNull] ExprParser.ElseBodyContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.doWhile"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDoWhile([NotNull] ExprParser.DoWhileContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.while"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhile([NotNull] ExprParser.WhileContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.whileBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileBody([NotNull] ExprParser.WhileBodyContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.changeValue"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitChangeValue([NotNull] ExprParser.ChangeValueContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.equation"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEquation([NotNull] ExprParser.EquationContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.print"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrint([NotNull] ExprParser.PrintContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr([NotNull] ExprParser.ExprContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="ExprParser.print_arguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrint_arguments([NotNull] ExprParser.Print_argumentsContext context);
}
} // namespace Kompilyatory
