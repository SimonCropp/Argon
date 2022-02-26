// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeMetadataPropertyHandling : TestFixtureBase
{
    public class User
    {
        public string Name { get; set; }
    }

    [Fact]
    public void Example()
    {
        try
        {
            #region DeserializeMetadataPropertyHandling
            var json = @"{
                  'Name': 'James',
                  'Password': 'Password1',
                  '$type': 'MyNamespace.User, MyAssembly'
                }";

            var o = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                // $type no longer needs to be first
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });

            var u = (User)o;

            Console.WriteLine(u.Name);
            // James
            #endregion
        }
        catch
        {
        }
    }
}