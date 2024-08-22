namespace RedditStats.Services.Interfaces
{
    public interface IStatisticsService
    {
        void ProcessPost(string subreddit, dynamic post);

        (string User, int PostCount) GetUserWithMostPosts(string subreddit);

        (string PostId, int Upvotes) GetPostWithMostUpvotes(string subreddit);
    }
}
