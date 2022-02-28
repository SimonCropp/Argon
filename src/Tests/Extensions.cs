#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}
#endif
public record TextReaderState(JsonToken TokenType, int LineNumber, int LinePosition, string Path, int Depth, object Value);
public record ReaderState(JsonToken TokenType, string Path, int Depth, object Value);

public static class Extensions
{
    public static async Task VerifyReaderState(
        this JsonReader reader,
        [CallerFilePath] string sourceFile = "")
    {
        var tokens = new List<ReaderState>();
        while (await reader.ReadAsync())
        {
            tokens.Add(
                new ReaderState(
                    reader.TokenType,
                    reader.Path,
                    reader.Depth,
                    reader.Value));
        }

        await Verify(tokens, null, sourceFile);
    }

    public static async Task VerifyReaderState(
        this JsonTextReader reader,
        [CallerFilePath] string sourceFile = "")
    {
        var tokens = new List<TextReaderState>();
        while (await reader.ReadAsync())
        {
            tokens.Add(
                new TextReaderState(
                    reader.TokenType,
                    reader.LineNumber,
                    reader.LinePosition,
                    reader.Path,
                    reader.Depth,
                    reader.Value));
        }

        await Verify(tokens, null, sourceFile);
    }

    public static string GetOffset(this DateTime d)
    {
        var chars = new char[8];
        var pos = DateTimeUtils.WriteDateTimeOffset(chars, 0, DateTime.SpecifyKind(d, DateTimeKind.Local).GetUtcOffset());

        return new string(chars, 0, pos);
    }

    public static string BytesToHex(this byte[] bytes)
    {
        return BitConverter.ToString(bytes);
    }

    public static byte[] HexToBytes(this string hex)
    {
        var fixedHex = hex.Replace("-", string.Empty);

        // array to put the result in
        var bytes = new byte[fixedHex.Length / 2];
        // variable to determine shift of high/low nibble
        var shift = 4;
        // offset of the current byte in the array
        var offset = 0;
        // loop the characters in the string
        foreach (var c in fixedHex)
        {
            // get character code in range 0-9, 17-22
            // the % 32 handles lower case characters
            var b = (c - '0') % 32;
            // correction for a-f
            if (b > 9)
            {
                b -= 7;
            }
            // store nibble (4 bits) in byte array
            bytes[offset] |= (byte)(b << shift);
            // toggle the shift variable between 0 and 4
            shift ^= 4;
            // move to next byte
            if (shift != 0)
            {
                offset++;
            }
        }
        return bytes;
    }
}