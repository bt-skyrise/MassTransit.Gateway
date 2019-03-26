using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;

namespace MassTransit.Gateway.MessageBuilder
{
    public static class DynamicTypeBuilder
    {
        public static Type BuildMessageType(string className, string messageJson)
        {
            if (className.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(className));
            if (messageJson.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(messageJson));

            var properties = JObject.Parse(messageJson).Children()
                .Where(x => x.Type == JTokenType.Property)
                .Select(x => (JProperty) x)
                .Select(GetPropertyDefinition)
                .ToArray();

            var definition = new MessageTypeDefinition(className, properties);

            return BuildMessageType(definition);
        }

        private static PropertyDefinition GetPropertyDefinition(JProperty j)
        {
            var typeCode = ((JValue) j.Value).Type;
            Type type;
            switch (typeCode)
            {
                case JTokenType.Integer:
                    type = typeof(long);
                    break;
                case JTokenType.Float:
                    type = typeof(float);
                    break;
                case JTokenType.String:
                    type = typeof(string);
                    break;
                case JTokenType.Boolean:
                    type = typeof(bool);
                    break;
                case JTokenType.Date:
                    type = typeof(DateTime);
                    break;
                case JTokenType.Bytes:
                    type = typeof(byte[]);
                    break;
                case JTokenType.Guid:
                    type = typeof(Guid);
                    break;
                case JTokenType.Uri:
                    type = typeof(Uri);
                    break;
                case JTokenType.TimeSpan:
                    type = typeof(TimeSpan);
                    break;
                default:
                    throw new NotImplementedByDesignException($"JSON type {typeCode.ToString()} is not supported");
            }
            return new PropertyDefinition(j.Name, type);
        }

        public static Type BuildMessageType(MessageTypeDefinition messageTypeDefinition)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(Guid.NewGuid().ToString()),
                AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            var typeBuilder = moduleBuilder.DefineType(messageTypeDefinition.ClassName, TypeAttributes.Public);

            foreach (var propertyDefinition in messageTypeDefinition.PropertyDefinitions)
            {
                AddProperty(typeBuilder, propertyDefinition.Name, propertyDefinition.Type);
            }

            var typeInfo = typeBuilder.CreateTypeInfo();
            return typeInfo;
        }

        private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(propertyName,
                PropertyAttributes.HasDefault, propertyType, null);

            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName |
                                                MethodAttributes.HideBySig;

            var getterBuilder =
                typeBuilder.DefineMethod("get_" + propertyName,
                    getSetAttr,
                    propertyType,
                    Type.EmptyTypes);

            var getterIlGenerator = getterBuilder.GetILGenerator();

            getterIlGenerator.Emit(OpCodes.Ldarg_0);
            getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIlGenerator.Emit(OpCodes.Ret);

            var setterBuilder =
                typeBuilder.DefineMethod("set_" + propertyName,
                    getSetAttr,
                    null,
                    new[] {propertyType});

            var setterIlGenerator = setterBuilder.GetILGenerator();

            setterIlGenerator.Emit(OpCodes.Ldarg_0);
            setterIlGenerator.Emit(OpCodes.Ldarg_1);
            setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            setterIlGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
        }
    }
}