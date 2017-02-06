using RData.JsonRpc;
using RData.Responses;

namespace RData.Requests.User
{
    public class AuthenticateRequest : JsonRpcRequest<AuthenticateRequest.Parameters, BooleanResponse>
    {
        public class Parameters
        {
            [LitJson.JsonAlias("userId")]
            public string UserId { get; set; }
        }

        public override string Method
        {
            get { return "authenticate"; }
        }

        public override bool IsBulked
        {
            get { return false; }
        }

        public AuthenticateRequest(string userId)
        {
            Params = new Parameters()
            {
                UserId = userId
            };
        }
    }
}