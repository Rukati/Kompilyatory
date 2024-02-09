using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

class ILGenerator
{
    private static Type GetTypeFromName(string typeName)
    {
        switch (typeName.ToLower())
        {
            case "int":
                return typeof(int);
            // Добавьте другие типы по мере необходимости
            default:
                throw new ArgumentException($"Unsupported type: {typeName}");
        }
    }

    public static void GenerateILFromAST(List<Kompilyatory.Program.AST> ast)
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

        // Создаем класс для хранения переменных
        var typeBuilder = moduleBuilder.DefineType("VariableContainer", TypeAttributes.Public);

        foreach (var item in ast)
        {
            var type = item.Initialization.TYPE;
            var id = item.Initialization.ID;
            var expr = item.Initialization.EXPR;

            var fieldType = GetTypeFromName(type);

            // Создаем поле в классе для каждой переменной
            var fieldBuilder = typeBuilder.DefineField(id, fieldType, FieldAttributes.Private | FieldAttributes.Static);

            // Создаем метод инициализации для установки начального значения переменной
            var methodBuilder = typeBuilder.DefineMethod($"Initialize_{id}", MethodAttributes.Static | MethodAttributes.Public, typeof(void), Type.EmptyTypes);
            var ilGenerator = methodBuilder.GetILGenerator();

            // Генерация кода IL для установки начального значения переменной
            foreach (var value in expr)
            {
                if (int.TryParse(value, out int intValue))
                {
                    // Если значение является целым числом, загрузите его как int
                    ilGenerator.Emit(OpCodes.Ldc_I4, intValue);
                }
                else if (double.TryParse(value, out double doubleValue))
                {
                    // Если значение является числом с плавающей точкой, загрузите его как double
                    ilGenerator.Emit(OpCodes.Ldc_R8, doubleValue);
                }
                else
                {
                  
                }
            }

            // Сохранение последнего значения в поле
            ilGenerator.Emit(OpCodes.Stsfld, fieldBuilder);

            ilGenerator.Emit(OpCodes.Ret);
        }

        // Создание типа
        var generatedType = typeBuilder.CreateType();

        // Сохранение сборки
        assemblyBuilder.Save("DynamicAssembly.dll", PortableExecutableKinds.ILOnly, ImageFileMachine.I386);

    }
}
