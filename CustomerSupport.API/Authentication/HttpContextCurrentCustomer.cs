using CustomerSupport.Application.Authentication;

namespace CustomerSupport.Api.Authentication;

internal sealed class HttpContextCurrentCustomer(IHttpContextAccessor httpContextAccessor)
    : ICurrentCustomer
{
    public Guid CustomerId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No active HTTP request is available.");

            if (httpContext.User.Identity?.IsAuthenticated is not true)
            {
                throw new InvalidOperationException("No authenticated customer is available.");
            }

            var customerIdValue = httpContext.User.FindFirst("customer_id")?.Value;

            if (!Guid.TryParse(customerIdValue, out var customerId) || customerId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "The authenticated access token does not contain a valid customer identifier.");
            }

            return customerId;
        }
    }
}
