namespace Twist.Core.Auth.Interfaces
{
    public interface ITwistCredentials
    {
		public string ConsumerKey { get; set; }
		public string ConsumerSecret { get; set; }
		public string RequestToken { get; set; }
		public string RequestTokenSecret { get; set; }
		public string AccessToken { get; set; }
		public string AccessTokenSecret { get; set; }
		public string UserId { get; set; }
		public string ScreenName { get; set; }
		public string PinCode { get; set; }
	}
}
