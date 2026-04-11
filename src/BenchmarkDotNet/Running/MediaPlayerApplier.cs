using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Detectors;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;

namespace BenchmarkDotNet.Running
{
    internal class MediaPlayerApplier : DisposeAtProcessTermination
    {
        private readonly ILogger logger;
        private bool wasPlaying;

        internal MediaPlayerApplier(ILogger logger) => this.logger = logger;

        protected override void Dispose(bool exiting)
        {
            ResumeMediaIfNeeded();
            base.Dispose(exiting);
        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This code is protected with a runtime OS platform check")]
        internal void Apply()
        {
            if (!OsDetector.IsWindows())
                return;

            try
            {
                if (MediaPlayerHelper.IsMediaPlaying())
                {
                    MediaPlayerHelper.PauseMedia();
                    wasPlaying = true;
                    logger.WriteLineInfo("// Paused media playback during benchmarks.");
                }
            }
            catch (Exception ex)
            {
                logger.WriteLineError($"// Cannot pause media playback (error message: {ex.Message})");
            }
        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This code is protected with a runtime OS platform check")]
        private void ResumeMediaIfNeeded()
        {
            if (wasPlaying && OsDetector.IsWindows())
            {
                try
                {
                    MediaPlayerHelper.ResumeMedia();
                    wasPlaying = false;
                    logger.WriteLineInfo("// Resumed media playback after benchmarks.");
                }
                catch (Exception ex)
                {
                    logger.WriteLineError($"// Cannot resume media playback (error message: {ex.Message})");
                }
            }
        }
    }
}
