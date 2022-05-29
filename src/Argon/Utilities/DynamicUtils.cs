// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Dynamic;
using System.Linq.Expressions;

static class DynamicUtils
{
    internal static class BinderWrapper
    {
        public const string CSharpAssemblyName = "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        const string BinderTypeName = $"Microsoft.CSharp.RuntimeBinder.Binder, {CSharpAssemblyName}";
        const string CSharpArgumentInfoTypeName = $"Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, {CSharpAssemblyName}";
        const string CSharpArgumentInfoFlagsTypeName = $"Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, {CSharpAssemblyName}";
        const string CSharpBinderFlagsTypeName = $"Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, {CSharpAssemblyName}";

        static object? getCSharpArgumentInfoArray;
        static object? setCSharpArgumentInfoArray;
        static MethodCall<object?, object?>? getMemberCall;
        static MethodCall<object?, object?>? setMemberCall;
        static bool init;

        static void Init()
        {
            if (init)
            {
                return;
            }

            var binderType = Type.GetType(BinderTypeName, false);
            if (binderType == null)
            {
                throw new InvalidOperationException($"Could not resolve type '{BinderTypeName}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.");
            }

            // None
            getCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0);
            // None, Constant | UseCompileTimeType
            setCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0, 3);
            CreateMemberCalls();

            init = true;
        }

        static object CreateSharpArgumentInfoArray(params int[] values)
        {
            var csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName)!;
            var csharpArgumentInfoFlags = Type.GetType(CSharpArgumentInfoFlagsTypeName)!;

            var a = Array.CreateInstance(csharpArgumentInfoType, values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                var createArgumentInfoMethod = csharpArgumentInfoType.GetMethod("Create", new[] {csharpArgumentInfoFlags, typeof(string)})!;
                var arg = createArgumentInfoMethod.Invoke(null, new object?[] {0, null});
                a.SetValue(arg, i);
            }

            return a;
        }

        static void CreateMemberCalls()
        {
            var csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName, true)!;
            var csharpBinderFlagsType = Type.GetType(CSharpBinderFlagsTypeName, true)!;
            var binderType = Type.GetType(BinderTypeName, true)!;

            var csharpArgumentInfoTypeEnumerableType = typeof(IEnumerable<>).MakeGenericType(csharpArgumentInfoType);

            var getMemberMethod = binderType.GetMethod("GetMember", new[] {csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType})!;
            getMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(getMemberMethod);

            var setMemberMethod = binderType.GetMethod("SetMember", new[] {csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType})!;
            setMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(setMemberMethod);
        }

        public static CallSiteBinder GetMember(string name, Type context)
        {
            Init();
            MiscellaneousUtils.Assert(getMemberCall != null);
            MiscellaneousUtils.Assert(getCSharpArgumentInfoArray != null);
            return (CallSiteBinder) getMemberCall(null, 0, name, context, getCSharpArgumentInfoArray)!;
        }

        public static CallSiteBinder SetMember(string name, Type context)
        {
            Init();
            MiscellaneousUtils.Assert(setMemberCall != null);
            MiscellaneousUtils.Assert(setCSharpArgumentInfoArray != null);
            return (CallSiteBinder) setMemberCall(null, 0, name, context, setCSharpArgumentInfoArray)!;
        }
    }

    public static IEnumerable<string> GetDynamicMemberNames(this IDynamicMetaObjectProvider dynamicProvider)
    {
        var metaObject = dynamicProvider.GetMetaObject(Expression.Constant(dynamicProvider));
        return metaObject.GetDynamicMemberNames();
    }
}

class NoThrowGetBinderMember : GetMemberBinder
{
    readonly GetMemberBinder innerBinder;

    public NoThrowGetBinderMember(GetMemberBinder innerBinder)
        : base(innerBinder.Name, innerBinder.IgnoreCase)
    {
        this.innerBinder = innerBinder;
    }

    public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject? errorSuggestion)
    {
        var retMetaObject = innerBinder.Bind(target, Array.Empty<DynamicMetaObject>());

        var noThrowVisitor = new NoThrowExpressionVisitor();
        var resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

        var finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
        return finalMetaObject;
    }
}

class NoThrowSetBinderMember : SetMemberBinder
{
    readonly SetMemberBinder innerBinder;

    public NoThrowSetBinderMember(SetMemberBinder innerBinder)
        : base(innerBinder.Name, innerBinder.IgnoreCase)
    {
        this.innerBinder = innerBinder;
    }

    public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject? errorSuggestion)
    {
        var retMetaObject = innerBinder.Bind(target, new[] {value});

        var noThrowVisitor = new NoThrowExpressionVisitor();
        var resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

        return new(resultExpression, retMetaObject.Restrictions);
    }
}

class NoThrowExpressionVisitor : ExpressionVisitor
{
    internal static readonly object ErrorResult = new();

    protected override Expression VisitConditional(ConditionalExpression node)
    {
        // if the result of a test is to throw an error, rewrite to result an error result value
        if (node.IfFalse.NodeType == ExpressionType.Throw)
        {
            return Expression.Condition(node.Test, node.IfTrue, Expression.Constant(ErrorResult));
        }

        return base.VisitConditional(node);
    }
}