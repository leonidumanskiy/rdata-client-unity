using RData.LitJson;

namespace RData.JsonRpc
{
    public class JsonRpcError<TErrorData>
    {
        [JsonAlias("code")]
        public int Code { get; set; }

        [JsonAlias("message")]
        public string Message { get; set; }

        [JsonAlias("data")]
        public TErrorData Data { get; set; }
    }
}