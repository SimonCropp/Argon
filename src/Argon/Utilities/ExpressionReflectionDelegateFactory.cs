// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class ExpressionReflectionDelegateFactory : ReflectionDelegateFactory
{
    static readonly ExpressionReflectionDelegateFactory instance = new();

    internal static ReflectionDelegateFactory Instance => instance;

    public override ObjectConstructor CreateParameterizedConstructor(MethodBase method)
    {
        var type = typeof(object);

        var parameters = Expression.Parameter(typeof(object[]), "args");

        var call = BuildMethodCall(method, type, null, parameters);

        return Expression.Lambda<ObjectConstructor>(call, parameters)
            .Compile();
    }

    public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
    {
        var type = typeof(object);

        var targetParameter = Expression.Parameter(type, "target");
        var argsParameter = Expression.Parameter(typeof(object[]), "args");

        var call = BuildMethodCall(method, type, targetParameter, argsParameter);

        return Expression.Lambda<MethodCall<T, object?>>(call, targetParameter, argsParameter)
            .Compile();
    }

    class ByRefParameter
    {
        public Expression Value;
        public ParameterExpression Variable;
        public bool IsOut;

        public ByRefParameter(Expression value, ParameterExpression variable, bool isOut)
        {
            Value = value;
            Variable = variable;
            IsOut = isOut;
        }
    }

    static Expression BuildMethodCall(MethodBase method, Type type, ParameterExpression? targetParameterExpression, ParameterExpression argsParameterExpression)
    {
        var parametersInfo = method.GetParameters();

        Expression[] argsExpression;
        IList<ByRefParameter> refParameterMap;
        if (parametersInfo.Length == 0)
        {
            argsExpression = Array.Empty<Expression>();
            refParameterMap = Array.Empty<ByRefParameter>();
        }
        else
        {
            argsExpression = new Expression[parametersInfo.Length];
            refParameterMap = new List<ByRefParameter>();

            for (var i = 0; i < parametersInfo.Length; i++)
            {
                var parameter = parametersInfo[i];
                var parameterType = parameter.ParameterType;
                var isByRef = false;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType()!;
                    isByRef = true;
                }

                var index = Expression.Constant(i);

                var paramAccessor = Expression.ArrayIndex(argsParameterExpression, index);

                var argExpression = EnsureCastExpression(paramAccessor, parameterType, !isByRef);

                if (isByRef)
                {
                    var variable = Expression.Variable(parameterType);
                    refParameterMap.Add(new(argExpression, variable, parameter.IsOut));

                    argExpression = variable;
                }

                argsExpression[i] = argExpression;
            }
        }

        Expression call;
        if (method.IsConstructor)
        {
            call = Expression.New((ConstructorInfo) method, argsExpression);
        }
        else if (method.IsStatic)
        {
            call = Expression.Call((MethodInfo) method, argsExpression);
        }
        else
        {
            var readParameter = EnsureCastExpression(targetParameterExpression!, method.DeclaringType!);

            call = Expression.Call(readParameter, (MethodInfo) method, argsExpression);
        }

        if (method is MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType == typeof(void))
            {
                call = Expression.Block(call, Expression.Constant(null));
            }
            else
            {
                call = EnsureCastExpression(call, type);
            }
        }
        else
        {
            call = EnsureCastExpression(call, type);
        }

        if (refParameterMap.Count > 0)
        {
            var variableExpressions = new List<ParameterExpression>(refParameterMap.Count);
            var bodyExpressions = new List<Expression>();
            foreach (var p in refParameterMap)
            {
                if (!p.IsOut)
                {
                    bodyExpressions.Add(Expression.Assign(p.Variable, p.Value));
                }

                variableExpressions.Add(p.Variable);
            }

            bodyExpressions.Add(call);

            call = Expression.Block(variableExpressions, bodyExpressions);
        }

        return call;
    }

    public override Func<T> CreateDefaultConstructor<T>(Type type)
    {
        // avoid error from expressions compiler because of abstract class
        if (type.IsAbstract)
        {
            return () => (T) Activator.CreateInstance(type)!;
        }

        try
        {
            var resultType = typeof(T);

            Expression expression = Expression.New(type);

            expression = EnsureCastExpression(expression, resultType);

            return Expression.Lambda<Func<T>>(expression).Compile();
        }
        catch
        {
            // an error can be thrown if constructor is not valid on Win8
            // will have INVOCATION_FLAGS_NON_W8P_FX_API invocation flag
            return () => (T) Activator.CreateInstance(type)!;
        }
    }

    public override Func<T, object?> CreateGet<T>(PropertyInfo property)
    {
        var instanceType = typeof(T);
        var resultType = typeof(object);

        var instanceParameter = Expression.Parameter(instanceType, "instance");
        Expression result;

        var getMethod = property.GetMethod;
        if (getMethod == null)
        {
            throw new ArgumentException("Property does not have a getter.");
        }

        if (getMethod.IsStatic)
        {
            result = Expression.MakeMemberAccess(null, property);
        }
        else
        {
            var readParameter = EnsureCastExpression(instanceParameter, property.DeclaringType!);

            result = Expression.MakeMemberAccess(readParameter, property);
        }

        result = EnsureCastExpression(result, resultType);

        return Expression.Lambda<Func<T, object>>(result, instanceParameter)
            .Compile();
    }

    public override Func<T, object?> CreateGet<T>(FieldInfo field)
    {
        var sourceParameter = Expression.Parameter(typeof(T), "source");

        Expression fieldExpression;
        if (field.IsStatic)
        {
            fieldExpression = Expression.Field(null, field);
        }
        else
        {
            var sourceExpression = EnsureCastExpression(sourceParameter, field.DeclaringType!);

            fieldExpression = Expression.Field(sourceExpression, field);
        }

        fieldExpression = EnsureCastExpression(fieldExpression, typeof(object));

        return Expression.Lambda<Func<T, object?>>(fieldExpression, sourceParameter)
            .Compile();
    }

    public override Action<T, object?> CreateSet<T>(FieldInfo field)
    {
        // use reflection for structs
        // expression doesn't correctly set value
        if (field.DeclaringType!.IsValueType || field.IsInitOnly)
        {
            return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(field);
        }

        var sourceParameter = Expression.Parameter(typeof(T), "source");
        var valueParameter = Expression.Parameter(typeof(object), "value");

        Expression fieldExpression;
        if (field.IsStatic)
        {
            fieldExpression = Expression.Field(null, field);
        }
        else
        {
            var source = EnsureCastExpression(sourceParameter, field.DeclaringType);

            fieldExpression = Expression.Field(source, field);
        }

        var value = EnsureCastExpression(valueParameter, fieldExpression.Type);

        var assign = Expression.Assign(fieldExpression, value);

        return Expression.Lambda<Action<T, object?>>(assign, sourceParameter, valueParameter)
            .Compile();
    }

    public override Action<T, object?> CreateSet<T>(PropertyInfo property)
    {
        // use reflection for structs
        // expression doesn't correctly set value
        if (property.DeclaringType!.IsValueType)
        {
            return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(property);
        }

        var instanceType = typeof(T);
        var valueType = typeof(object);

        var instanceParameter = Expression.Parameter(instanceType, "instance");

        var valueParameter = Expression.Parameter(valueType, "value");
        var readValueParameter = EnsureCastExpression(valueParameter, property.PropertyType);

        var setMethod = property.SetMethod;
        if (setMethod == null)
        {
            throw new ArgumentException("Property does not have a setter.");
        }

        Expression set;
        if (setMethod.IsStatic)
        {
            set = Expression.Call(setMethod, readValueParameter);
        }
        else
        {
            var readInstanceParameter = EnsureCastExpression(instanceParameter, property.DeclaringType);

            set = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
        }

        return Expression.Lambda<Action<T, object?>>(set, instanceParameter, valueParameter)
            .Compile();
    }

    static Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
    {
        var expressionType = expression.Type;

        // check if a cast or conversion is required
        if (expressionType == targetType ||
            (!expressionType.IsValueType && targetType.IsAssignableFrom(expressionType)))
        {
            return expression;
        }

        if (!targetType.IsValueType)
        {
            return Expression.Convert(expression, targetType);
        }

        Expression convert = Expression.Unbox(expression, targetType);

        if (allowWidening && targetType.IsPrimitive)
        {
            var toTargetTypeMethod = typeof(Convert)
                .GetMethod($"To{targetType.Name}", new[] {typeof(object)});

            if (toTargetTypeMethod != null)
            {
                convert = Expression.Condition(
                    Expression.TypeIs(expression, targetType),
                    convert,
                    Expression.Call(toTargetTypeMethod, expression));
            }
        }

        return Expression.Condition(
            Expression.Equal(expression, Expression.Constant(null, typeof(object))),
            Expression.Default(targetType),
            convert);

    }
}