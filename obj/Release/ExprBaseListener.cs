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
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="IExprListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6.6")]
[System.CLSCompliant(false)]
public partial class ExprBaseListener : IExprListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.prog"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterProg([NotNull] ExprParser.ProgContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.prog"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitProg([NotNull] ExprParser.ProgContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStat([NotNull] ExprParser.StatContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.stat"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStat([NotNull] ExprParser.StatContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.callFunc"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCallFunc([NotNull] ExprParser.CallFuncContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.callFunc"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCallFunc([NotNull] ExprParser.CallFuncContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.funcID"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFuncID([NotNull] ExprParser.FuncIDContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.funcID"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFuncID([NotNull] ExprParser.FuncIDContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.argc"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArgc([NotNull] ExprParser.ArgcContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.argc"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArgc([NotNull] ExprParser.ArgcContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.arguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArguments([NotNull] ExprParser.ArgumentsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.arguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArguments([NotNull] ExprParser.ArgumentsContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.initialization"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInitialization([NotNull] ExprParser.InitializationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.initialization"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInitialization([NotNull] ExprParser.InitializationContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.function"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFunction([NotNull] ExprParser.FunctionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.function"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFunction([NotNull] ExprParser.FunctionContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.parameter"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterParameter([NotNull] ExprParser.ParameterContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.parameter"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitParameter([NotNull] ExprParser.ParameterContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.funType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFunType([NotNull] ExprParser.FunTypeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.funType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFunType([NotNull] ExprParser.FunTypeContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.return"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterReturn([NotNull] ExprParser.ReturnContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.return"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitReturn([NotNull] ExprParser.ReturnContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.for"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFor([NotNull] ExprParser.ForContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.for"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFor([NotNull] ExprParser.ForContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.for_init"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFor_init([NotNull] ExprParser.For_initContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.for_init"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFor_init([NotNull] ExprParser.For_initContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.for_body"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFor_body([NotNull] ExprParser.For_bodyContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.for_body"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFor_body([NotNull] ExprParser.For_bodyContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.if"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIf([NotNull] ExprParser.IfContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.if"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIf([NotNull] ExprParser.IfContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.ifBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIfBody([NotNull] ExprParser.IfBodyContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.ifBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIfBody([NotNull] ExprParser.IfBodyContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.elseBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterElseBody([NotNull] ExprParser.ElseBodyContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.elseBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitElseBody([NotNull] ExprParser.ElseBodyContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.doWhile"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDoWhile([NotNull] ExprParser.DoWhileContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.doWhile"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDoWhile([NotNull] ExprParser.DoWhileContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.while"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterWhile([NotNull] ExprParser.WhileContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.while"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitWhile([NotNull] ExprParser.WhileContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.whileBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterWhileBody([NotNull] ExprParser.WhileBodyContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.whileBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitWhileBody([NotNull] ExprParser.WhileBodyContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.changeValue"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterChangeValue([NotNull] ExprParser.ChangeValueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.changeValue"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitChangeValue([NotNull] ExprParser.ChangeValueContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.equation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEquation([NotNull] ExprParser.EquationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.equation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEquation([NotNull] ExprParser.EquationContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.print"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPrint([NotNull] ExprParser.PrintContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.print"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPrint([NotNull] ExprParser.PrintContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpr([NotNull] ExprParser.ExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpr([NotNull] ExprParser.ExprContext context) { }

	/// <summary>
	/// Enter a parse tree produced by <see cref="ExprParser.print_arguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPrint_arguments([NotNull] ExprParser.Print_argumentsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ExprParser.print_arguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPrint_arguments([NotNull] ExprParser.Print_argumentsContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace Kompilyatory
