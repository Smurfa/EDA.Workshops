using Xunit;
using FluentAssertions;

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
            var result = sut.Run(destinations);

            // assert
            result.Should().Be(actual);
        }
    }
}
