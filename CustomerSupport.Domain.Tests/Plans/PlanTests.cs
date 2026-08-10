using CustomerSupport.Domain.Plans;
using Xunit;

namespace CustomerSupport.Domain.Tests.Plans;

public sealed class PlanTests
{
    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        Assert.Throws<ArgumentException>(() => Plan.Create(Guid.Empty, "Pro"));
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Plan.Create(Guid.NewGuid(), "   "));
    }

    [Fact]
    public void Create_WithValidIdAndName_CreatesPlan()
    {
        var id= Guid.Parse("40000000-0000-0000-0000-000000000001");
        var name = "Pro";
        var plan = Plan.Create(id, name);
        Assert.Equal(id, plan.Id);
        Assert.Equal(name, plan.Name);
    }
}
