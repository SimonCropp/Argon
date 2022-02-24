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

class BidirectionalDictionary<TFirst, TSecond>
{
    readonly IDictionary<TFirst, TSecond> firstToSecond;
    readonly IDictionary<TSecond, TFirst> secondToFirst;
    readonly string duplicateFirstErrorMessage;
    readonly string duplicateSecondErrorMessage;

    public BidirectionalDictionary()
        : this(EqualityComparer<TFirst>.Default, EqualityComparer<TSecond>.Default)
    {
    }

    public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer)
        : this(
            firstEqualityComparer,
            secondEqualityComparer,
            "Duplicate item already exists for '{0}'.",
            "Duplicate item already exists for '{0}'.")
    {
    }

    public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer,
        string duplicateFirstErrorMessage, string duplicateSecondErrorMessage)
    {
        firstToSecond = new Dictionary<TFirst, TSecond>(firstEqualityComparer);
        secondToFirst = new Dictionary<TSecond, TFirst>(secondEqualityComparer);
        this.duplicateFirstErrorMessage = duplicateFirstErrorMessage;
        this.duplicateSecondErrorMessage = duplicateSecondErrorMessage;
    }

    public void Set(TFirst first, TSecond second)
    {
        if (firstToSecond.TryGetValue(first, out var existingSecond))
        {
            if (!existingSecond!.Equals(second))
            {
                throw new ArgumentException(string.Format(duplicateFirstErrorMessage, first));
            }
        }

        if (secondToFirst.TryGetValue(second, out var existingFirst))
        {
            if (!existingFirst!.Equals(first))
            {
                throw new ArgumentException(string.Format(duplicateSecondErrorMessage, second));
            }
        }

        firstToSecond.Add(first, second);
        secondToFirst.Add(second, first);
    }

    public bool TryGetByFirst(TFirst first, out TSecond second)
    {
        return firstToSecond.TryGetValue(first, out second);
    }

    public bool TryGetBySecond(TSecond second, out TFirst first)
    {
        return secondToFirst.TryGetValue(second, out first);
    }
}