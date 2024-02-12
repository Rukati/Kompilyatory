﻿using System;
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
class LL
{
    private static LLVMContextRef context = LLVM.ContextCreate();
    private static LLVMBuilderRef builder = LLVM.CreateBuilderInContext(context);
    private static LLVMModuleRef module = LLVM.ModuleCreateWithName("RUKATICOMPILATOREZSYSHARP");

    static private List<string> CalculatingTheExpression(List<string> expr, string type = "int")
    { 
        if (expr.Count == 1 && char.IsDigit(expr[0][0])) return expr;
        if (expr.Count == 1 && !char.IsDigit(expr[0][0])) return ast.First(item => item.State.Initialization.ID == expr[0]).State.Initialization.EXPR;
        if (expr.Count == 0) WriteWrong($"An uninitialized variable was used");

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

        Type targetType = null;
        if (type == "int") targetType = typeof(int);
        else if (type == "float") targetType = typeof(float);
        else if (type == "bool") targetType = typeof(bool);
        else if (type == "string") targetType = typeof(string);

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
            float i;
            if (!float.TryParse(expr[0], NumberStyles.Float, CultureInfo.InvariantCulture, out i))
            {
                try {
                    var RNAME = ast.Any(item => item.State.Initialization.ID == expr[0]) ? ast.First(item => item.State.Initialization.ID == expr[0]) : null;
                    if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                        WriteWrong($"The name \" {expr[0]} \" does not exist in the current context.");
                    right = RNAME != null ? CalculatingTheExpression(RNAME.State.Initialization.EXPR, type)[0] : expr[0];
                } catch (Exception ex) { WriteWrong($"The name \"{expr[0]}\" does not exist in the current context."); }
            }
            else right = expr[0]; 

            if (!float.TryParse(expr[1], NumberStyles.Float, CultureInfo.InvariantCulture, out i))
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

            switch (char.Parse(expr[2]))
            {
                case '+':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) + float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '-':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) - float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '/':
                    try { tempAST.State.Initialization.EXPR.Add((float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) / float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture)).ToString()); }
                    catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
                case '*':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left, NumberStyles.Float, CultureInfo.InvariantCulture) * float.Parse(right, NumberStyles.Float, CultureInfo.InvariantCulture)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST.State.Initialization.EXPR, type);
            }
        }

        return null;
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

            if (item.State.Initialization != null)
            {
                LLVMTypeRef _lLVMType = default(LLVMTypeRef);
                switch (item.State.Initialization.TYPE)
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

                if (item.State.Initialization.EXPR.Count >= 3)
                {
                    item.State.Initialization.EXPR = CalculatingTheExpression(item.State.Initialization.EXPR);
                }
                if (item.State.Initialization.EXPR.Count == 1) _InstructionInitialization(_lLVMType, item.State.Initialization.ID, item.State.Initialization.EXPR[0]);
                else _InstructionInitialization(_lLVMType, item.State.Initialization.ID);
            }
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
        
    }
    static private void _InstructionWHILE(WHILE stateWhile, LLVMBasicBlockRef entryBlock)
    {
        LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
        LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");

       
        LLVM.BuildBr(builder, conditionBlock);

        LLVM.PositionBuilderAtEnd(builder, conditionBlock);

        LLVMValueRef leftValue = LLVM.ConstInt(LLVM.Int32Type(), ulong.Parse(CalculatingTheExpression(stateWhile.left)[0]), false);
        LLVMValueRef rightValue = LLVM.ConstInt(LLVM.Int32Type(), ulong.Parse(CalculatingTheExpression(stateWhile.right)[0]), false);
        LLVMValueRef comparison;

        switch (stateWhile.Operator)
        {
            case "==":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue, "compare_eq");
                break;
            case "!=":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, leftValue, rightValue, "compare_ne");
                break;
            default:
                throw new System.Exception("Unsupported comparison operator");
        }

        LLVM.BuildCondBr(builder, comparison, loopBlock, endBlock);


        LLVM.PositionBuilderAtEnd(builder, loopBlock);
        foreach (AST astNode in stateWhile.body)
        {
            if (astNode.State.Initialization != null)
            {
                LLVMTypeRef _lLVMType = default(LLVMTypeRef);
                switch (astNode.State.Initialization.TYPE)
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

                if (astNode.State.Initialization.EXPR.Count >= 3)
                {
                    astNode.State.Initialization.EXPR = CalculatingTheExpression(astNode.State.Initialization.EXPR);
                }
                if (astNode.State.Initialization.EXPR.Count == 1)
                {
                    _InstructionInitialization(_lLVMType, astNode.State.Initialization.ID, astNode.State.Initialization.EXPR[0]);
                }
                else
                {
                    _InstructionInitialization(_lLVMType, astNode.State.Initialization.ID);
                }
            }
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
            else if (astNode.State.iF != null) _InstructionIF(astNode.State.iF, loopBlock);
            else if (astNode.State.whilE != null) _InstructionWHILE (astNode.State.whilE, loopBlock);
        }
        LLVM.BuildBr(builder, conditionBlock);

        LLVM.PositionBuilderAtEnd(builder, endBlock);
    }
    static private void _InstructionIF(IF stateIf, LLVMBasicBlockRef entryBlock)
    {
        LLVMValueRef leftValue = LLVM.ConstInt(LLVM.Int32Type(), ulong.Parse(CalculatingTheExpression(stateIf.left)[0]), false);
        LLVMValueRef rightValue = LLVM.ConstInt(LLVM.Int32Type(), ulong.Parse(CalculatingTheExpression(stateIf.right)[0]), false);

        LLVMValueRef comparison;

        switch (stateIf.Operator)
        {
            case "==":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue, "compare_eq");
                break;
            case "!=":
                comparison = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntNE, leftValue, rightValue, "compare_ne");
                break;
            default:
                throw new System.Exception("Unsupported comparison operator");
        }
        LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);

        LLVMBasicBlockRef ifTrue = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(currentBlock), "if_true");
        LLVMBasicBlockRef ifFalse = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(currentBlock), "if_false");
        LLVMBasicBlockRef EndBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(currentBlock), "endBlockIf");

        LLVM.BuildCondBr(builder, comparison, ifTrue, ifFalse);

        LLVM.PositionBuilderAtEnd(builder, ifTrue);
        foreach (AST astNode in stateIf.body)
        {
            if (astNode.State.Initialization != null)
            {
                LLVMTypeRef _lLVMType = default(LLVMTypeRef);
                switch (astNode.State.Initialization.TYPE)
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

                if (astNode.State.Initialization.EXPR.Count >= 3)
                {
                    astNode.State.Initialization.EXPR = CalculatingTheExpression(astNode.State.Initialization.EXPR);
                }
                if (astNode.State.Initialization.EXPR.Count == 1)
                {
                    _InstructionInitialization(_lLVMType, astNode.State.Initialization.ID, astNode.State.Initialization.EXPR[0]);
                }
                else
                {
                    _InstructionInitialization(_lLVMType, astNode.State.Initialization.ID);
                }
            }
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
            if (astNode.State.Initialization != null)
            {
                LLVMTypeRef _lLVMType = default(LLVMTypeRef);
                switch (astNode.State.Initialization.TYPE)
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

                if (astNode.State.Initialization.EXPR.Count >= 3)
                {
                    astNode.State.Initialization.EXPR = CalculatingTheExpression(astNode.State.Initialization.EXPR);
                }
                if (astNode.State.Initialization.EXPR.Count == 1)
                {
                    _InstructionInitialization(_lLVMType, astNode.State.Initialization.ID, astNode.State.Initialization.EXPR[0]);
                }
                else
                {
                    _InstructionInitialization(_lLVMType, astNode.State.Initialization.ID);
                }
            }
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
    static private void _InstructionInitialization(LLVMTypeRef typeRef, string name, string value = "")
    {
        var variable = LLVM.BuildAlloca(builder, typeRef, name);
        _valueLocaleVariable.Add(name, default);
        if (!string.IsNullOrEmpty(value))
        {
            var typeKind = LLVM.GetTypeKind(typeRef);

            switch (typeKind)
            {
                case LLVMTypeKind.LLVMIntegerTypeKind:
                    
                    long parsedValue = long.Parse(value);
                    LLVMValueRef constant;

                    if (value[0] == '-')
                    {
                        var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                        var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(parsedValue), new LLVMBool(0));

                        constant = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else constant = LLVM.ConstInt(LLVM.Int32Type(), (ulong)parsedValue, new LLVMBool(0));

                    LLVM.BuildStore(builder, constant, variable);

                    _valueLocaleVariable[name] = variable;
                    break;
                default:
                    break;
            }
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
                var variableValue = LLVM.BuildLoad(builder, variable, "variable_value");
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
            formatString = LLVM.BuildGlobalStringPtr(builder, $"{OutputString}\n", $"Value_{OutputString}_string");
            args = new LLVMValueRef[] { formatString };
        }
        var getPuts = LLVM.GetNamedFunction(module, "printf");
        LLVM.BuildCall(builder, getPuts, args, $"printf_call_{OutputString}");
    }
}