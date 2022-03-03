// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if !NETSTANDARD2_0
using System.Reflection.Emit;

class DynamicReflectionDelegateFactory : ReflectionDelegateFactory
{
    internal static DynamicReflectionDelegateFactory Instance { get; } = new();

    static DynamicMethod CreateDynamicMethod(string name, Type? returnType, Type[] parameterTypes, Type owner)
    {
        var dynamicMethod = !owner.IsInterface
            ? new(name, returnType, parameterTypes, owner, true)
            : new DynamicMethod(name, returnType, parameterTypes, owner.Module, true);

        return dynamicMethod;
    }

    public override ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
    {
        var dynamicMethod = CreateDynamicMethod(method.ToString()!, typeof(object), new[] { typeof(object[]) }, method.DeclaringType!);
        var generator = dynamicMethod.GetILGenerator();

        GenerateCreateMethodCallIL(method, generator, 0);

        return (ObjectConstructor<object>)dynamicMethod.CreateDelegate(typeof(ObjectConstructor<object>));
    }

    public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
    {
        var dynamicMethod = CreateDynamicMethod(method.ToString()!, typeof(object), new[] { typeof(object), typeof(object[]) }, method.DeclaringType!);
        var generator = dynamicMethod.GetILGenerator();

        GenerateCreateMethodCallIL(method, generator, 1);

        return (MethodCall<T, object?>)dynamicMethod.CreateDelegate(typeof(MethodCall<T, object?>));
    }

    static void GenerateCreateMethodCallIL(MethodBase method, ILGenerator generator, int argsIndex)
    {
        var args = method.GetParameters();

        var argsOk = generator.DefineLabel();

        // throw an error if the number of argument values doesn't match method parameters
        generator.Emit(OpCodes.Ldarg, argsIndex);
        generator.Emit(OpCodes.Ldlen);
        generator.Emit(OpCodes.Ldc_I4, args.Length);
        generator.Emit(OpCodes.Beq, argsOk);
        generator.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(Type.EmptyTypes)!);
        generator.Emit(OpCodes.Throw);

        generator.MarkLabel(argsOk);

        if (!method.IsConstructor && !method.IsStatic)
        {
            generator.PushInstance(method.DeclaringType!);
        }

        var localConvertible = generator.DeclareLocal(typeof(IConvertible));
        var localObject = generator.DeclareLocal(typeof(object));

        var variableAddressOpCode = args.Length < 256 ? OpCodes.Ldloca_S : OpCodes.Ldloca;
        var variableLoadOpCode = args.Length < 256 ? OpCodes.Ldloc_S : OpCodes.Ldloc;

