namespace RData.Exceptions
{
    public class RDataException : System.Exception
    {
        public RDataException()
        {
        }

        public RDataException(string message)
            : base(message)
        {
        }

        public RDataException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        public RDataException(JsonRpc.JsonRpcError<string> error)
            : base(error.Message)
        {
        }
    }
}