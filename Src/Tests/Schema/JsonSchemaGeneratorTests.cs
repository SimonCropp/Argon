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
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Schema;

public class JsonSchemaGeneratorTests : TestFixtureBase
{
    [Fact]
    public void Generate_GenericDictionary()
    {
        var generator = new JsonSchemaGenerator();
        var schema = generator.Generate(typeof(Dictionary<string, List<string>>));

        var json = schema.ToString();

        StringAssert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": [
      ""array"",
      ""null""
    ],
    ""items"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    }
  }
}", json);

        var value = new Dictionary<string, List<string>>
        {
            {"HasValue", new List<string> {"first", "second", null}},
            {"NoValue", null}
        };

        var valueJson = JsonConvert.SerializeObject(value, Formatting.Indented);
        var o = JObject.Parse(valueJson);

        Xunit.Assert.True(o.IsValid(schema));
    }

#if !NET5_0_OR_GREATER
    [Fact]
    public void Generate_DefaultValueAttributeTestClass()
    {
        var generator = new JsonSchemaGenerator();
        var schema = generator.Generate(typeof(DefaultValueAttributeTestClass));

        var json = schema.ToString();

        StringAssert.AreEqual(@"{
  ""description"": ""DefaultValueAttributeTestClass description!"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
    ""TestField1"": {
      ""required"": true,
      ""type"": ""integer"",
      ""default"": 21
    },
    ""TestProperty1"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""TestProperty1Value""
    }
  }
}", json);
    }
