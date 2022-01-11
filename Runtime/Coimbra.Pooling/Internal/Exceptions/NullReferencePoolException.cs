namespace Coimbra
{
    internal sealed class NullReferencePoolException : System.NullReferenceException
    {
        public NullReferencePoolException() : base("prefab is null!") { }
    }
}
