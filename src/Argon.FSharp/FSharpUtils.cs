// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Microsoft.FSharp.Reflection;

static class FSharpUtils
{
    static FSharpUtils()
    {
        PreComputeUnionConstructor = CreateFSharpFuncCall("PreComputeUnionConstructor");
    }

    public static MethodCall<object?, object> PreComputeUnionConstructor { get; }

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

    static MethodCall<object?, object> CreateFSharpFuncCall(string methodName)
    {
        var innerMethodInfo = GetMethodWithNonPublicFallback( typeof(FSharpValue), methodName, BindingFlags.Public | BindingFlags.Static);
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