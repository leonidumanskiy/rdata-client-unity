using RData.LitJson;

namespace RData.JsonRpc
{
    public class JsonRpcBaseResponse
    {
        [JsonAlias("jsonrpc")]
        public virtual string JsonRpc { get { return "2.0"; } }

        [JsonAlias("id")]
        public virtual string Id { get; set; }
    }
}