#endif

    [Fact]
    public void Generate_Person()
    {
        var generator = new JsonSchemaGenerator();
        var schema = generator.Generate(typeof(Person));

        var json = schema.ToString();

        StringAssert.AreEqual(@"{
  ""id"": ""Person"",
  ""title"": ""Title!"",
  ""description"": ""JsonObjectAttribute description!"",
  ""type"": ""object"",
  ""properties"": {
    ""Name"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""BirthDate"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""LastModified"": {
      ""required"": true,
      ""type"": ""string""
    }
  }
}", json);
    }

    [Fact]
    public void Generate_UserNullable()
    {
        var generator = new JsonSchemaGenerator();
        var schema = generator.Generate(typeof(UserNullable));

        var json = schema.ToString();

        StringAssert.AreEqual(@"{
  ""type"": ""object"",
  ""properties"": {
    ""Id"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""FName"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""LName"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""RoleId"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""NullableRoleId"": {
      ""required"": true,
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""NullRoleId"": {
      ""required"": true,
      ""type"": [
        ""integer"",
        ""null""
      ]
    },
    ""Active"": {
      ""required"": true,
      ""type"": [
        ""boolean"",
        ""null""
      ]
    }
  }
}", json);
    }

    [Fact]
    public void Generate_RequiredMembersClass()
    {
        var generator = new JsonSchemaGenerator();
        var schema = generator.Generate(typeof(RequiredMembersClass));

        Assert.AreEqual(JsonSchemaType.String, schema.Properties["FirstName"].Type);
        Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["MiddleName"].Type);
        Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["LastName"].Type);
        Assert.AreEqual(JsonSchemaType.String, schema.Properties["BirthDate"].Type);
    }

    [Fact]
    public void Generate_Store()
    {
        var generator = new JsonSchemaGenerator();
        var schema = generator.Generate(typeof(Store));

        Assert.AreEqual(11, schema.Properties.Count);

        var productArraySchema = schema.Properties["product"];
        var productSchema = productArraySchema.Items[0];

        Assert.AreEqual(4, productSchema.Properties.Count);
    }

    [Fact]
    public void MissingSchemaIdHandlingTest()
    {
        var generator = new JsonSchemaGenerator();

        var schema = generator.Generate(typeof(Store));
        Assert.AreEqual(null, schema.Id);

        generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;
        schema = generator.Generate(typeof(Store));
        Assert.AreEqual(typeof(Store).FullName, schema.Id);

        generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseAssemblyQualifiedName;
        schema = generator.Generate(typeof(Store));
        Assert.AreEqual(typeof(Store).AssemblyQualifiedName, schema.Id);
    }

    [Fact]
    public void CircularReferenceError()
    {
        ExceptionAssert.Throws<Exception>(() =>
        {
            var generator = new JsonSchemaGenerator();
            generator.Generate(typeof(CircularReferenceClass));
        }, @"Unresolved circular reference for type 'Argon.Tests.TestObjects.CircularReferenceClass'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.");
    }

    [Fact]
    public void CircularReferenceWithTypeNameId()
    {
        var generator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName
        };

        var schema = generator.Generate(typeof(CircularReferenceClass), true);

        Assert.AreEqual(JsonSchemaType.String, schema.Properties["Name"].Type);
        Assert.AreEqual(typeof(CircularReferenceClass).FullName, schema.Id);
        Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Null, schema.Properties["Child"].Type);
        Assert.AreEqual(schema, schema.Properties["Child"]);
    }

    [Fact]
    public void CircularReferenceWithExplicitId()
    {
        var generator = new JsonSchemaGenerator();

        var schema = generator.Generate(typeof(CircularReferenceWithIdClass));

        Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["Name"].Type);
        Assert.AreEqual("MyExplicitId", schema.Id);
        Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Null, schema.Properties["Child"].Type);
        Assert.AreEqual(schema, schema.Properties["Child"]);
    }

    [Fact]
    public void GenerateSchemaForType()
    {
        var generator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName
        };

        var schema = generator.Generate(typeof(Type));

        Assert.AreEqual(JsonSchemaType.String, schema.Type);

        var json = JsonConvert.SerializeObject(typeof(Version), Formatting.Indented);

        var v = new JValue(json);
        Xunit.Assert.True(v.IsValid(schema));
    }

    [Fact]
    public void GenerateSchemaForISerializable()
    {
        var generator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName
        };

        var schema = generator.Generate(typeof(ISerializableTestObject));

        Assert.AreEqual(JsonSchemaType.Object, schema.Type);
        Assert.AreEqual(true, schema.AllowAdditionalProperties);
        Assert.AreEqual(null, schema.Properties);
    }

    [Fact]
    public void GenerateSchemaForDBNull()
    {
        var generator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName
        };

        var schema = generator.Generate(typeof(DBNull));

        Assert.AreEqual(JsonSchemaType.Null, schema.Type);
    }

    public class CustomDirectoryInfoMapper : DefaultContractResolver
    {
        public CustomDirectoryInfoMapper()
        {
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType == typeof(DirectoryInfo))
            {
                return base.CreateObjectContract(objectType);
            }

            return base.CreateContract(objectType);
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            var c = new JsonPropertyCollection(type);
            c.AddRange(properties.Where(m => m.PropertyName != "Root"));

            return c;
        }
    }

    [Fact]
    public void GenerateSchemaCamelCase()
    {
        var generator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName,
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                IgnoreSerializableAttribute = true
            }
        };

        var schema = generator.Generate(typeof(VersionOld), true);

        var json = schema.ToString();

        StringAssert.AreEqual(@"{
  ""id"": ""Argon.Tests.TestObjects.VersionOld"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""major"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""minor"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""build"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""revision"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""majorRevision"": {
      ""required"": true,
      ""type"": ""integer""
    },
    ""minorRevision"": {
      ""required"": true,
      ""type"": ""integer""
    }
  }
}", json);
    }

    [Fact]
    public void GenerateSchemaSerializable()
    {
        var generator = new JsonSchemaGenerator();

        var contractResolver = new DefaultContractResolver
        {
            IgnoreSerializableAttribute = false
        };

        generator.ContractResolver = contractResolver;
        generator.UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName;

        var schema = generator.Generate(typeof(SerializableTestObject), true);

        var json = schema.ToString();

        StringAssert.AreEqual(@"{
  ""id"": ""Argon.Tests.Schema.SerializableTestObject"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""additionalProperties"": false,
  ""properties"": {
    ""_name"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    }
  }
}", json);

        var jsonWriter = new JTokenWriter();
        var serializer = new JsonSerializer
        {
            ContractResolver = contractResolver
        };
        serializer.Serialize(jsonWriter, new SerializableTestObject
        {
            Name = "Name!"
        });


        var errors = new List<string>();
        jsonWriter.Token.Validate(schema, (_, args) => errors.Add(args.Message));

        Assert.AreEqual(0, errors.Count);

        StringAssert.AreEqual(@"{
  ""_name"": ""Name!""
}", jsonWriter.Token.ToString());

        var c = jsonWriter.Token.ToObject<SerializableTestObject>(serializer);
        Assert.AreEqual("Name!", c.Name);
    }

    public enum SortTypeFlag
    {
        No = 0,
        Asc = 1,
        Desc = -1
    }

    public class X
    {
        public SortTypeFlag x;
    }

    [Fact]
    public void GenerateSchemaWithNegativeEnum()
    {
        var jsonSchemaGenerator = new JsonSchemaGenerator();
        var schema = jsonSchemaGenerator.Generate(typeof(X));

        var json = schema.ToString();

        StringAssert.AreEqual(@"{
  ""type"": ""object"",
  ""properties"": {
    ""x"": {
      ""required"": true,
      ""type"": ""integer"",
      ""enum"": [
        0,
        1,
        -1
      ]
    }
  }
}", json);
    }

    [Fact]
    public void CircularCollectionReferences()
    {
        var type = typeof(Workspace);
        var jsonSchemaGenerator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName
        };

        var jsonSchema = jsonSchemaGenerator.Generate(type);

        // should succeed
        Xunit.Assert.NotNull(jsonSchema);
    }

    [Fact]
    public void CircularReferenceWithMixedRequires()
    {
        var jsonSchemaGenerator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName
        };

        var jsonSchema = jsonSchemaGenerator.Generate(typeof(CircularReferenceClass));
        var json = jsonSchema.ToString();

        StringAssert.AreEqual(@"{
  ""id"": ""Argon.Tests.TestObjects.CircularReferenceClass"",
  ""type"": [
    ""object"",
    ""null""
  ],
  ""properties"": {
    ""Name"": {
      ""required"": true,
      ""type"": ""string""
    },
    ""Child"": {
      ""$ref"": ""Argon.Tests.TestObjects.CircularReferenceClass""
    }
  }
}", json);
    }

    [Fact]
    public void JsonPropertyWithHandlingValues()
    {
        var jsonSchemaGenerator = new JsonSchemaGenerator
        {
            UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName
        };

        var jsonSchema = jsonSchemaGenerator.Generate(typeof(JsonPropertyWithHandlingValues));
        var json = jsonSchema.ToString();

        StringAssert.AreEqual(@"{
  ""id"": ""Argon.Tests.TestObjects.JsonPropertyWithHandlingValues"",
  ""required"": true,
  ""type"": [
    ""object"",
    ""null""
  ],
  ""properties"": {
    ""DefaultValueHandlingIgnoreProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingIncludeProperty"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingPopulateProperty"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""DefaultValueHandlingIgnoreAndPopulateProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ],
      ""default"": ""Default!""
    },
    ""NullValueHandlingIgnoreProperty"": {
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""NullValueHandlingIncludeProperty"": {
      ""required"": true,
      ""type"": [
        ""string"",
        ""null""
      ]
    },
    ""ReferenceLoopHandlingErrorProperty"": {
      ""$ref"": ""Argon.Tests.TestObjects.JsonPropertyWithHandlingValues""
    },
    ""ReferenceLoopHandlingIgnoreProperty"": {
      ""$ref"": ""Argon.Tests.TestObjects.JsonPropertyWithHandlingValues""
    },
    ""ReferenceLoopHandlingSerializeProperty"": {
      ""$ref"": ""Argon.Tests.TestObjects.JsonPropertyWithHandlingValues""
    }
  }
}", json);
    }

    [Fact]
    public void GenerateForNullableInt32()
    {
        var jsonSchemaGenerator = new JsonSchemaGenerator();

        var jsonSchema = jsonSchemaGenerator.Generate(typeof(NullableInt32TestClass));
        var json = jsonSchema.ToString();

        StringAssert.AreEqual(@"{
  ""type"": ""object"",
  ""properties"": {
    ""Value"": {
      ""required"": true,
      ""type"": [
        ""integer"",
        ""null""
      ]
    }
  }
}", json);
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortTypeFlagAsString
    {
        No = 0,
        Asc = 1,
        Desc = -1
    }

    public class Y
    {
        public SortTypeFlagAsString y;
    }
}

