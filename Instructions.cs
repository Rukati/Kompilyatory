using System;
using static Kompilyatory.Program;
using LLVMSharp;
using System.Collections.Generic;
using System.Linq;
using static Kompilyatory.LL;


namespace Kompilyatory
{
    public class Instructions
    {
        public static void callFunction(callFunction function, ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> local)
        {
            var getPuts = LLVM.GetNamedFunction(module, $"{function.ID}");
            if (getPuts.Pointer == IntPtr.Zero) WriteWrong($"Function \"{function.ID}\" does not exist");

            LLVMValueRef[] args = new LLVMValueRef[function.argc.Count];
            int index = 0;
            
            var insertBlock = LLVM.GetInsertBlock(builder);
            foreach (var argument in function.argc)
            {
                if (argument[0] == '$')
                {
                    args[index] = LLVM.BuildLoad(builder, FindBlockWithVariable(argument.Substring(1), insertBlock,ref local).Item2,$"{argument.Substring(1)}_args");
                }
                else
                {
                    args[index] = LLVM.ConstReal(LLVM.FloatType(), float.Parse(argument));
                }
                index++;
            }
            
            LLVM.BuildCall(builder, getPuts, args, "");
        }
        public static void buildFunction(function function, LLVMBasicBlockRef previousBlock)
        {
            LLVMTypeRef[] llvmTypeRefs = new LLVMTypeRef[function.args.Count];
            int index = 0;
            foreach (var variable in function.args)
            {
                switch (variable.TYPE)
                {
                    case "int":
                        llvmTypeRefs[index] = LLVMTypeRef.Int32Type();
                        break;
                    case "float":
                        llvmTypeRefs[index] = LLVMTypeRef.FloatType();
                        break;
                    default:
                        WriteWrong($"Incorrect data type \"{variable.TYPE}\"\n in the function \"{function.ID}\"");
                        break;
                }
                index++;
            }

            LLVMTypeRef ReturnType = default(LLVMTypeRef);
            switch (function.type)
            {
                case "void":
                    ReturnType = LLVM.VoidType();
                    break;
                case "int":
                    ReturnType = LLVM.Int32Type();
                    break;
                case "float":
                    ReturnType = LLVM.FloatType();
                    break;
                default:
                    WriteWrong($"Incorrect return type \"{function.type}\" in function \"{function.ID}\"");
                    break;
            }

            var functionType = LLVM.FunctionType(ReturnType, llvmTypeRefs, false);
            var Function = LLVM.AddFunction(module, function.ID, functionType);
            var entry = LLVM.AppendBasicBlock(Function, function.ID);
            LLVM.PositionBuilderAtEnd(builder, entry);
            
            var newLocalFunction = new Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>>()
            {
                { LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>() }
            };
            foreach (var item in function.args)
            {
                Instructions.Initialization(item,ref newLocalFunction);
            }
            foreach (var item in function.body) item.HandlingStatus(entry, ref newLocalFunction);
            LLVM.BuildRet(builder,LLVM.ConstInt(LLVMTypeRef.Int32Type(),10,false));
            LLVM.PositionBuilderAtEnd(builder, previousBlock);
        }
        public static void changeValue(ChangeValue StateInfo,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            var blockWithVariable = FindBlockWithVariable(StateInfo.ID, insertBlock, ref _valueLocaleVariable).Item1;

            if (blockWithVariable.Equals(default(LLVMBasicBlockRef))) WriteWrong($"Unknown variable: {StateInfo.ID}");

            initialization variable = _valueLocaleVariable[blockWithVariable]
                .FirstOrDefault(item => item.ContainsKey(StateInfo.ID))[StateInfo.ID];
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

                    long parsedValueInt = long.Parse(CalculatingTheExpression(StateInfo.expr,ref _valueLocaleVariable)[0]);
                    long differenceValue = parsedValueInt - long.Parse(variable.EXPR[0]);
                    LLVMValueRef constInt;

                    if (StateInfo.expr[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                        var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(differenceValue), new LLVMBool(0));

                        constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)differenceValue, new LLVMBool(0));
                    var NowValueIntVariable = LLVM.BuildLoad(builder, variable.ValueRef, $"{StateInfo.ID}_value");
                    var changeValueInt = LLVM.BuildAdd(builder, constInt, NowValueIntVariable, "changeValue");
                    LLVM.BuildStore(builder, changeValueInt, variable.ValueRef);
                    variable.EXPR[0] = (long.Parse(variable.EXPR[0]) + differenceValue).ToString();
                    break;
                case LLVMTypeKind.LLVMFloatTypeKind:

