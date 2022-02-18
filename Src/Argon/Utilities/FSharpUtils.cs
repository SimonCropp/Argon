#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

class FSharpFunction
{
    readonly object? _instance;
    readonly MethodCall<object?, object> _invoker;

    public FSharpFunction(object? instance, MethodCall<object?, object> invoker)
    {
        _instance = instance;
        _invoker = invoker;
    }

    public object Invoke(params object[] args)
    {
        var o = _invoker(_instance, args);

        return o;
    }
}

class FSharpUtils
{
    FSharpUtils(Assembly fsharpCoreAssembly)
    {
        FSharpCoreAssembly = fsharpCoreAssembly;

        var fsharpType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpType");

        var isUnionMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "IsUnion", BindingFlags.Public | BindingFlags.Static);
        IsUnion = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(isUnionMethodInfo)!;

        var getUnionCasesMethodInfo = GetMethodWithNonPublicFallback(fsharpType, "GetUnionCases", BindingFlags.Public | BindingFlags.Static);
        GetUnionCases = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(getUnionCasesMethodInfo)!;

        var fsharpValue = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.FSharpValue");

        PreComputeUnionTagReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionTagReader");
        PreComputeUnionReader = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionReader");
        PreComputeUnionConstructor = CreateFSharpFuncCall(fsharpValue, "PreComputeUnionConstructor");

        var unionCaseInfo = fsharpCoreAssembly.GetType("Microsoft.FSharp.Reflection.UnionCaseInfo");

        GetUnionCaseInfoName = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Name")!)!;
        GetUnionCaseInfoTag = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("Tag")!)!;
        GetUnionCaseInfoDeclaringType = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(unionCaseInfo.GetProperty("DeclaringType")!)!;
        GetUnionCaseInfoFields = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(unionCaseInfo.GetMethod("GetFields"));

        var listModule = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.ListModule");
        _ofSeq = listModule.GetMethod("OfSeq");

        _mapType = fsharpCoreAssembly.GetType("Microsoft.FSharp.Collections.FSharpMap`2");
    }

    static readonly object Lock = new();
    static FSharpUtils? _instance;

    public static FSharpUtils Instance
    {
        get
        {
            MiscellaneousUtils.Assert(_instance != null);
            return _instance;
        }
    }

    MethodInfo _ofSeq;
    Type _mapType;

    public Assembly FSharpCoreAssembly { get; private set; }
    public MethodCall<object?, object> IsUnion { get; private set; }
    public MethodCall<object?, object> GetUnionCases { get; private set; }
    public MethodCall<object?, object> PreComputeUnionTagReader { get; private set; }
    public MethodCall<object?, object> PreComputeUnionReader { get; private set; }
    public MethodCall<object?, object> PreComputeUnionConstructor { get; private set; }
    public Func<object, object> GetUnionCaseInfoDeclaringType { get; private set; }
    public Func<object, object> GetUnionCaseInfoName { get; private set; }
    public Func<object, object> GetUnionCaseInfoTag { get; private set; }
    public MethodCall<object, object?> GetUnionCaseInfoFields { get; private set; }

    public const string FSharpSetTypeName = "FSharpSet`1";
    public const string FSharpListTypeName = "FSharpList`1";
    public const string FSharpMapTypeName = "FSharpMap`2";

    public static void EnsureInitialized(Assembly fsharpCoreAssembly)
    {
        if (_instance == null)
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new FSharpUtils(fsharpCoreAssembly);
                }
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
        var invokeFunc = innerMethodInfo.ReturnType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);

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

    public ObjectConstructor<object> CreateSeq(Type t)
    {
        var seqType = _ofSeq.MakeGenericMethod(t);

        return JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(seqType);
    }

    public ObjectConstructor<object> CreateMap(Type keyType, Type valueType)
    {
        var creatorDefinition = typeof(FSharpUtils).GetMethod("BuildMapCreator");

        var creatorGeneric = creatorDefinition.MakeGenericMethod(keyType, valueType);

        return (ObjectConstructor<object>)creatorGeneric.Invoke(this, null);
    }

    public ObjectConstructor<object> BuildMapCreator<TKey, TValue>()
    {
        var genericMapType = _mapType.MakeGenericType(typeof(TKey), typeof(TValue));
        var ctor = genericMapType.GetConstructor(new[] { typeof(IEnumerable<Tuple<TKey, TValue>>) });
        var ctorDelegate = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(ctor);

        ObjectConstructor<object> creator = args =>
        {
            // convert dictionary KeyValuePairs to Tuples
            var values = (IEnumerable<KeyValuePair<TKey, TValue>>)args[0]!;
            var tupleValues = values.Select(kv => new Tuple<TKey, TValue>(kv.Key, kv.Value));

            return ctorDelegate(tupleValues);
        };

        return creator;
    }
}