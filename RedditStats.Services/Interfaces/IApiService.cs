using Newtonsoft.Json.Linq;

namespace RedditStats.Services.Interfaces
{
    public interface IApiService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subreddit">string subreddit</param>
        /// <returns></returns>
        Task<JArray> GetNewPostsAsync(string subreddit);
    }
}
