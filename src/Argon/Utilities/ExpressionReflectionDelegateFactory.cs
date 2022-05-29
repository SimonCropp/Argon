// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Linq.Expressions;

class ExpressionReflectionDelegateFactory : ReflectionDelegateFactory
{
    static readonly ExpressionReflectionDelegateFactory instance = new();

    internal static ReflectionDelegateFactory Instance => instance;

    public override ObjectConstructor CreateParameterizedConstructor(MethodBase method)
    {
        var type = typeof(object);

        var argsParameterExpression = Expression.Parameter(typeof(object[]), "args");

        var callExpression = BuildMethodCall(method, type, null, argsParameterExpression);

        var lambdaExpression = Expression.Lambda(typeof(ObjectConstructor), callExpression, argsParameterExpression);

        var compiled = (ObjectConstructor) lambdaExpression.Compile();
        return compiled;
    }

    public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
    {
        var type = typeof(object);

        var targetParameterExpression = Expression.Parameter(type, "target");
        var argsParameterExpression = Expression.Parameter(typeof(object[]), "args");

        var callExpression = BuildMethodCall(method, type, targetParameterExpression, argsParameterExpression);

        var lambdaExpression = Expression.Lambda(typeof(MethodCall<T, object>), callExpression, targetParameterExpression, argsParameterExpression);

        var compiled = (MethodCall<T, object?>) lambdaExpression.Compile();
        return compiled;
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

                Expression indexExpression = Expression.Constant(i);

                Expression paramAccessorExpression = Expression.ArrayIndex(argsParameterExpression, indexExpression);

                var argExpression = EnsureCastExpression(paramAccessorExpression, parameterType, !isByRef);

                if (isByRef)
                {
                    var variable = Expression.Variable(parameterType);
                    refParameterMap.Add(new(argExpression, variable, parameter.IsOut));

                    argExpression = variable;
                }

                argsExpression[i] = argExpression;
            }
        }

        Expression callExpression;
        if (method.IsConstructor)
        {
            callExpression = Expression.New((ConstructorInfo) method, argsExpression);
        }
        else if (method.IsStatic)
        {
            callExpression = Expression.Call((MethodInfo) method, argsExpression);
        }
        else
        {
            var readParameter = EnsureCastExpression(targetParameterExpression!, method.DeclaringType!);

            callExpression = Expression.Call(readParameter, (MethodInfo) method, argsExpression);
        }

        if (method is MethodInfo m)
        {
            if (m.ReturnType != typeof(void))
            {
                callExpression = EnsureCastExpression(callExpression, type);
            }
            else
            {
                callExpression = Expression.Block(callExpression, Expression.Constant(null));
            }
        }
        else
        {
            callExpression = EnsureCastExpression(callExpression, type);
        }

        if (refParameterMap.Count > 0)
        {
            var variableExpressions = new List<ParameterExpression>();
            var bodyExpressions = new List<Expression>();
            foreach (var p in refParameterMap)
            {
                if (!p.IsOut)
                {
                    bodyExpressions.Add(Expression.Assign(p.Variable, p.Value));
                }

                variableExpressions.Add(p.Variable);
            }

            bodyExpressions.Add(callExpression);

            callExpression = Expression.Block(variableExpressions, bodyExpressions);
        }

        return callExpression;
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

            var lambdaExpression = Expression.Lambda(typeof(Func<T>), expression);

            var compiled = (Func<T>) lambdaExpression.Compile();
            return compiled;
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

        var parameterExpression = Expression.Parameter(instanceType, "instance");
        Expression resultExpression;

        var getMethod = property.GetMethod;
        if (getMethod == null)
        {
            throw new ArgumentException("Property does not have a getter.");
        }

        if (getMethod.IsStatic)
        {
            resultExpression = Expression.MakeMemberAccess(null, property);
        }
        else
        {
            var readParameter = EnsureCastExpression(parameterExpression, property.DeclaringType!);

            resultExpression = Expression.MakeMemberAccess(readParameter, property);
        }

        resultExpression = EnsureCastExpression(resultExpression, resultType);

        var lambdaExpression = Expression.Lambda(typeof(Func<T, object>), resultExpression, parameterExpression);

        var compiled = (Func<T, object?>) lambdaExpression.Compile();
        return compiled;
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

        var compiled = Expression.Lambda<Func<T, object?>>(fieldExpression, sourceParameter).Compile();
        return compiled;
    }

    public override Action<T, object?> CreateSet<T>(FieldInfo field)
    {
        // use reflection for structs
        // expression doesn't correctly set value
        if (field.DeclaringType!.IsValueType || field.IsInitOnly)
        {
            return LateBoundReflectionDelegateFactory.Instance.CreateSet<T>(field);
        }

        var sourceParameterExpression = Expression.Parameter(typeof(T), "source");
        var valueParameterExpression = Expression.Parameter(typeof(object), "value");

        Expression fieldExpression;
        if (field.IsStatic)
        {
            fieldExpression = Expression.Field(null, field);
        }
        else
        {
            var sourceExpression = EnsureCastExpression(sourceParameterExpression, field.DeclaringType);

            fieldExpression = Expression.Field(sourceExpression, field);
        }

        var valueExpression = EnsureCastExpression(valueParameterExpression, fieldExpression.Type);

        var assignExpression = Expression.Assign(fieldExpression, valueExpression);

        var lambdaExpression = Expression.Lambda(typeof(Action<T, object>), assignExpression, sourceParameterExpression, valueParameterExpression);

        var compiled = (Action<T, object?>) lambdaExpression.Compile();
        return compiled;
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

        Expression setExpression;
        if (setMethod.IsStatic)
        {
            setExpression = Expression.Call(setMethod, readValueParameter);
        }
        else
        {
            var readInstanceParameter = EnsureCastExpression(instanceParameter, property.DeclaringType);

            setExpression = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
        }

        var lambdaExpression = Expression.Lambda(typeof(Action<T, object?>), setExpression, instanceParameter, valueParameter);

        var compiled = (Action<T, object?>) lambdaExpression.Compile();
        return compiled;
    }

    static Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
    {
        var expressionType = expression.Type;

        // check if a cast or conversion is required
        if (expressionType == targetType || (!expressionType.IsValueType && targetType.IsAssignableFrom(expressionType)))
        {
            return expression;
        }

        if (targetType.IsValueType)
        {
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

        return Expression.Convert(expression, targetType);
    }
}