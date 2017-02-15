using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;

namespace RData.Requests.Contexts
{
    public class RestoreInterruptedContextsRequest : JsonRpcRequest<RestoreInterruptedContextsRequest.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "restoreInterruptedContexts"; }
        }

        public override bool IsBulked
        {
            get { return false; }
        }

        public class Parameters {}

        public RestoreInterruptedContextsRequest() : base() { }
    }
}