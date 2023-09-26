// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

sealed class DynamicProxyMetaObject<T> : DynamicMetaObject
{
    readonly DynamicProxy<T> proxy;

    internal DynamicProxyMetaObject(Expression expression, T value, DynamicProxy<T> proxy)
        : base(expression, BindingRestrictions.Empty, value!) =>
        this.proxy = proxy;

    bool IsOverridden(string method) =>
        ReflectionUtils.IsMethodOverridden(proxy.GetType(), typeof(DynamicProxy<T>), method);

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryGetMember)))
        {
            return CallMethodWithResult(nameof(DynamicProxy<T>.TryGetMember), binder, NoArgs, e => binder.FallbackGetMember(this, e));
        }

        return base.BindGetMember(binder);
    }

    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TrySetMember)))
        {
            return CallMethodReturnLast(nameof(DynamicProxy<T>.TrySetMember), binder, GetArgs(value), e => binder.FallbackSetMember(this, value, e));
        }

        return base.BindSetMember(binder, value);
    }

    public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryDeleteMember)))
        {
            return CallMethodNoResult(nameof(DynamicProxy<T>.TryDeleteMember), binder, NoArgs, e => binder.FallbackDeleteMember(this, e));
        }

        return base.BindDeleteMember(binder);
    }

    public override DynamicMetaObject BindConvert(ConvertBinder binder)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryConvert)))
        {
            return CallMethodWithResult(nameof(DynamicProxy<T>.TryConvert), binder, NoArgs, e => binder.FallbackConvert(this, e));
        }

        return base.BindConvert(binder);
    }

    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
    {
        if (!IsOverridden(nameof(DynamicProxy<T>.TryInvokeMember)))
        {
            return base.BindInvokeMember(binder, args);
        }

        //
        // Generate a tree like:
        //
        // {
        //   object result;
        //   TryInvokeMember(payload, out result)
        //      ? result
        //      : TryGetMember(payload, out result)
        //          ? FallbackInvoke(result)
        //          : fallbackResult
        // }
        //
        // Then it calls FallbackInvokeMember with this tree as the
        // "error", giving the language the option of using this
        // tree or doing .NET binding.
        //
        Fallback fallback = e => binder.FallbackInvokeMember(this, args, e);

        return BuildCallMethodWithResult(
            nameof(DynamicProxy<T>.TryInvokeMember),
            binder,
            GetArgArray(args),
            BuildCallMethodWithResult(
                nameof(DynamicProxy<T>.TryGetMember),
                new GetBinderAdapter(binder),
                NoArgs,
                fallback(null),
                e => binder.FallbackInvoke(e!, args, null)
            ),
            null
        );
    }

    public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryCreateInstance)))
        {
            return CallMethodWithResult(nameof(DynamicProxy<T>.TryCreateInstance), binder, GetArgArray(args), e => binder.FallbackCreateInstance(this, args, e));
        }

        return base.BindCreateInstance(binder, args);
    }

    public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryInvoke)))
        {
            return CallMethodWithResult(nameof(DynamicProxy<T>.TryInvoke), binder, GetArgArray(args), e => binder.FallbackInvoke(this, args, e));
        }

        return base.BindInvoke(binder, args);
    }

    public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryBinaryOperation)))
        {
            return CallMethodWithResult(nameof(DynamicProxy<T>.TryBinaryOperation), binder, GetArgs(arg), e => binder.FallbackBinaryOperation(this, arg, e));
        }

        return base.BindBinaryOperation(binder, arg);
    }

    public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) =>
        IsOverridden(nameof(DynamicProxy<T>.TryUnaryOperation))
            ? CallMethodWithResult(nameof(DynamicProxy<T>.TryUnaryOperation), binder, NoArgs, e => binder.FallbackUnaryOperation(this, e))
            : base.BindUnaryOperation(binder);

    public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryGetIndex)))
        {
            return CallMethodWithResult(nameof(DynamicProxy<T>.TryGetIndex), binder, GetArgArray(indexes), e => binder.FallbackGetIndex(this, indexes, e));
        }

        return base.BindGetIndex(binder, indexes);
    }

    public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TrySetIndex)))
        {
            return CallMethodReturnLast(nameof(DynamicProxy<T>.TrySetIndex), binder, GetArgArray(indexes, value), e => binder.FallbackSetIndex(this, indexes, value, e));
        }

        return base.BindSetIndex(binder, indexes, value);
    }

    public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
    {
        if (IsOverridden(nameof(DynamicProxy<T>.TryDeleteIndex)))
        {
            return CallMethodNoResult(nameof(DynamicProxy<T>.TryDeleteIndex), binder, GetArgArray(indexes), e => binder.FallbackDeleteIndex(this, indexes, e));
        }

        return base.BindDeleteIndex(binder, indexes);
    }

    delegate DynamicMetaObject Fallback(DynamicMetaObject? errorSuggestion);

    static Expression[] NoArgs => Array.Empty<Expression>();

    static IEnumerable<Expression> GetArgs(params DynamicMetaObject[] args) =>
        args.Select(arg =>
        {
            var exp = arg.Expression;
            return exp.Type.IsValueType ? Expression.Convert(exp, typeof(object)) : exp;
        });

    static Expression[] GetArgArray(DynamicMetaObject[] args) =>
        new[] {Expression.NewArrayInit(typeof(object), GetArgs(args))};

    static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value)
    {
        var exp = value.Expression;
        return new[]
        {
            Expression.NewArrayInit(typeof(object), GetArgs(args)),
            exp.Type.IsValueType ? Expression.Convert(exp, typeof(object)) : exp
        };
    }

    static ConstantExpression Constant(DynamicMetaObjectBinder binder)
    {
        var type = binder.GetType();
        while (!type.IsVisible)
        {
            type = type.BaseType!;
        }

        return Expression.Constant(binder, type);
    }

    /// <summary>
    /// Helper method for generating a MetaObject which calls a
    /// specific method on Dynamic that returns a result
    /// </summary>
    DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback, Fallback? fallbackInvoke = null)
    {
        //
        // First, call fallback to do default binding
        // This produces either an error or a call to a .NET member
        //
        var fallbackResult = fallback(null);

        return BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);
    }

    DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, DynamicMetaObject fallbackResult, Fallback? fallbackInvoke)
    {
        //
        // Build a new expression like:
        // {
        //   object result;
        //   TryGetMember(payload, out result) ? fallbackInvoke(result) : fallbackResult
        // }
        //
        var result = Expression.Parameter(typeof(object), null);

        Expression[] callArgs =
            [
            Expression.Convert(Expression, typeof(T)),
            Constant(binder),
            ..args,
            result
            ];

        var resultMetaObject = new DynamicMetaObject(result, BindingRestrictions.Empty);

        // Need to add a conversion if calling TryConvert
        if (binder.ReturnType != typeof(object))
        {
            var convert = Expression.Convert(resultMetaObject.Expression, binder.ReturnType);
            // will always be a cast or unbox

            resultMetaObject = new(convert, resultMetaObject.Restrictions);
        }

        if (fallbackInvoke != null)
        {
            resultMetaObject = fallbackInvoke(resultMetaObject);
        }

        var callDynamic = new DynamicMetaObject(
            Expression.Block(
                new[] {result},
                Expression.Condition(
                    Expression.Call(
                        Expression.Constant(proxy),
                        typeof(DynamicProxy<T>).GetMethod(methodName)!,
                        callArgs
                    ),
                    resultMetaObject.Expression,
                    fallbackResult.Expression,
                    binder.ReturnType
                )
            ),
            GetRestrictions().Merge(resultMetaObject.Restrictions).Merge(fallbackResult.Restrictions)
        );

        return callDynamic;
    }

    /// <summary>
    /// Helper method for generating a MetaObject which calls a
    /// specific method on Dynamic, but uses one of the arguments for
    /// the result.
    /// </summary>
    DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, IEnumerable<Expression> args, Fallback fallback)
    {
        //
        // First, call fallback to do default binding
        // This produces either an error or a call to a .NET member
        //
        var fallbackResult = fallback(null);

        //
        // Build a new expression like:
        // {
        //   object result;
        //   TrySetMember(payload, result = value) ? result : fallbackResult
        // }
        //
        var result = Expression.Parameter(typeof(object), null);

        Expression[] callArgs =
            [
            Expression.Convert(Expression, typeof(T)),
            Constant(binder),
            ..args
            ];

        callArgs[^1] = Expression.Assign(result, callArgs[^1]);

        return new(
            Expression.Block(
                new[] {result},
                Expression.Condition(
                    Expression.Call(
                        Expression.Constant(proxy),
                        typeof(DynamicProxy<T>).GetMethod(methodName)!,
                        callArgs
                    ),
                    result,
                    fallbackResult.Expression,
                    typeof(object)
                )
            ),
            GetRestrictions().Merge(fallbackResult.Restrictions)
        );
    }

    /// <summary>
    /// Helper method for generating a MetaObject which calls a
    /// specific method on Dynamic, but uses one of the arguments for
    /// the result.
    /// </summary>
    DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
    {
        //
        // First, call fallback to do default binding
        // This produces either an error or a call to a .NET member
        //
        var fallbackResult = fallback(null);

        Expression[] callArgs =
            [
            Expression.Convert(Expression, typeof(T)),
            Constant(binder),
            ..args
            ];

        //
        // Build a new expression like:
        //   if (TryDeleteMember(payload)) { } else { fallbackResult }
        //
        return new(
            Expression.Condition(
                Expression.Call(
                    Expression.Constant(proxy),
                    typeof(DynamicProxy<T>).GetMethod(methodName)!,
                    callArgs
                ),
                Expression.Empty(),
                fallbackResult.Expression,
                typeof(void)
            ),
            GetRestrictions().Merge(fallbackResult.Restrictions)
        );
    }

    /// <summary>
    /// Returns a Restrictions object which includes our current restrictions merged
    /// with a restriction limiting our type
    /// </summary>
    BindingRestrictions GetRestrictions()
    {
        if (Value == null && HasValue)
        {
            return BindingRestrictions.GetInstanceRestriction(Expression, null);
        }

        return BindingRestrictions.GetTypeRestriction(Expression, LimitType);
    }

    public override IEnumerable<string> GetDynamicMemberNames() =>
        proxy.GetDynamicMemberNames((T) Value!);

    // It is okay to throw NotSupported from this binder. This object
    // is only used by DynamicObject.GetMember--it is not expected to
    // (and cannot) implement binding semantics. It is just so the DO
    // can use the Name and IgnoreCase properties.
    sealed class GetBinderAdapter : GetMemberBinder
    {
        internal GetBinderAdapter(InvokeMemberBinder binder) :
            base(binder.Name, binder.IgnoreCase)
        {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject? errorSuggestion) =>
            throw new NotSupportedException();
    }
}