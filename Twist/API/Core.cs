using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Twist.API
{
	/// <summary>
	/// Twitter アクセスコアクラス
	/// </summary>
	public class Core : Credentials
	{

		#region Properties

		/// <summary>
		///  HttpClient インスタンス の管理を行います。
		/// </summary>
		private HttpClient _Client { get; set; }

		/// <summary>
		/// トークン生成時の乱数 インスタンス
		/// </summary>
		private Random _Rand { get; set; } = new Random();

		#endregion

		#region Constractor 

		/// <summary>
		/// Core クラスコンストラクタ : インスタンス生成用
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="client"> HttpClient </param>
		public Core(string ck, string cs, HttpClient client) : base(ck, cs) => _Client = client;

		/// <summary>
		/// Core クラスコンストラクタ : 設定保持用
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="at"> Access Token </param>
		/// <param name="ats"> Access Token Secret </param>
		/// <param name="id"> User ID </param>
		/// <param name="name"> Screen Name </param>
		/// <param name="client"> HttpClient </param>
		public Core(string ck, string cs, string at, string ats, string id, string name, HttpClient client)
			: base(ck, cs, at, ats, id, name) => _Client = client;

		#endregion

		#region OAuth Authorize Method's.

		/// <summary>
		/// リクエストトークンの取得を行います。
		/// </summary>
		/// <param name="url"> リクエストトークン取得用のエンドポイントURL </param>
		/// <returns></returns>
		public async Task GetRequestTokenAsync(string url)
		{
			Debug.WriteLine("------------ リクエストトークン 生成開始 -----------------");

			var response = await RequestAsync(base.ConsumerKey, base.ConsumerSecret, "", "", url, HttpMethod.Get);
			var perseData = _ParseStrings(response);

			base.RequestToken = perseData["oauth_token"];
			base.RequestTokenSecret = perseData["oauth_token_secret"];

			Debug.WriteLine("------------ リクエストトークン 生成完了 -----------------");
		}

		/// <summary>
		/// 認証ページの URL を取得します。
		/// </summary>
		/// <param name="url"> 認証ページのエンドポイントURL </param>
		/// <returns> 認証ページの URL </returns>
		public Uri GetAuthorizeUrl(string url)
		{
			Debug.WriteLine("------------ 認証用URL 取得シーケンス実施 -----------------");

			if (base.RequestToken != null)
				return new Uri($"{url}?oauth_token={base.RequestToken}");
			else
				throw new Exception("リクエストトークンが未設定です。GetRequestTokenAsync() をコールしてください。");
		}

		/// <summary>
		/// Access Token の取得を行います。
		/// </summary>
		/// <param name="url"> Access Token 取得を行うエンドポイントURL </param>
		/// <param name="pin"> 認証ページにて取得した PIN コード </param>
		/// <returns> AccessToken, AccessTokenSecret, ユーザID, スクリーンネームを Tuple 形式にて返却 </returns>
		public async Task<(string at, string ats, string uid, string sn)> GetAccessTokenAsync(string url, string pin)
		{
			Debug.WriteLine("------------ アクセストークン 生成開始 ----------------- >> " + pin);

			var parameters = new Dictionary<string, string> { { "oauth_verifier", pin } };
			var response = await RequestAsync(base.ConsumerKey, base.ConsumerSecret,
				base.RequestToken, base.RequestTokenSecret, url, HttpMethod.Post, parameters);

			var perseData = _ParseStrings(response);

			base.AccessToken = perseData["oauth_token"];
			base.AccessTokenSecret = perseData["oauth_token_secret"];
			base.UserId = perseData["user_id"];
			base.ScreenName = perseData["screen_name"];

			Debug.WriteLine("------------ アクセストークン 生成完了 -----------------");

			return (base.AccessToken, base.AccessTokenSecret, base.UserId, base.ScreenName);
		}

		/// <summary>
		/// 認証パラメータを生成します。
		/// </summary>
		/// <param name="parameters"> 認証パラメータ生成用データ </param>
		/// <param name="spl"> 分割文字(規定値：アンパサンド) </param>
		/// <param name="braket"> ブラケット(規定値："") </param>
		/// <returns> 認証パラメータ </returns>
		private string _OAuthParameters(IDictionary<string, string> parameters, string spl = "&", string braket = "")
		{
			return string.Join(spl, from p in parameters select $"{UrlEncode(p.Key)}={braket}{UrlEncode(p.Value)}{braket}");
		}

		/// <summary>
		/// Twitter に対して GETリクエスト/POSTリクエスト を行います。
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="token"> Token </param>
		/// <param name="ts"> Token Secret </param>
		/// <param name="url"> リクエストURL </param>
		/// <param name="type"> GET/POST タイプ </param>
		/// <param name="parameters"> リクエストデータ </param>
		/// <param name="stream"></param>
		/// <returns> リクエスト結果(謎の拡張子：JSON) </returns>
		public async Task<string> RequestAsync(string ck, string cs, string token, string ts,
			string url, HttpMethod type, IDictionary<string, string> parameters = null, Stream stream = null)
		{
			Debug.WriteLine("------------ リクエスト開始 ----------------- >> " + type.ToString() + " " + url);

			var oauthParameters = _GenerateParameters(token, ck);

			if (type == HttpMethod.Get && parameters != null)
				url += $"?{_OAuthParameters(parameters)}";

			var request = new HttpRequestMessage(type, url);
			HttpResponseMessage response = null;

			if (oauthParameters != null)
			{
				if (parameters != null)
				{
					foreach (var p in parameters)
						oauthParameters.Add(p.Key, p.Value);
				}

				string signature = _GenerateSignature(cs, ts, type.ToString(), url, oauthParameters);
				oauthParameters.Add("oauth_signature", signature);

				request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", _OAuthParameters(oauthParameters, ",", "\""));
			}

			Exception tmp = null;
			await Task.Run(async () =>
			{
				try
				{
					if (type == HttpMethod.Get)
					{
						Debug.WriteLine($"---------------- リクエストヘッダ情報 {type.ToString()} --------------------");
						Debug.WriteLine($" Header : {request.Headers.ToString()}");
						Debug.WriteLine("---------------- リクエストヘッダ情報 ここまで -------------------------------");
					}
					if (type == HttpMethod.Post)
					{
						if (parameters != null)
						{
							if (stream == null)
							{
								request.Content = new FormUrlEncodedContent(parameters);
							}
							else
							{
								var multi = new MultipartFormDataContent("hoge");
								multi.Add(new StreamContent(stream), "media");
								request.Content = multi;
							}
						}

						Debug.WriteLine($"---------------- リクエストヘッダ情報 {type.ToString()} --------------------");
						Debug.WriteLine($" Body   : {await request.Content.ReadAsStringAsync()}");
						Debug.WriteLine($" Header : {request.Headers.ToString()}");
						Debug.WriteLine("---------------- リクエストヘッダ情報 ここまで -------------------------------");
					}

					request.Headers.ExpectContinue = false;
					response = await _Client.SendAsync(request);

					if (response.StatusCode != HttpStatusCode.OK)
						throw new HttpRequestException($"HTTP 通信エラーが発生しました。" +
							$"ステータス : {response.StatusCode} {await response.Content.ReadAsStringAsync()} 対象：HttpMultiRequestAsync()");
				}
				catch (HttpRequestException e) { tmp = e; }
			});

			if (tmp != null)
				Console.WriteLine($"{tmp.StackTrace} : {tmp.Message}");

			var result = await response.Content.ReadAsStringAsync();
			return result;
		}

		/// <summary>
		/// Signature 生成を行います。
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="ts"> Token Secret </param>
		/// <param name="httpMethod"> http method data </param>
		/// <param name="url"> シグネチャ生成対象URL </param>
		/// <param name="parameters"> 認証パラメータ </param>
		/// <returns> HMACSHA化された Base64 シグネチャ </returns>
		private string _GenerateSignature(string ck, string ts, string httpMethod, string url, IDictionary<string, string> parameters)
		{
			Debug.WriteLine("------------ シグネチャ生成開始 -----------------");

			var sort = new SortedDictionary<string, string>(parameters);
			var uri = new Uri(url);
			var unQueryStringUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
			var signatureBase = $"{httpMethod}&{UrlEncode(unQueryStringUrl)}&{UrlEncode(_OAuthParameters(sort))}";

			HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(UrlEncode(ck) + '&' + UrlEncode(ts ?? "")));

			Debug.WriteLine("------------ シグネチャ生成完了 -----------------");

			return Convert.ToBase64String(hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(signatureBase)));
		}

		/// <summary>
		/// 認証にて使用するパラメータを構築します。
		/// </summary>
		/// <param name="token"> トークンデータ </param>
		/// <param name="ck"> Consumer Key </param>
		/// <returns> ソート後の認証パラメータ </returns>
		private SortedDictionary<string, string> _GenerateParameters(string token, string ck)
		{
			Debug.WriteLine("------------ パラメータ生成開始 -----------------");

			var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			var timeStamp = Convert.ToInt64(ts.TotalSeconds).ToString();

			var result = new SortedDictionary<string, string>
			{
				{ "oauth_consumer_key", ck },
				{ "oauth_nonce", _GenerateNonce(32) },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", timeStamp },
				{ "oauth_version", "1.0" }
			};

			if (!string.IsNullOrEmpty(token))
				result.Add("oauth_token", token);

			Debug.WriteLine($"  oauth_consumer_key = {ck}");
			Debug.WriteLine($"  oauth_signature_method = HMAC-SHA1");
			Debug.WriteLine($"  oauth_timestamp = {timeStamp}");
			Debug.WriteLine($"  oauth_nonce = {_GenerateNonce(32)}");
			Debug.WriteLine($"  oauth_version = 1.0");

			Debug.WriteLine("------------ パラメータ生成完了 -----------------");

			return result;
		}

		/// <summary>
		/// 与えられた文字列を分割します。
		/// </summary>
		/// <param name="query"> 分割前の文字列 </param>
		/// <returns> 分割したデータ(Dictionary 形式) </returns>
		private Dictionary<string, string> _ParseStrings(string query)
		{
			var persedStr = new Dictionary<string, string>();

			foreach (var items in query.Split('&'))
			{
				var sepalator = items.Split('=');
				persedStr.Add(sepalator[0], sepalator[1]);
			}

			return persedStr;
		}

		/// <summary>
		/// ワンタイムトークンを生成します。
		/// </summary>
		/// <param name="len"> トークン生成の文字数 </param>
		/// <returns> ワンタイムトークン </returns>
		private string _GenerateNonce(int len)
		{
			string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return str.Aggregate(new StringBuilder(len), (sb, s) => sb.Append(str[_Rand.Next(str.Length)])).ToString();
		}

		/// <summary>
		/// 与えられた URL をエンコードします。
		/// </summary>
		/// <param name="value"> エンコード対象の URL </param>
		/// <returns> エンコード後の URL </returns>
		public string UrlEncode(string value)
		{
			string unreserved = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
			var result = new StringBuilder();

			foreach (var b in Encoding.UTF8.GetBytes(value))
			{
				var isReserved = (b < 0x80 && unreserved.IndexOf((char)b) != -1);
				result.Append(isReserved ? ((char)b).ToString() : $"%{(int)b:X2}");
			}

			return result.ToString();
		}

		#endregion

	}
}
