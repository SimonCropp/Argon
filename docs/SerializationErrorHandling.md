# Error handling during serialization and deserialization.

Error handling lets you catch an error and choose whether to handle it and continue with serialization or let the error bubble up and be thrown in your application.

Error handling is defined through two methods: the `Argon.JsonSerializer.Error` event on JsonSerializer and the `Argon.OnErrorAttribute`.


## Error Event

The `Argon.JsonSerializer.Error` event is an event handler found on `Argon.JsonSerializer`. The error event is raised whenever an exception is thrown while serializing or deserializing JSON. Like all settings found on JsonSerializer, it can also be set on `Argon.JsonSerializerSettings` and passed to the serialization methods on JsonConvert.

<!-- snippet: SerializationErrorHandling -->
<a id='snippet-SerializationErrorHandling'></a>
```cs
var errors = new List<string>();

var c = JsonConvert.DeserializeObject<List<DateTime>>(
    """
    [
      '2009-09-09T00:00:00Z',
      'I am not a date and will error!',
      [
        1
      ],
      '1977-02-20T00:00:00Z',
      null,
      '2000-12-01T00:00:00Z'
    ]
    """,
    new JsonSerializerSettings
    {
        DeserializeError = (currentObject, originalObject, path, member, exception, markAsHandled) =>
        {
            errors.Add(exception.Message);
            markAsHandled();
        },
        Converters = {new IsoDateTimeConverter()}
    });

// 2009-09-09T00:00:00Z
// 1977-02-20T00:00:00Z
// 2000-12-01T00:00:00Z

// The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
// Unexpected token parsing date. Expected String, got StartArray.
// Cannot convert null value to System.DateTime.
```
<sup><a href='/src/ArgonTests/Documentation/SerializationTests.cs#L184-L219' title='Snippet source file'>snippet source</a> | <a href='#snippet-SerializationErrorHandling' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In this example we are deserializing a JSON array to a collection of DateTimes. On the JsonSerializerSettings a handler has been assigned to the `Error` event which will log the error message and mark the error as handled.

The result of deserializing the JSON is three successfully deserialized dates and three error messages: one for the badly formatted string ("I am not a date and will error!"), one for the nested JSON array, and one for the null value since the list doesn't allow nullable DateTimes. The event handler has logged these messages and Json.NET has continued on deserializing the JSON because the errors were marked as handled.

One thing to note with error handling in Json.NET is that an unhandled error will bubble up and raise the event on each of its parents. For example an unhandled error when serializing a collection of objects will be raised twice, once against the object and then again on the collection. This will let you handle an error either where it occurred or on one of its parents.

<!-- snippet: SerializationErrorHandlingWithParent -->
<a id='snippet-SerializationErrorHandlingWithParent'></a>
```cs
var errors = new List<string>();

var serializer = new JsonSerializer
{
    SerializeError = (currentObject, originalObject, path, member, exception, markAsHandled) =>
    {
        // only log an error once
        if (currentObject == originalObject)
        {
            errors.Add(exception.Message);
        }
    },
    DeserializeError = (currentObject, originalObject, path, member, exception, markAsHandled) =>
    {
        // only log an error once
        if (currentObject == originalObject)
        {
            errors.Add(exception.Message);
        }
    },
};
```
<sup><a href='/src/ArgonTests/Documentation/SerializationTests.cs#L245-L269' title='Snippet source file'>snippet source</a> | <a href='#snippet-SerializationErrorHandlingWithParent' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If you aren't immediately handling an error and only want to perform an action against it once, then you can check to see whether the `Argon.ErrorEventArgs`'s CurrentObject is equal to the OriginalObject. OriginalObject is the object that threw the error and CurrentObject is the object that the event is being raised against. They will only equal the first time the event is raised against the OriginalObject.

## OnErrorAttribute

The `Argon.OnErrorAttribute` works much like the other [NET serialization attributes](SerializationAttributes). To use it you simply place the attribute on a method that takes the correct parameters: a StreamingContext and an ErrorContext. The name of the method doesn't matter.

<!-- snippet: SerializationErrorHandlingAttributeObject -->
<a id='snippet-SerializationErrorHandlingAttributeObject'></a>
```cs
public class PersonError :
    IJsonOnSerializeError
{
    public string Name { get; set; }
    public int Age { get; set; }

    public List<string> Roles
    {
        get
        {
            if (field == null)
            {
                throw new("Roles not loaded!");
            }

            return field;
        }
        set;
    }

    public string Title { get; set; }

    public void OnSerializeError(object originalObject, string path, object member, Exception exception, Action markAsHandled) =>
        markAsHandled();
}
```
<sup><a href='/src/ArgonTests/Documentation/SerializationTests.cs#L272-L300' title='Snippet source file'>snippet source</a> | <a href='#snippet-SerializationErrorHandlingAttributeObject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In this example accessing the Roles property will throw an exception when no roles have been set. The HandleError method will set the error when serializing Roles as handled and allow the continued serializing the class.

<!-- snippet: SerializationErrorHandlingAttributeExample -->
<a id='snippet-SerializationErrorHandlingAttributeExample'></a>
```cs
var person = new PersonError
{
    Name = "George Michael Bluth",
    Age = 16,
    Roles = null,
    Title = "Mister Manager"
};

var settings = new JsonSerializerSettings();
settings.AddInterfaceCallbacks();
var json = JsonConvert.SerializeObject(
    person,
    Formatting.Indented,
    settings);

Console.WriteLine(json);
//{
//  "Name": "George Michael Bluth",
//  "Age": 16,
//  "Title": "Mister Manager"
//}
```
<sup><a href='/src/ArgonTests/Documentation/SerializationTests.cs#L305-L329' title='Snippet source file'>snippet source</a> | <a href='#snippet-SerializationErrorHandlingAttributeExample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * SerializationAttributes
 * `Argon.JsonSerializer.Error`
 * `Argon.OnErrorAttribute`
