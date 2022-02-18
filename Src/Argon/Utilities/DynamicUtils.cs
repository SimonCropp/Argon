﻿#region License
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

using System.Dynamic;
using System.Linq.Expressions;

static class DynamicUtils
{
    internal static class BinderWrapper
    {
        public const string CSharpAssemblyName = "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        const string BinderTypeName = "Microsoft.CSharp.RuntimeBinder.Binder, " + CSharpAssemblyName;
        const string CSharpArgumentInfoTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo, " + CSharpAssemblyName;
        const string CSharpArgumentInfoFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, " + CSharpAssemblyName;
        const string CSharpBinderFlagsTypeName = "Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, " + CSharpAssemblyName;

        static object? _getCSharpArgumentInfoArray;
        static object? _setCSharpArgumentInfoArray;
        static MethodCall<object?, object?>? _getMemberCall;
        static MethodCall<object?, object?>? _setMemberCall;
        static bool _init;

        static void Init()
        {
            if (!_init)
            {
                var binderType = Type.GetType(BinderTypeName, false);
                if (binderType == null)
                {
                    throw new InvalidOperationException("Could not resolve type '{0}'. You may need to add a reference to Microsoft.CSharp.dll to work with dynamic types.".FormatWith(CultureInfo.InvariantCulture, BinderTypeName));
                }

                // None
                _getCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0);
                // None, Constant | UseCompileTimeType
                _setCSharpArgumentInfoArray = CreateSharpArgumentInfoArray(0, 3);
                CreateMemberCalls();

                _init = true;
            }
        }

        static object CreateSharpArgumentInfoArray(params int[] values)
        {
            var csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName);
            var csharpArgumentInfoFlags = Type.GetType(CSharpArgumentInfoFlagsTypeName);

            var a = Array.CreateInstance(csharpArgumentInfoType, values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                var createArgumentInfoMethod = csharpArgumentInfoType.GetMethod("Create", new[] { csharpArgumentInfoFlags, typeof(string) });
                var arg = createArgumentInfoMethod.Invoke(null, new object?[] { 0, null });
                a.SetValue(arg, i);
            }

            return a;
        }

        static void CreateMemberCalls()
        {
            var csharpArgumentInfoType = Type.GetType(CSharpArgumentInfoTypeName, true);
            var csharpBinderFlagsType = Type.GetType(CSharpBinderFlagsTypeName, true);
            var binderType = Type.GetType(BinderTypeName, true);

            var csharpArgumentInfoTypeEnumerableType = typeof(IEnumerable<>).MakeGenericType(csharpArgumentInfoType);

            var getMemberMethod = binderType.GetMethod("GetMember", new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
            _getMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(getMemberMethod);

            var setMemberMethod = binderType.GetMethod("SetMember", new[] { csharpBinderFlagsType, typeof(string), typeof(Type), csharpArgumentInfoTypeEnumerableType });
            _setMemberCall = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object?>(setMemberMethod);
        }

        public static CallSiteBinder GetMember(string name, Type context)
        {
            Init();
            MiscellaneousUtils.Assert(_getMemberCall != null);
            MiscellaneousUtils.Assert(_getCSharpArgumentInfoArray != null);
            return (CallSiteBinder)_getMemberCall(null, 0, name, context, _getCSharpArgumentInfoArray)!;
        }

        public static CallSiteBinder SetMember(string name, Type context)
        {
            Init();
            MiscellaneousUtils.Assert(_setMemberCall != null);
            MiscellaneousUtils.Assert(_setCSharpArgumentInfoArray != null);
            return (CallSiteBinder)_setMemberCall(null, 0, name, context, _setCSharpArgumentInfoArray)!;
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
    readonly GetMemberBinder _innerBinder;

    public NoThrowGetBinderMember(GetMemberBinder innerBinder)
        : base(innerBinder.Name, innerBinder.IgnoreCase)
    {
        _innerBinder = innerBinder;
    }

    public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
    {
        var retMetaObject = _innerBinder.Bind(target, CollectionUtils.ArrayEmpty<DynamicMetaObject>());

        var noThrowVisitor = new NoThrowExpressionVisitor();
        var resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

        var finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
        return finalMetaObject;
    }
}

class NoThrowSetBinderMember : SetMemberBinder
{
    readonly SetMemberBinder _innerBinder;

    public NoThrowSetBinderMember(SetMemberBinder innerBinder)
        : base(innerBinder.Name, innerBinder.IgnoreCase)
    {
        _innerBinder = innerBinder;
    }

    public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
    {
        var retMetaObject = _innerBinder.Bind(target, new[] { value });

        var noThrowVisitor = new NoThrowExpressionVisitor();
        var resultExpression = noThrowVisitor.Visit(retMetaObject.Expression);

        var finalMetaObject = new DynamicMetaObject(resultExpression, retMetaObject.Restrictions);
        return finalMetaObject;
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