                    long parsedValueFloat = long.Parse(CalculatingTheExpression(StateInfo.expr,ref _valueLocaleVariable)[0]);
                    long differenceValueFloat = parsedValueFloat - long.Parse(variable.EXPR[0]);

                    LLVMValueRef constFloat;

                    if (CalculatingTheExpression(StateInfo.expr,ref _valueLocaleVariable)[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstInt(LLVM.FloatType(), 0, new LLVMBool(0));
                        var absoluteValue = LLVM.ConstInt(LLVM.FloatType(), (ulong)Math.Abs(differenceValueFloat), new LLVMBool(0));

                        constFloat = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else constFloat = LLVM.ConstInt(LLVM.FloatType(), (ulong)differenceValueFloat, new LLVMBool(0));
                    var NowValueFloatVariable = LLVM.BuildLoad(builder, variable.ValueRef, $"{StateInfo.ID}_value");
                    var changeValue = LLVM.BuildAdd(builder, constFloat, NowValueFloatVariable, "changeValue");
                    LLVM.BuildStore(builder, changeValue, variable.ValueRef);
                    variable.EXPR[0] = (float.Parse(variable.EXPR[0]) + differenceValueFloat).ToString();
                    break;
                default:
                    break;
            }
        }
        public static void _while(WHILE stateWhile, 
            LLVMBasicBlockRef entryBlock, 
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");

            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, conditionBlock);

            LLVM.BuildCondBr(builder, _BuildEquation(stateWhile.left, stateWhile.right, stateWhile.Operator, ref _valueLocaleVariable), loopBlock, endBlock);

