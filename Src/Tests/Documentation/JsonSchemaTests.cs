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
using System.Collections.Generic;
using System.IO;
using Argon.Linq;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Schema;

namespace Argon.Tests.Documentation
{
    public class JsonSchemaTests
    {
        public void IsValidBasic()
        {
            #region IsValidBasic
            var schemaJson = @"{
              'description': 'A person',
              'type': 'object',
              'properties':
              {
                'name': {'type':'string'},
                'hobbies': {
                  'type': 'array',
                  'items': {'type':'string'}
                }
              }
            }";

            var schema = JsonSchema.Parse(schemaJson);

            var person = JObject.Parse(@"{
              'name': 'James',
              'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
            }");

            var valid = person.IsValid(schema);
            // true
            #endregion
        }

        public void IsValidMessages()
        {
            var schemaJson = @"{
               'description': 'A person',
               'type': 'object',
               'properties':
               {
                 'name': {'type':'string'},
                 'hobbies': {
                   'type': 'array',
                   'items': {'type':'string'}
                 }
               }
             }";

            #region IsValidMessages
            var schema = JsonSchema.Parse(schemaJson);

            var person = JObject.Parse(@"{
              'name': null,
              'hobbies': ['Invalid content', 0.123456789]
            }");

            IList<string> messages;
            var valid = person.IsValid(schema, out messages);
            // false
            // Invalid type. Expected String but got Null. Line 2, position 21.
            // Invalid type. Expected String but got Float. Line 3, position 51.
            #endregion
        }

        public void JsonValidatingReader()
        {
            var schemaJson = "{}";

            #region JsonValidatingReader
            var json = @"{
              'name': 'James',
              'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
            }";

            var reader = new JsonTextReader(new StringReader(json));

            var validatingReader = new JsonValidatingReader(reader);
            validatingReader.Schema = JsonSchema.Parse(schemaJson);

            IList<string> messages = new List<string>();
            validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

            var serializer = new JsonSerializer();
            var p = serializer.Deserialize<Person>(validatingReader);
            #endregion
        }

        public void LoadJsonSchema()
        {
            #region LoadJsonSchema
            // load from a string
            var schema1 = JsonSchema.Parse(@"{'type':'object'}");

            // load from a file
            using (TextReader reader = File.OpenText(@"c:\schema\Person.json"))
            {
                var schema2 = JsonSchema.Read(new JsonTextReader(reader));

                // do stuff
            }
            #endregion
        }

        public void ManuallyCreateJsonSchema()
        {
            #region ManuallyCreateJsonSchema
            var schema = new JsonSchema();
            schema.Type = JsonSchemaType.Object;
            schema.Properties = new Dictionary<string, JsonSchema>
            {
                { "name", new JsonSchema { Type = JsonSchemaType.String } },
                {
                    "hobbies", new JsonSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new List<JsonSchema> { new JsonSchema { Type = JsonSchemaType.String } }
                    }
                },
            };

            var person = JObject.Parse(@"{
              'name': 'James',
              'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox', 'LOLCATS']
            }");

            var valid = person.IsValid(schema);
            // true
            #endregion

            Assert.IsTrue(valid);
        }
    }
}

#pragma warning restore 618
