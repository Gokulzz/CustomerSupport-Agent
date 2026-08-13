namespace CustomerSupport.Application.Authentication;

public interface ICurrentCustomer
{
    Guid CustomerId { get; }
}
