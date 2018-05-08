namespace Twist.API.REST
{
	public class Urls
	{
		public string Update { get; set; } = "https://api.twitter.com/1.1/statuses/update.json";
		public string Upload { get; set; } = "https://upload.twitter.com/1.1/media/upload.json";
		public string HomeTimeLine { get; set; } = "https://api.twitter.com/1.1/statuses/home_timeline.json";
	}
}
