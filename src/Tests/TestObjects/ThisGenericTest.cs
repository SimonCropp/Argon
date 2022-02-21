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

namespace TestObjects;

public class ThisGenericTest<T> where T : IKeyValueId
{
    Dictionary<string, T> _dict1 = new();

    public string MyProperty { get; set; }

    public void Add(T item)
    {
        _dict1.Add(item.Key, item);
    }

    public T this[string key]
    {
        get => _dict1[key];
        set => _dict1[key] = value;
    }

    public T this[int id]
    {
        get { return _dict1.Values.FirstOrDefault(x => x.Id == id); }
        set
        {
            var item = this[id];

            if (item == null)
            {
                Add(value);
            }
            else
            {
                _dict1[item.Key] = value;
            }
        }
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public T[] TheItems
    {
        get => _dict1.Values.ToArray();
        set
        {
            foreach (var item in value)
            {
                Add(item);
            }
        }
    }
}