        for (var i = 0; i < args.Length; i++)
        {
            var parameter = args[i];
            var parameterType = parameter.ParameterType;

            if (parameterType.IsByRef)
            {
                parameterType = parameterType.GetElementType()!;

                var localVariable = generator.DeclareLocal(parameterType);

                // don't need to set variable for 'out' parameter
                if (!parameter.IsOut)
                {
                    generator.PushArrayInstance(argsIndex, i);

                    if (parameterType.IsValueType)
                    {
                        var skipSettingDefault = generator.DefineLabel();
                        var finishedProcessingParameter = generator.DefineLabel();

                        // check if parameter is not null
                        generator.Emit(OpCodes.Brtrue_S, skipSettingDefault);

                        // parameter has no value, initialize to default
                        generator.Emit(variableAddressOpCode, localVariable);
                        generator.Emit(OpCodes.Initobj, parameterType);
                        generator.Emit(OpCodes.Br_S, finishedProcessingParameter);

                        // parameter has value, get value from array again and unbox and set to variable
                        generator.MarkLabel(skipSettingDefault);
                        generator.PushArrayInstance(argsIndex, i);
                        generator.UnboxIfNeeded(parameterType);
                        generator.Emit(OpCodes.Stloc_S, localVariable);

                        // parameter finished, we out!
                        generator.MarkLabel(finishedProcessingParameter);
                    }
                    else
                    {
                        generator.UnboxIfNeeded(parameterType);
                        generator.Emit(OpCodes.Stloc_S, localVariable);
                    }
                }

                generator.Emit(variableAddressOpCode, localVariable);
            }
            else if (parameterType.IsValueType)
            {
                generator.PushArrayInstance(argsIndex, i);
                generator.Emit(OpCodes.Stloc_S, localObject);

                // have to check that value type parameters aren't null
                // otherwise they will error when unboxed
                var skipSettingDefault = generator.DefineLabel();
                var finishedProcessingParameter = generator.DefineLabel();

                // check if parameter is not null
                generator.Emit(OpCodes.Ldloc_S, localObject);
                generator.Emit(OpCodes.Brtrue_S, skipSettingDefault);

                // parameter has no value, initialize to default
                var localVariable = generator.DeclareLocal(parameterType);
                generator.Emit(variableAddressOpCode, localVariable);
                generator.Emit(OpCodes.Initobj, parameterType);
                generator.Emit(variableLoadOpCode, localVariable);
                generator.Emit(OpCodes.Br_S, finishedProcessingParameter);

                // argument has value, try to convert it to parameter type
                generator.MarkLabel(skipSettingDefault);

                if (parameterType.IsPrimitive)
                {
                    // for primitive types we need to handle type widening (e.g. short -> int)
                    var toParameterTypeMethod = typeof(IConvertible)
                        .GetMethod($"To{parameterType.Name}", new[] { typeof(IFormatProvider) });

                    if (toParameterTypeMethod != null)
                    {
                        var skipConvertible = generator.DefineLabel();

                        // check if argument type is an exact match for parameter type
                        // in this case we may use cheap unboxing instead
                        generator.Emit(OpCodes.Ldloc_S, localObject);
                        generator.Emit(OpCodes.Isinst, parameterType);
                        generator.Emit(OpCodes.Brtrue_S, skipConvertible);

                        // types don't match, check if argument implements IConvertible
                        generator.Emit(OpCodes.Ldloc_S, localObject);
                        generator.Emit(OpCodes.Isinst, typeof(IConvertible));
                        generator.Emit(OpCodes.Stloc_S, localConvertible);
                        generator.Emit(OpCodes.Ldloc_S, localConvertible);
                        generator.Emit(OpCodes.Brfalse_S, skipConvertible);

                        // convert argument to parameter type
                        generator.Emit(OpCodes.Ldloc_S, localConvertible);
                        generator.Emit(OpCodes.Ldnull);
                        generator.Emit(OpCodes.Callvirt, toParameterTypeMethod);
                        generator.Emit(OpCodes.Br_S, finishedProcessingParameter);

                        generator.MarkLabel(skipConvertible);
                    }
                }

                // we got here because either argument type matches parameter (conversion will succeed),
                // or argument type doesn't match parameter, but we're out of options (conversion will fail)
                generator.Emit(OpCodes.Ldloc_S, localObject);

                generator.UnboxIfNeeded(parameterType);

                // parameter finished, we out!
                generator.MarkLabel(finishedProcessingParameter);
            }
            else
            {
                generator.PushArrayInstance(argsIndex, i);

                generator.UnboxIfNeeded(parameterType);
            }
        }

        if (method.IsConstructor)
        {
            generator.Emit(OpCodes.Newobj, (ConstructorInfo)method);
        }
        else
        {
            generator.CallMethod((MethodInfo)method);
        }

        var returnType = method.IsConstructor
            ? method.DeclaringType!
            : ((MethodInfo)method).ReturnType;

        if (returnType == typeof(void))
        {
            generator.Emit(OpCodes.Ldnull);
        }
        else
        {
            generator.BoxIfNeeded(returnType);
        }

