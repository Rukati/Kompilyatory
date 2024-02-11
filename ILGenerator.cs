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
class LL
{
    private static LLVMContextRef context = LLVM.ContextCreate();
    private static LLVMBuilderRef builder = LLVM.CreateBuilderInContext(context);
    private static LLVMModuleRef module = LLVM.ModuleCreateWithName("RUKATICOMPILATOREZSYSHARP");

    static private List<string> CalculatingTheExpression(AST itemAST, string state = "initialization")
    {
        string type = "";
        List<string> expr = new List<string>();

        switch (state)
        {
            case "print":
                type = "int";
                expr = itemAST.State.Print.VALUE;
                break;
            case "initialization":
                type = "int";
                expr = itemAST.State.Initialization.EXPR;
                break;
        }
        if (expr.Count == 1) return expr;
        if (expr.Count == 0) WriteWrong($"An uninitialized variable was used: {itemAST.State.Initialization.ID}");



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
            expr.Insert(k - 2, CalculatingTheExpression(intervalExpr)[0]);
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
                    right = RNAME != null ? CalculatingTheExpression(RNAME)[0] : expr[0];
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
                    left = LNAME != null ? CalculatingTheExpression(LNAME)[0] : expr[1];
                }
                catch (Exception ex) { WriteWrong($"The name \"{expr[1]}\" does not exist in the current context."); }
            }
            else left = expr[1];


            switch (char.Parse(expr[2]))
            {
                case '+':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) + int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
                case '-':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) - int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
                case '/':
                    try { tempAST.State.Initialization.EXPR.Add((int.Parse(left) / int.Parse(right)).ToString()); }
                    catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
                case '*':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) * int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
            }
        }
        else if (targetType == typeof(float))
        {
            float i;
            if (!float.TryParse(expr[0], out i))
            {
                try {
                    var RNAME = ast.Any(item => item.State.Initialization.ID == expr[0]) ? ast.First(item => item.State.Initialization.ID == expr[0]) : null;
                    if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                        WriteWrong($"The name \" {expr[0]} \" does not exist in the current context.");
                    right = RNAME != null ? CalculatingTheExpression(RNAME)[0] : expr[0];
                } catch (Exception ex) { WriteWrong($"The name \"{expr[0]}\" does not exist in the current context."); }
            }
            else right = expr[0]; ;

            if (!float.TryParse(expr[1], out i))
            {
                try
                {
                    var LNAME = ast.Any(item => item.State.Initialization.ID == expr[1]) ? ast.First(item => item.State.Initialization.ID == expr[1]) : null;
                    if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                        WriteWrong($"The name \"  {expr[1]}  \" does not exist in the current context.");
                    left = LNAME != null ? CalculatingTheExpression(LNAME)[0] : expr[1];
                } catch (Exception ex) { WriteWrong($"The name \"{expr[1]}\" does not exist in the current context."); }
            }
            else left = expr[1];

            switch (char.Parse(expr[2]))
            {
                case '+':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left) + float.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
                case '-':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left) - float.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
                case '/':
                    try { tempAST.State.Initialization.EXPR.Add((float.Parse(left) / float.Parse(right)).ToString()); }
                    catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
                case '*':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left) * float.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(expr.Skip(3));
                    return CalculatingTheExpression(tempAST);
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
            /*var constant = LLVM.ConstInt(LLVM.Int32Type(), item., new LLVMBool(0));
            LLVM.BuildStore(builder, constant, variable);
*/
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
                    item.State.Initialization.EXPR = CalculatingTheExpression(item);
                }
                if (item.State.Initialization.EXPR.Count == 1) _InstructionInitialization(_lLVMType, item.State.Initialization.ID, item.State.Initialization.EXPR[0]);
                else _InstructionInitialization(_lLVMType, item.State.Initialization.ID);
            }
            else if (item.State.Print != null)
            {
                if (item.State.Print.VALUE.Count >= 3)
                    _InstructionDisplay($"{CalculatingTheExpression(item, "print")[0]}");
                else _InstructionDisplay(item.State.Print.VALUE[0]);
            }




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
    static private void _InstructionInitialization(LLVMTypeRef typeRef, string name, string value = "")
    {
        var variable = LLVM.BuildAlloca(builder, typeRef, name);
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

                    // Сохранение значения в переменной
                    LLVM.BuildStore(builder, constant, variable);

                    _valueLocaleVariable.Add(name, variable);
                    break;
                default:
                    break;
            }
        }
    }
    static private void _InstructionDisplay(string OutputString)
    {
        LLVMValueRef formatString = default;
        LLVMValueRef[] args = default;

        if (!_valueLocaleVariable.TryGetValue(OutputString,out var variable))
        {
            formatString = LLVM.BuildGlobalStringPtr(builder, $"{OutputString}\n", "format_string");
            args = new LLVMValueRef[] { formatString };
        }
        else
        {
           // variableValue = LLVM.BuildLoad(builder, variable, "variable_value");
            formatString = LLVM.BuildGlobalStringPtr(builder, "%d\n", "format_string");
            var variableValue = LLVM.BuildLoad(builder, variable, "variable_value");
            args = new LLVMValueRef[] { formatString, variableValue };
        }

        var getPuts = LLVM.GetNamedFunction(module, "printf");
        LLVM.BuildCall(builder, getPuts, args, "printf_call");
    }
}