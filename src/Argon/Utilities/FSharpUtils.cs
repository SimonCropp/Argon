// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Microsoft.FSharp.Reflection;

class FSharpFunction
{
    readonly object? instance;
    readonly MethodCall<object?, object> invoker;

    public FSharpFunction(object? instance, MethodCall<object?, object> invoker)
    {
        this.instance = instance;
        this.invoker = invoker;
    }

    public object Invoke(params object[] args) =>
        invoker(instance, args);
}

static class FSharpUtils
{
    static FSharpUtils()
    {
        var fsharpValue = typeof(FSharpValue);

        PreComputeUnionTagReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionTagReader");
        PreComputeUnionReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionReader");
        PreComputeUnionConstructor = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionConstructor");

        var unionCaseInfo = typeof(UnionCaseInfo);

        GetUnionCaseInfoName = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Name")!)!;
        GetUnionCaseInfoTag = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Tag")!)!;
        GetUnionCaseInfoDeclaringType = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("DeclaringType")!)!;
        GetUnionCaseInfoFields = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(unionCaseInfo.GetMethod("GetFields")!);
    }

    public static MethodCall<object?, object> PreComputeUnionTagReader { get; }
    public static MethodCall<object?, object> PreComputeUnionReader { get; }
    public static MethodCall<object?, object> PreComputeUnionConstructor { get; }
    public static Func<object, object> GetUnionCaseInfoDeclaringType { get; }
    public static Func<object, object> GetUnionCaseInfoName { get; }
    public static Func<object, object> GetUnionCaseInfoTag { get; }
    public static MethodCall<object, object?> GetUnionCaseInfoFields { get; }

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

        return (target, args) =>
        {
            var result = call(target, args);
            return new FSharpFunction(result, invoke);
        };
    }
}