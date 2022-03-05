// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeserializeMissingMemberHandling : TestFixtureBase
{
    #region DeserializeMissingMemberHandlingTypes

    public class Account
    {
        public string FullName { get; set; }
        public bool Deleted { get; set; }
    }

    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeMissingMemberHandlingUsage

        var json = @"{
              'FullName': 'Dan Deleted',
              'Deleted': true,
              'DeletedDate': '2013-01-20T00:00:00'
            }";

        try
        {
            JsonConvert.DeserializeObject<Account>(json, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });
        }
        catch (JsonSerializationException exception)
        {
            Console.WriteLine(exception.Message);
            // Could not find member 'DeletedDate' on object of type 'Account'. Path 'DeletedDate', line 4, position 23.
        }

        #endregion
    }
}