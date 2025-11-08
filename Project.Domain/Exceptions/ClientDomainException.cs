namespace Project.Domain.Exceptions
{
    public class ClientDomainException : DomainException
    {
        public ClientDomainException(string message) : base(message) { }
    }
}