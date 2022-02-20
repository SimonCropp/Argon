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

using Argon;

static class ReflectionUtils
{
    public static bool IsVirtual(this PropertyInfo propertyInfo)
    {
        ValidationUtils.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

        var m = propertyInfo.GetGetMethod(true);
        if (m != null && m.IsVirtual)
        {
            return true;
        }

        m = propertyInfo.GetSetMethod(true);
        if (m != null && m.IsVirtual)
        {
            return true;
        }

        return false;
    }

    public static MethodInfo? GetBaseDefinition(this PropertyInfo propertyInfo)
    {
        ValidationUtils.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

        var m = propertyInfo.GetGetMethod(true);
        if (m != null)
        {
            return m.GetBaseDefinition();
        }

        return propertyInfo.GetSetMethod(true)?.GetBaseDefinition();
    }

    public static bool IsPublic(PropertyInfo property)
    {
        var getMethod = property.GetGetMethod();
        if (getMethod != null && getMethod.IsPublic)
        {
            return true;
        }
        var setMethod = property.GetSetMethod();
        if (setMethod != null && setMethod.IsPublic)
        {
            return true;
        }

        return false;
    }

    public static Type? GetObjectType(object? v)
    {
        return v?.GetType();
    }

    public static string GetTypeName(Type t, TypeNameAssemblyFormatHandling assemblyFormat, ISerializationBinder? binder)
    {
        var fullyQualifiedTypeName = GetFullyQualifiedTypeName(t, binder);

        switch (assemblyFormat)
        {
            case TypeNameAssemblyFormatHandling.Simple:
                return RemoveAssemblyDetails(fullyQualifiedTypeName);
            case TypeNameAssemblyFormatHandling.Full:
                return fullyQualifiedTypeName;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    static string GetFullyQualifiedTypeName(Type t, ISerializationBinder? binder)
    {
        if (binder != null)
        {
            binder.BindToName(t, out var assemblyName, out var typeName);
            return typeName + (assemblyName == null ? "" : $", {assemblyName}");
        }

        return t.AssemblyQualifiedName;
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

    public static bool HasDefaultConstructor(Type t, bool nonPublic)
    {
        ValidationUtils.ArgumentNotNull(t, nameof(t));

        if (t.IsValueType)
        {
            return true;
        }

        return GetDefaultConstructor(t, nonPublic) != null;
    }

    public static ConstructorInfo GetDefaultConstructor(Type t)
    {
        return GetDefaultConstructor(t, false);
    }

    public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
    {
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
        if (nonPublic)
        {
            bindingFlags = bindingFlags | BindingFlags.NonPublic;
        }

        return t.GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());
    }

    public static bool IsNullable(Type t)
    {
        ValidationUtils.ArgumentNotNull(t, nameof(t));

        if (t.IsValueType)
        {
            return IsNullableType(t);
        }

        return true;
    }

    public static bool IsNullableType(Type t)
    {
        ValidationUtils.ArgumentNotNull(t, nameof(t));

        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static Type EnsureNotNullableType(Type t)
    {
        return IsNullableType(t)
            ? Nullable.GetUnderlyingType(t)
            : t;
    }

    public static Type EnsureNotByRefType(Type t)
    {
        return t.IsByRef && t.HasElementType
            ? t.GetElementType()
            : t;
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

    public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, [NotNullWhen(true)]out Type? implementingType)
    {
        ValidationUtils.ArgumentNotNull(type, nameof(type));
        ValidationUtils.ArgumentNotNull(genericInterfaceDefinition, nameof(genericInterfaceDefinition));

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
        return InheritsGenericDefinition(type, genericClassDefinition, out _);
    }

    public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type? implementingType)
    {
        ValidationUtils.ArgumentNotNull(type, nameof(type));
        ValidationUtils.ArgumentNotNull(genericClassDefinition, nameof(genericClassDefinition));

        if (!genericClassDefinition.IsClass || !genericClassDefinition.IsGenericTypeDefinition)
        {
            throw new ArgumentNullException($"'{genericClassDefinition}' is not a generic class definition.");
        }

        return InheritsGenericDefinitionInternal(type, genericClassDefinition, out implementingType);
    }

    static bool InheritsGenericDefinitionInternal(Type currentType, Type genericClassDefinition, out Type? implementingType)
    {
        do
        {
            if (currentType.IsGenericType && genericClassDefinition == currentType.GetGenericTypeDefinition())
            {
                implementingType = currentType;
                return true;
            }

            currentType = currentType.BaseType;
        }
        while (currentType != null);

        implementingType = null;
        return false;
    }

    /// <summary>
    /// Gets the type of the typed collection's items.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The type of the typed collection's items.</returns>
    public static Type? GetCollectionItemType(Type type)
    {
        ValidationUtils.ArgumentNotNull(type, nameof(type));

        if (type.IsArray)
        {
            return type.GetElementType();
        }
        if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out var genericListType))
        {
            if (genericListType!.IsGenericTypeDefinition)
            {
                throw new($"Type {type} is not a collection.");
            }

            return genericListType!.GetGenericArguments()[0];
        }
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return null;
        }

