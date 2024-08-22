namespace RedditStats.Dto
{
    public class AppSettings
    {
        public AppSettings_Logging Logging { get; set; }
        public string[] Subreddits { get; set; }
        public AppSettings_RedditApi RedditApi { get; set; }
    }

    public class AppSettings_RedditApi
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserAgent { get; set; } 
    }

    public class AppSettings_Logging
    {
        public AppSettings_Loglevel LogLevel { get; set; }
    }

    public class AppSettings_Loglevel
    {
        public string Default { get; set; }
        public string MicrosoftAspNetCore { get; set; }
    }
}
