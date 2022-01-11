namespace Coimbra
{
    internal sealed class InactivePoolException : SpawnPoolException
    {
        public InactivePoolException() : base("Pool is inactive!") { }
    }
}
