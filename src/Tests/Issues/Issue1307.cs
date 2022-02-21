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

public class Issue1307 : TestFixtureBase
{
    public class MyOtherClass
    {
        [JsonConverter(typeof(MyJsonConverter))]
        public MyClass2 InstanceOfMyClass { get; set; }
    }

    public class MyClass2
    {
        public int[] Dummy { get; set; }
    }

    internal class MyJsonConverter : JsonConverter
    {
        static readonly JsonLoadSettings _jsonLoadSettings = new() { CommentHandling = CommentHandling.Ignore };

        public override bool CanConvert(Type type)
        {
            return typeof(MyClass2).Equals(type);
        }

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader, _jsonLoadSettings);

            if (token.Type == JTokenType.Object)
            {
                return token.ToObject<MyClass2>();
            }

            if (token.Type == JTokenType.Array)
            {
                var result = new MyClass2
                {
                    Dummy = token.Select(t => (int)t).ToArray()
                };
                return result;
            }

            if (token.Type == JTokenType.Comment)
            {
                throw new InvalidProgramException();
            }
            return existingValue;
        }

        #region Do not use this converter for writing.

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        #endregion

    }

    [Fact]
    public void Test()
    {
        var json = @"{
  ""instanceOfMyClass"":
    /* Comment explaining that this is a legacy data contract: */
    [ 1, 2, 3 ]
}";

        var c = JsonConvert.DeserializeObject<MyOtherClass>(json);
        Assert.Equal(3, c.InstanceOfMyClass.Dummy.Length);
    }
}