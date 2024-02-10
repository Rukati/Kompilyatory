using System;
using System.Reflection;
using System.Reflection.Emit;
using static Kompilyatory.Program;
using LLVMSharp;
using System.Runtime.Remoting.Contexts;
using System.Collections.Generic;
using System.Linq;
class ILGenerator
{
    private static LLVMContextRef context = LLVM.ContextCreate();
    private static LLVMBuilderRef builder = LLVM.CreateBuilderInContext(context);
    private static LLVMModuleRef module = LLVM.ModuleCreateWithName("RUKATICOMPILATOREZSYSHARP");

    static private List<string> CalculatingTheValue(AST itemAST)
    {
        if (itemAST.State.Initialization.EXPR.Count == 1) return itemAST.State.Initialization.EXPR;
        if (itemAST.State.Initialization.EXPR.Count == 0) WriteWrong("An uninitialized variable was used");



        int j;
        while (Char.IsLetterOrDigit(char.Parse(itemAST.State.Initialization.EXPR[2])))
        {
            int k = 0;
            for (; ; k++)
            {
                if (itemAST.State.Initialization.EXPR[k].Length == 1)
                    if (!char.IsLetterOrDigit(itemAST.State.Initialization.EXPR[k][0])) break;
            }

            AST intervalExpr = new AST();
            intervalExpr.State = new state();
            intervalExpr.State.Initialization = new initialization();
            intervalExpr.State.Initialization.EXPR = new List<string>();
            intervalExpr.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.GetRange(k - 2, 3));
            intervalExpr.State.Initialization.TYPE = itemAST.State.Initialization.TYPE;

            itemAST.State.Initialization.EXPR.RemoveRange(k - 2, 3);
            itemAST.State.Initialization.EXPR.Insert(k - 2, CalculatingTheValue(intervalExpr)[0]);
        }

        Type targetType = null;
        if (itemAST.State.Initialization.TYPE == "int") targetType = typeof(int);
        else if (itemAST.State.Initialization.TYPE == "float") targetType = typeof(float);
        else if (itemAST.State.Initialization.TYPE == "bool") targetType = typeof(bool);
        else if (itemAST.State.Initialization.TYPE == "string") targetType = typeof(string);

        string right = null;
        string left = null;

        AST tempAST = new AST();
        tempAST.State = new state();
        tempAST.State.Initialization = new initialization()
        {
            EXPR = new List<string>(),
            TYPE = itemAST.State.Initialization.TYPE,
        };
        if (targetType == typeof(int))
        {
            int i;
            if (!int.TryParse(itemAST.State.Initialization.EXPR[0], out i))
            {
                var RNAME = ast.Any(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[0]) ? ast.First(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[0]) : null;
                if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                    WriteWrong($"The name {itemAST.State.Initialization.EXPR[0]} does not exist in the current context.");
                right = RNAME != null ? CalculatingTheValue(RNAME)[0] : itemAST.State.Initialization.EXPR[0];
            }
            else right = itemAST.State.Initialization.EXPR[0];

            if (!int.TryParse(itemAST.State.Initialization.EXPR[1], out i))
            {
                var LNAME = ast.Any(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[1]) ? ast.First(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[1]) : null;
                if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                    WriteWrong($"The name {itemAST.State.Initialization.EXPR[1]} does not exist in the current context.");
                left = LNAME != null ? CalculatingTheValue(LNAME)[0] : itemAST.State.Initialization.EXPR[1];
            }
            else left = itemAST.State.Initialization.EXPR[1];


            switch (char.Parse(itemAST.State.Initialization.EXPR[2]))
            {
                case '+':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) + int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
                case '-':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) - int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
                case '/':
                    try { tempAST.State.Initialization.EXPR.Add((int.Parse(left) / int.Parse(right)).ToString()); }
                    catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
                case '*':
                    tempAST.State.Initialization.EXPR.Add((int.Parse(left) * int.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
            }
        }
        else if (targetType == typeof(float))
        {
            float i;
            if (!float.TryParse(itemAST.State.Initialization.EXPR[0], out i))
            {
                var RNAME = ast.Any(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[0]) ? ast.First(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[0]) : null;
                if (i == 0 && RNAME == null) // i = 0 -> ExceptionParse | RNAME = null -> ExceptionFind
                    WriteWrong($"The name {itemAST.State.Initialization.EXPR[0]} does not exist in the current context.");
                right = RNAME != null ? CalculatingTheValue(RNAME)[0] : itemAST.State.Initialization.EXPR[0];
            }
            else right = itemAST.State.Initialization.EXPR[0]; ;

            if (!float.TryParse(itemAST.State.Initialization.EXPR[1], out i))
            {
                var LNAME = ast.Any(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[1]) ? ast.First(item => item.State.Initialization.ID == itemAST.State.Initialization.EXPR[1]) : null;
                if (i == 0 && LNAME == null) // i = 0 -> ExceptionParse | LNAME = null -> ExceptionFind
                    WriteWrong($"The name {itemAST.State.Initialization.EXPR[1]} does not exist in the current context.");
                left = LNAME != null ? CalculatingTheValue(LNAME)[0] : itemAST.State.Initialization.EXPR[1];
            }
            else left = itemAST.State.Initialization.EXPR[1];

            switch (char.Parse(itemAST.State.Initialization.EXPR[2]))
            {
                case '+':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left) + float.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
                case '-':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left) - float.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
                case '/':
                    try { tempAST.State.Initialization.EXPR.Add((float.Parse(left) / float.Parse(right)).ToString()); }
                    catch (DivideByZeroException ex) { WriteWrong(ex.Message); }
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
                case '*':
                    tempAST.State.Initialization.EXPR.Add((float.Parse(left) * float.Parse(right)).ToString());
                    tempAST.State.Initialization.EXPR.AddRange(itemAST.State.Initialization.EXPR.Skip(3));
                    return CalculatingTheValue(tempAST);
            }
        }

        return null;
    }

    static public void GenerateIL(initialization node)
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

        foreach (var item in ast)
        {
            var variable = LLVM.BuildAlloca(builder, LLVM.Int32Type(), $"{item.State.Initialization.ID}");
            /*var constant = LLVM.ConstInt(LLVM.Int32Type(), item., new LLVMBool(0));
            LLVM.BuildStore(builder, constant, variable);
*/
            if (item.State.Initialization != null)
                if (item.State.Initialization.EXPR.Count >= 3) {
                    item.State.Initialization.EXPR = CalculatingTheValue(item);
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

    static private void PrinfState(string OutputString)
    {
        var formatString = LLVM.BuildGlobalStringPtr(builder, OutputString, "format_string");

        LLVMTypeRef retType = LLVM.FunctionType(LLVMTypeRef.Int32Type(), new LLVMTypeRef[] { LLVMTypeRef.PointerType(LLVMTypeRef.Int8Type(), 0), }, true);
        LLVM.AddFunction(module, "printf", retType);

        var getPuts = LLVM.GetNamedFunction(module, "printf");


        var putsArgs = new LLVMValueRef[] { formatString };
        LLVM.BuildCall(builder, getPuts, putsArgs, "printf_call");
    }
}
