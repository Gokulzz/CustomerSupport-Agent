namespace CustomerSupport.Application.Authentication;

public interface IAccessTokenService
{
    AccessToken Create(Guid userId, Guid customerId, string email);
}
