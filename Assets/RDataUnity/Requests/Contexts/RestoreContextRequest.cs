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

        public class Parameters
        {
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }
        }

        public RestoreContextRequest(RDataBaseContext context)
        {
            Params = new Parameters()
            {
                Id = context.Id,
            };
        }
    }
}