using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

using Twist;

namespace TwistSDK
{
	class Program
	{

		#region Field

		#region API Keys

		private const string _ConsumerKey = "your consumer key";
		private const string _ConsumerSecret = "your consumer secret";

		#endregion

		#endregion Field

		#region Properties

		private static Twitter _Twitter { get; set; }
		private static HttpClient _Client { get; set; }
		private static HttpClientHandler _ClientHandler { get; set; } = new HttpClientHandler();

		#endregion Properties

		#region Main Method

		static async Task Main(string[] args)
		{
			_Client = new HttpClient(_ClientHandler);
			_Twitter = new Twitter(_ConsumerKey, _ConsumerSecret, _Client);

			Console.WriteLine(" ----------- 認証ページ表示開始 ------------------");
			Process.Start(await _Twitter.GenerateAuthorizeAsync());
			Console.WriteLine(" ----------- 認証ページ表示完了 ------------------");

			var pin = string.Empty;
			Console.Write("your pin code = ");
			pin = Console.ReadLine();

			Console.WriteLine(" ----------- 認証キー取得開始 ------------------");
			await _Twitter.GetAccessTokenAsync(pin);
			Console.WriteLine(" ----------- 認証キー取得完了 ------------------");

			Console.WriteLine(" ----------- Twitter 投稿開始 ------------------");

			// 画像なしツイート
			var text = "nyan!";
			await _Twitter.UpdateWithTextAsync(text);

			// 画像付きツイート
			var imagePath = @"Your hope posting picture path.";
			Console.Write($"picture path = {imagePath}\r\n");

			await _Twitter.UpdateWithMediaAsync(text, imagePath);

			Console.WriteLine(" ----------- Twitter 投稿完了 ------------------");

#if DEBUG
			Console.WriteLine("続行するには何かキーを押してください．．．");
			Console.ReadKey();
#endif

		}

		#endregion Main Method

	}
}
