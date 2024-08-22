using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditStats.Dto;
using RedditStats.Services.Interfaces;
using System.Collections.Concurrent;

namespace RedditStats.Services.Services
{
    public class RedditPollingService : BackgroundService
    {
        private readonly IApiService _apiService;
        private readonly IStatisticsService _statisticsService;
        private readonly IOptions<AppSettings> _applicationSettings;
        private readonly ILogger<RedditPollingService> _logger;
        private readonly ConcurrentBag<string> _subreddits;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30); // Adjust interval as needed

        public RedditPollingService(IApiService apiService, IStatisticsService statisticsService, ILogger<RedditPollingService> logger, IOptions<AppSettings> applicationSettings)
        {
            _apiService = apiService;
            _statisticsService = statisticsService;
            _applicationSettings = applicationSettings;
            _logger = logger;

            _subreddits = new ConcurrentBag<string>(_applicationSettings.Value.Subreddits);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var tasks = _subreddits.Select(subreddit => Task.Run(async () =>
                    {
                        try
                        {
                            var posts = await _apiService.GetNewPostsAsync(subreddit);
                            foreach (var post in posts)
                            {
                                _statisticsService.ProcessPost(subreddit, post);
                            }

                            LogStatistics(subreddit);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error occurred while processing subreddit: {subreddit}");
                        }
                    }));

                    await Task.WhenAll(tasks);

                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while polling Reddit");
                }
            }
        }

        private void LogStatistics(string subreddit)
        {
            var (user, postCount) = _statisticsService.GetUserWithMostPosts(subreddit);
            var (postId, upvotes) = _statisticsService.GetPostWithMostUpvotes(subreddit);
             
            Console.WriteLine($"Subreddit: {subreddit}");
            Console.WriteLine($"User with most posts: {user} ({postCount} posts)");
            Console.WriteLine($"Post with most upvotes: {postId} ({upvotes} upvotes)");
        }
    }
}
