using System;
using LitJson;

namespace RData.JsonRpc
{
    public abstract class JsonRpcRequest<TParams, TResponse> : JsonRpcBaseRequest
    {
        [JsonAlias("params")]
        public TParams Params { get; set; }
        
        [JsonIgnore]
        public TResponse Response { get; private set; }

        public sealed override void SetResponse(object response)
        {
            if (response is TResponse)
                Response = (TResponse)response;
            else
                throw new JsonRpcException(string.Format("Response object is {0}, expected it to be {1}", response.GetType().Name, typeof(TResponse).Name));
        }

        protected JsonRpcRequest() : base()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}