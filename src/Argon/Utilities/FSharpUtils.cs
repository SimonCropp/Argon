﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class FSharpFunction
{
    readonly object? instance;
    readonly MethodCall<object?, object> invoker;

    public FSharpFunction(object? instance, MethodCall<object?, object> invoker)
    {
        this.instance = instance;
        this.invoker = invoker;
    }

    public object Invoke(params object[] args)
    {
        return invoker(instance, args);
    }
}

class FSharpUtils
{
    FSharpUtils(Assembly fsharpCoreAssembly)
    {
        FSharpCoreAssembly = fsharpCoreAssembly;

        var fsharpType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpType")!;

        var isUnionMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "IsUnion", BindingFlags.Public | BindingFlags.Static);
        IsUnion = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(isUnionMethodInfo)!;

        var getUnionCasesMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "GetUnionCases", BindingFlags.Public | BindingFlags.Static);
        GetUnionCases = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(getUnionCasesMethodInfo)!;

        var fsharpValue = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpValue")!;

        PreComputeUnionTagReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionTagReader");
        PreComputeUnionReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionReader");
        PreComputeUnionConstructor = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionConstructor");

        var unionCaseInfo = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.UnionCaseInfo")!;

        GetUnionCaseInfoName = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Name")!)!;
        GetUnionCaseInfoTag = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Tag")!)!;
        GetUnionCaseInfoDeclaringType = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("DeclaringType")!)!;
        GetUnionCaseInfoFields = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(unionCaseInfo.GetMethod("GetFields")!);

        var listModule = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.ListModule")!;
        ofSeq = listModule.GetMethod("OfSeq")!;

        mapType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.FSharpMap`2")!;
    }

    static readonly object Lock = new();
    static FSharpUtils? instance;

    public static FSharpUtils Instance
    {
        get
        {
            MiscellaneousUtils.Assert(instance != null);
            return instance;
        }
    }

    MethodInfo ofSeq;
    Type mapType;

    public Assembly FSharpCoreAssembly { get; }
    public MethodCall<object?, object> IsUnion { get; }
    public MethodCall<object?, object> GetUnionCases { get; }
    public MethodCall<object?, object> PreComputeUnionTagReader { get; }
    public MethodCall<object?, object> PreComputeUnionReader { get; }
    public MethodCall<object?, object> PreComputeUnionConstructor { get; }
    public Func<object, object> GetUnionCaseInfoDeclaringType { get; }
    public Func<object, object> GetUnionCaseInfoName { get; }
    public Func<object, object> GetUnionCaseInfoTag { get; }
    public MethodCall<object, object?> GetUnionCaseInfoFields { get; }

    public const string FSharpSetTypeName = "FSharpSet`1";
    public const string FSharpListTypeName = "FSharpList`1";
    public const string FSharpMapTypeName = "FSharpMap`2";

    public static void EnsureInitialized(Assembly fsharpCoreAssembly)
    {
        if (instance == null)
        {
            lock (Lock)
            {
                instance ??= new(fsharpCoreAssembly);
            }
        }
    }

    static MethodInfo GetMethodWithNonPublicFallback(Type type, string methodName, BindingFlags bindingFlags)
    {
        var methodInfo = type.GetMethod(methodName, bindingFlags);

        // if no matching method then attempt to find with nonpublic flag
        // this is required because in WinApps some methods are private but always using NonPublic breaks medium trust
        // https://github.com/JamesNK/Newtonsoft.Json/pull/649
        // https://github.com/JamesNK/Newtonsoft.Json/issues/821
        if (methodInfo == null && (bindingFlags & BindingFlags.NonPublic) != BindingFlags.NonPublic)
        {
            methodInfo = type.GetMethod(methodName, bindingFlags | BindingFlags.NonPublic);
        }

        return methodInfo!;
    }

    static MethodCall<object?, object> CreateFSharpFuncCall(Type type, string methodName)
    {
        var innerMethodInfo = GetMethodWithNonPublicFallback(type, methodName, BindingFlags.Public | BindingFlags.Static);
        var invokeFunc = innerMethodInfo.ReturnType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance)!;

        var call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(innerMethodInfo);
        MethodCall<object?, object> invoke = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(invokeFunc)!;

        MethodCall<object?, object> createFunction = (target, args) =>
        {
            var result = call(target, args);

            var f = new FSharpFunction(result, invoke);
            return f;
        };

        return createFunction;
    }

    public ObjectConstructor<object> CreateSeq(Type type)
    {
        var seqType = ofSeq.MakeGenericMethod(type);

        return JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(seqType);
    }

    public ObjectConstructor<object> CreateMap(Type keyType, Type valueType)
    {
        var creatorDefinition = typeof(FSharpUtils).GetMethod("BuildMapCreator")!;

        var creatorGeneric = creatorDefinition.MakeGenericMethod(keyType, valueType);

        return (ObjectConstructor<object>) creatorGeneric.Invoke(this, null)!;
    }

    public ObjectConstructor<object> BuildMapCreator<TKey, TValue>()
    {
        var genericMapType = mapType.MakeGenericType(typeof(TKey), typeof(TValue));
        var ctor = genericMapType.GetConstructor(new[] {typeof(IEnumerable<Tuple<TKey, TValue>>)})!;
        var ctorDelegate = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(ctor);

        ObjectConstructor<object> creator = args =>
        {
            // convert dictionary KeyValuePairs to Tuples
            var values = (IEnumerable<KeyValuePair<TKey, TValue>>) args[0]!;
            var tupleValues = values.Select(kv => new Tuple<TKey, TValue>(kv.Key, kv.Value));

            return ctorDelegate(tupleValues);
        };

        return creator;
    }
}