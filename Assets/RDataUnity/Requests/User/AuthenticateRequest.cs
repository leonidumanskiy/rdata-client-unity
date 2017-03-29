using RData.JsonRpc;
using RData.Responses;
using RData.LitJson;

namespace RData.Requests.User
{
    public class AuthorizationRequest : JsonRpcRequest<AuthorizationRequest.Parameters, BooleanResponse>
    {
        [JsonAlias("method")]
        public override string Method
        {
            get { return "authorize"; }
        }

        [JsonIgnore]
        public override bool IsBulked
        {
            get { return false; }
        }

        public class Parameters
        {
            [RData.LitJson.JsonAlias("userId")]
            public string UserId { get; set; }
        }

        public AuthorizationRequest() : base() { }

        public AuthorizationRequest(string userId) : base()
        {
            Params = new Parameters()
            {
                UserId = userId
            };
        }
    }
}