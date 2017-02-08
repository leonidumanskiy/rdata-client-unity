using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;

namespace RData.Requests.Contexts
{
    public class RestoreContextRequest : JsonRpcRequest<RestoreContextRequest.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "restoreContext"; }
        }

        public override bool IsBulked
        {
            get { return true; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }
        }

        public RestoreContextRequest() : base() { }

        public RestoreContextRequest(RDataBaseContext context) : base()
        {
            Params = new Parameters()
            {
                Id = context.Id,
            };
        }
    }
}