// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1962
{
    [Fact]
    public void Test_Default()
    {
        var json = @"// comment
[ 1, 2, 42 ]";
        var token = JToken.Parse(json);

        Assert.Equal(JTokenType.Comment, token.Type);
        Assert.Equal(" comment", ((JValue)token).Value);
    }

    //TODO: should be able to extra array after comment
//     [Fact]
//     public void Test_LoadComments()
//     {
//         var json = @"// comment
// [ 1, 2, 42 ]";
//         var token = JToken.Parse(json, new()
//         {
//             CommentHandling = CommentHandling.Load
//         });
//
//         Assert.Equal(JTokenType.Comment, token.Type);
//         Assert.Equal(" comment", ((JValue)token).Value);
//
//         var obj = token.ToObject<int[]>();
//         Assert.Null(obj);
//     }

    [Fact]
    public void Test_IgnoreComments()
    {
        var json = @"// comment
[ 1, 2, 42 ]";
        var token = JToken.Parse(json, new()
        {
            CommentHandling = CommentHandling.Ignore
        });

        Assert.Equal(JTokenType.Array, token.Type);
        Assert.Equal(3, token.Count());
        Assert.Equal(1, (int)token[0]);
        Assert.Equal(2, (int)token[1]);
        Assert.Equal(42, (int)token[2]);
    }
}