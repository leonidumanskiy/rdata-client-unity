namespace RData.Exceptions
{
    public class NonAuthorizedException : RDataException
    {
        public NonAuthorizedException()
        {
        }

        public NonAuthorizedException(string message)
            : base(message)
        {
        }

        public NonAuthorizedException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        public NonAuthorizedException(JsonRpc.JsonRpcError<string> error)
            : base(error.Message)
        {
        }
    }
}