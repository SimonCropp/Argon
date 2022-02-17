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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Newtonsoft.Json.Utilities;
using System.Linq;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
    internal static class JsonTypeReflector
    {
        private static bool? _dynamicCodeGeneration;
        private static bool? _fullyTrusted;

        public const string IdPropertyName = "$id";
        public const string RefPropertyName = "$ref";
        public const string TypePropertyName = "$type";
        public const string ValuePropertyName = "$value";
        public const string ArrayValuesPropertyName = "$values";

        public const string ShouldSerializePrefix = "ShouldSerialize";
        public const string SpecifiedPostfix = "Specified";

        public const string ConcurrentDictionaryTypeName = "System.Collections.Concurrent.ConcurrentDictionary`2";

        private static readonly ThreadSafeStore<Type, Func<object[]?, object>> CreatorCache = 
            new ThreadSafeStore<Type, Func<object[]?, object>>(GetCreator);

        private static readonly ThreadSafeStore<Type, Type?> AssociatedMetadataTypesCache = new ThreadSafeStore<Type, Type?>(GetAssociateMetadataTypeFromAttribute);
        private static ReflectionObject? _metadataTypeAttributeReflectionObject;

        public static T? GetCachedAttribute<T>(object attributeProvider) where T : Attribute
        {
            return CachedAttributeGetter<T>.GetAttribute(attributeProvider);
        }

        public static bool CanTypeDescriptorConvertString(Type type, out TypeConverter typeConverter)
        {
            typeConverter = TypeDescriptor.GetConverter(type);

            // use the objectType's TypeConverter if it has one and can convert to a string
            if (typeConverter != null)
            {
                Type converterType = typeConverter.GetType();

                if (!string.Equals(converterType.FullName, "System.ComponentModel.ComponentConverter", StringComparison.Ordinal)
                    && !string.Equals(converterType.FullName, "System.ComponentModel.ReferenceConverter", StringComparison.Ordinal)
                    && !string.Equals(converterType.FullName, "System.Windows.Forms.Design.DataSourceConverter", StringComparison.Ordinal)
                    && converterType != typeof(TypeConverter))
                {
                    return typeConverter.CanConvertTo(typeof(string));
                }

            }

            return false;
        }

        public static DataContractAttribute? GetDataContractAttribute(Type type)
        {
            // DataContractAttribute does not have inheritance
            Type currentType = type;

            while (currentType != null)
            {
                DataContractAttribute? result = CachedAttributeGetter<DataContractAttribute>.GetAttribute(currentType);
                if (result != null)
                {
                    return result;
                }

                currentType = currentType.BaseType();
            }

            return null;
        }

        public static DataMemberAttribute? GetDataMemberAttribute(MemberInfo memberInfo)
        {
            // DataMemberAttribute does not have inheritance

            // can't override a field
            if (memberInfo.MemberType() == MemberTypes.Field)
            {
                return CachedAttributeGetter<DataMemberAttribute>.GetAttribute(memberInfo);
            }

            // search property and then search base properties if nothing is returned and the property is virtual
            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
            DataMemberAttribute? result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(propertyInfo);
            if (result == null)
            {
                if (propertyInfo.IsVirtual())
                {
                    Type currentType = propertyInfo.DeclaringType;

                    while (result == null && currentType != null)
                    {
                        PropertyInfo baseProperty = (PropertyInfo)ReflectionUtils.GetMemberInfoFromType(currentType, propertyInfo);
                        if (baseProperty != null && baseProperty.IsVirtual())
                        {
                            result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(baseProperty);
                        }

                        currentType = currentType.BaseType();
                    }
                }
            }

            return result;
        }

        public static MemberSerialization GetObjectMemberSerialization(Type objectType, bool ignoreSerializableAttribute)
        {
            JsonObjectAttribute? objectAttribute = GetCachedAttribute<JsonObjectAttribute>(objectType);
            if (objectAttribute != null)
            {
                return objectAttribute.MemberSerialization;
            }

            DataContractAttribute? dataContractAttribute = GetDataContractAttribute(objectType);
            if (dataContractAttribute != null)
            {
                return MemberSerialization.OptIn;
            }

            if (!ignoreSerializableAttribute && IsSerializable(objectType))
            {
                return MemberSerialization.Fields;
            }

            // the default
            return MemberSerialization.OptOut;
        }

        public static JsonConverter? GetJsonConverter(object attributeProvider)
        {
            JsonConverterAttribute? converterAttribute = GetCachedAttribute<JsonConverterAttribute>(attributeProvider);

            if (converterAttribute != null)
            {
                Func<object[]?, object> creator = CreatorCache.Get(converterAttribute.ConverterType);
                if (creator != null)
                {
                    return (JsonConverter)creator(converterAttribute.ConverterParameters);
                }
            }

            return null;
        }

        /// <summary>
        /// Lookup and create an instance of the <see cref="JsonConverter"/> type described by the argument.
        /// </summary>
        /// <param name="converterType">The <see cref="JsonConverter"/> type to create.</param>
        /// <param name="args">Optional arguments to pass to an initializing constructor of the JsonConverter.
        /// If <c>null</c>, the default constructor is used.</param>
        public static JsonConverter CreateJsonConverterInstance(Type converterType, object[]? args)
        {
            Func<object[]?, object> converterCreator = CreatorCache.Get(converterType);
            return (JsonConverter)converterCreator(args);
        }

        public static NamingStrategy CreateNamingStrategyInstance(Type namingStrategyType, object[]? args)
        {
            Func<object[]?, object> converterCreator = CreatorCache.Get(namingStrategyType);
            return (NamingStrategy)converterCreator(args);
        }

        public static NamingStrategy? GetContainerNamingStrategy(JsonContainerAttribute containerAttribute)
        {
            if (containerAttribute.NamingStrategyInstance == null)
            {
                if (containerAttribute.NamingStrategyType == null)
                {
                    return null;
                }

                containerAttribute.NamingStrategyInstance = CreateNamingStrategyInstance(containerAttribute.NamingStrategyType, containerAttribute.NamingStrategyParameters);
            }

            return containerAttribute.NamingStrategyInstance;
        }

        private static Func<object[]?, object> GetCreator(Type type)
        {
            Func<object>? defaultConstructor = (ReflectionUtils.HasDefaultConstructor(type, false))
                ? ReflectionDelegateFactory.CreateDefaultConstructor<object>(type)
                : null;

            return (parameters) =>
            {
                try
                {
                    if (parameters != null)
                    {
                        Type[] paramTypes = parameters.Select(param =>
                        {
                            if (param == null)
                            {
                                throw new InvalidOperationException("Cannot pass a null parameter to the constructor.");
                            }

                            return param.GetType();
                        }).ToArray();
                        ConstructorInfo parameterizedConstructorInfo = type.GetConstructor(paramTypes);

                        if (parameterizedConstructorInfo != null)
                        {
                            ObjectConstructor<object> parameterizedConstructor = ReflectionDelegateFactory.CreateParameterizedConstructor(parameterizedConstructorInfo);
                            return parameterizedConstructor(parameters);
                        }
                        else
                        {
                            throw new JsonException("No matching parameterized constructor found for '{0}'.".FormatWith(CultureInfo.InvariantCulture, type));
                        }
                    }

                    if (defaultConstructor == null)
                    {
                        throw new JsonException("No parameterless constructor defined for '{0}'.".FormatWith(CultureInfo.InvariantCulture, type));
                    }

                    return defaultConstructor();
                }
                catch (Exception ex)
                {
                    throw new JsonException("Error creating '{0}'.".FormatWith(CultureInfo.InvariantCulture, type), ex);
                }
            };
        }

        private static Type? GetAssociatedMetadataType(Type type)
        {
            return AssociatedMetadataTypesCache.Get(type);
        }

        private static Type? GetAssociateMetadataTypeFromAttribute(Type type)
        {
            Attribute[] customAttributes = ReflectionUtils.GetAttributes(type, null, true);

            foreach (Attribute attribute in customAttributes)
            {
                Type attributeType = attribute.GetType();

                // only test on attribute type name
                // attribute assembly could change because of type forwarding, etc
                if (string.Equals(attributeType.FullName, "System.ComponentModel.DataAnnotations.MetadataTypeAttribute", StringComparison.Ordinal))
                {
                    const string metadataClassTypeName = "MetadataClassType";

                    if (_metadataTypeAttributeReflectionObject == null)
                    {
                        _metadataTypeAttributeReflectionObject = ReflectionObject.Create(attributeType, metadataClassTypeName);
                    }

                    return (Type?)_metadataTypeAttributeReflectionObject.GetValue(attribute, metadataClassTypeName);
                }
            }

            return null;
        }

        private static T? GetAttribute<T>(Type type) where T : Attribute
        {
            T? attribute;

            Type? metadataType = GetAssociatedMetadataType(type);
            if (metadataType != null)
            {
                attribute = ReflectionUtils.GetAttribute<T>(metadataType, true);
                if (attribute != null)
                {
                    return attribute;
                }
            }

            attribute = ReflectionUtils.GetAttribute<T>(type, true);
            if (attribute != null)
            {
                return attribute;
            }

            foreach (Type typeInterface in type.GetInterfaces())
            {
                attribute = ReflectionUtils.GetAttribute<T>(typeInterface, true);
                if (attribute != null)
                {
                    return attribute;
                }
            }

            return null;
        }

        private static T? GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            T? attribute;

            Type? metadataType = GetAssociatedMetadataType(memberInfo.DeclaringType);
            if (metadataType != null)
            {
                MemberInfo metadataTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(metadataType, memberInfo);

                if (metadataTypeMemberInfo != null)
                {
                    attribute = ReflectionUtils.GetAttribute<T>(metadataTypeMemberInfo, true);
                    if (attribute != null)
                    {
                        return attribute;
                    }
                }
            }

            attribute = ReflectionUtils.GetAttribute<T>(memberInfo, true);
            if (attribute != null)
            {
                return attribute;
            }

            if (memberInfo.DeclaringType != null)
            {
                foreach (Type typeInterface in memberInfo.DeclaringType.GetInterfaces())
                {
                    MemberInfo interfaceTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(typeInterface, memberInfo);

                    if (interfaceTypeMemberInfo != null)
                    {
                        attribute = ReflectionUtils.GetAttribute<T>(interfaceTypeMemberInfo, true);
                        if (attribute != null)
                        {
                            return attribute;
                        }
                    }
                }
            }

            return null;
        }

        public static bool IsNonSerializable(object provider)
        {
            // no inheritance
            return (ReflectionUtils.GetAttribute<NonSerializedAttribute>(provider, false) != null);
        }

        public static bool IsSerializable(object provider)
        {
            // no inheritance
            return (ReflectionUtils.GetAttribute<SerializableAttribute>(provider, false) != null);
        }

        public static T? GetAttribute<T>(object provider) where T : Attribute
        {
            if (provider is Type type)
            {
                return GetAttribute<T>(type);
            }

            if (provider is MemberInfo memberInfo)
            {
                return GetAttribute<T>(memberInfo);
            }

            return ReflectionUtils.GetAttribute<T>(provider, true);
        }

