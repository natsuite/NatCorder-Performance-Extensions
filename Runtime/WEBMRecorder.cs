/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders {
    
    using Internal;

    /// <summary>
    /// WEBM video recorder.
    /// This recorder is only supported on WebGL.
    /// </summary>
    public sealed class WEBMRecorder : NativeRecorder { // INCOMPLETE

        #region --Client API--
        /// <summary>
        /// Create a WEBM recorder.
        /// </summary>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="frameRate">Video frame rate.</param>
        /// <param name="sampleRate">Audio sample rate. Pass 0 for no audio.</param>
        /// <param name="channelCount">Audio channel count. Pass 0 for no audio.</param>
        /// <param name="videoBitRate">Video bit rate in bits per second.</param>
        /// <param name="audioBitRate">Audio bit rate in bits per second.</param>
        public WEBMRecorder (
            int width,
            int height,
            float frameRate,
            int sampleRate = 0,
            int channelCount = 0,
            int videoBitRate = 10_000_000,
            int audioBitRate = 64_000 // INCOMPLETE // CHECK
        ) : base(WebBridge.CreateWEBMRecorder(width, height, frameRate, sampleRate, channelCount, videoBitRate)) { }
        #endregion
    }
}