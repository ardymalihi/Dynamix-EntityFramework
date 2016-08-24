using System;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using Dynamix.EntityFramework.Model;

namespace Dynamix.EntityFramework.Runtime
{
    public class DynamicTypeBuilder
    {
        public static TypeBuilder GetTypeBuilder(string TypeName, Type BaseType, Type GenericType = null)
        {
            AssemblyName an = new AssemblyName("DynamicAssembly." + TypeName);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule." + TypeName);
            TypeBuilder tb = null;

            if (GenericType != null)
            {
                tb = moduleBuilder.DefineType("DynamicType." + TypeName
                                     , TypeAttributes.Public |
                                     TypeAttributes.Class |
                                     TypeAttributes.AutoClass |
                                     TypeAttributes.AnsiClass |
                                     TypeAttributes.BeforeFieldInit |
                                     TypeAttributes.AutoLayout
                                     , BaseType.MakeGenericType(GenericType));
            }
            else
            {
                tb = moduleBuilder.DefineType("DynamicType." + TypeName
                                     , TypeAttributes.Public |
                                     TypeAttributes.Class |
                                     TypeAttributes.AutoClass |
                                     TypeAttributes.AnsiClass |
                                     TypeAttributes.BeforeFieldInit |
                                     TypeAttributes.AutoLayout
                                     , BaseType);
            }

            return tb;
        }

        public static void SaveTypeBuilder(string TypeName, Type ParentType, string FileName)
        {
            AssemblyName an = new AssemblyName("DynamicAssembly." + TypeName);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule." + TypeName);

            TypeBuilder tb = moduleBuilder.DefineType("DynamicType." + TypeName
                                 , TypeAttributes.Public |
                                 TypeAttributes.Class |
                                 TypeAttributes.AutoClass |
                                 TypeAttributes.AnsiClass |
                                 TypeAttributes.BeforeFieldInit |
                                 TypeAttributes.AutoLayout,ParentType);


            assemblyBuilder.Save(FileName);
        }



        public static PropertyBuilder CreateProperty(TypeBuilder builder, string propertyName, Type propertyType, bool notifyChanged)
        {
            FieldBuilder fieldBuilder = builder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = builder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            MethodBuilder getPropertyBuiler = CreatePropertyGetter(builder, fieldBuilder);
            MethodBuilder setPropertyBuiler = null;
            if (notifyChanged)
            {
                setPropertyBuiler = CreatePropertySetterWithNotifyChanged(builder, fieldBuilder, propertyName);
            }
            else
            {
                setPropertyBuiler = CreatePropertySetter(builder, fieldBuilder);
            }

            propertyBuilder.SetGetMethod(getPropertyBuiler);
            propertyBuilder.SetSetMethod(setPropertyBuiler);

            return propertyBuilder;
        }

        private static MethodBuilder CreateRaisePropertyChanged(TypeBuilder typeBuilder, FieldBuilder eventField)
        {
            MethodBuilder raisePropertyChangedBuilder =
                typeBuilder.DefineMethod(
                "RaisePropertyChanged",
                MethodAttributes.Family | MethodAttributes.Virtual,
                null, new Type[] { typeof(string) });

            ILGenerator raisePropertyChangedIl =
            raisePropertyChangedBuilder.GetILGenerator();
            Label labelExit = raisePropertyChangedIl.DefineLabel();

            // if (PropertyChanged == null)
            // {
            //      return;
            // }
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_0);
            raisePropertyChangedIl.Emit(OpCodes.Ldfld, eventField);
            raisePropertyChangedIl.Emit(OpCodes.Ldnull);
            raisePropertyChangedIl.Emit(OpCodes.Ceq);
            raisePropertyChangedIl.Emit(OpCodes.Brtrue, labelExit);

