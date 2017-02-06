using System;

namespace RData.JsonRpc
{
    public class JsonRpcException : Exception
    {
        public JsonRpcException()
        {
        }

        public JsonRpcException(string message)
            : base(message)
        {
        }

        public JsonRpcException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}