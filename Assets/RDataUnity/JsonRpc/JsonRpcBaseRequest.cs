using LitJson;

namespace RData.JsonRpc
{
    public class JsonRpcBaseRequest
    {
        [JsonAlias("jsonrpc")]
        public virtual string JsonRpc { get { return "2.0"; } }
        
        [JsonAlias("id")]
        public virtual string Id { get; set; }

        [JsonAlias("method")]
        public virtual string Method { get; set; }

        [JsonAlias("created_at")]
        public virtual long CreatedAt { get; set; } // This is not used by the server, but the the rdata library instead

        [JsonIgnore]
        public virtual bool IsBulked { get; set; }
        
        public virtual void SetResponse(object response){}
        
        public JsonRpcBaseRequest()
        {
            CreatedAt = Tools.Time.DateTimeToUnixTimeMilliseconds(System.DateTime.UtcNow);
        }
    }
}
