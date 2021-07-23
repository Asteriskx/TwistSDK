namespace Twist.Interop
{
    internal static class EndPoints
    {
        /// <summary>
        /// Access Token を取得する際に必要なエンドポイント 
        /// </summary>
        internal static readonly string AccessToken = "https://api.twitter.com/oauth/access_token";

        /// <summary>
        /// 認証を実施する際に必要なエンドポイント
        /// </summary>
        internal static readonly string Authorize = "https://api.twitter.com/oauth/authorize";

        /// <summary>
        /// 画像アップロードを実施する際に必要なエンドポイント
        /// </summary>
        internal static readonly string ChunkUpload = "https://upload.twitter.com/1.1/media/upload.json";

        /// <summary>
        /// 認証画面を表示する際に必要なエンドポイント
        /// </summary>
        internal static readonly string RequestToken = "https://api.twitter.com/oauth/request_token";

        /// <summary>
        /// 投稿を実施する際に必要なエンドポイント
        /// </summary>
        internal static readonly string Update = "https://api.twitter.com/1.1/statuses/update.json";
    }
}
