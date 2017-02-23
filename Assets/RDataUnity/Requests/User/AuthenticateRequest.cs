using RData.JsonRpc;
using RData.Responses;
using LitJson;

namespace RData.Requests.User
{
    public class AuthenticateRequest : JsonRpcRequest<AuthenticateRequest.Parameters, BooleanResponse>
    {
        [JsonAlias("method")]
        public override string Method
        {
            get { return "authenticate"; }
        }

        [JsonIgnore]
        public override bool IsBulked
        {
            get { return false; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("userId")]
            public string UserId { get; set; }
        }

        public AuthenticateRequest() : base() { }

        public AuthenticateRequest(string userId) : base()
        {
            Params = new Parameters()
            {
                UserId = userId
            };
        }
    }
}