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

        public class Parameters
        {
            [LitJson.JsonAlias("userId")]
            public string UserId { get; set; }
        }

        public override bool IsBulked
        {
            get { return false; }
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