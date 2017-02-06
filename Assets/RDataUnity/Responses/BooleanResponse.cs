using RData.JsonRpc;

namespace RData.Responses
{
    public class BooleanResponse : JsonRpcResponse<bool>
    {
        public BooleanResponse() { }

        public BooleanResponse(bool value)
        {
            this.Result = value;
        }
    }
}