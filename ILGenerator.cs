using System;
using System.Reflection;
using System.Reflection.Emit;
using static Kompilyatory.Program;
using LLVMSharp;
using System.Runtime.Remoting.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.ComponentModel;
using Kompilyatory;
using System.Xml.Linq;
using Antlr4.Runtime.Dfa;
class LL
{
    private static LLVMContextRef context = LLVM.ContextCreate();
    private static LLVMBuilderRef builder = LLVM.CreateBuilderInContext(context);
    private static LLVMModuleRef module = LLVM.ModuleCreateWithName("RUKATICOMPILATOREZSYSHARP");

    static private List<string> CalculatingTheExpression(List<string> expr, string type = "int")
    {
        Type targetType = null;
        if (type == "int") targetType = typeof(int);
        else if (type == "float") targetType = typeof(float);
        else if (type == "bool") targetType = typeof(bool);
        else if (type == "string") targetType = typeof(string);
        
        if (expr.Count == 1)
        {
            expr[0] = expr[0].Replace(',', '.');
            if (int.TryParse(expr[0], out int k) ||( targetType == typeof(float) && float.TryParse(ContainsDecimal(expr[0]) ? expr[0] : expr[0] + ".0", NumberStyles.Float, CultureInfo.InvariantCulture, out float n)))
                return expr;
            else if (char.IsLetter(expr[0][0]))
                return ast.First(item => item.State.Initialization.ID == expr[0]).State.Initialization.EXPR;
        }

        if (expr.Count == 0)
            WriteWrong($"An uninitialized variable was used");

        int j;
        while (expr[2].Length >= 1 && Char.IsLetterOrDigit(char.Parse(expr[2][0].ToString())))
        {
            int k = 0;
            for (; ; k++)
            {
                if (expr[k].Length == 1)
                    if (!char.IsLetterOrDigit(expr[k][0])) break;
            }

            AST intervalExpr = new AST();
            intervalExpr.State = new state();
            intervalExpr.State.Initialization = new initialization();
            intervalExpr.State.Initialization.EXPR = new List<string>();
            intervalExpr.State.Initialization.EXPR.AddRange(expr.GetRange(k - 2, 3));
            intervalExpr.State.Initialization.TYPE = type;

            expr.RemoveRange(k - 2, 3);
            expr.Insert(k - 2, CalculatingTheExpression(intervalExpr.State.Initialization.EXPR,type)[0]);
        }

        string right = null;
        string left = null;

        AST tempAST = new AST();
        tempAST.State = new state();
        tempAST.State.Initialization = new initialization()
        {
            EXPR = new List<string>(),
            TYPE = type,
        };
        if (targetType == typeof(int))
        {
            int i;
            if (!int.TryParse(expr[0], out i))
            {
                try
                {
                    var RNAME = ast.Any(item => item.State.Initialization.ID == expr[0]) ? ast.First(item => item.State.Initialization.ID == expr[0]) : null;
                    if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                        WriteWrong($"The name \"  {expr[0]}  \" does not exist in the current context.");
                    right = RNAME != null ? CalculatingTheExpression(RNAME.State.Initialization.EXPR,type)[0] : expr[0];
                }
                catch (Exception ex) { WriteWrong($"The name \" {expr[0]} \" does not exist in the current context."); }
            }
            else right = expr[0];

            if (!int.TryParse(expr[1], out i))
            {
                try
                {
                    var LNAME = ast.Any(item => item.State.Initialization.ID == expr[1]) ? ast.First(item => item.State.Initialization.ID == expr[1]) : null;
                    if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                        WriteWrong($"The name \" {expr[1]} \" does not exist in the current context.");
                    left = LNAME != null ? CalculatingTheExpression(LNAME.State.Initialization.EXPR, type)[0] : expr[1];
                }
                catch (Exception ex) { WriteWrong($"The name \"{expr[1]}\" does not exist in the current context."); }
            }
            else left = expr[1];


            switch (char.Parse(expr[2]))
            {
                case '+':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) + int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '-':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) - int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '/':
                    try { tempAST.State.Initialization.EXPR.Add((int.Parse(left) / int.Parse(right)).ToString()); }
                    catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '*':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) * int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
            }
        }
        else if (targetType == typeof(float))
        {
            expr[0] = expr[0].Replace(',','.');
            float i;
            if (!float.TryParse(ContainsDecimal(expr[0]) ? expr[0] : expr[0] + ".0", NumberStyles.Float, CultureInfo.InvariantCulture, out i))
            {
                try {
                    var RNAME = ast.Any(item => item.State.Initialization.ID == expr[0]) ? ast.First(item => item.State.Initialization.ID == expr[0]) : null;
                    if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                        WriteWrong($"The name \" {expr[0]} \" does not exist in the current context.");
                    right = RNAME != null ? CalculatingTheExpression(RNAME.State.Initialization.EXPR, type)[0] : expr[0];
                } catch (Exception ex) { WriteWrong($"The name \"{expr[0]}\" does not exist in the current context."); }
            }
            else right = expr[0];

            expr[1] = expr[1].Replace(',', '.');
            if (!float.TryParse(ContainsDecimal(expr[1]) ? expr[1] : expr[1] + ".0", NumberStyles.Float, CultureInfo.InvariantCulture, out i))
            {
                try
                {
                    var LNAME = ast.Any(item => item.State.Initialization.ID == expr[1]) ? ast.First(item => item.State.Initialization.ID == expr[1]) : null;
                    if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                        WriteWrong($"The name \"  {expr[1]}  \" does not exist in the current context.");
                    left = LNAME != null ? CalculatingTheExpression(LNAME.State.Initialization.EXPR, type)[0] : expr[1];
                } catch (Exception ex) { WriteWrong($"The name \"{expr[1]}\" does not exist in the current context."); }
            }
            else left = expr[1];

            left = left.Replace(',', '.'); left = ContainsDecimal(left) ? left : left + ".0";
            right = right.Replace(',', '.'); right = ContainsDecimal(right) ? right : right + ".0";


            switch (char.Parse(expr[2]))
            {
                case '+':
                    tempAST.State.Initialization.EXPR.Add(((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) + float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture))).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '-':
                    tempAST.State.Initialization.EXPR.Add(((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) - float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture))).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '/':
                    try
                    {
                        tempAST.State.Initialization.EXPR.Add(((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) / float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture))).ToString());
                    }
                    catch (DivideByZeroException ex)
                    {
                        WriteWrong(ex.Message);
                    }
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '*':
                    tempAST.State.Initialization.EXPR.Add(((float)(float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) * float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture))).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
            }
        }

        return null;
    }
    static private bool ContainsDecimal(string str)
    {
        return str.Contains(".") ;
    }
    static public void Gen()
    {
        LLVM.InitializeX86TargetInfo();
        LLVM.InitializeX86Target();
        LLVM.InitializeX86TargetMC();
        LLVM.InitializeX86AsmParser();
        LLVM.InitializeX86AsmPrinter();

        var functionType = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[] { }, new LLVMBool(0));
        var mainFunction = LLVM.AddFunction(module, "main", functionType);
        var entry = LLVM.AppendBasicBlock(mainFunction, "entry");
        LLVM.PositionBuilderAtEnd(builder, entry);

        LLVMTypeRef retType = LLVM.FunctionType(LLVMTypeRef.Int32Type(), new LLVMTypeRef[] { LLVMTypeRef.PointerType(LLVMTypeRef.Int8Type(), 0), }, true);
        LLVM.AddFunction(module, "printf", retType);

        foreach (var item in ast)
        {

            if (item.State.Initialization != null) _InstructionInitialization(item.State.Initialization);
            else if (item.State.Writeln != null)
            {
                foreach (var valuePrint in item.State.Writeln.VALUE)
                {
                    if (valuePrint.Exists(x => x.Keys.First() == "expr"))
                    {
                        var exprValue = valuePrint.Find(x => x.Keys.First() == "expr");
                        _InstructionDisplay(CalculatingTheExpression(exprValue["expr"], exprValue["value"][0])[0], exprValue["value"][0]);
                    }
                    else _InstructionDisplay(valuePrint[0][valuePrint[0].Keys.First()][0], valuePrint[0].Keys.First());

                }
                /*if (item.State.Print.VALUE.Count >= 3)
                    _InstructionDisplay($"{CalculatingTheExpression(item.State.Print.VALUE., "print")[0]}");
                //else _InstructionDisplay(item.State.Print.VALUE[0]);*/
            }
            else if (item.State.iF != null) _InstructionIF(item.State.iF, entry);
            else if (item.State.whilE != null) _InstructionWHILE(item.State.whilE, entry);
            else if (item.State.changeValue != null) _InstructionChangeValue(item.State.changeValue);
        }

        // Возвращаем 0
        LLVM.BuildRet(builder, LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0)));

        // Выводим модуль в консоль
        LLVM.DumpModule(module);

        // Записываем модуль в файл .ll
        LLVM.WriteBitcodeToFile(module, "output.ll");

        // Освобождаем ресурсы
        LLVM.DisposeBuilder(builder);
        LLVM.DisposeModule(module);
        LLVM.ContextDispose(context);
    }
    static private Dictionary <string, LLVMValueRef> _valueLocaleVariable = new Dictionary<string, LLVMValueRef>();
    static private void _InstructionChangeValue(ChangeValue StateInfo)
    {
        if (!_valueLocaleVariable.ContainsKey(StateInfo.ID)) WriteWrong($"Unknown variable: {StateInfo.ID}");
        initialization variable = ast.First(x => x.State.Initialization.ID == StateInfo.ID).State.Initialization;
        LLVMTypeRef _lLVMType = default(LLVMTypeRef);
        switch (variable.TYPE)
        {
            case "int":
                _lLVMType = LLVMTypeRef.Int32Type();
                break;
            case "float":
                _lLVMType = LLVMTypeRef.FloatType();
                break;
            case "bool":
                _lLVMType = LLVMTypeRef.Int1Type();
                break;
            case "string":
                _lLVMType = LLVM.PointerType(LLVM.Int8Type(), 0);
                break;
        }

        var typeKind = LLVM.GetTypeKind(_lLVMType);

        switch (typeKind)
        {
            case LLVMTypeKind.LLVMIntegerTypeKind:

                long parsedValueInt = long.Parse(CalculatingTheExpression(StateInfo.expr)[0]);
                long differenceValue = parsedValueInt - long.Parse(variable.EXPR[0]);
                LLVMValueRef constInt;

                if (StateInfo.expr[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                    var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(differenceValue), new LLVMBool(0));

                    constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)differenceValue, new LLVMBool(0));
                var NowValueIntVariable = LLVM.BuildLoad(builder, _valueLocaleVariable[StateInfo.ID], $"{StateInfo.ID}_value");
                var changeValueInt = LLVM.BuildAdd(builder, constInt, NowValueIntVariable, "changeValue");
                LLVM.BuildStore(builder, changeValueInt, _valueLocaleVariable[StateInfo.ID]);
                ast.First(x => x.State.Initialization.ID == StateInfo.ID).State.Initialization.EXPR[0] = differenceValue.ToString();
                break;
            case LLVMTypeKind.LLVMFloatTypeKind:

                long parsedValueFloat = long.Parse(CalculatingTheExpression(StateInfo.expr)[0]);
                long differenceValueFloat = parsedValueFloat- long.Parse(variable.EXPR[0]);

                LLVMValueRef constFloat;

                if (CalculatingTheExpression(StateInfo.expr)[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstInt(LLVM.FloatType(), 0, new LLVMBool(0));
                    var absoluteValue = LLVM.ConstInt(LLVM.FloatType(), (ulong)Math.Abs(differenceValueFloat), new LLVMBool(0));

                    constFloat = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constFloat = LLVM.ConstInt(LLVM.FloatType(), (ulong)differenceValueFloat, new LLVMBool(0));
                var NowValueFloatVariable = LLVM.BuildLoad(builder, _valueLocaleVariable[StateInfo.ID], $"{StateInfo.ID}_value");
                var changeValue = LLVM.BuildAdd(builder, constFloat, NowValueFloatVariable, "changeValue");
                LLVM.BuildStore(builder, changeValue, _valueLocaleVariable[StateInfo.ID]);
                ast.First(x => x.State.Initialization.ID == StateInfo.ID).State.Initialization.EXPR[0] = differenceValueFloat.ToString();
                break;
            default:
                break;
        }
    }
    static private void _InstructionWHILE(WHILE stateWhile, LLVMBasicBlockRef entryBlock)
    {
        LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
        LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");
       
        LLVM.BuildBr(builder, conditionBlock);

        LLVM.PositionBuilderAtEnd(builder, conditionBlock);

        LLVMValueRef leftVariable = default(LLVMValueRef);
        LLVMValueRef rightVariable = default(LLVMValueRef);
        

        if (stateWhile.left.Count == 1)
        {
            LLVMValueRef constInt;
            if (_valueLocaleVariable.TryGetValue(stateWhile.left[0], out leftVariable))
                leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
            else
            {
                if (stateWhile.left[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                    var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(long.Parse(stateWhile.left[0])), new LLVMBool(0));

                    constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)long.Parse(stateWhile.left[0]), new LLVMBool(0));
                leftVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "LeftVariableInit");
                LLVM.BuildStore(builder, constInt, leftVariable);
                leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
            }
        }
        else
        {
            LLVMValueRef constInt;
            long parsedValueInt = long.Parse(CalculatingTheExpression(stateWhile.left)[0]);
            if (stateWhile.left[0][0] == '-')
            {
                var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(parsedValueInt), new LLVMBool(0));

                constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
            }
            else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)parsedValueInt, new LLVMBool(0));
            if (_valueLocaleVariable.TryGetValue(stateWhile.left[0], out leftVariable))
                leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
            else
            {
                leftVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "LeftVariableInit");
                LLVM.BuildStore(builder, constInt, leftVariable);
                leftVariable = LLVM.BuildLoad(builder, leftVariable, "LeftValueInCompare");
            }
        }
        if (stateWhile.right.Count == 1)
        {
            LLVMValueRef constInt;
            if (_valueLocaleVariable.TryGetValue(stateWhile.right[0], out rightVariable))
                rightVariable = LLVM.BuildLoad(builder, rightVariable, "RightValueInCompare");
            else
            {
                if (stateWhile.right[0][0] == '-')
                {
                    var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                    var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(long.Parse(stateWhile.right[0])), new LLVMBool(0));

                    constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                }
                else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)long.Parse(stateWhile.right[0]), new LLVMBool(0));
                rightVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "RighVariableInit");
                LLVM.BuildStore(builder, constInt, rightVariable);
                rightVariable = LLVM.BuildLoad(builder, rightVariable, "RightValueInCompare");
            }
        }
        else
        {
            LLVMValueRef constInt;
            long parsedValueInt = long.Parse(CalculatingTheExpression(stateWhile.right)[0]);
            if (stateWhile.right[0][0] == '-')
            {
                var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(parsedValueInt), new LLVMBool(0));

                constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
            }
            else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)parsedValueInt, new LLVMBool(0));
            if (_valueLocaleVariable.TryGetValue(stateWhile.right[0], out rightVariable))
                rightVariable = LLVM.BuildLoad(builder, rightVariable, "RifgtValueInCompare");
            else
            {
                rightVariable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), "RighVariableInit");
                LLVM.BuildStore(builder, constInt, rightVariable);
                rightVariable = LLVM.BuildLoad(builder, rightVariable, "RightValueInCompare");
            }
        }
        LLVMValueRef icmpVariable = default(LLVMValueRef);
        switch (stateWhile.Operator)
        {
            case "==":
                icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, leftVariable, rightVariable, "compare_eq");
                break;
            case "!=":
                icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, leftVariable, rightVariable, "compare_ne");
                break;
            case "<":
                icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLT, leftVariable, rightVariable, "compare_lt");
                break;
            case ">":
                icmpVariable = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGT, leftVariable, rightVariable, "compare_gt");
                break;
        }
        LLVM.BuildCondBr(builder, icmpVariable, loopBlock, endBlock);

        LLVM.PositionBuilderAtEnd(builder, loopBlock);
        foreach (AST astNode in stateWhile.body)
        {
            if (astNode.State.Initialization != null) _InstructionInitialization(astNode.State.Initialization);
            else if (astNode.State.Writeln != null)
            {
                foreach (var valuePrint in astNode.State.Writeln.VALUE)
                {
                    if (valuePrint.Exists(x => x.Keys.First() == "expr"))
                    {
                        var exprValue = valuePrint.Find(x => x.Keys.First() == "expr");
                        _InstructionDisplay(CalculatingTheExpression(exprValue["expr"], exprValue["value"][0])[0], exprValue["value"][0]);
                    }
                    else
                    {
                        _InstructionDisplay(valuePrint[0][valuePrint[0].Keys.First()][0], valuePrint[0].Keys.First());
                    }
                }
            }
            else if (astNode.State.whilE != null) _InstructionWHILE(astNode.State.whilE, entryBlock);
            else if (astNode.State.iF != null) _InstructionIF(astNode.State.iF, entryBlock);
            else if (astNode.State.changeValue != null) _InstructionChangeValue(astNode.State.changeValue);
        }
        LLVM.BuildBr(builder,conditionBlock);

        LLVM.PositionBuilderAtEnd(builder, endBlock);
    }
    static private void _InstructionIF(IF stateIf, LLVMBasicBlockRef entryBlock)
    {
        LLVMValueRef leftValue = LLVM.ConstInt(LLVM.Int32Type(), ulong.Parse(CalculatingTheExpression(stateIf.left)[0]), false);
        LLVMValueRef rightValue = LLVM.ConstInt(LLVM.Int32Type(), ulong.Parse(CalculatingTheExpression(stateIf.right)[0]), false);

        LLVMValueRef comparison = default(LLVMValueRef);

        switch (stateIf.Operator)
        {
            case "==":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue, "compare_eq");
                break;
            case "!=":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, leftValue, rightValue, "compare_ne");
                break;
            case "<":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSLT, leftValue, rightValue, "compare_lt");
                break;
            case ">":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntSGT, leftValue, rightValue, "compare_gt");
                break;
        }

        LLVMBasicBlockRef ifTrue = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "if_true");
        LLVMBasicBlockRef ifFalse = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "if_false");
        LLVMBasicBlockRef EndBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "endBlockIf");

        LLVM.BuildCondBr(builder, comparison, ifTrue, ifFalse);

        LLVM.PositionBuilderAtEnd(builder, ifTrue);
        foreach (AST astNode in stateIf.body)
        {
            if (astNode.State.Initialization != null) _InstructionInitialization(astNode.State.Initialization);
            else if (astNode.State.Writeln != null)
            {
                foreach (var valuePrint in astNode.State.Writeln.VALUE)
                {
                    if (valuePrint.Exists(x => x.Keys.First() == "expr"))
                    {
                        var exprValue = valuePrint.Find(x => x.Keys.First() == "expr");
                        _InstructionDisplay(CalculatingTheExpression(exprValue["expr"], exprValue["value"][0])[0], exprValue["value"][0]);
                    }
                    else
                    {
                        _InstructionDisplay(valuePrint[0][valuePrint[0].Keys.First()][0], valuePrint[0].Keys.First());
                    }
                }
            }
            else if (astNode.State.iF != null)
            {
                _InstructionIF(astNode.State.iF, ifTrue);
            }
        }
        LLVM.BuildBr(builder, EndBlock);
        
        LLVM.PositionBuilderAtEnd(builder, ifFalse);
        foreach (AST astNode in stateIf.Else["body"])
        {
            if (astNode.State.Initialization != null) _InstructionInitialization(astNode.State.Initialization);
            else if (astNode.State.Writeln != null)
            {
                foreach (var valuePrint in astNode.State.Writeln.VALUE)
                {
                    if (valuePrint.Exists(x => x.Keys.First() == "expr"))
                    {
                        var exprValue = valuePrint.Find(x => x.Keys.First() == "expr");
                        _InstructionDisplay(CalculatingTheExpression(exprValue["expr"], exprValue["value"][0])[0], exprValue["value"][0]);
                    }
                    else
                    {
                        _InstructionDisplay(valuePrint[0][valuePrint[0].Keys.First()][0], valuePrint[0].Keys.First());
                    }
                }
            }
            else if (astNode.State.iF != null)
            {
                _InstructionIF(astNode.State.iF, ifTrue);
            }
        }
        LLVM.BuildBr(builder, EndBlock);


        LLVM.PositionBuilderAtEnd(builder, EndBlock);
        //LLVM.BuildBr(builder, entryBlock);
        //LLVM.PositionBuilderAtEnd(builder, ifFalse);
        //_InstructionDisplay("False","line");
        //LLVM.BuildBr(builder, entryBlock);

        /* // Генерация кода для блока false
        LLVM.PositionBuilderAtEnd(builder, falseBlock);
        // Здесь можно добавить код, который будет выполняться в случае, если условие не выполнено
        */
    }
    static private void _InstructionInitialization(initialization InitValue)
    {
        if (InitValue.EXPR.Count >= 3)
        {
            InitValue.EXPR = CalculatingTheExpression(InitValue.EXPR,InitValue.TYPE);
        }

        LLVMTypeRef _lLVMType = default(LLVMTypeRef);
        switch (InitValue.TYPE)
        {
            case "int":
                _lLVMType = LLVMTypeRef.Int32Type();
                break;
            case "float":
                _lLVMType = LLVMTypeRef.FloatType();
                break;
            case "bool":
                _lLVMType = LLVMTypeRef.Int1Type();
                break;
            case "string":
                _lLVMType = LLVM.PointerType(LLVM.Int8Type(), 0);
                break;
        }

        var variable = LLVM.BuildAlloca(builder, _lLVMType, InitValue.ID);
        _valueLocaleVariable[InitValue.ID] = variable;
        if (InitValue.EXPR.Count == 1)
        {
            LLVMValueRef value = default(LLVMValueRef);
            if (InitValue.EXPR[0][0] == '-')
            {
                var positiveValue = LLVM.ConstInt(_lLVMType, ulong.Parse(CalculatingTheExpression(InitValue.EXPR)[0]), new LLVMBool(0));

                var zeroValue = LLVM.ConstInt(_lLVMType, 0, new LLVMBool(0));

                value = LLVM.BuildSub(builder, zeroValue, positiveValue, "negative_value");

            }
            else value = LLVM.ConstInt(_lLVMType, ulong.Parse(CalculatingTheExpression(InitValue.EXPR)[0]), new LLVMBool(0));
            LLVM.BuildStore(builder, value, variable);
        }
       
    }
    static private void _InstructionDisplay(string OutputString,string TypeInpuutStr)
    {
        LLVMValueRef formatString = default;
        LLVMValueRef[] args = default;

        if (TypeInpuutStr == "variable")
        {
            if (_valueLocaleVariable.TryGetValue(OutputString, out var variable) && variable.Pointer != IntPtr.Zero)
            {
                formatString = LLVM.BuildGlobalStringPtr(builder, "%d\n", $"Variable_{OutputString}_string");
                var variableValue = LLVM.BuildLoad(builder, variable, "Writeln_variable_value");
                args = new LLVMValueRef[] { formatString, variableValue };
            }
            else
            {
                if (_valueLocaleVariable.ContainsKey(OutputString)) WriteWrong($"Uninitialized variable: {OutputString}");
                else WriteWrong($"Unknown variable: {OutputString}");
            }
        }
        else
        {
            formatString = LLVM.BuildGlobalStringPtr(builder, $"{OutputString}\n", $"Writeln_value_{OutputString}_string");
            args = new LLVMValueRef[] { formatString };
        }
        var getPuts = LLVM.GetNamedFunction(module, "printf");
        LLVM.BuildCall(builder, getPuts, args, $"printf_call_{OutputString}");
    }
}