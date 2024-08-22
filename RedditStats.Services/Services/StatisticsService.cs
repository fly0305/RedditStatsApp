using RedditStats.Services.Interfaces;
using System.Collections.Concurrent;

namespace RedditStats.Services.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _userPostCount = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _postUpvotes = new();

        public void ProcessPost(string subreddit, dynamic post)
        {
            var userDict = _userPostCount.GetOrAdd(subreddit, _ => new ConcurrentDictionary<string, int>());
            var postDict = _postUpvotes.GetOrAdd(subreddit, _ => new ConcurrentDictionary<string, int>());

            string username = post.data.author;
            userDict.AddOrUpdate(username, 1, (key, value) => value + 1);

            if (post?.data?.id != null && post?.data?.ups != null)
            {
                string postId = post.data.id.ToString();
                int upvotes = (int)post.data.ups;

                postDict.AddOrUpdate(postId, upvotes, (key, value) => value + upvotes);
            }
            else
            {
                Console.WriteLine("Invalid post data: ID or Upvotes are missing.");
            }
        }

        public (string User, int PostCount) GetUserWithMostPosts(string subreddit)
        {
            if (_userPostCount.TryGetValue(subreddit, out var userDict))
            {
                var userWithMostPosts = userDict.OrderByDescending(x => x.Value).FirstOrDefault();
                return (userWithMostPosts.Key, userWithMostPosts.Value);
            }
            return (null, 0);
        }

        public (string PostId, int Upvotes) GetPostWithMostUpvotes(string subreddit)
        {
            if (_postUpvotes.TryGetValue(subreddit, out var postDict))
            {
                var postWithMostUpvotes = postDict.OrderByDescending(x => x.Value).FirstOrDefault();
                return (postWithMostUpvotes.Key, postWithMostUpvotes.Value);
            }
            return (null, 0);
        }
    }
}
