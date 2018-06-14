using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using Twist.API;

namespace Twist
{
	/// <summary>
	/// Twitter へアクセスするためのラッパークラス
	/// </summary>
	public class Twitter
	{

		#region Field

		/// <summary>
		/// Access Token を取得する際に必要なエンドポイントURL
		/// </summary>
		public static readonly string AccessTokenUrl = "https://api.twitter.com/oauth/access_token";

		/// <summary>
		/// 認証を実施する際に必要なエンドポイントURL
		/// </summary>
		public static readonly string AuthorizeUrl = "https://api.twitter.com/oauth/authorize";

		/// <summary>
		/// 画像アップロードを実施する際に必要なエンドポイントURL
		/// </summary>
		public static readonly string ChunkUpload = "https://upload.twitter.com/1.1/media/upload.json";

		/// <summary>
		/// 認証画面を表示する際に必要なエンドポイントURL
		/// </summary>
		public static readonly string RequestTokenUrl = "https://api.twitter.com/oauth/request_token";

		/// <summary>
		/// 投稿を実施する際に必要なエンドポイントURL
		/// </summary>
		public static readonly string Update = "https://api.twitter.com/1.1/statuses/update.json";

		#endregion field

		#region Properties

		/// <summary>
		/// Twitter アクセスラッパーAPI の管理を行います。
		/// </summary>
		private Core _Core { get; set; }

		#endregion

		#region Constractor 

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="client"> HttpClient </param>
		public Twitter(string ck, string cs, HttpClient client) => this._Core = new Core(ck, cs, client);

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
			=> this._Core = new Core(ck, cs, at, ats, id, name, client);

		#endregion Constractor

		#region TwitterAPI Access Wrapper Methods

		/// <summary>
		/// 認証を実施します。
		/// </summary>
		/// <returns></returns>
		public async Task AuthorizeAsync()
		{
			Debug.WriteLine("------------ 認証シーケンス開始 -----------------");

			await this._Core.GetRequestTokenAsync(RequestTokenUrl);

			Uri url = this._Core.GetAuthorizeUrl(AuthorizeUrl);
			Process.Start(url.ToString());

			Debug.WriteLine("------------ 認証シーケンス完了 ----------------- >> " + url.ToString());
		}

		/// <summary>
		/// Twitter へ リクエストを投げるための薄いラッパーメソッド
		/// </summary>
		/// <param name="url"> リクエストURL(エンドポイント) </param>
		/// <param name="type"> リクエストタイプ(GET/POST) </param>
		/// <param name="query"> テキストデータ </param>
		/// <param name="stream"> 画像データ(画像がない場合は、null扱い) </param>
		/// <returns></returns>
		public Task<string> Request(string url, HttpMethod type, IDictionary<string, string> query, Stream stream = null)
		{
			if (stream == null)
				return this._Core.RequestAsync(this._Core.ConsumerKey, this._Core.ConsumerSecret, this._Core.AccessToken, this._Core.AccessTokenSecret, url, type, query);
			else
				return this._Core.RequestAsync(this._Core.ConsumerKey, this._Core.ConsumerSecret, this._Core.AccessToken, this._Core.AccessTokenSecret, url, type, query, stream);
		}

		/// <summary>
		/// Access Token の取得を行うためのラッパーメソッド
		/// </summary>
		/// <param name="AccessTokenUrl"> Access Token を取得する際に必要なエンドポイントURL </param>
		/// <param name="pin"> 認証時に表示された PIN コード </param>
		/// <returns> 各種認証キー：ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret </returns>
		public async Task GetAccessTokenAsync(string pin) =>
			(this._Core.AccessToken, this._Core.AccessTokenSecret, this._Core.UserId, this._Core.ScreenName) = await this._Core.GetAccessTokenAsync(AccessTokenUrl, pin);

		#endregion TwitterAPI Access Wrapper Methods

	}
}
