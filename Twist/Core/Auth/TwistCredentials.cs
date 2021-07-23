using Twist.Core.Auth.Interfaces;

namespace Twist.Core.Auth
{
	/// <summary>
	/// Twist 認証データ保持クラス
	/// </summary>
	internal class TwistCredentials : ITwistCredentials
	{
        #region Constractor 

        /// <summary>
        /// インスタンス生成用
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        internal TwistCredentials(string consumerKey, string consumerSecret)
		{
			ICredentials = this;
			ICredentials.ConsumerKey = consumerKey;
			ICredentials.ConsumerSecret = consumerSecret;
		}

		/// <summary>
		/// 設定保持用
		/// </summary>
		/// <param name="consumerKey"></param>
		/// <param name="consumerSecret"></param>
		/// <param name="accessToken"></param>
		/// <param name="accessTokenSecret"></param>
		/// <param name="userId"></param>
		/// <param name="screenName"></param>
		internal TwistCredentials(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string userId, string screenName)
		{
			ICredentials = this;
			ICredentials.ConsumerKey = consumerKey;
			ICredentials.ConsumerSecret = consumerSecret;
			ICredentials.AccessToken = accessToken;
			ICredentials.AccessTokenSecret = accessTokenSecret;
			ICredentials.UserId = userId;
			ICredentials.ScreenName = screenName;
		}

		#endregion

		#region Properties

		public ITwistCredentials ICredentials { get; set; }

		/// <summary>
		/// Consumer Key の管理を行います。
		/// </summary>
		string ITwistCredentials.ConsumerKey { get; set; }

		/// <summary>
		/// Consumer Secret の管理を行います。
		/// </summary>
		string ITwistCredentials.ConsumerSecret { get; set; }

		/// <summary>
		/// Request Token の管理を行います。
		/// </summary>
		string ITwistCredentials.RequestToken { get; set; }

		/// <summary>
		/// Request Token Secret の管理を行います。
		/// </summary>
		string ITwistCredentials.RequestTokenSecret { get; set; }

		/// <summary>
		/// Access Token の管理を行います。
		/// </summary>
		string ITwistCredentials.AccessToken { get; set; }

		/// <summary>S
		/// Access Token Secret の管理を行います。
		/// </summary>
		string ITwistCredentials.AccessTokenSecret { get; set; }

		/// <summary>
		/// User Id の管理を行います。
		/// </summary>
		string ITwistCredentials.UserId { get; set; }

		/// <summary>
		/// Screen Name の管理を行います。
		/// </summary>
		string ITwistCredentials.ScreenName { get; set; }

		/// <summary>
		/// Pin Code の管理を行います。
		/// </summary>
		string ITwistCredentials.PinCode { get; set; }

		#endregion Properties
	}
}
