using RData.JsonRpc;
using RData.Responses;
using RData.Events;
using RData.Contexts;
using LitJson;

namespace RData.Requests.Contexts
{
    public class UpdateContextDataVariableRequest : JsonRpcRequest<UpdateContextDataVariableRequest.Parameters, BooleanResponse>
    {
        [JsonAlias("method")]
        public override string Method
        {
            get { return "updateContextDataVariable"; }
        }

        [JsonIgnore]
        public override bool IsBulked
        {
            get { return true; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("id")]
            public string Id { get; set; }

            [LitJson.JsonAlias("key")]
            public string Key { get; set; }

            [LitJson.JsonAlias("value")]
            public object Value { get; set; }

            [LitJson.JsonAlias("timeUpdated")]
            public long TimeUpdated { get; set; }
        }

        public UpdateContextDataVariableRequest() : base() { }

        public UpdateContextDataVariableRequest(RDataBaseContext context, string key, object value, long? timeUpdated = null) : this()
        {
            Params = new Parameters()
            {
                Id = context.Id,
                Key = key,
                Value = value,
                TimeUpdated = timeUpdated.HasValue ? timeUpdated.Value : Tools.Time.UnixTimeMilliseconds
            };
        }
    }
}