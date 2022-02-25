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

class ReflectionMember
{
    public Type? MemberType { get; set; }
    public Func<object, object?>? Getter { get; set; }
    public Action<object, object?>? Setter { get; set; }
}

class ReflectionObject
{
    public ObjectConstructor<object>? Creator { get; }
    public IDictionary<string, ReflectionMember> Members { get; }

    ReflectionObject(ObjectConstructor<object>? creator)
    {
        Members = new Dictionary<string, ReflectionMember>();
        Creator = creator;
    }

    public object? GetValue(object target, string member)
    {
        var getter = Members[member].Getter!;
        return getter(target);
    }

    public Type GetType(string member)
    {
        return Members[member].MemberType!;
    }

    public static ReflectionObject Create(Type type, params string[] memberNames)
    {
        return Create(type, null, memberNames);
    }

    public static ReflectionObject Create(Type type, MethodBase? creator, params string[] memberNames)
    {
        var delegateFactory = JsonTypeReflector.ReflectionDelegateFactory;

        ObjectConstructor<object>? creatorConstructor = null;
        if (creator != null)
        {
            creatorConstructor = delegateFactory.CreateParameterizedConstructor(creator);
        }
        else
        {
            if (type.HasDefaultConstructor(false))
            {
                var ctor = delegateFactory.CreateDefaultConstructor<object>(type);

                creatorConstructor = _ => ctor();
            }
        }

        var d = new ReflectionObject(creatorConstructor);

        foreach (var memberName in memberNames)
        {
            var members = type.GetMember(memberName, BindingFlags.Instance | BindingFlags.Public);
            if (members.Length != 1)
            {
                throw new ArgumentException($"Expected a single member with the name '{memberName}'.");
            }

            var member = members.Single();

            var reflectionMember = new ReflectionMember();

            switch (member.MemberType)
            {
                case MemberTypes.Field:
                case MemberTypes.Property:
                    if (member.CanReadMemberValue(false))
                    {
                        reflectionMember.Getter = delegateFactory.CreateGet<object>(member);
                    }

                    if (member.CanSetMemberValue(false, false))
                    {
                        reflectionMember.Setter = delegateFactory.CreateSet<object>(member);
                    }
                    break;
                case MemberTypes.Method:
                    var method = (MethodInfo)member;
                    if (method.IsPublic)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 0 && method.ReturnType != typeof(void))
                        {
                            var call = delegateFactory.CreateMethodCall<object>(method);
                            reflectionMember.Getter = target => call(target);
                        }
                        else if (parameters.Length == 1 && method.ReturnType == typeof(void))
                        {
                            var call = delegateFactory.CreateMethodCall<object>(method);
                            reflectionMember.Setter = (target, arg) => call(target, arg);
                        }
                    }
                    break;
                default:
                    throw new ArgumentException($"Unexpected member type '{member.MemberType}' for member '{member.Name}'.");
            }

            reflectionMember.MemberType = member.GetMemberUnderlyingType();

            d.Members[memberName] = reflectionMember;
        }

        return d;
    }
}