/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Inputs {

    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Unity.Collections.LowLevel.Unsafe;
    using Rendering;

    /// <summary>
    /// Recorder input for recording video frames from textures with hardware acceleration on iOS Metal.
    /// </summary>
    public sealed class MTLTextureInput : ITextureInput {

        #region --Client API--
        /// <summary>
        /// Create a Metal texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="multithreading">Enable multithreading. This improves recording performance.</param>
        public MTLTextureInput (IMediaRecorder recorder, bool multithreading = false) {
            // Check platform
            if (Application.platform != RuntimePlatform.IPhonePlayer)
                throw new InvalidOperationException(@"MTLTextureInput can only be used on iOS");
            // Check render API
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal)
                throw new InvalidOperationException(@"MTLTextureInput can only be used with Metal");
            // Save
            this.recorder = recorder;
            this.readback = new MTLReadback(recorder.frameSize.width, recorder.frameSize.height, multithreading);
        }

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public unsafe void CommitFrame (Texture texture, long timestamp) => readback.Request(
            texture,
            pixelBuffer => recorder?.CommitFrame(
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(pixelBuffer),
                timestamp
            )
        );

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {
            recorder = default;
            readback.Dispose();
        }
        #endregion


        #region --Operations--

        private IMediaRecorder recorder;
        private readonly MTLReadback readback;
        
        (int, int) ITextureInput.frameSize => recorder.frameSize;
        #endregion
    }
}