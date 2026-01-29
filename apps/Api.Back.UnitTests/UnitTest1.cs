using Api.Back.Models;

namespace Api.Back.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        TestModel model = new TestModel
        {
            Id = 1,
            Name = "Test"
        };
        Assert.True(true);
    }
}
