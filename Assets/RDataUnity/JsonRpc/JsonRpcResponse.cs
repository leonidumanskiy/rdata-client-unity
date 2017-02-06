using LitJson;

namespace RData.JsonRpc
{
    public class JsonRpcResponse<TResult> : JsonRpcBaseResponse
    {
        [JsonAlias("result")]
        public TResult Result { get; set; }

        [JsonAlias("error")]
        public JsonRpcError<object> Error { get; set; }
    }
}
