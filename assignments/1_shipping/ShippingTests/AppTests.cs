using Xunit;
using FluentAssertions;
using Newtonsoft.Json;

namespace shipping
{
    public class AppTests
    {
        [Theory]
        [InlineData(5, "B")]
        [InlineData(5, "A")]
        [InlineData(5, "A", "B")]
        [InlineData(7, "A", "B", "B")]
        [InlineData(29, "A", "A", "B", "A", "B", "B", "A", "B")]
        [InlineData(29, "A", "A", "A", "A", "B", "B", "B", "B")]
        [InlineData(49, "B", "B", "B", "B", "A", "A", "A", "A")]
        public void Run(int actual, params string[] destinations)
        {
            // arrange
            var sut = new App();

            // act
            var (result, _) = sut.Run(destinations);

            // assert
            result.Should().Be(actual);
        }

        [Theory]
        [InlineData(5, "A", "B")]
        public void Run_again(int actual, params string[] destinations)
        {
            // arrange
            var sut = new App();

            // act
            var (_, events) = sut.Run(destinations);

            var temp = JsonConvert.SerializeObject(events);
            // assert
            //events..Should().Be(actual);
        }
    }
}
