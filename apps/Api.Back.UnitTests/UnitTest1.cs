namespace Api.Back.UnitTests;

using Api.Back.Models;

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
