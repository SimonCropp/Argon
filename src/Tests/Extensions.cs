public static class Extensions
{
    public static string GetOffset(this DateTime d, DateFormatHandling dateFormatHandling)
    {
        var chars = new char[8];
        var pos = DateTimeUtils.WriteDateTimeOffset(chars, 0, DateTime.SpecifyKind(d, DateTimeKind.Local).GetUtcOffset(), dateFormatHandling);

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