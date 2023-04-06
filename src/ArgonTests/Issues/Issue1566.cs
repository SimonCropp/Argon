// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1566 : TestFixtureBase
{
    [Fact]
    public void Github_deserialize_pr_state_should_be_case_insensitive()
    {
        // Arrange
        var jsonWithUppercase = "{\"state\": \"APPROVED\"}";
        var jsonWithLowercase = "{\"state\": \"approved\"}";

        // Act
        var jsonObjectWithUppercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithUppercase);
        var jsonObjectWithLowercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithLowercase);

        // Assert
        Assert.Equal(GitHubPullRequestReviewState.Approved, jsonObjectWithUppercase.State);
        Assert.Equal(GitHubPullRequestReviewState.Approved, jsonObjectWithLowercase.State);
    }

    [Fact]
    public void Github_deserialize_pr_state_changes_requested_should_be_case_insensitive()
    {
        // Arrange
        var jsonWithUppercase = "{\"state\": \"CHANGES_REQUESTED\"}";
        var jsonWithLowercase = "{\"state\": \"changes_requested\"}";

        // Act
        var jsonObjectWithUppercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithUppercase);
        var jsonObjectWithLowercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithLowercase);

        // Assert
        Assert.Equal(GitHubPullRequestReviewState.ChangesRequested, jsonObjectWithUppercase.State);
        Assert.Equal(GitHubPullRequestReviewState.ChangesRequested, jsonObjectWithLowercase.State);
    }

    public enum GitHubPullRequestReviewState
    {
        [EnumMember(Value = "approved")]
        Approved,

        [EnumMember(Value = "changes_requested")]
        ChangesRequested,

        [EnumMember(Value = "commented")]
        Commented,

        [EnumMember(Value = "dismissed")]
        Dismissed,

        [EnumMember(Value = "pending")]
        Pending
    }

    public class GitHubPullRequestReview
    {
        [JsonProperty("state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public GitHubPullRequestReviewState State;
    }
}