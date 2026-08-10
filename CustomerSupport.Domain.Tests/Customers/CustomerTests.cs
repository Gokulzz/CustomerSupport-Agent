using CustomerSupport.Domain.Customers;
using Xunit;

namespace CustomerSupport.Domain.Tests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(Guid.Empty, "Alice"));
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(Guid.NewGuid(), "   "));
    }
}
