// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ReadMultipleContentWithJsonReader : TestFixtureBase
{
    #region ReadMultipleContentWithJsonReaderTypes

    public class Role
    {
        public string Name { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region ReadMultipleContentWithJsonReaderUsage

        var json = "{ 'name': 'Admin' }{ 'name': 'Publisher' }";

        var roles = new List<Role>();

        var reader = new JsonTextReader(new StringReader(json))
        {
            SupportMultipleContent = true
        };

        while (true)
        {
            if (!reader.Read())
            {
                break;
            }

            var serializer = new JsonSerializer();
            var role = serializer.Deserialize<Role>(reader);

            roles.Add(role);
        }

        foreach (var role in roles)
        {
            Console.WriteLine(role.Name);
        }

        // Admin
        // Publisher

        #endregion

        Assert.Equal(2, roles.Count);
        Assert.Equal("Admin", roles[0].Name);
        Assert.Equal("Publisher", roles[1].Name);
    }
}