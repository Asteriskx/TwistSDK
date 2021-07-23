using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;

using Twist.Auth.Interfaces;
using Twist.Interop;
using Twist.Core.Auth;

namespace Twist
{
	/// <summary>
	/// Twitter へアクセスするためのラッパークラス
	/// </summary>
	public class Twitter
	{
		#region Properties

		/// <summary>
		/// Twitter アクセスラッパーAPI の管理を行います。
		/// </summary>
		private readonly IAuthenticator _authenticator;

		#endregion

		#region Constractor 

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="client"> HttpClient </param>
		public Twitter(string ck, string cs, HttpClient client) =>
			_authenticator = new Authenticator(ck, cs, client);

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="at"> Access Token </param>
		/// <param name="ats"> Access Token Secret </param>
		/// <param name="id"> User ID </param>
		/// <param name="name"> Screen Name </param>
		/// <param name="client"> HttpClient </param>
		public Twitter(string ck, string cs, string at, string ats, string id, string name, HttpClient client)
			=> _authenticator = new Authenticator(ck, cs, at, ats, id, name, client);

		#endregion Constractor

		#region Inner class

		private class MediaData
		{
			[JsonProperty("media_id_string")]
			public string MediaIdString { get; set; }
		}

		#endregion Inner class

		#region Twitter API Access Wrapper Methods

		/// <summary>
		/// 認証用のURLを返却します。
		/// </summary>
		/// <returns> 認証用のURL </returns>
		public async Task<string> GenerateAuthorizeAsync()
		{
			await _authenticator.GetRequestTokenAsync(EndPoints.RequestToken);
			Uri url = _authenticator.GetAuthorizeUrl(EndPoints.Authorize);

			return url.ToString();
		}

		/// <summary>
		/// Twitter へツイートを非同期にて行います。
		/// </summary>
		/// <param name="text">ツイート内容</param>
		/// <returns></returns>
		public async Task UpdateWithTextAsync(string text)
		{
			var query = new Dictionary<string, string> { { "status", text } };
			await _RequestAsync(EndPoints.Update, HttpMethod.Post, query);
		}

		/// <summary>
		/// Twitter へ画像付きツイートを非同期にて行います。
		/// </summary>
		/// <param name="text">ツイート内容</param>
		/// <param name="path">画像ファイルパス</param>
		/// <returns></returns>
		public async Task UpdateWithMediaAsync(string text, string path)
		{
			using var image = new FileStream(path, FileMode.Open, FileAccess.Read);

			var id = await _RequestAsync(EndPoints.ChunkUpload, HttpMethod.Post, new Dictionary<string, string>() { }, image);
			var deserialize = JsonConvert.DeserializeObject<MediaData>(id).MediaIdString;

			var query = new Dictionary<string, string> { { "status", text }, { "media_ids", deserialize } };
			await _RequestAsync(EndPoints.Update, HttpMethod.Post, query);
		}

		/// <summary>
		/// Twitter へ画像付きツイートを非同期にて行います。
		/// </summary>
		/// <param name="text">ツイート内容</param>
		/// <param name="stream">画像データ：Stream 形式</param>
		/// <returns></returns>
		public async Task UpdateWithMediaAsync(string text, Stream stream)
		{
			if (stream != null)
			{
				var id = await _RequestAsync(EndPoints.ChunkUpload, HttpMethod.Post, new Dictionary<string, string>() { }, stream);
				var deserialize = JsonConvert.DeserializeObject<MediaData>(id).MediaIdString;

				var query = new Dictionary<string, string> { { "status", text }, { "media_ids", deserialize } };
				await _RequestAsync(EndPoints.Update, HttpMethod.Post, query);
			}
			else
			{
				throw new Exception("stream is Empty.");
			}
		}

		/// <summary>
		/// Access Token の取得を行うためのラッパーメソッド
		/// </summary>
		/// <param name="pin"> 認証時に表示された PIN コード </param>
		/// <returns> 各種認証キー：ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret </returns>
		public async Task GetAccessTokenAsync(string pin) =>
			(_authenticator.ICredentials.AccessToken, 
			_authenticator.ICredentials.AccessTokenSecret, 
			_authenticator.ICredentials.UserId, 
			_authenticator.ICredentials.ScreenName) 
				= await _authenticator.GetAccessTokenAsync(EndPoints.AccessToken, pin);

		/// <summary>
		/// Twitter へ リクエストを投げるための薄いラッパーメソッド
		/// </summary>
		/// <param name="url"> リクエストURL(エンドポイント) </param>
		/// <param name="type"> リクエストタイプ(GET/POST) </param>
		/// <param name="query"> テキストデータ </param>
		/// <param name="stream"> 画像データ(画像がない場合は、null扱い) </param>
		/// <returns></returns>
		private async Task<string> _RequestAsync(string url, HttpMethod type, IDictionary<string, string> query, Stream stream = null)
		{
			return await _authenticator.RequestAsync(
				_authenticator.ICredentials.ConsumerKey,
				_authenticator.ICredentials.ConsumerSecret,
				_authenticator.ICredentials.AccessToken,
				_authenticator.ICredentials.AccessTokenSecret,
				url,
				type,
				query,
				stream
			);
		}

		#endregion Twitter API Access Wrapper Methods
	}
}
