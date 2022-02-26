// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

class LateBoundReflectionDelegateFactory : ReflectionDelegateFactory
{
    static readonly LateBoundReflectionDelegateFactory instance = new();

    internal static ReflectionDelegateFactory Instance => instance;

    public override ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
    {
        if (method is ConstructorInfo c)
        {
            // don't convert to method group to avoid medium trust issues
            // https://github.com/JamesNK/Newtonsoft.Json/issues/476
            return a => c.Invoke(a);
        }

        return a => method.Invoke(null, a)!;
    }

    public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
    {
        if (method is ConstructorInfo c)
        {
            return (_, a) => c.Invoke(a);
        }

        return (o, a) => method.Invoke(o, a);
    }

    public override Func<T> CreateDefaultConstructor<T>(Type type)
    {
        if (type.IsValueType)
        {
            return () => (T)Activator.CreateInstance(type)!;
        }

        var constructorInfo = type.GetDefaultConstructor(true);

        return () => (T)constructorInfo.Invoke(null);
    }

    public override Func<T, object?> CreateGet<T>(PropertyInfo property)
    {
        return o => property.GetValue(o, null);
    }

    public override Func<T, object?> CreateGet<T>(FieldInfo field)
    {
        return o => field.GetValue(o);
    }

    public override Action<T, object?> CreateSet<T>(FieldInfo field)
    {
        return (o, v) => field.SetValue(o, v);
    }

    public override Action<T, object?> CreateSet<T>(PropertyInfo property)
    {
        return (o, v) => property.SetValue(o, v, null);
    }
}