        generator.Return();
    }

    public override Func<T> CreateDefaultConstructor<T>(Type type)
    {
        var dynamicMethod = CreateDynamicMethod($"Create{type.FullName}", typeof(T), Type.EmptyTypes, type);
        dynamicMethod.InitLocals = true;
        var generator = dynamicMethod.GetILGenerator();

        GenerateCreateDefaultConstructorIL(type, generator, typeof(T));

        return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
    }

    static void GenerateCreateDefaultConstructorIL(Type type, ILGenerator generator, Type delegateType)
    {
        if (type.IsValueType)
        {
            generator.DeclareLocal(type);
            generator.Emit(OpCodes.Ldloc_0);

            // only need to box if the delegate isn't returning the value type
            if (type != delegateType)
            {
                generator.Emit(OpCodes.Box, type);
            }
        }
        else
        {
            var constructorInfo =
                type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);

            if (constructorInfo == null)
            {
                throw new ArgumentException($"Could not get constructor for {type}.");
            }

            generator.Emit(OpCodes.Newobj, constructorInfo);
        }

        generator.Return();
    }

    public override Func<T, object?> CreateGet<T>(PropertyInfo property)
    {
        var dynamicMethod = CreateDynamicMethod($"Get{property.Name}", typeof(object), new[] { typeof(T) }, property.DeclaringType!);
        var generator = dynamicMethod.GetILGenerator();

        GenerateCreateGetPropertyIL(property, generator);

        return (Func<T, object?>)dynamicMethod.CreateDelegate(typeof(Func<T, object?>));
    }

    static void GenerateCreateGetPropertyIL(PropertyInfo property, ILGenerator generator)
    {
        var getMethod = property.GetMethod;
        if (getMethod == null)
        {
            throw new ArgumentException($"Property '{property.Name}' does not have a getter.");
        }

        if (!getMethod.IsStatic)
        {
            generator.PushInstance(property.DeclaringType!);
        }

        generator.CallMethod(getMethod);
        generator.BoxIfNeeded(property.PropertyType);
        generator.Return();
    }

    public override Func<T, object?> CreateGet<T>(FieldInfo field)
    {
        if (field.IsLiteral)
        {
            var constantValue = field.GetValue(null);
            Func<T, object?> getter = _ => constantValue;
            return getter;
        }

        var dynamicMethod = CreateDynamicMethod($"Get{field.Name}", typeof(T), new[] { typeof(object) }, field.DeclaringType!);
        var generator = dynamicMethod.GetILGenerator();

        GenerateCreateGetFieldIL(field, generator);

        return (Func<T, object?>)dynamicMethod.CreateDelegate(typeof(Func<T, object?>));
    }

    static void GenerateCreateGetFieldIL(FieldInfo field, ILGenerator generator)
    {
        if (field.IsStatic)
        {
            generator.Emit(OpCodes.Ldsfld, field);
        }
        else
        {
            generator.PushInstance(field.DeclaringType!);
            generator.Emit(OpCodes.Ldfld, field);
        }

        generator.BoxIfNeeded(field.FieldType);
        generator.Return();
    }

    public override Action<T, object?> CreateSet<T>(FieldInfo field)
    {
        var dynamicMethod = CreateDynamicMethod($"Set{field.Name}", null, new[] { typeof(T), typeof(object) }, field.DeclaringType!);
        var generator = dynamicMethod.GetILGenerator();

        GenerateCreateSetFieldIL(field, generator);

        return (Action<T, object?>)dynamicMethod.CreateDelegate(typeof(Action<T, object?>));
    }

    static void GenerateCreateSetFieldIL(FieldInfo field, ILGenerator generator)
    {
        if (!field.IsStatic)
        {
            generator.PushInstance(field.DeclaringType!);
        }

        generator.Emit(OpCodes.Ldarg_1);
        generator.UnboxIfNeeded(field.FieldType);

        if (field.IsStatic)
        {
            generator.Emit(OpCodes.Stsfld, field);
        }
        else
        {
            generator.Emit(OpCodes.Stfld, field);
        }

        generator.Return();
    }

    public override Action<T, object?> CreateSet<T>(PropertyInfo property)
    {
        var dynamicMethod = CreateDynamicMethod($"Set{property.Name}", null, new[] { typeof(T), typeof(object) }, property.DeclaringType!);
        var generator = dynamicMethod.GetILGenerator();

        GenerateCreateSetPropertyIL(property, generator);

        return (Action<T, object?>)dynamicMethod.CreateDelegate(typeof(Action<T, object>));
    }

    static void GenerateCreateSetPropertyIL(PropertyInfo property, ILGenerator generator)
    {
        var setMethod = property.SetMethod!;
        if (!setMethod.IsStatic)
        {
            generator.PushInstance(property.DeclaringType!);
        }

        generator.Emit(OpCodes.Ldarg_1);
        generator.UnboxIfNeeded(property.PropertyType);
        generator.CallMethod(setMethod);
        generator.Return();
    }
}

#endif