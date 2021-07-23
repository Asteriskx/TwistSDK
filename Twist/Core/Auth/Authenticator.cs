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

using Twist.Auth.Interfaces;
using Twist.Core.Auth.Interfaces;

namespace Twist.Core.Auth
{
	/// <summary>
	/// Twitter APIv1.1 認証周りを担当するクラス
	/// </summary>
	public class Authenticator : IAuthenticator
	{
		#region Properties

		/// <summary>
		/// HTTP クライアント
		/// </summary>
		private HttpClient _Client { get; set; }

		/// <summary>
		/// トークン 生成時の乱数 インスタンス
		/// </summary>
		private Random _SeedToken { get; set; } = new Random();

		/// <summary>
		/// 認証情報 : I/F経由用
		/// </summary>
		public ITwistCredentials ICredentials { get; set; }

		#endregion

		#region Constractor 

		/// <summary>
		/// コンストラクタ : インスタンス生成用
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="client"> HttpClient </param>
		internal Authenticator(string ck, string cs, HttpClient client)
		{
			ICredentials = new TwistCredentials(ck, cs);
			_Client = client;
		}

		/// <summary>
		/// コンストラクタ : 設定保持用
		/// </summary>
		/// <param name="ck"> Consumer Key </param>
		/// <param name="cs"> Consumer Secret </param>
		/// <param name="at"> Access Token </param>
		/// <param name="ats"> Access Token Secret </param>
		/// <param name="id"> User ID </param>
		/// <param name="name"> Screen Name </param>
		/// <param name="client"> HttpClient </param>
		internal Authenticator(string ck, string cs, string at, string ats, string id, string name, HttpClient client)
		{
			ICredentials = new TwistCredentials(ck, cs, at, ats, id, name);
			_Client = client;
		}

		#endregion

		#region OAuth Authorize Method's

		/// <summary>
		/// リクエスト トークンの取得を行います。
		/// </summary>
		/// <param name="url"> リクエスト トークン取得用のエンド ポイント URL </param>
		/// <returns></returns>
		async Task IAuthenticator.GetRequestTokenAsync(string url)
		{
			IAuthenticator authenticator = this;
			var response = await authenticator.RequestAsync(ICredentials.ConsumerKey, ICredentials.ConsumerSecret, "", "", url, HttpMethod.Get);
			var perseData = _ParseStrings(response);

			ICredentials.RequestToken = perseData["oauth_token"];
			ICredentials.RequestTokenSecret = perseData["oauth_token_secret"];
		}

		/// <summary>
		/// 認証ページの URL を取得します。
		/// </summary>
		/// <param name="url"> 認証ページのエンドポイントURL </param>
		/// <returns> 認証ページの URL </returns>
		 Uri IAuthenticator.GetAuthorizeUrl(string url)
		{
			if (ICredentials.RequestToken is not null)
				return new Uri($"{url}?oauth_token={ICredentials.RequestToken}");
			else
				throw new Exception("リクエストトークンが未設定です。GetRequestTokenAsync() をコールしてください。");
		}

		/// <summary>
		/// Access Token の取得を行います。
		/// </summary>
		/// <param name="url"> Access Token 取得を行うエンドポイントURL </param>
		/// <param name="pin"> 認証ページにて取得した PIN コード </param>
		/// <returns> AccessToken, AccessTokenSecret, ユーザID, スクリーンネームを Tuple 形式にて返却 </returns>
		async Task<(string at, string ats, string uid, string sn)> IAuthenticator.GetAccessTokenAsync(string url, string pin)
		{
			var parameters = new Dictionary<string, string> { { "oauth_verifier", pin } };
			IAuthenticator authenticator = this;

			var response = await authenticator.RequestAsync(
				ICredentials.ConsumerKey, 
				ICredentials.ConsumerSecret,
				ICredentials.RequestToken, 
				ICredentials.RequestTokenSecret, 
				url, 
				HttpMethod.Post, 
				parameters
			);

			var perseData = _ParseStrings(response);

			ICredentials.AccessToken = perseData["oauth_token"];
			ICredentials.AccessTokenSecret = perseData["oauth_token_secret"];
			ICredentials.UserId = perseData["user_id"];
			ICredentials.ScreenName = perseData["screen_name"];

			return (ICredentials.AccessToken, ICredentials.AccessTokenSecret, ICredentials.UserId, ICredentials.ScreenName);
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
		async Task<string> IAuthenticator.RequestAsync(string ck, string cs, string token, string ts,
			string url, HttpMethod type, IDictionary<string, string> parameters = null, Stream stream = null)
		{
			var oauthParameters = _GenerateParameters(token, ck);

			if (type == HttpMethod.Get && parameters is not null)
				url += $"?{_OAuthParameters(parameters)}";

			HttpRequestMessage request = new(type, url);
			HttpResponseMessage response = null;

			if (oauthParameters is not null)
			{
				if (parameters is not null)
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
					if (type == HttpMethod.Post)
					{
						if (parameters is not null)
						{
							request.Content = (stream is not null) ?
								new MultipartFormDataContent("dummy_data") { { new StreamContent(stream), "media" } } :
								new FormUrlEncodedContent(parameters);
						}
					}

					request.Headers.ExpectContinue = false;
					response = await _Client.SendAsync(request);

					if (response.StatusCode != HttpStatusCode.OK)
						throw new HttpRequestException($"HTTP 通信エラー。" +
							$"ステータス : {response.StatusCode} {await response.Content.ReadAsStringAsync()} 対象：HttpMultiRequestAsync()");
				}
				catch (HttpRequestException e) { tmp = e; }
			});

			if (tmp is not null)
				Debug.WriteLine($"{tmp.StackTrace} : {tmp.Message}");

			var result = await response.Content.ReadAsStringAsync();
			return result;
		}

		/// <summary>
		/// 認証パラメータを生成します。
		/// </summary>
		/// <param name="parameters"> 認証パラメータ生成用データ </param>
		/// <param name="spl"> 分割文字(規定値：アンパサンド) </param>
		/// <param name="braket"> ブラケット(規定値："") </param>
		/// <returns> 認証パラメータ </returns>
		private string _OAuthParameters(IDictionary<string, string> parameters, string spl = "&", string braket = "") =>
			string.Join(spl, from p in parameters select $"{_UrlEncode(p.Key)}={braket}{_UrlEncode(p.Value)}{braket}");

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
			SortedDictionary<string, string> sort = new(parameters);
			Uri uri = new(url);
			var unQueryStringUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
			var signatureBase = $"{httpMethod}&{_UrlEncode(unQueryStringUrl)}&{_UrlEncode(_OAuthParameters(sort))}";

			HMACSHA1 hmacsha1 = new(Encoding.ASCII.GetBytes(_UrlEncode(ck) + '&' + _UrlEncode(ts ?? "")));
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
			return str.Aggregate(new StringBuilder(len), (sb, s) => sb.Append(str[_SeedToken.Next(str.Length)])).ToString();
		}

		/// <summary>
		/// 与えられた URL をエンコードします。
		/// </summary>
		/// <param name="value"> エンコード対象の URL </param>
		/// <returns> エンコード後の URL </returns>
		private string _UrlEncode(string value)
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
