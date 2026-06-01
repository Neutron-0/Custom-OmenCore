using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OmenCore.Services.KeyboardLighting;
using Xunit;

namespace OmenCoreApp.Tests.Services.KeyboardLighting
{
    public class WmiBiosBackendTests
    {
        [Fact]
        public async Task VerifyColorReadbackWithRetriesAsync_ThirdReadMatches_ReturnsSuccess()
        {
            var expected = new[]
            {
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(0, 0, 255),
                Color.FromArgb(255, 255, 0)
            };

            var firstReadBack = new[]
            {
                Color.Black,
                Color.Black,
                Color.Black,
                Color.Black
            };

            var secondReadBack = new[]
            {
                Color.FromArgb(10, 10, 10),
                Color.FromArgb(10, 10, 10),
                Color.FromArgb(10, 10, 10),
                Color.FromArgb(10, 10, 10)
            };

            var retryReads = new Queue<Color[]?>();
            retryReads.Enqueue(secondReadBack);
            retryReads.Enqueue(expected);

            var delayCalls = new List<int>();

            var result = await WmiBiosBackend.VerifyColorReadbackWithRetriesAsync(
                expected,
                firstReadBack,
                () => Task.FromResult(retryReads.Dequeue()),
                ms =>
                {
                    delayCalls.Add(ms);
                    return Task.CompletedTask;
                });

            result.ColorsMatch.Should().BeTrue();
            result.LastReadBack.Should().NotBeNull();
            result.LastReadBack!.Select(c => c.ToArgb()).Should().Equal(expected.Select(c => c.ToArgb()));
            delayCalls.Should().Equal(150, 500);
            retryReads.Should().BeEmpty();
        }

        [Fact]
        public async Task VerifyColorReadbackWithRetriesAsync_AllRetriesMismatch_ReturnsFailure()
        {
            var expected = new[]
            {
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(0, 0, 255),
                Color.FromArgb(255, 255, 0)
            };

            var firstReadBack = new[]
            {
                Color.Black,
                Color.Black,
                Color.Black,
                Color.Black
            };

            var retryReads = new Queue<Color[]?>();
            retryReads.Enqueue(new[] { Color.FromArgb(3, 3, 3), Color.FromArgb(3, 3, 3), Color.FromArgb(3, 3, 3), Color.FromArgb(3, 3, 3) });
            retryReads.Enqueue(new[] { Color.FromArgb(4, 4, 4), Color.FromArgb(4, 4, 4), Color.FromArgb(4, 4, 4), Color.FromArgb(4, 4, 4) });

            var delayCalls = new List<int>();

            var result = await WmiBiosBackend.VerifyColorReadbackWithRetriesAsync(
                expected,
                firstReadBack,
                () => Task.FromResult(retryReads.Dequeue()),
                ms =>
                {
                    delayCalls.Add(ms);
                    return Task.CompletedTask;
                });

            result.ColorsMatch.Should().BeFalse();
            result.LastReadBack.Should().NotBeNull();
            result.LastReadBack!.All(c => c.R == 4 && c.G == 4 && c.B == 4).Should().BeTrue();
            delayCalls.Should().Equal(150, 500);
            retryReads.Should().BeEmpty();
        }
    }
}