            // this.PropertyChanged(this,
            // new PropertyChangedEventArgs(propertyName));
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_0);
            raisePropertyChangedIl.Emit(OpCodes.Ldfld, eventField);
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_0);
            raisePropertyChangedIl.Emit(OpCodes.Ldarg_1);
            raisePropertyChangedIl.Emit(OpCodes.Newobj,
                typeof(PropertyChangedEventArgs)
                .GetConstructor(new[] { typeof(string) }));
            raisePropertyChangedIl.EmitCall(OpCodes.Callvirt,
                typeof(PropertyChangedEventHandler)
                .GetMethod("Invoke"), null);

            // return;
            raisePropertyChangedIl.MarkLabel(labelExit);
            raisePropertyChangedIl.Emit(OpCodes.Ret);

            return raisePropertyChangedBuilder;
        }


        private static MethodBuilder CreatePropertySetterWithNotifyChanged(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, string PropertyName)
        {
            //Raise
            MethodInfo m = typeof(PocoBase).GetMethod("RaisePropertyChanged",
                                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                         null, new[] { typeof(object), typeof(string) }, null);

            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + fieldBuilder.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { fieldBuilder.FieldType });

            ILGenerator setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldstr, PropertyName);
            setIL.Emit(OpCodes.Call, m);
            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Ret);
            return setMethodBuilder;

        }


        public static PropertyBuilder CreateVirtualProperty(TypeBuilder classBuilder, string propertyName, Type propertyTypeBuilder)
        {
            FieldBuilder fieldBuilder = classBuilder.DefineField("_" + propertyName, propertyTypeBuilder, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = classBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyTypeBuilder, null);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual;
            var mbIdGetAccessor = classBuilder.DefineMethod("get_" + propertyName, getSetAttr, propertyTypeBuilder, Type.EmptyTypes);

            var numberGetIL = mbIdGetAccessor.GetILGenerator();
            numberGetIL.Emit(OpCodes.Ldarg_0);
            numberGetIL.Emit(OpCodes.Ldfld, fieldBuilder);
            numberGetIL.Emit(OpCodes.Ret);

            var mbIdSetAccessor = classBuilder.DefineMethod("set_" + propertyName, getSetAttr, null, new Type[] { propertyTypeBuilder });

            var numberSetIL = mbIdSetAccessor.GetILGenerator();
            numberSetIL.Emit(OpCodes.Ldarg_0);
            numberSetIL.Emit(OpCodes.Ldarg_1);
            numberSetIL.Emit(OpCodes.Stfld, fieldBuilder);
            numberSetIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(mbIdGetAccessor);
            propertyBuilder.SetSetMethod(mbIdSetAccessor);

            return propertyBuilder;
        }

        private static MethodBuilder CreatePropertyGetter(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + fieldBuilder.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, fieldBuilder.FieldType, Type.EmptyTypes);

            ILGenerator getIL = getMethodBuilder.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            return getMethodBuilder;
        }

        private static MethodBuilder CreatePropertySetter(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + fieldBuilder.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { fieldBuilder.FieldType });

            ILGenerator setIL = setMethodBuilder.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            return setMethodBuilder;
        }



        public static object SetProperty(object Target, string Name, object value, bool ignoreIfTargetIsNull)
        {

            if (ignoreIfTargetIsNull && Target == null) return null;

            object[] values = { value };

            object oldProperty = GetProperty(Target, Name);

            PropertyInfo targetProperty = Target.GetType().GetProperty(Name);

            if (targetProperty == null)
            {
                throw new System.Exception("Object " + Target.ToString() + "   does not have Target Property " + Name);

            }


            targetProperty.GetSetMethod().Invoke(Target, values);


            return oldProperty;

        }



        public static object GetProperty(object Target, string Name)
        {
            PropertyInfo targetProperty = Target.GetType().GetProperty(Name);

            if (targetProperty == null)
            {
                return null;
            }
            else
            {
                return targetProperty.GetGetMethod().Invoke(Target, null);
            }

        }

    }
}
