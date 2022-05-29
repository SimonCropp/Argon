// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

abstract class ReflectionDelegateFactory
{
    public Func<T, object?> CreateGet<T>(MemberInfo member)
    {
        if (member is PropertyInfo property)
        {
            // https://github.com/dotnet/corefx/issues/26053
            if (property.PropertyType.IsByRef)
            {
                throw new InvalidOperationException($"Could not create getter for {property}. ByRef return values are not supported.");
            }

            return CreateGet<T>(property);
        }

        if (member is FieldInfo field)
        {
            return CreateGet<T>(field);
        }

        throw new($"Could not create getter for {member}.");
    }

    public Action<T, object?> CreateSet<T>(MemberInfo member)
    {
        if (member is PropertyInfo property)
        {
            return CreateSet<T>(property);
        }

        if (member is FieldInfo field)
        {
            return CreateSet<T>(field);
        }

        throw new($"Could not create setter for {member}.");
    }

    public abstract MethodCall<T, object?> CreateMethodCall<T>(MethodBase method);
    public abstract ObjectConstructor CreateParameterizedConstructor(MethodBase method);
    public abstract Func<T> CreateDefaultConstructor<T>(Type type);
    public abstract Func<T, object?> CreateGet<T>(PropertyInfo property);
    public abstract Func<T, object?> CreateGet<T>(FieldInfo field);
    public abstract Action<T, object?> CreateSet<T>(FieldInfo field);
    public abstract Action<T, object?> CreateSet<T>(PropertyInfo property);
}