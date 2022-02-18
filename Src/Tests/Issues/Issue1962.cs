﻿#region License
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

using Xunit;

namespace Argon.Tests.Issues;

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

    [Fact]
    public void Test_LoadComments()
    {
        var json = @"// comment
[ 1, 2, 42 ]";
        var token = JToken.Parse(json, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Load
        });

        Assert.Equal(JTokenType.Comment, token.Type);
        Assert.Equal(" comment", ((JValue)token).Value);

        var obj = token.ToObject<int[]>();
        Assert.Null(obj);
    }

    [Fact]
    public void Test_IgnoreComments()
    {
        var json = @"// comment
[ 1, 2, 42 ]";
        var token = JToken.Parse(json, new JsonLoadSettings
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