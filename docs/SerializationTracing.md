# Debugging with Serialization Tracing

The Json.NET serializer supports logging and debugging using the `Argon.Serialization.ITraceWriter` interface. By assigning a trace writer you can capture serialization messages and errors and debug what happens inside the Json.NET serializer when serializing and deserializing JSON.


## ITraceWriter

A trace writer can be assigned using properties on JsonSerializerSettings or JsonSerializer.

snippet: MemoryTraceWriterExample

Json.NET has two implementations of ITraceWriter: `Argon.Serialization.MemoryTraceWriter`, which keeps messages in memory for simple debugging, like the example above, and `Argon.Serialization.DiagnosticsTraceWriter`, which writes messages to any System.Diagnostics.TraceListeners your application is using.


## Custom ITraceWriter

To write messages using your existing logging framework, just implement a custom version of ITraceWriter.

snippet: CustomTraceWriterExample


## Related Topics

 * `Argon.JsonSerializer`
 * `Argon.Serialization.ITraceWriter`
 * `Argon.Serialization.MemoryTraceWriter`
 * `Argon.Serialization.DiagnosticsTraceWriter`