        throw new($"Type {type} is not a collection.");
    }

    public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type? keyType, out Type? valueType)
    {
        ValidationUtils.ArgumentNotNull(dictionaryType, nameof(dictionaryType));

        if (ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<,>), out var genericDictionaryType))
        {
            if (genericDictionaryType!.IsGenericTypeDefinition)
            {
                throw new($"Type {dictionaryType} is not a dictionary.");
            }

            var dictionaryGenericArguments = genericDictionaryType!.GetGenericArguments();

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
    /// <param name="member">The member.</param>
    /// <returns>The underlying type of the member.</returns>
    public static Type GetMemberUnderlyingType(MemberInfo member)
    {
        ValidationUtils.ArgumentNotNull(member, nameof(member));

        switch (member.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo)member).FieldType;
            case MemberTypes.Property:
                return ((PropertyInfo)member).PropertyType;
            case MemberTypes.Event:
                return ((EventInfo)member).EventHandlerType;
            case MemberTypes.Method:
                return ((MethodInfo)member).ReturnType;
            default:
                throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, EventInfo or MethodInfo", nameof(member));
        }
    }

    public static bool IsByRefLikeType(Type type)
    {
        if (!type.IsValueType)
        {
            return false;
        }

        // IsByRefLike flag on type is not available in netstandard2.0
        var attributes = GetAttributes(type, null, false);
        for (var i = 0; i < attributes.Length; i++)
        {
            if (string.Equals(attributes[i].GetType().FullName, "System.Runtime.CompilerServices.IsByRefLikeAttribute", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
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
        ValidationUtils.ArgumentNotNull(property, nameof(property));

        return property.GetIndexParameters().Length > 0;
    }

    /// <summary>
    /// Gets the member's value on the object.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The member's value on the object.</returns>
    public static object GetMemberValue(MemberInfo member, object target)
    {
        ValidationUtils.ArgumentNotNull(member, nameof(member));
        ValidationUtils.ArgumentNotNull(target, nameof(target));

        switch (member.MemberType)
        {
            case MemberTypes.Field:
                return ((FieldInfo)member).GetValue(target);
            case MemberTypes.Property:
                try
                {
                    return ((PropertyInfo)member).GetValue(target, null);
                }
                catch (TargetParameterCountException e)
                {
                    throw new ArgumentException($"MemberInfo '{member.Name}' has index parameters", e);
                }
            default:
                throw new ArgumentException($"MemberInfo '{member.Name}' is not of type FieldInfo or PropertyInfo", nameof(member));
        }
    }

    /// <summary>
    /// Sets the member's value on the target object.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <param name="target">The target.</param>
    /// <param name="value">The value.</param>
    public static void SetMemberValue(MemberInfo member, object target, object? value)
    {
        ValidationUtils.ArgumentNotNull(member, nameof(member));
        ValidationUtils.ArgumentNotNull(target, nameof(target));

        switch (member.MemberType)
        {
            case MemberTypes.Field:
                ((FieldInfo)member).SetValue(target, value);
                break;
            case MemberTypes.Property:
                ((PropertyInfo)member).SetValue(target, value, null);
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
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                var fieldInfo = (FieldInfo)member;

                if (nonPublic)
                {
                    return true;
                }
                else if (fieldInfo.IsPublic)
                {
                    return true;
                }
                return false;
            case MemberTypes.Property:
                var propertyInfo = (PropertyInfo)member;

                if (!propertyInfo.CanRead)
                {
                    return false;
                }
                if (nonPublic)
                {
                    return true;
                }
                return propertyInfo.GetGetMethod(nonPublic) != null;
            default:
                return false;
        }
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
        switch (member.MemberType)
        {
            case MemberTypes.Field:
                var fieldInfo = (FieldInfo)member;

                if (fieldInfo.IsLiteral)
                {
                    return false;
                }
                if (fieldInfo.IsInitOnly && !canSetReadOnly)
                {
                    return false;
                }
                if (nonPublic)
                {
                    return true;
                }
                if (fieldInfo.IsPublic)
                {
                    return true;
                }
                return false;
            case MemberTypes.Property:
                var propertyInfo = (PropertyInfo)member;

                if (!propertyInfo.CanWrite)
                {
                    return false;
                }
                if (nonPublic)
                {
                    return true;
                }
                return propertyInfo.GetSetMethod(nonPublic) != null;
            default:
                return false;
        }
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
                foreach (var memberInfo in groupedMember)
                {
                    // this is a bit hacky
                    // if the hiding property is hiding a base property and it is virtual
                    // then this ensures the derived property gets used
                    if (resolvedMembers.Count == 0)
                    {
                        resolvedMembers.Add(memberInfo);
                    }
                    else if (!IsOverridenGenericMember(memberInfo, bindingAttr) || memberInfo.Name == "Item")
                    {
                        // two members with the same name were declared on a type
                        // this can be done via IL emit, e.g. Moq
                        if (resolvedMembers.Any(m => m.DeclaringType == memberInfo.DeclaringType))
                        {
                            continue;
                        }

                        resolvedMembers.Add(memberInfo);
                    }
                }

                distinctMembers.AddRange(resolvedMembers);
            }
        }

        return distinctMembers;
    }

    static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
    {
        if (memberInfo.MemberType != MemberTypes.Property)
        {
            return false;
        }

        var propertyInfo = (PropertyInfo)memberInfo;
        if (!IsVirtual(propertyInfo))
        {
            return false;
        }

        var declaringType = propertyInfo.DeclaringType;
        if (!declaringType.IsGenericType)
        {
            return false;
        }
        var genericTypeDefinition = declaringType.GetGenericTypeDefinition();
        if (genericTypeDefinition == null)
        {
            return false;
        }
        var members = genericTypeDefinition.GetMember(propertyInfo.Name, bindingAttr);
        if (members.Length == 0)
        {
            return false;
        }
        var memberUnderlyingType = GetMemberUnderlyingType(members[0]);
        if (!memberUnderlyingType.IsGenericParameter)
        {
            return false;
        }

        return true;
    }

    public static T? GetAttribute<T>(object attributeProvider) where T : Attribute
    {
        return GetAttribute<T>(attributeProvider, true);
    }

    public static T? GetAttribute<T>(object attributeProvider, bool inherit) where T : Attribute
    {
        var attributes = GetAttributes<T>(attributeProvider, inherit);

        return attributes?.FirstOrDefault();
    }

    public static T[] GetAttributes<T>(object attributeProvider, bool inherit) where T : Attribute
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
        ValidationUtils.ArgumentNotNull(attributeProvider, nameof(attributeProvider));

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
                var customAttributeProvider = (ICustomAttributeProvider)attributeProvider;
                var result = attributeType != null ? customAttributeProvider.GetCustomAttributes(attributeType, inherit) : customAttributeProvider.GetCustomAttributes(inherit);

                return (Attribute[])result;
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

    public static MemberInfo GetMemberInfoFromType(Type targetType, MemberInfo memberInfo)
    {
        const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        switch (memberInfo.MemberType)
        {
            case MemberTypes.Property:
                var propertyInfo = (PropertyInfo)memberInfo;

                var types = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();

                return targetType.GetProperty(propertyInfo.Name, bindingAttr, null, propertyInfo.PropertyType, types, null);
            default:
                return targetType.GetMember(memberInfo.Name, memberInfo.MemberType, bindingAttr).SingleOrDefault();
        }
    }

    public static IEnumerable<FieldInfo> GetFields(Type targetType, BindingFlags bindingAttr)
    {
        ValidationUtils.ArgumentNotNull(targetType, nameof(targetType));

        var fieldInfos = new List<MemberInfo>(targetType.GetFields(bindingAttr));
        // Type.GetFields doesn't return inherited private fields
        // manually find private fields from base class
        GetChildPrivateFields(fieldInfos, targetType, bindingAttr);

        return fieldInfos.Cast<FieldInfo>();
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
        ValidationUtils.ArgumentNotNull(targetType, nameof(targetType));

        var propertyInfos = new List<PropertyInfo>(targetType.GetProperties(bindingAttr));

        // GetProperties on an interface doesn't return properties from its interfaces
        if (targetType.IsInterface)
        {
            foreach (var i in targetType.GetInterfaces())
            {
                propertyInfos.AddRange(i.GetProperties(bindingAttr));
            }
        }

        GetChildPrivateProperties(propertyInfos, targetType, bindingAttr);

        // a base class private getter/setter will be inaccessible unless the property was gotten from the base class
        for (var i = 0; i < propertyInfos.Count; i++)
        {
            var member = propertyInfos[i];
            if (member.DeclaringType != targetType)
            {
                var declaredMember = (PropertyInfo)GetMemberInfoFromType(member.DeclaringType, member);
                propertyInfos[i] = declaredMember;
            }
        }

        return propertyInfos;
    }

    public static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
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
            foreach (var propertyInfo in targetType.GetProperties(bindingAttr))
            {
                var subTypeProperty = propertyInfo;

                if (!subTypeProperty.IsVirtual())
                {
                    if (!IsPublic(subTypeProperty))
                    {
                        // have to test on name rather than reference because instances are different
                        // depending on the type that GetProperties was called on
                        var index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name);
                        if (index == -1)
                        {
                            initialProperties.Add(subTypeProperty);
                        }
                        else
                        {
                            var childProperty = initialProperties[index];
                            // don't replace public child with private base
                            if (!IsPublic(childProperty))
                            {
                                // replace nonpublic properties for a child, but gotten from
                                // the parent with the one from the child
                                // the property gotten from the child will have access to private getter/setter
                                initialProperties[index] = subTypeProperty;
                            }
                        }
                    }
                    else
                    {
                        var index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                                                                   && p.DeclaringType == subTypeProperty.DeclaringType);

                        if (index == -1)
                        {
                            initialProperties.Add(subTypeProperty);
                        }
                    }
                }
                else
                {
                    var subTypePropertyDeclaringType = subTypeProperty.GetBaseDefinition()?.DeclaringType ?? subTypeProperty.DeclaringType;

                    var index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                                                               && p.IsVirtual()
                                                               && (p.GetBaseDefinition()?.DeclaringType ?? p.DeclaringType).IsAssignableFrom(subTypePropertyDeclaringType));

                    // don't add a virtual property that has an override
                    if (index == -1)
                    {
                        initialProperties.Add(subTypeProperty);
                    }
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