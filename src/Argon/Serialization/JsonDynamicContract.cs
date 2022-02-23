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

using System.Dynamic;

namespace Argon;

/// <summary>
/// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
/// </summary>
public class JsonDynamicContract : JsonContainerContract
{
    /// <summary>
    /// Gets the object's properties.
    /// </summary>
    public JsonPropertyCollection Properties { get; }

    /// <summary>
    /// Gets or sets the property name resolver.
    /// </summary>
    public Func<string, string>? PropertyNameResolver { get; set; }

    readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object>>> callSiteGetters =
        new(CreateCallSiteGetter);

    readonly ThreadSafeStore<string, CallSite<Func<CallSite, object, object?, object>>> callSiteSetters =
        new(CreateCallSiteSetter);

    static CallSite<Func<CallSite, object, object>> CreateCallSiteGetter(string name)
    {
        var getMemberBinder = (GetMemberBinder)DynamicUtils.BinderWrapper.GetMember(name, typeof(DynamicUtils));

        return CallSite<Func<CallSite, object, object>>.Create(new NoThrowGetBinderMember(getMemberBinder));
    }

    static CallSite<Func<CallSite, object, object?, object>> CreateCallSiteSetter(string name)
    {
        var binder = (SetMemberBinder)DynamicUtils.BinderWrapper.SetMember(name, typeof(DynamicUtils));

        return CallSite<Func<CallSite, object, object?, object>>.Create(new NoThrowSetBinderMember(binder));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDynamicContract"/> class.
    /// </summary>
    public JsonDynamicContract(Type underlyingType)
        : base(underlyingType)
    {
        ContractType = JsonContractType.Dynamic;

        Properties = new JsonPropertyCollection(UnderlyingType);
    }

    internal bool TryGetMember(IDynamicMetaObjectProvider dynamicProvider, string name, out object? value)
    {
        var callSite = callSiteGetters.Get(name);

        var result = callSite.Target(callSite, dynamicProvider);

        if (ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult))
        {
            value = null;
            return false;
        }

        value = result;
        return true;
    }

    internal bool TrySetMember(IDynamicMetaObjectProvider dynamicProvider, string name, object? value)
    {
        var callSite = callSiteSetters.Get(name);

        var result = callSite.Target(callSite, dynamicProvider, value);

        return !ReferenceEquals(result, NoThrowExpressionVisitor.ErrorResult);
    }
}