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

public class NamingStrategySkipSpecifiedNames : TestFixtureBase
{
    #region NamingStrategySkipSpecifiedNamesTypes
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "UPN")]
        public string Upn { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region NamingStrategySkipSpecifiedNamesUsage
        var user = new User
        {
            FirstName = "John",
            LastName = "Smith",
            Upn = "john.smith@acme.com"
        };

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                OverrideSpecifiedNames = false
            }
        };

        var json = JsonConvert.SerializeObject(user, new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented
        });

        Console.WriteLine(json);
        // {
        //   "firstName": "John",
        //   "lastName": "Smith",
        //   "UPN": "john.smith@acme.com"
        // }
        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""firstName"": ""John"",
  ""lastName"": ""Smith"",
  ""UPN"": ""john.smith@acme.com""
}", json);
    }
}