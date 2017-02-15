using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;

namespace RData.Requests.Contexts
{
    public class EndInterruptedContextsRequest : JsonRpcRequest<EndInterruptedContextsRequest.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "restoreInterruptedContexts"; }
        }

        public override bool IsBulked
        {
            get { return false; }
        }

        public class Parameters { }

        public EndInterruptedContextsRequest() : base() { }
    }
}