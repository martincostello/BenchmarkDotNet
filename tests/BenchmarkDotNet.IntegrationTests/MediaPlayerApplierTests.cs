using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Tests.Loggers;
using BenchmarkDotNet.Tests.XUnit;
using Xunit.Abstractions;

namespace BenchmarkDotNet.IntegrationTests
{
    public class MediaPlayerApplierTests
    {
        private readonly ITestOutputHelper output;

        public MediaPlayerApplierTests(ITestOutputHelper output) => this.output = output;

        [FactEnvSpecific("Media player detection is only supported on Windows", EnvRequirement.WindowsOnly)]
        public void IsMediaPlayingDoesNotThrowOnWindows()
        {
            // Just verify that the method runs without throwing; we don't know if music is playing in CI
            bool result = MediaPlayerHelper.IsMediaPlaying();
            output.WriteLine($"IsMediaPlaying returned: {result}");
        }

        [FactEnvSpecific("Media player applier is only relevant on Windows", EnvRequirement.WindowsOnly)]
        public void ApplyAndDisposeDoNotThrowOnWindows()
        {
            var logger = new OutputLogger(output);
            using var applier = new MediaPlayerApplier(logger);

            // Should not throw even when no media is playing
            applier.Apply();
        }

        [Fact]
        public void ApplyDoesNotThrowOnNonWindows()
        {
            var logger = new OutputLogger(output);
            using var applier = new MediaPlayerApplier(logger);

            // Should silently no-op on non-Windows platforms
            applier.Apply();
        }
    }
}
