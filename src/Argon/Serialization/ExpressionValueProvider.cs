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

namespace Argon;

/// <summary>
/// Get and set values for a <see cref="MemberInfo"/> using dynamic methods.
/// </summary>
public class ExpressionValueProvider : IValueProvider
{
    readonly MemberInfo member;
    Func<object, object?>? _getter;
    Action<object, object?>? setter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionValueProvider"/> class.
    /// </summary>
    /// <param name="member">The member info.</param>
    public ExpressionValueProvider(MemberInfo member)
    {
        this.member = member;
    }

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="target">The target to set the value on.</param>
    /// <param name="value">The value to set on the target.</param>
    public void SetValue(object target, object? value)
    {
        try
        {
            setter ??= ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(member);

#if !RELEASE
            // dynamic method doesn't check whether the type is 'legal' to set
            // add this check for unit tests
            if (value == null)
            {
                if (!ReflectionUtils.GetMemberUnderlyingType(member).IsNullable())
                {
                    throw new JsonSerializationException($"Incompatible value. Cannot set {member} to null.");
                }
            }
            else if (!ReflectionUtils.GetMemberUnderlyingType(member).IsInstanceOfType(value))
            {
                throw new JsonSerializationException($"Incompatible value. Cannot set {member} to type {value.GetType()}.");
            }
#endif

            setter(target, value);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"Error setting value to '{member.Name}' on '{target.GetType()}'.", ex);
        }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="target">The target to get the value from.</param>
    /// <returns>The value.</returns>
    public object? GetValue(object target)
    {
        try
        {
            _getter ??= ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(member);

            return _getter(target);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"Error getting value from '{member.Name}' on '{target.GetType()}'.", ex);
        }
    }
}