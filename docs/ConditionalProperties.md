# Conditional Property Serialization

Json.NET has the ability to conditionally serialize properties by placing a ShouldSerialize method on a class. This functionality is similar to the [XmlSerializer ShouldSerialize feature](http://msdn.microsoft.com/en-us/library/53b8022e.aspx).


## ShouldSerialize

To conditionally serialize a property, add a method that returns boolean with the same name as the property and then prefix the method name with ShouldSerialize. The result of the method determines whether the property is serialized. If the method returns true then the property will be serialized, if it returns false then the property will be skipped.

snippet: EmployeeShouldSerializeExample

snippet: ShouldSerializeClassTest


## IContractResolver

ShouldSerialize can also be set using an `Argon.Serialization.IContractResolver`. Conditionally serializing a property using an IContractResolver is useful avoid placing a ShouldSerialize method on a class or are unable to.

snippet: ShouldSerializeContractResolver


## Related Topics

 * `Argon.JsonSerializer`
 * `Argon.Serialization.IContractResolver`
 * `Argon.Serialization.JsonProperty.ShouldSerialize`