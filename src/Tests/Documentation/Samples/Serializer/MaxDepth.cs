// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class MaxDepth : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region MaxDepth
        var json = @"[
              [
                [
                  '1',
                  'Two',
                  'III'
                ]
              ]
            ]";

        try
        {
            JsonConvert.DeserializeObject<List<IList<IList<string>>>>(json, new JsonSerializerSettings
            {
                MaxDepth = 2
            });
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine(ex.Message);
            // The reader's MaxDepth of 2 has been exceeded. Path '[0][0]', line 3, position 12.
        }
        #endregion
    }
}