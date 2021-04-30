/* 
*   NatCorder
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders {

    using System;
    using System.Collections.Concurrent;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Recorder wrapper which dispatches commits to worker threads.
    /// </summary>
    public sealed class DispatchRecorder : IMediaRecorder { // DEPLOY // Rename this

        #region --Client API--
        /// <summary>
        /// Frame size.
        /// </summary>
        public (int width, int height) frameSize => recorder.frameSize;

        /// <summary>
        /// Create a dispatch recorder.
        /// </summary>
        /// <param name="recorder">Recorder to commit frames to.</param>
        public DispatchRecorder (IMediaRecorder recorder) {
            this.recorder = recorder;
            this.queue = new ConcurrentQueue<(byte[], long)>();
            this.cts = new CancellationTokenSource();
            this.task = new Task(() => {
                while (!cts.Token.IsCancellationRequested || queue.Count > 0) // We can't drop frames
                    if (queue.TryDequeue(out var frame))
                        recorder.CommitFrame(frame.Item1, frame.Item2);
            }, cts.Token, TaskCreationOptions.LongRunning);
            task.Start();
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer containing video frame to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public unsafe void CommitFrame<T> (T[] pixelBuffer, long timestamp) where T : unmanaged {
            fixed (T* baseAddress = pixelBuffer)
                CommitFrame(baseAddress, timestamp);
        }

        /// <summary>
        /// Commit a video pixel buffer for encoding.
        /// The pixel buffer MUST have an RGBA8888 pixel layout.
        /// </summary>
        /// <param name="nativeBuffer">Pixel buffer in native memory to commit.</param>
        /// <param name="timestamp">Pixel buffer timestamp in nanoseconds.</param>
        public unsafe void CommitFrame (void* nativeBuffer, long timestamp) {
            // Recorders MUST NOT drop frames so we copy and dispatch
            // Sucks for memory pressure, but is good for performance when native commit is bottleneck
            var pixelBuffer = new byte[recorder.frameSize.width * recorder.frameSize.height * 4];
            Marshal.Copy((IntPtr)nativeBuffer, pixelBuffer, 0, pixelBuffer.Length);
            queue.Enqueue((pixelBuffer, timestamp));
        }

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="sampleBuffer">Sample buffer to commit.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public unsafe void CommitSamples (float[] sampleBuffer, long timestamp) {
            fixed (float* baseAddress = sampleBuffer)
                CommitSamples(baseAddress, sampleBuffer.Length, timestamp);
        }

        /// <summary>
        /// Commit an audio sample buffer for encoding.
        /// The sample buffer MUST be a linear PCM buffer interleaved by channel.
        /// </summary>
        /// <param name="nativeBuffer">Sample buffer in native memory to commit.</param>
        /// <param name="sampleCount">Total number of samples in the buffer.</param>
        /// <param name="timestamp">Sample buffer timestamp in nanoseconds.</param>
        public unsafe void CommitSamples (float* nativeBuffer, int sampleCount, long timestamp) => recorder.CommitSamples(nativeBuffer, sampleCount, timestamp);

        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <returns>Path to directory containing image sequence.</returns>
        public Task<string> FinishWriting () {
            cts.Cancel();
            return task.ContinueWith(t => recorder.FinishWriting()).Unwrap();
        }
        #endregion


        #region --Operations--
        private readonly IMediaRecorder recorder;
        private readonly ConcurrentQueue<(byte[], long)> queue;
        private readonly CancellationTokenSource cts;
        private readonly Task task;
        #endregion
    }
}