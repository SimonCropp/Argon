#nullable enable
using Microsoft.AspNetCore.JsonPatch;

static class Resources
{
    public static string FormatInvalidValueForPath(string path) =>
        $"Invalid value for path: {path}";

    public static string FormatTargetLocationAtPathSegmentNotFound(string segment) =>
        $"Target location at path segment not found: {segment}";

    public static string FormatValueForTargetSegmentCannotBeNullOrEmpty(string segment)=>
        $"Value for target segment cannot be null or empty: {segment}";

    public static string FormatInvalidIndexValue(string segment) =>
        $"Invalid index value: {segment}";

    public static string FormatValueNotEqualToTestValue(object? currentValue, object value, string segment) =>
        $"Value not equal to test value. CurrentValue: {currentValue}. Value: {value}. Segment: {segment}";

    public static string FormatInvalidValueForProperty(object value) =>
        $"Invalid value for property: {value}";

    public static string FormatCannotUpdateProperty(string segment) =>
        $"Cannot update property: {segment}";

    public static string FormatCannotReadProperty(string segment) =>
        $"Cannot read property: {segment}";

    public static string InvalidJsonPatchDocument = "Invalid Json patch document";

    public static string FormatInvalidJsonPatchOperation(string op) =>
        $"Invalid Json patch operation: {op}";

    public static string FormatCannotPerformOperation(string operationOp, string path) =>
        $"Cannot perform operation: {operationOp}. Path: {path}";

    public static string FormatTargetLocationNotFound(string operationOp, string path) =>
        $"Target location not found: {operationOp}. Path: {path}";

    public static string FormatValueAtListPositionNotEqualToTestValue(object? currentValue, object value, int positionInfoIndex)=>
        $"Value at list position not equal to test value. CurrentValue: {currentValue}. Value: {value}. PositionInfoIndex: {positionInfoIndex}";

    public static string FormatPatchNotSupportedForArrays(string? listTypeFullName) =>
        $"Patch not supported for arrays: {listTypeFullName}";

    public static string FormatIndexOutOfBounds(string segment) =>
        $"Index out of bounds: {segment}";

    public static string TestOperationNotSupported = "Test operation not supported";

    public static string FormatInvalidPathSegment(string key) =>
        $"Invalid path segment: {key}";
}