            LLVM.PositionBuilderAtEnd(builder, loopBlock);
            _valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>());
            foreach (AST astNode in stateWhile.body) astNode.HandlingStatus(entryBlock,ref _valueLocaleVariable);
            _valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock);
        }
        public static void _if(IF stateIf,
            LLVMBasicBlockRef entryBlock,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            LLVMBasicBlockRef ifTrue = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "if_true");
            LLVMBasicBlockRef ifFalse = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "if_false");
            LLVMBasicBlockRef EndBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "endBlockIf");

            LLVM.BuildCondBr(builder, _BuildEquation(stateIf.left,stateIf.right,stateIf.Operator,ref _valueLocaleVariable), ifTrue, ifFalse);

            // IF
            LLVM.PositionBuilderAtEnd(builder, ifTrue);
            _valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>());
            foreach (AST astNode in stateIf.body) astNode.HandlingStatus(entryBlock, ref _valueLocaleVariable);
            _valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, EndBlock);

            // ELSE
            LLVM.PositionBuilderAtEnd(builder, ifFalse);
            _valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>());
            foreach (AST astNode in stateIf.Else["body"]) astNode.HandlingStatus(entryBlock, ref _valueLocaleVariable);
            _valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, EndBlock);

            LLVM.PositionBuilderAtEnd(builder, EndBlock);
        }
        public static void _doWhile( doWhile stateDoWhile, LLVMBasicBlockRef entryBlock,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");

            LLVM.BuildBr(builder, loopBlock);

            LLVM.PositionBuilderAtEnd(builder, loopBlock);

            _valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>());
            foreach (AST astNode in stateDoWhile.body) astNode.HandlingStatus(entryBlock, ref _valueLocaleVariable);
            _valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, conditionBlock);

            LLVM.BuildCondBr(builder, _BuildEquation(stateDoWhile.left,stateDoWhile.right,stateDoWhile.Operator,ref _valueLocaleVariable), loopBlock, endBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock);
        }
        public static void _for(FOR @for, LLVMBasicBlockRef entryBlock,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_end");
            if (@for.Init.ID != "") Initialization(@for.Init, ref _valueLocaleVariable);

            LLVM.BuildBr(builder, conditionBlock);
            LLVM.PositionBuilderAtEnd(builder, conditionBlock);
            _valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>());
            
            LLVM.BuildCondBr(builder, _BuildEquation(@for.Equation.left, @for.Equation.right, @for.Equation.Operator, ref _valueLocaleVariable), loopBlock, endBlock);
            LLVM.PositionBuilderAtEnd(builder, loopBlock);
            _valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, initialization>>());

            foreach (AST astNode in @for.body) astNode.HandlingStatus(entryBlock, ref _valueLocaleVariable);
            changeValue(@for.changeValue, ref _valueLocaleVariable);
            _valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock); 
        }
        public static void Initialization(initialization InitValue, 
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            
            var insertBlock = LLVM.GetInsertBlock(builder);
            if (InitValue.EXPR != null)
            {
                if (InitValue.EXPR.Count >= 3)
                {
                    InitValue.EXPR = CalculatingTheExpression(InitValue.EXPR, ref _valueLocaleVariable, InitValue.TYPE);
                }
            }
            else InitValue.EXPR = new List<string>();

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
            InitValue.ValueRef = variable;
           
            _valueLocaleVariable[insertBlock].Add(new Dictionary<string, initialization>()
            {
                { InitValue.ID, InitValue }
            });
            if (InitValue.EXPR.Count == 1)
            {
                LLVMValueRef value = default(LLVMValueRef);
                if (InitValue.EXPR[0][0] == '-')
                {
                    var positiveValue = LLVM.ConstInt(_lLVMType, ulong.Parse(CalculatingTheExpression(InitValue.EXPR,ref _valueLocaleVariable)[0]), new LLVMBool(0));

                    var zeroValue = LLVM.ConstInt(_lLVMType, 0, new LLVMBool(0));

                    value = LLVM.BuildSub(builder, zeroValue, positiveValue, "negative_value");

                }
                else value = LLVM.ConstInt(_lLVMType, ulong.Parse(CalculatingTheExpression(InitValue.EXPR,ref _valueLocaleVariable)[0]), new LLVMBool(0));
                LLVM.BuildStore(builder, value, variable);
            }
           
        }
        public static void Display(string OutputString,in string TypeInpuutStr, 
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, initialization>>> _valueLocaleVariable)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            LLVMValueRef formatString = default;
            LLVMValueRef[] args = default;

            if (TypeInpuutStr == "variable")
            {
                var variable = new initialization();
                var blockWithVariable = FindBlockWithVariable(OutputString, insertBlock, ref _valueLocaleVariable).Item1;
        
                if (!blockWithVariable.Equals(default(LLVMBasicBlockRef)))
                {
                    variable = _valueLocaleVariable[blockWithVariable]
                        .FirstOrDefault(item => item.ContainsKey(OutputString))?[OutputString];
                    if (blockWithVariable.Pointer == insertBlock.Pointer)
                    {
                        // Переменная найдена в текущем блоке
                        var variableValue = LLVM.BuildLoad(builder, variable.ValueRef, "Writeln_variable_value");
                        formatString = LLVM.BuildGlobalStringPtr(builder, "%d\n", $"Variable_{OutputString}_string");
                        args = new LLVMValueRef[] { formatString, variableValue };
                    }
                    else
                    {
                        // Переменная найдена в другом блоке
                        formatString = LLVM.BuildGlobalStringPtr(builder, "%d\n", $"Variable_{OutputString}_string");
                        var variableValue = LLVM.BuildLoad(builder, variable.ValueRef, "Writeln_variable_value");
                        args = new LLVMValueRef[] { formatString, variableValue };
                    }
                }
                else
                {
                    // Переменная не найдена
                    WriteWrong($"Unknown variable: {OutputString}");
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
}