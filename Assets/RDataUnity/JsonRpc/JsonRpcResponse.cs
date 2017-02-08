using LitJson;

namespace RData.JsonRpc
{
    public class JsonRpcResponse<TResult> : JsonRpcBaseResponse
    {
        [JsonAlias("result")]
        public TResult Result { get; set; }

        [JsonAlias("error")]
        public JsonRpcError<string> Error { get; set; } // errors with string data currently supported

        public bool HasError
        {
            get { return Error != null; }
        }
    }
}