public class NullableInt32TestClass
{
    public int? Value { get; set; }
}

public class DMDSLBase
{
    public String Comment;
}

public class Workspace : DMDSLBase
{
    public ControlFlowItemCollection Jobs = new();
}

public class ControlFlowItemBase : DMDSLBase
{
    public String Name;
}

public class ControlFlowItem : ControlFlowItemBase //A Job
{
    public TaskCollection Tasks = new();
    public ContainerCollection Containers = new();
}

public class ControlFlowItemCollection : List<ControlFlowItem>
{
}

public class Task : ControlFlowItemBase
{
    public DataFlowTaskCollection DataFlowTasks = new();
    public BulkInsertTaskCollection BulkInsertTask = new();
}

public class TaskCollection : List<Task>
{
}

public class Container : ControlFlowItemBase
{
  public ControlFlowItemCollection ContainerJobs = new();
}

public class ContainerCollection : List<Container>
{
}

public class DataFlowTask_DSL : ControlFlowItemBase
{
}

public class DataFlowTaskCollection : List<DataFlowTask_DSL>
{
}

public class SequenceContainer_DSL : Container
{
}

public class BulkInsertTaskCollection : List<BulkInsertTask_DSL>
{
}

public class BulkInsertTask_DSL
{
}

[Serializable]
public sealed class SerializableTestObject
{
    private string _name;

    public string Name
    {
        get => _name;
        set => _name = value;
    }
}

#pragma warning restore 618