#if DEBUG
        internal static void SetFullyTrusted(bool? fullyTrusted)
        {
            _fullyTrusted = fullyTrusted;
        }

        internal static void SetDynamicCodeGeneration(bool dynamicCodeGeneration)
        {
            _dynamicCodeGeneration = dynamicCodeGeneration;
        }
#endif

        public static bool DynamicCodeGeneration
        {
            [SecuritySafeCritical]
            get
            {
                if (_dynamicCodeGeneration == null)
                {
#if !NETSTANDARD2_0
                    try
                    {
                        new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
                        new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess).Demand();
                        new SecurityPermission(SecurityPermissionFlag.SkipVerification).Demand();
                        new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                        new SecurityPermission(PermissionState.Unrestricted).Demand();
                        _dynamicCodeGeneration = true;
                    }
                    catch (Exception)
                    {
                        _dynamicCodeGeneration = false;
                    }
#else
                    _dynamicCodeGeneration = false;
#endif
                }

                return _dynamicCodeGeneration.GetValueOrDefault();
            }
        }

        public static bool FullyTrusted
        {
            get
            {
                if (_fullyTrusted == null)
                {
                    AppDomain appDomain = AppDomain.CurrentDomain;

                    _fullyTrusted = appDomain.IsHomogenous && appDomain.IsFullyTrusted;
                }

                return _fullyTrusted.GetValueOrDefault();
            }
        }

        public static ReflectionDelegateFactory ReflectionDelegateFactory
        {
            get
            {
#if !NETSTANDARD2_0
                if (DynamicCodeGeneration)
                {
                    return DynamicReflectionDelegateFactory.Instance;
                }

                return LateBoundReflectionDelegateFactory.Instance;
#else
                return ExpressionReflectionDelegateFactory.Instance;
#endif
            }
        }
    }
}