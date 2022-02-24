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

static class ReflectionUtils
{
    public static T GetValueOrDefault<T>(this T? target, T? fallback)
        where T : struct, Enum
    {
        if (target.HasValue)
        {
            return target.Value;
        }

        if (fallback.HasValue)
        {
            return fallback.Value;
        }

        return default;
    }

    public static bool IsVirtual(this PropertyInfo property)
    {
        return property.Method() is {IsVirtual: true};
    }

    static MethodInfo? Method(this PropertyInfo property)
    {
        var m = property.GetMethod;
        if (m != null)
        {
            return m;
        }

        m = property.SetMethod;
        if (m != null)
        {
            return m;
        }

        return null;
    }

    static MethodInfo? GetBaseDefinition(this PropertyInfo property)
    {
        return property.Method()?.GetBaseDefinition();
    }

    public static bool IsPublic(PropertyInfo property)
    {
        return property.Method() is {IsPublic: true};
    }

    public static Type? GetObjectType(object? v)
    {
        return v?.GetType();
    }

    public static string GetTypeName(Type type, TypeNameAssemblyFormatHandling? assemblyFormat, ISerializationBinder? binder)
    {
        var fullyQualifiedTypeName = GetFullyQualifiedTypeName(type, binder);

        return assemblyFormat switch
        {
            null => RemoveAssemblyDetails(fullyQualifiedTypeName),
            TypeNameAssemblyFormatHandling.Simple => RemoveAssemblyDetails(fullyQualifiedTypeName),
            TypeNameAssemblyFormatHandling.Full => fullyQualifiedTypeName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    static string GetFullyQualifiedTypeName(Type type, ISerializationBinder? binder)
    {
        if (binder == null)
        {
            return type.AssemblyQualifiedName;
        }

        binder.BindToName(type, out var assemblyName, out var typeName);
        return typeName + (assemblyName == null ? "" : $", {assemblyName}");
    }

    static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
    {
        var builder = new StringBuilder();

        // loop through the type name and filter out qualified assembly details from nested type names
        var writingAssemblyName = false;
        var skippingAssemblyDetails = false;
        var followBrackets = false;
        for (var i = 0; i < fullyQualifiedTypeName.Length; i++)
        {
            var current = fullyQualifiedTypeName[i];
            switch (current)
            {
                case '[':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = true;
                    builder.Append(current);
                    break;
                case ']':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = false;
                    builder.Append(current);
                    break;
                case ',':
                    if (followBrackets)
                    {
                        builder.Append(current);
                    }
                    else if (!writingAssemblyName)
                    {
                        writingAssemblyName = true;
                        builder.Append(current);
                    }
                    else
                    {
                        skippingAssemblyDetails = true;
                    }

                    break;
                default:
                    followBrackets = false;
                    if (!skippingAssemblyDetails)
                    {
                        builder.Append(current);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    public static bool HasDefaultConstructor(Type type, bool nonPublic)
    {
        if (type.IsValueType)
        {
            return true;
        }

        return GetDefaultConstructor(type, nonPublic) != null;
    }

    public static ConstructorInfo GetDefaultConstructor(this Type type)
    {
        return GetDefaultConstructor(type, false);
    }

    public static ConstructorInfo GetDefaultConstructor(this Type type, bool nonPublic)
    {
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
        if (nonPublic)
        {
            bindingFlags |= BindingFlags.NonPublic;
        }

        return type.GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());
    }

    public static bool IsNullable(Type type)
    {
        return !type.IsValueType || IsNullableType(type);
    }

    public static bool IsNullableType(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static Type EnsureNotNullableType(Type type)
    {
        if (IsNullableType(type))
        {
            return Nullable.GetUnderlyingType(type);
        }

        return type;
    }

    public static Type EnsureNotByRefType(Type type)
    {
        if (type.IsByRef &&
            type.HasElementType)
        {
            return type.GetElementType();
        }

        return type;
    }

    public static bool IsGenericDefinition(Type type, Type genericInterfaceDefinition)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var t = type.GetGenericTypeDefinition();
        return t == genericInterfaceDefinition;
    }

    public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
    {
        return ImplementsGenericDefinition(type, genericInterfaceDefinition, out _);
    }

    public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, [NotNullWhen(true)] out Type? implementingType)
    {
        if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
        {
            throw new ArgumentNullException($"'{genericInterfaceDefinition}' is not a generic interface definition.");
        }

        if (type.IsInterface)
        {
            if (type.IsGenericType)
            {
                var interfaceDefinition = type.GetGenericTypeDefinition();

                if (genericInterfaceDefinition == interfaceDefinition)
                {
                    implementingType = type;
                    return true;
                }
            }
        }

        foreach (var i in type.GetInterfaces())
        {
            if (i.IsGenericType)
            {
                var interfaceDefinition = i.GetGenericTypeDefinition();

                if (genericInterfaceDefinition == interfaceDefinition)
                {
                    implementingType = i;
                    return true;
                }
            }
        }

        implementingType = null;
        return false;
    }

    public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
    {
        if (!genericClassDefinition.IsClass || !genericClassDefinition.IsGenericTypeDefinition)
        {
            throw new ArgumentNullException($"'{genericClassDefinition}' is not a generic class definition.");
        }

        do
        {
            if (type.IsGenericType && genericClassDefinition == type.GetGenericTypeDefinition())
            {
                return true;
            }

            type = type.BaseType;
        } while (type != null);

        return false;
    }

    /// <summary>
    /// Gets the type of the typed collection's items.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The type of the typed collection's items.</returns>
    public static Type? GetCollectionItemType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out var genericListType))
        {
            if (genericListType.IsGenericTypeDefinition)
            {
                throw new($"Type {type} is not a collection.");
            }

            return genericListType.GetGenericArguments()[0];
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return null;
        }

        throw new($"Type {type} is not a collection.");
    }

    public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type? keyType, out Type? valueType)
    {
        if (ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<,>), out var genericDictionaryType))
        {
            if (genericDictionaryType.IsGenericTypeDefinition)
            {
                throw new($"Type {dictionaryType} is not a dictionary.");
            }

            var dictionaryGenericArguments = genericDictionaryType.GetGenericArguments();

            keyType = dictionaryGenericArguments[0];
            valueType = dictionaryGenericArguments[1];
            return;
        }

        if (typeof(IDictionary).IsAssignableFrom(dictionaryType))
        {
            keyType = null;
            valueType = null;
            return;
        }

        throw new($"Type {dictionaryType} is not a dictionary.");
    }

    /// <summary>
    /// Gets the member's underlying type.
    /// </summary>
    /// <returns>The underlying type of the member.</returns>
    public static Type GetMemberUnderlyingType(MemberInfo member)
    {
        return member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo) member).FieldType,
            MemberTypes.Property => ((PropertyInfo) member).PropertyType,
            MemberTypes.Event => ((EventInfo) member).EventHandlerType,
            MemberTypes.Method => ((MethodInfo) member).ReturnType,
            _ => throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, EventInfo or MethodInfo", nameof(member))
        };
    }

    public static bool IsByRefLikeType(Type type)
    {
        if (!type.IsValueType)
        {
            return false;
        }

        // IsByRefLike flag on type is not available in netstandard2.0
        var attributes = type.GetCustomAttributesData();
        return attributes.Any(t => string.Equals(t.AttributeType.FullName, "System.Runtime.CompilerServices.IsByRefLikeAttribute", StringComparison.Ordinal));
    }

    /// <summary>
    /// Determines whether the property is an indexed property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>
    /// 	<c>true</c> if the property is an indexed property; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsIndexedProperty(PropertyInfo property)
    {
        return property.GetIndexParameters().Length > 0;
    }

    /// <summary>
    /// Gets the member's value on the object.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <returns>The member's value on the object.</returns>
    public static object GetMemberValue(MemberInfo member, object target)
    {
        if (member is PropertyInfo property)
        {
            try
            {
                return property.GetValue(target, null);
            }
            catch (TargetParameterCountException e)
            {
                throw new ArgumentException($"MemberInfo '{member.Name}' has index parameters", e);
            }
        }

        if (member is FieldInfo field)
        {
            return field.GetValue(target);
        }

        throw new ArgumentException($"MemberInfo '{member.Name}' is not of type FieldInfo or PropertyInfo", nameof(member));
    }

    /// <summary>
    /// Sets the member's value on the target object.
    /// </summary>
    public static void SetMemberValue(MemberInfo member, object target, object? value)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                ((FieldInfo) member).SetValue(target, value);
                break;
            case MemberTypes.Property:
                ((PropertyInfo) member).SetValue(target, value, null);
                break;
            default:
                throw new ArgumentException($"MemberInfo '{member.Name}' must be of type FieldInfo or PropertyInfo", nameof(member));
        }
    }

    /// <summary>
    /// Determines whether the specified MemberInfo can be read.
    /// </summary>
    /// <param name="member">The MemberInfo to determine whether can be read.</param>
    /// /// <param name="nonPublic">if set to <c>true</c> then allow the member to be gotten non-publicly.</param>
    /// <returns>
    /// 	<c>true</c> if the specified MemberInfo can be read; otherwise, <c>false</c>.
    /// </returns>
    public static bool CanReadMemberValue(MemberInfo member, bool nonPublic)
    {
        if (member is PropertyInfo property)
        {
            if (!property.CanRead)
            {
                return false;
            }

            if (nonPublic)
            {
                return true;
            }

            return property.GetGetMethod(nonPublic) != null;
        }

        if (member is FieldInfo field)
        {
            return nonPublic || field.IsPublic;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified MemberInfo can be set.
    /// </summary>
    /// <param name="member">The MemberInfo to determine whether can be set.</param>
    /// <param name="nonPublic">if set to <c>true</c> then allow the member to be set non-publicly.</param>
    /// <param name="canSetReadOnly">if set to <c>true</c> then allow the member to be set if read-only.</param>
    /// <returns>
    /// 	<c>true</c> if the specified MemberInfo can be set; otherwise, <c>false</c>.
    /// </returns>
    public static bool CanSetMemberValue(MemberInfo member, bool nonPublic, bool canSetReadOnly)
    {
        if (member is PropertyInfo property)
        {
            if (!property.CanWrite)
            {
                return false;
            }

            if (nonPublic)
            {
                return true;
            }

            return property.GetSetMethod(nonPublic) != null;
        }

        if (member is FieldInfo field)
        {
            if (field.IsLiteral)
            {
                return false;
            }

            if (field.IsInitOnly && !canSetReadOnly)
            {
                return false;
            }

            return nonPublic || field.IsPublic;
        }

        return false;
    }

    public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
    {
        var targetMembers = new List<MemberInfo>();

        targetMembers.AddRange(GetFields(type, bindingAttr));
        targetMembers.AddRange(GetProperties(type, bindingAttr));

        // for some reason .NET returns multiple members when overriding a generic member on a base class
        // http://social.msdn.microsoft.com/Forums/en-US/b5abbfee-e292-4a64-8907-4e3f0fb90cd9/reflection-overriden-abstract-generic-properties?forum=netfxbcl
        // filter members to only return the override on the topmost class
        // update: I think this is fixed in .NET 3.5 SP1 - leave this in for now...
        var distinctMembers = new List<MemberInfo>(targetMembers.Count);

        foreach (var groupedMember in targetMembers.GroupBy(m => m.Name))
        {
            var count = groupedMember.Count();

            if (count == 1)
            {
                distinctMembers.Add(groupedMember.First());
            }
            else
            {
                var resolvedMembers = new List<MemberInfo>();
                foreach (var member in groupedMember)
                {
                    // this is a bit hacky
                    // if the hiding property is hiding a base property and it is virtual
                    // then this ensures the derived property gets used
                    if (resolvedMembers.Count == 0)
                    {
                        resolvedMembers.Add(member);
                    }
                    else if (!IsOverridenGenericMember(member, bindingAttr) || member.Name == "Item")
                    {
                        // two members with the same name were declared on a type
                        // this can be done via IL emit, e.g. Moq
                        if (resolvedMembers.Any(m => m.DeclaringType == member.DeclaringType))
                        {
                            continue;
                        }

                        resolvedMembers.Add(member);
                    }
                }

                distinctMembers.AddRange(resolvedMembers);
            }
        }

        return distinctMembers;
    }

    static bool IsOverridenGenericMember(MemberInfo member, BindingFlags bindingAttr)
    {
        if (member.MemberType != MemberTypes.Property)
        {
            return false;
        }

        var property = (PropertyInfo) member;
        if (!IsVirtual(property))
        {
            return false;
        }

        var declaringType = property.DeclaringType;
        if (!declaringType.IsGenericType)
        {
            return false;
        }

        var genericTypeDefinition = declaringType.GetGenericTypeDefinition();
        if (genericTypeDefinition == null)
        {
            return false;
        }

        var members = genericTypeDefinition.GetMember(property.Name, bindingAttr);
        if (members.Length == 0)
        {
            return false;
        }

        var memberUnderlyingType = GetMemberUnderlyingType(members[0]);
        return memberUnderlyingType.IsGenericParameter;
    }

    public static T? GetAttribute<T>(object attributeProvider, bool inherit) where T : Attribute
    {
        var attributes = GetAttributes<T>(attributeProvider, inherit);

        return attributes?.FirstOrDefault();
    }

    static T[] GetAttributes<T>(object attributeProvider, bool inherit) where T : Attribute
    {
        var a = GetAttributes(attributeProvider, typeof(T), inherit);

        if (a is T[] attributes)
        {
            return attributes;
        }

        return a.Cast<T>().ToArray();
    }

    public static Attribute[] GetAttributes(object attributeProvider, Type? attributeType, bool inherit)
    {
        var provider = attributeProvider;

        // http://hyperthink.net/blog/getcustomattributes-gotcha/
        // ICustomAttributeProvider doesn't do inheritance

        switch (provider)
        {
            case Type t:
                var array = attributeType != null ? t.GetCustomAttributes(attributeType, inherit) : t.GetCustomAttributes(inherit);
                var attributes = array.Cast<Attribute>().ToArray();
                return attributes;
            case Assembly a:
                return attributeType != null ? Attribute.GetCustomAttributes(a, attributeType) : Attribute.GetCustomAttributes(a);
            case MemberInfo mi:
                return attributeType != null ? Attribute.GetCustomAttributes(mi, attributeType, inherit) : Attribute.GetCustomAttributes(mi, inherit);
            case Module m:
                return attributeType != null ? Attribute.GetCustomAttributes(m, attributeType, inherit) : Attribute.GetCustomAttributes(m, inherit);
            case ParameterInfo p:
                return attributeType != null ? Attribute.GetCustomAttributes(p, attributeType, inherit) : Attribute.GetCustomAttributes(p, inherit);
            default:
                var customAttributeProvider = (ICustomAttributeProvider) attributeProvider;
                var result = attributeType != null ? customAttributeProvider.GetCustomAttributes(attributeType, inherit) : customAttributeProvider.GetCustomAttributes(inherit);

                return (Attribute[]) result;
        }
    }

    public static StructMultiKey<string?, string> SplitFullyQualifiedTypeName(string fullyQualifiedTypeName)
    {
        var assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

        string typeName;
        string? assemblyName;

        if (assemblyDelimiterIndex != null)
        {
            typeName = fullyQualifiedTypeName.Trim(0, assemblyDelimiterIndex.GetValueOrDefault());
            assemblyName = fullyQualifiedTypeName.Trim(assemblyDelimiterIndex.GetValueOrDefault() + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.GetValueOrDefault() - 1);
        }
        else
        {
            typeName = fullyQualifiedTypeName;
            assemblyName = null;
        }

        return new StructMultiKey<string?, string>(assemblyName, typeName);
    }

    static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
    {
        // we need to get the first comma following all surrounded in brackets because of generic types
        // e.g. System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
        var scope = 0;
        for (var i = 0; i < fullyQualifiedTypeName.Length; i++)
        {
            var current = fullyQualifiedTypeName[i];
            switch (current)
            {
                case '[':
                    scope++;
                    break;
                case ']':
                    scope--;
                    break;
                case ',':
                    if (scope == 0)
                    {
                        return i;
                    }

                    break;
            }
        }

        return null;
    }

    public static MemberInfo GetMemberInfoFromType(Type targetType, MemberInfo member)
    {
        const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        switch (member.MemberType)
        {
            case MemberTypes.Property:
                var property = (PropertyInfo) member;

                var types = property.GetIndexParameters().Select(p => p.ParameterType).ToArray();

                return targetType.GetProperty(property.Name, bindingAttr, null, property.PropertyType, types, null);
            default:
                return targetType.GetMember(member.Name, member.MemberType, bindingAttr).SingleOrDefault();
        }
    }

    public static IEnumerable<FieldInfo> GetFields(Type targetType, BindingFlags bindingAttr)
    {
        var fields = new List<MemberInfo>(targetType.GetFields(bindingAttr));
        // Type.GetFields doesn't return inherited private fields
        // manually find private fields from base class
        GetChildPrivateFields(fields, targetType, bindingAttr);

        return fields.Cast<FieldInfo>();
    }

    static void GetChildPrivateFields(IList<MemberInfo> initialFields, Type targetType, BindingFlags bindingAttr)
    {
        // fix weirdness with private FieldInfos only being returned for the current Type
        // find base type fields and add them to result
        if ((bindingAttr & BindingFlags.NonPublic) != 0)
        {
            // modify flags to not search for public fields
            var nonPublicBindingAttr = bindingAttr.RemoveFlag(BindingFlags.Public);

            while ((targetType = targetType.BaseType) != null)
            {
                // filter out protected fields
                var childPrivateFields =
                    targetType.GetFields(nonPublicBindingAttr).Where(f => f.IsPrivate);

                initialFields.AddRange(childPrivateFields);
            }
        }
    }

    public static IEnumerable<PropertyInfo> GetProperties(Type targetType, BindingFlags bindingAttr)
    {
        var properties = new List<PropertyInfo>(targetType.GetProperties(bindingAttr));

        // GetProperties on an interface doesn't return properties from its interfaces
        if (targetType.IsInterface)
        {
            foreach (var i in targetType.GetInterfaces())
            {
                properties.AddRange(i.GetProperties(bindingAttr));
            }
        }

        GetChildPrivateProperties(properties, targetType, bindingAttr);

        // a base class private getter/setter will be inaccessible unless the property was gotten from the base class
        for (var i = 0; i < properties.Count; i++)
        {
            var member = properties[i];
            if (member.DeclaringType != targetType)
            {
                var declaredMember = (PropertyInfo) GetMemberInfoFromType(member.DeclaringType, member);
                properties[i] = declaredMember;
            }
        }

        return properties;
    }

    static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
    {
        return (bindingAttr & flag) == flag
            ? bindingAttr ^ flag
            : bindingAttr;
    }

    static void GetChildPrivateProperties(IList<PropertyInfo> initialProperties, Type targetType, BindingFlags bindingAttr)
    {
        // fix weirdness with private PropertyInfos only being returned for the current Type
        // find base type properties and add them to result

        // also find base properties that have been hidden by subtype properties with the same name

        while ((targetType = targetType.BaseType) != null)
        {
            foreach (var property in targetType.GetProperties(bindingAttr))
            {
                if (property.IsVirtual())
                {
                    var subTypePropertyDeclaringType = property.GetBaseDefinition()?.DeclaringType ?? property.DeclaringType;

                    var index = initialProperties.IndexOf(p =>
                        p.Name == property.Name &&
                        p.IsVirtual() &&
                        (p.GetBaseDefinition()?.DeclaringType ?? p.DeclaringType).IsAssignableFrom(subTypePropertyDeclaringType));

                    // don't add a virtual property that has an override
                    if (index == -1)
                    {
                        initialProperties.Add(property);
                    }

                    continue;
                }

                if (IsPublic(property))
                {
                    var publicIndex = initialProperties.IndexOf(p => p.Name == property.Name
                                                                     && p.DeclaringType == property.DeclaringType);

                    if (publicIndex == -1)
                    {
                        initialProperties.Add(property);
                    }

                    continue;
                }

                // have to test on name rather than reference because instances are different
                // depending on the type that GetProperties was called on
                var nonPublicIndex = initialProperties.IndexOf(p => p.Name == property.Name);
                if (nonPublicIndex == -1)
                {
                    initialProperties.Add(property);
                    continue;
                }

                var childProperty = initialProperties[nonPublicIndex];
                // don't replace public child with private base
                if (!IsPublic(childProperty))
                {
                    // replace nonpublic properties for a child, but gotten from
                    // the parent with the one from the child
                    // the property gotten from the child will have access to private getter/setter
                    initialProperties[nonPublicIndex] = property;
                }
            }
        }
    }

    public static bool IsMethodOverridden(Type currentType, Type methodDeclaringType, string method)
    {
        return currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Any(info =>
                info.Name == method &&
                // check that the method overrides the original on DynamicObjectProxy
                info.DeclaringType != methodDeclaringType
                && info.GetBaseDefinition().DeclaringType == methodDeclaringType
            );
    }

    public static object? GetDefaultValue(Type type)
    {
        if (!type.IsValueType)
        {
            return null;
        }

        switch (ConvertUtils.GetTypeCode(type))
        {
            case PrimitiveTypeCode.Boolean:
                return false;
            case PrimitiveTypeCode.Char:
            case PrimitiveTypeCode.SByte:
            case PrimitiveTypeCode.Byte:
            case PrimitiveTypeCode.Int16:
            case PrimitiveTypeCode.UInt16:
            case PrimitiveTypeCode.Int32:
            case PrimitiveTypeCode.UInt32:
                return 0;
            case PrimitiveTypeCode.Int64:
            case PrimitiveTypeCode.UInt64:
                return 0L;
            case PrimitiveTypeCode.Single:
                return 0f;
            case PrimitiveTypeCode.Double:
                return 0.0;
            case PrimitiveTypeCode.Decimal:
                return 0m;
            case PrimitiveTypeCode.DateTime:
                return new DateTime();
            case PrimitiveTypeCode.BigInteger:
                return new BigInteger();
            case PrimitiveTypeCode.Guid:
                return new Guid();
            case PrimitiveTypeCode.DateTimeOffset:
                return new DateTimeOffset();
        }

        if (IsNullable(type))
        {
            return null;
        }

        // possibly use IL initobj for perf here?
        return Activator.CreateInstance(type);
    }
}