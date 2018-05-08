using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Twist.API.OAuth;
using Twist.API.REST;


namespace Twist
{
	class Program
	{
		private static Twitter _Twitter { get; set; }
		private static HttpClient _Client { get; set; }
		private static HttpClientHandler _ClientHandler { get; set; } = new HttpClientHandler();
		private static Urls _Urls { get; set; } = new Urls();

		#region API Keys

		private const string _ConsumerKey = "your consumer key.";
		private const string _ConsumerKeySecret = "your consumer secret key.";

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

			Console.WriteLine(" ----------- Twitter 投稿開始 ------------------");

			// 画像投稿に関して
			var imagePath = @"C:\Users\Asterisk\Desktop\asterisk.jpg";
			Console.Write($"picture path = {imagePath}");

			using (var image = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
			{
				// Twitter へ chunk upload を事前に行い、media_id を取得
				var id = await _Twitter.Request(_Urls.Upload, HttpMethod.Post, new Dictionary<string, string>() { }, image);
				dynamic deserialize = JsonConvert.DeserializeObject(id);

				// 文字ツイートと画像を紐付ける => media_ids 
				var query = new Dictionary<string, string> { { "status", "#Twist 画像投稿テスト"}, { "media_ids", deserialize.media_id_string.Value } };
				await _Twitter.Request(_Urls.Update, HttpMethod.Post, query);
			}

			Console.WriteLine(" ----------- Twitter 投稿完了 ------------------");

#if DEBUG
			Console.WriteLine("続行するには何かキーを押してください．．．");
			Console.ReadKey();
#endif

		}
	}
}
