// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class FakeArrayPool : IArrayPool<char>
{
    public readonly List<char[]> FreeArrays = new();
    public readonly List<char[]> UsedArrays = new();

    public char[] Rent(int minimumLength)
    {
        var a = FreeArrays.FirstOrDefault(b => b.Length >= minimumLength);
        if (a != null)
        {
            FreeArrays.Remove(a);
            UsedArrays.Add(a);

            return a;
        }

        a = new char[minimumLength];
        UsedArrays.Add(a);

        return a;
    }

    public void Return(char[] array)
    {
        if (UsedArrays.Remove(array))
        {
            FreeArrays.Add(array);

            // smallest first so the first array large enough is rented
            FreeArrays.Sort((b1, b2) => Comparer<int>.Default.Compare(b1.Length, b2.Length));
        }
    }
}