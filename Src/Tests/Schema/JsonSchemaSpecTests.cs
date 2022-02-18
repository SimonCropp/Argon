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

#pragma warning disable 618

using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using TestCaseSource = Xunit.MemberDataAttribute;

namespace Argon.Tests.Schema;

public class JsonSchemaSpecTest
{
    public string FileName { get; set; }
    public string TestCaseDescription { get; set; }
    public JObject Schema { get; set; }
    public string TestDescription { get; set; }
    public JToken Data { get; set; }
    public bool IsValid { get; set; }
    public int TestNumber { get; set; }

    public override string ToString()
    {
        return FileName + " - " + TestCaseDescription + " - " + TestDescription;
    }
}

public class JsonSchemaSpecTests : TestFixtureBase
{
    [Theory]
    [TestCaseSource(nameof(GetSpecTestDetails))]
    public void SpecTest(JsonSchemaSpecTest jsonSchemaSpecTest)
    {
        var s = JsonSchema.Read(jsonSchemaSpecTest.Schema.CreateReader());

        var v = jsonSchemaSpecTest.Data.IsValid(s, out var e);
        var errorMessages = (e != null ? e.ToArray() : null) ?? new string[0];

        Assert.AreEqual(jsonSchemaSpecTest.IsValid, v);
    }

    public static IList<object[]> GetSpecTestDetails()
    {
        IList<JsonSchemaSpecTest> specTests = new List<JsonSchemaSpecTest>();

        // get test files location relative to the test project dll
        var baseTestPath = Path.Combine("Schema", "Specs");

        var testFiles = Directory.GetFiles(baseTestPath, "*.json", SearchOption.AllDirectories);

        // read through each of the *.json test files and extract the test details
        foreach (var testFile in testFiles)
        {
            var testJson = System.IO.File.ReadAllText(testFile);

            var a = JArray.Parse(testJson);

            foreach (JObject testCase in a)
            {
                foreach (JObject test in testCase["tests"])
                {
                    var jsonSchemaSpecTest = new JsonSchemaSpecTest
                    {
                        FileName = Path.GetFileName(testFile),
                        TestCaseDescription = (string)testCase["description"],
                        Schema = (JObject)testCase["schema"],
                        TestDescription = (string)test["description"],
                        Data = test["data"],
                        IsValid = (bool)test["valid"],
                        TestNumber = specTests.Count + 1
                    };

                    specTests.Add(jsonSchemaSpecTest);
                }
            }
        }

        specTests = specTests.Where(s => s.FileName != "dependencies.json"
                                         && s.TestCaseDescription != "multiple disallow subschema"
                                         && s.TestCaseDescription != "types from separate schemas are merged"
                                         && s.TestCaseDescription != "when types includes a schema it should fully validate the schema"
                                         && s.TestCaseDescription != "types can include schemas").ToList();

        return specTests.Select(s => new object[] { s }).ToList();
    }
}
#pragma warning restore 618