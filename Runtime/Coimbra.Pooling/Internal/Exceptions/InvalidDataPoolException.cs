namespace Coimbra
{
    internal sealed class InvalidDataPoolException : System.ArgumentException
    {
        public InvalidDataPoolException(string paramName) : base("PoolData is not valid!", paramName, new NullReferencePoolException()) { }
    }
}
