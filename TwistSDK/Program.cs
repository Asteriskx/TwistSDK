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

		private const string _ConsumerKey = "consumer key";
		private const string _ConsumerSecret = "consumer secret";

		#endregion Field

		#region Properties

		private static Twitter _Twitter { get; set; }
		private static HttpClient _Client { get; set; }
		private static HttpClientHandler _ClientHandler { get; set; } = new HttpClientHandler();

		#endregion Properties

		static async Task Main()
		{
			_Client = new HttpClient(_ClientHandler);
			_Twitter = new Twitter(_ConsumerKey, _ConsumerSecret, _Client);

			Console.WriteLine(" ----------- 認証ページ表示開始 ------------------");
			Process process = new();
			var url = await _Twitter.GenerateAuthorizeAsync();
			try
			{
				process.StartInfo.UseShellExecute = true;
				process.StartInfo.FileName = url;
				process.Start();
			}
			catch (Exception)
			{
				throw new Exception($"Unable to open a browser. Please manually open: {url}");
			}

			Console.WriteLine(" ----------- 認証ページ表示完了 ------------------");

			Console.Write("your pin code = ");
			string pin = Console.ReadLine();

			Console.WriteLine(" ----------- 認証キー取得開始 ------------------");
			await _Twitter.GetAccessTokenAsync(pin);
			Console.WriteLine(" ----------- 認証キー取得完了 ------------------");

			Console.WriteLine(" ----------- Twitter 投稿開始 ------------------");

			// 画像なしツイート
			string text = "test";
			await _Twitter.UpdateWithTextAsync(text);

			// 画像付きツイート
			//var imagePath = @"Your hope posting picture path.";
			//Console.Write($"picture path = {imagePath}\r\n");

			//await _Twitter.UpdateWithMediaAsync(text, imagePath);

			Console.WriteLine(" ----------- Twitter 投稿完了 ------------------");
		}
	}
}