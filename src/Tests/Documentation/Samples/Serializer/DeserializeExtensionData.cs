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

namespace Argon.Tests.Documentation.Samples.Serializer;

public class DeserializeExtensionData : TestFixtureBase
{
    #region DeserializeExtensionDataTypes
    public class DirectoryAccount
    {
        // normal deserialization
        public string DisplayName { get; set; }

        // these properties are set in OnDeserialized
        public string UserName { get; set; }
        public string Domain { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> _additionalData;

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            // SAMAccountName is not deserialized to any property
            // and so it is added to the extension data dictionary
            var samAccountName = (string)_additionalData["SAMAccountName"];

            Domain = samAccountName.Split('\\')[0];
            UserName = samAccountName.Split('\\')[1];
        }

        public DirectoryAccount()
        {
            _additionalData = new Dictionary<string, JToken>();
        }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeExtensionDataUsage
        var json = @"{
              'DisplayName': 'John Smith',
              'SAMAccountName': 'contoso\\johns'
            }";

        var account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

        Console.WriteLine(account.DisplayName);
        // John Smith

        Console.WriteLine(account.Domain);
        // contoso

        Console.WriteLine(account.UserName);
        // johns
        #endregion

        Assert.Equal("John Smith", account.DisplayName);
        Assert.Equal("contoso", account.Domain);
        Assert.Equal("johns", account.UserName);
    }
}