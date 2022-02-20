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
    public abstract ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method);
    public abstract Func<T> CreateDefaultConstructor<T>(Type type);
    public abstract Func<T, object?> CreateGet<T>(PropertyInfo property);
    public abstract Func<T, object?> CreateGet<T>(FieldInfo field);
    public abstract Action<T, object?> CreateSet<T>(FieldInfo field);
    public abstract Action<T, object?> CreateSet<T>(PropertyInfo property);
}