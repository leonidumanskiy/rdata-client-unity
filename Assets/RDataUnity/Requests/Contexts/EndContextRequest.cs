using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;

namespace RData.Requests.Contexts
{
    public class EndContextRequest : JsonRpcRequest<EndContextRequest.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "endContext"; }
        }

        public override bool IsBulked
        {
            get { return true; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }

            [LitJson.JsonAlias("timeEnded")]
            public long TimeEnded { get; set; }
        }

        public EndContextRequest() : base() { }

        public EndContextRequest(RDataBaseContext context) : base()
        {
            Params = new Parameters()
            {
                Id = context.Id,
                TimeEnded = Tools.Time.DateTimeToUnixTime(context.TimeEnded)
            };
        }

    }
}