using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace Twist.API.OAuth
{
	/// <summary>
	/// Twitter へ アクセスするための ラッパークラス
	/// </summary>
	public class Twitter : Credentials
	{
		public Core Auth { get; set; }
		private HttpClient _Client { get; set; }
		public Credentials Credentials { get; set; }

		public Twitter(string consumerKey, string consumerKeySecret, HttpClient client) : base(consumerKey, consumerKeySecret)
		{
			_Client = client;
			Auth = new Core(client);
			Credentials = new Credentials(consumerKey, consumerKeySecret);
		}

		public Twitter(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret,
			string userId, string screenName, HttpClient client) : base(consumerKey, consumerKeySecret, accessToken, accessTokenSecret)
		{
			_Client = client;
			Auth = new Core(client);
			Credentials = new Credentials(consumerKey, consumerKeySecret, accessToken, accessTokenSecret);
		}

		/// <summary>
		/// 認証を実施します。
		/// </summary>
		/// <returns></returns>
		public async Task AuthorizeAsync()
		{
			Debug.WriteLine("------------ 認証シーケンス開始 -----------------");

			await this.Auth.GetRequestTokenAsync(Credentials);

			Uri url = this.Auth.GetAuthorizeUrl(Credentials);
			Process.Start(url.ToString());

			Debug.WriteLine("------------ 認証シーケンス完了 ----------------- >> " + url.ToString());
		}

		/// <summary>
		/// Twitter へ リクエストを投げるための薄いラッパーメソッド
		/// </summary>
		/// <param name="url"></param>
		/// <param name="type"></param>
		/// <param name=""></param>
		/// <returns></returns>
		public Task<string> Request(string url, HttpMethod type, IDictionary<string, string> query, Stream stream = null)
		{
			if (stream == null)
				return this.Auth.RequestAsync(Credentials.ConsumerKey, Credentials.ConsumerKeySecret, Credentials.AccessToken, Credentials.AccessTokenSecret, url, type, query);
			else
				return this.Auth.RequestAsync(Credentials.ConsumerKey, Credentials.ConsumerKeySecret, Credentials.AccessToken, Credentials.AccessTokenSecret, url, type, query, stream);
		}

		/// <summary>
		/// Access Token の取得を行うためのラッパーメソッド。
		/// </summary>
		/// <param name="PIN"></param>
		/// <returns></returns>
		public async Task GetAccessTokenAsync(string pin) =>
			(this.AccessToken, this.AccessTokenSecret, this.UserId, this.ScreenName) = await this.Auth.GetAccessTokenAsync(Credentials, pin);

	}
}