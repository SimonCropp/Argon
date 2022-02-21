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

using JsonConstructor = Argon.JsonConstructorAttribute;

namespace Argon.Tests.Documentation.Samples.Serializer;

public class JsonConstructorAttribute : TestFixtureBase
{
    #region JsonConstructorAttributeTypes
    public class User
    {
        public string UserName { get; private set; }
        public bool Enabled { get; private set; }

        public User()
        {
        }

        [JsonConstructor]
        public User(string userName, bool enabled)
        {
            UserName = userName;
            Enabled = enabled;
        }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region JsonConstructorAttributeUsage
        var json = @"{
              ""UserName"": ""domain\\username"",
              ""Enabled"": true
            }";

        var user = JsonConvert.DeserializeObject<User>(json);

        Console.WriteLine(user.UserName);
        // domain\username
        #endregion

        Assert.Equal(@"domain\username", user.UserName);
    }
}