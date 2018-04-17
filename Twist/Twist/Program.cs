using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Twist.API.OAuth;
using Twist.API.REST;

namespace Twist
{
	class Program
	{
		private static Twitter _Twitter { get; set; }
		private static HttpClient _Client { get; set; }
		private static Urls _Urls { get; set; } = new Urls();
		private static HttpClientHandler _ClientHandler { get; set; } = new HttpClientHandler();

		#region API Keys

		private const string _ConsumerKey = "your consumer key.";
		private const string _ConsumerKeySecret = "your consumer key secret.";

		#endregion

		static async Task Main(string[] args)
		{
			_Client = new HttpClient(_ClientHandler);
			_Twitter = new Twitter(_ConsumerKey, _ConsumerKeySecret, _Client);

			Console.WriteLine(" ----------- 認証ページ表示開始 ------------------");
			await _Twitter.AuthorizeAsync();
			Console.WriteLine(" ----------- 認証ページ表示完了 ------------------");

			var pin = string.Empty;
			Console.Write("your pin code = ");
			pin = Console.ReadLine();

			Console.WriteLine(" ----------- 認証キー取得開始 ------------------");
			await _Twitter.GetAccessTokenAsync(pin);
			Console.WriteLine(" ----------- 認証キー取得完了 ------------------");

			var postData = $"もふもふしたい({new Random().Next(100)}回目)";
			var query = new Dictionary<string, string> { { "status", postData } };

			Console.WriteLine(" ----------- Twitter 投稿開始 ------------------");
			await _Twitter.Request(_Urls.Update, HttpMethod.Post, query);
			Console.WriteLine($"post : {postData}");
			Console.WriteLine(" ----------- Twitter 投稿完了 ------------------");

#if DEBUG
			Console.WriteLine("続行するには何かキーを押してください．．．");
			Console.ReadKey();
#endif

		}
	}
}
