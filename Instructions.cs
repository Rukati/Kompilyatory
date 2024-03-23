using System;
using static Kompilyatory.Program;
using LLVMSharp;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using static Kompilyatory.LL;


namespace Kompilyatory
{
    public class Instructions
    {
        public static void CallFunction(CallFunction function,Initialization variable, ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>> local)
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
                    //var block = FindBlockWithVariable(argument.Substring(1), insertBlock, ref local);
                    //if (block == null) WriteWrong($"A variable named \"{argument.Substring(1)}\" already exists\n");
                    //args[index] = LLVM.BuildLoad(builder, block.Item2.ValueRef,$"{argument.Substring(1)}_args");
                }
                else
                {
                    LLVMValueRef functionValue = LLVM.GetNamedFunction(module, function.ID);
                    LLVMTypeRef functionType = LLVM.TypeOf(functionValue);
                    var typeCode = LLVM.GetTypeKind(functionType);
                    switch (typeCode)
                    {
                        case LLVMTypeKind.LLVMFloatTypeKind:
                            args[index] = LLVM.ConstReal(LLVM.FloatType(), float.Parse(argument));
                            break;
                        case LLVMTypeKind.LLVMIntegerTypeKind:
                            long parsedValueInt = long.Parse(argument);
                            long differenceValue = parsedValueInt - long.Parse(variable.expr[0]);
                            if (argument[0] == '-')
                            {
                                var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                                var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(differenceValue), new LLVMBool(0));

                                args[index] = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                            }
                            else args[index] = LLVM.ConstInt(LLVM.Int32Type(), (ulong)differenceValue, new LLVMBool(0));
                            break;
                    }
                }
                index++;
            }

            if (variable != null)
            {
                if (!GetTypeRef(variable.type).Equals(getPuts.TypeOf().GetReturnType().GetElementType())) WriteWrong($"The data type of the return variable ({variable.Id}) must match the return type of the function ({function.ID})");

                var variableBuild = LLVM.BuildAlloca(builder, GetTypeRef(variable.type), variable.Id);
                variable.ValueRef = variableBuild;

                local[insertBlock].Add(new Dictionary<string, Initialization>()
                {
                    { variable.Id, variable }
                });
                LLVM.BuildStore(builder,LLVM.BuildCall(builder, getPuts, args, ""),variableBuild);
            }
            else
            {
                LLVM.BuildCall(builder, getPuts, args, "");
            }
        }
        /*
        public static void BuildFunction(Function function, LLVMBasicBlockRef previousBlock)
        {
            LLVMTypeRef[] llvmTypeRefs = new LLVMTypeRef[function.args.Count];
            int index = 0;
            foreach (var variable in function.args)
            {
                llvmTypeRefs[index] = GetTypeRef(variable.TYPE);
                index++;
            }

            LLVMTypeRef returnType = default(LLVMTypeRef);
            switch (function.type)
            {
                case "void":
                    returnType = LLVM.VoidType();
                    break;
                case "int":
                    returnType = LLVM.Int32Type();
                    break;
                case "float":
                    returnType = LLVM.FloatType();
                    break;
                default:
                    WriteWrong($"Incorrect return type \"{function.type}\" in function \"{function.ID}\"");
                    break;
            }

            var functionType = LLVM.FunctionType(returnType, llvmTypeRefs, false);
            var Function = LLVM.AddFunction(module, function.ID, functionType);
            var Block = LLVM.AppendBasicBlock(Function, function.ID);
            LLVM.PositionBuilderAtEnd(builder, Block);
            
            var newLocalFunction = new Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>>()
            {
                { Block, new List<Dictionary<string, Initialization>>() }
            };
            for (int i = 0; i < function.args.Count; i++)
            {
                var paramValue = LLVM.GetParam(Function, (uint)i);
                // Добавляем в область видимости
                Instructions.Initialization(function.args[i],ref newLocalFunction);
                LLVM.BuildStore(builder, paramValue, newLocalFunction[Block][i][function.args[i].ID].ValueRef);
                unsafe
                {
                    Console.WriteLine(Marshal.ReadInt32(paramValue.Pointer));
                    Console.WriteLine(*((int*)paramValue.Pointer));
                }
            }

            foreach (var item in function.body) item.HandlingStatus(Block, ref newLocalFunction);
            if (function.Return != null)
            {
                if (function.type == "void") WriteWrong($"Function \"{function.ID}\" can't return a value\nBecause it has a type \"{function.type}\"");
                if (function.Return.Count == 1)
                {
                    if (function.Return[0][0] == '$')
                    {
                        Initialization variableReturn =
                            FindBlockWithVariable(function.Return[0].Substring(1), Block, ref newLocalFunction).Item2;
                        if (variableReturn.TYPE != function.type)
                            WriteWrong(
                                $"The data type of the return variable ({variableReturn.TYPE}) must match the return type of the function ({function.type})");

                        LLVM.BuildRet(builder,
                            LLVM.BuildLoad(builder, variableReturn.ValueRef, $"variableReturn_{variableReturn.ID}"));
                    }
                    else
                    {
                        if (function.Return[0].Contains('.') && function.type == "float")
                        {
                            LLVM.BuildRet(builder,
                                LLVM.ConstReal(LLVMTypeRef.FloatType(),
                                    float.Parse(function.Return[0].Replace('.', ','))));
                        }
                        else
                        {
                            string returnValue = function.Return[0];
                            if (returnValue.Contains('.'))
                            {
                                returnValue = returnValue.Replace('.', ',');
                            }

                            LLVM.BuildRet(builder,
                                LLVM.ConstInt(LLVMTypeRef.Int32Type(),
                                    ulong.Parse(returnValue), new LLVMBool(0)));
                        }
                    }
                }
                else
                {
                    string ret = CalculatingTheExpression(function.Return, ref newLocalFunction, function.type)[0];
                }
            }
            
            LLVM.PositionBuilderAtEnd(builder, previousBlock);
        }
        */
        /*public static void ChangeValue(ChangeValue stateInfo,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>> valueLocaleVariable)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            var blockWithVariable = FindBlockWithVariable(stateInfo.ID, insertBlock, ref valueLocaleVariable).Item1;

            if (blockWithVariable.Equals(default(LLVMBasicBlockRef))) WriteWrong($"Unknown variable: {stateInfo.ID}");

            Initialization variable = valueLocaleVariable[blockWithVariable]
                .FirstOrDefault(item => item.ContainsKey(stateInfo.ID))[stateInfo.ID];
            LLVMTypeRef _lLVMType = GetTypeRef(variable.type);
            var typeKind = LLVM.GetTypeKind(_lLVMType);

            switch (typeKind)
            {
                case LLVMTypeKind.LLVMIntegerTypeKind:

                    long parsedValueInt = long.Parse(CalculatingTheExpression(stateInfo.expr,ref valueLocaleVariable)[0]);
                    long differenceValue = parsedValueInt - long.Parse(variable.expr[0]);
                    LLVMValueRef constInt;

                    if (stateInfo.expr[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstInt(LLVM.Int32Type(), 0, new LLVMBool(0));
                        var absoluteValue = LLVM.ConstInt(LLVM.Int32Type(), (ulong)Math.Abs(differenceValue), new LLVMBool(0));

                        constInt = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else constInt = LLVM.ConstInt(LLVM.Int32Type(), (ulong)differenceValue, new LLVMBool(0));
                    var nowValueIntVariable = LLVM.BuildLoad(builder, variable.ValueRef, $"{stateInfo.ID}_value");
                    var changeValueInt = LLVM.BuildAdd(builder, constInt, nowValueIntVariable, "changeValue");
                    LLVM.BuildStore(builder, changeValueInt, variable.ValueRef);
                    variable.expr[0] = (long.Parse(variable.expr[0]) + differenceValue).ToString();
                    break;
                case LLVMTypeKind.LLVMFloatTypeKind:

                    long parsedValueFloat = long.Parse(CalculatingTheExpression(stateInfo.expr,ref valueLocaleVariable)[0]);
                    long differenceValueFloat = parsedValueFloat - long.Parse(variable.expr[0]);

                    LLVMValueRef constFloat;

                    if (CalculatingTheExpression(stateInfo.expr,ref valueLocaleVariable)[0][0] == '-')
                    {
                        var zeroValue = LLVM.ConstInt(LLVM.FloatType(), 0, new LLVMBool(0));
                        var absoluteValue = LLVM.ConstInt(LLVM.FloatType(), (ulong)Math.Abs(differenceValueFloat), new LLVMBool(0));

                        constFloat = LLVM.BuildSub(builder, zeroValue, absoluteValue, "negative_value");
                    }
                    else constFloat = LLVM.ConstInt(LLVM.FloatType(), (ulong)differenceValueFloat, new LLVMBool(0));
                    var NowValueFloatVariable = LLVM.BuildLoad(builder, variable.ValueRef, $"{stateInfo.ID}_value");
                    var changeValue = LLVM.BuildAdd(builder, constFloat, NowValueFloatVariable, "changeValue");
                    LLVM.BuildStore(builder, changeValue, variable.ValueRef);
                    variable.expr[0] = (float.Parse(variable.expr[0]) + differenceValueFloat).ToString();
                    break;
                default:
                    break;
            }
        }*/
        /*public static void _while(While stateWhile, 
            LLVMBasicBlockRef entryBlock, 
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>> valueLocaleVariable)
        {
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");

            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, conditionBlock);

            LLVM.BuildCondBr(builder, _BuildEquation(stateWhile.left, stateWhile.right, stateWhile.Operator, ref valueLocaleVariable), loopBlock, endBlock);

            LLVM.PositionBuilderAtEnd(builder, loopBlock);
            valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, Initialization>>());
            foreach (AST astNode in stateWhile.body) astNode.HandlingStatus(entryBlock,ref valueLocaleVariable);
            valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock);
        }
        public static void _if(If stateIf,
            LLVMBasicBlockRef entryBlock,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>> valueLocaleVariable)
        {
            LLVMBasicBlockRef ifTrue = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "if_true");
            LLVMBasicBlockRef ifFalse = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "if_false");
            LLVMBasicBlockRef EndBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(entryBlock), "endBlockIf");

            LLVM.BuildCondBr(builder, _BuildEquation(stateIf.left,stateIf.right,stateIf.Operator,ref valueLocaleVariable), ifTrue, ifFalse);

            // IF
            LLVM.PositionBuilderAtEnd(builder, ifTrue);
            valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, Initialization>>());
            foreach (AST astNode in stateIf.body) astNode.HandlingStatus(entryBlock, ref valueLocaleVariable);
            valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, EndBlock);

            // ELSE
            LLVM.PositionBuilderAtEnd(builder, ifFalse);
            valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, Initialization>>());
            foreach (AST astNode in stateIf.Else["body"]) astNode.HandlingStatus(entryBlock, ref valueLocaleVariable);
            valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, EndBlock);

            LLVM.PositionBuilderAtEnd(builder, EndBlock);
        }
        public static void _doWhile( DoWhile stateDoWhile, LLVMBasicBlockRef entryBlock,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>> valueLocaleVariable)
        {
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "while_end");

            LLVM.BuildBr(builder, loopBlock);

            LLVM.PositionBuilderAtEnd(builder, loopBlock);

            valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, Initialization>>());
            foreach (AST astNode in stateDoWhile.body) astNode.HandlingStatus(entryBlock, ref valueLocaleVariable);
            valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, conditionBlock);

            LLVM.BuildCondBr(builder, _BuildEquation(stateDoWhile.left,stateDoWhile.right,stateDoWhile.Operator,ref valueLocaleVariable), loopBlock, endBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock);
        }
        public static void _for(For @for, LLVMBasicBlockRef entryBlock,
            ref Dictionary<LLVMBasicBlockRef, List<Dictionary<string, Initialization>>> valueLocaleVariable)
        {
            
            LLVMBasicBlockRef conditionBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_cond");
            LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_loop");
            LLVMBasicBlockRef endBlock = LLVM.AppendBasicBlock(LLVM.GetBasicBlockParent(LLVM.GetInsertBlock(builder)), "for_end");
            if (@for.Init.ID != "") Initialization(@for.Init, ref valueLocaleVariable);

            LLVM.BuildBr(builder, conditionBlock);
            LLVM.PositionBuilderAtEnd(builder, conditionBlock);
            valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, Initialization>>());
            
            LLVM.BuildCondBr(builder, _BuildEquation(@for.Equation.left, @for.Equation.right, @for.Equation.Operator, ref valueLocaleVariable), loopBlock, endBlock);
            LLVM.PositionBuilderAtEnd(builder, loopBlock);
            valueLocaleVariable.Add(LLVM.GetInsertBlock(LL.builder), new List<Dictionary<string, Initialization>>());

            foreach (AST astNode in @for.body) astNode.HandlingStatus(entryBlock, ref valueLocaleVariable);
            ChangeValue(@for.changeValue, ref valueLocaleVariable);

            if (@for.Init.ID != "")
            {
                valueLocaleVariable.Remove(LLVM.GetInsertBlock(LL.builder));
                var block = FindBlockWithVariable(@for.Init.ID, entryBlock, ref valueLocaleVariable).Item1;
                valueLocaleVariable[block].RemoveAll(dict => dict.ContainsKey(@for.Init.ID));
            }

            LLVM.BuildBr(builder, conditionBlock);

            LLVM.PositionBuilderAtEnd(builder, endBlock); 
        }*/
        public static void Initialization(Initialization initValue, ref Blocks blocks)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            
            if (blocks.FindVariable(initValue.Id) != null )
                WriteWrong($"A variable named \"{initValue.Id}\" already exists");
            
            /*
            if (initValue.func != null)
                if (initValue.func.ID != null) {CallFunction(initValue.func,initValue,ref valueLocaleVariable); return;}
                */
            
            /*
            if (initValue.EXPR != null)
            {
                if (initValue.EXPR.Count >= 3)
                {
                    initValue.EXPR = CalculatingTheExpression(initValue.EXPR, ref valueLocaleVariable, initValue.TYPE);
                }
            }*/
           // else initValue.expr = new List<string>();

            var variable = BuildVariable(ref initValue);
            initValue.ValueRef = variable;
            blocks.variable.Add(initValue.Id, initValue );

            if (initValue.expr.Count == 1)
            {
                if (initValue.expr[0][0] != '$')
                {
                    StoreValue(ref initValue);
                }
                else
                {
                    LLVM.BuildStore(builder,CalculatingTheExpression(initValue.expr, ref blocks, initValue.type),initValue.ValueRef);
                }
            }
            else
            {
                LLVM.BuildStore(builder,CalculatingTheExpression(initValue.expr, ref blocks, initValue.type),initValue.ValueRef);
            }

        }
        public static void Display(string outputString,in string typeOutputString, ref Blocks valueLocaleVariable)
        {
            var insertBlock = LLVM.GetInsertBlock(builder);
            LLVMValueRef formatString = default;
            LLVMValueRef[] args = default;

            if (typeOutputString == "variable")
            {
                Initialization variable;
                variable = valueLocaleVariable.FindVariable(outputString);
                if (variable == null)  // Переменная не найдена
                    WriteWrong($"Unknown variable \"{outputString}\" in display instruction");
                
                // Переменная найдена
                if (variable.type == "float")
                {
                    var doubleValue = LLVM.BuildFPExt(builder, LLVM.BuildLoad(builder,variable.ValueRef,"LoadValue"), LLVM.DoubleType(), "");
                    formatString = LLVM.BuildGlobalStringPtr(builder, "%0.1f\n\0", $"Format_string");
                    args = new LLVMValueRef[] { formatString, doubleValue };

                }
                else
                {
                    formatString = LLVM.BuildGlobalStringPtr(builder, "%d\n\0", $"Format_string");
                    args = new LLVMValueRef[] { formatString, LLVM.BuildLoad(builder,variable.ValueRef,"LoadValue") };
                }
                }
            else
            {
                formatString = LLVM.BuildGlobalStringPtr(builder, $"{outputString}\n", $"Writeln_value_{outputString}_string");
                args = new LLVMValueRef[] { formatString };
            }
            var getPuts = LLVM.GetNamedFunction(module, "printf");
            LLVM.BuildCall(builder, getPuts, args, $"");
        }
    }
}