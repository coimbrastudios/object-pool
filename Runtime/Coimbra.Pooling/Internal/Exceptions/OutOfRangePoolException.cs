namespace Coimbra
{
    internal sealed class OutOfRangePoolException : SpawnPoolException
    {
        public OutOfRangePoolException() : base("Pool has no available instance!") { }
    }
}
