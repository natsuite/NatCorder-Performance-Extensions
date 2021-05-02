/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Inputs {

    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Clocks;

    /// <summary>
    /// </summary>
    public sealed class BlitCameraInput : IDisposable { // INCOMPLETE // Inverted image on Metal/DX

        #region --Client API--
        /// <summary>
        /// Control number of successive camera frames to skip while recording.
        /// This is very useful for GIF recording, which typically has a lower framerate appearance.
        /// </summary>
        public int frameSkip;

        /// <summary>
        /// Create a video recording input from a game camera.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="camera">Game camera to record.</param>
        public BlitCameraInput (IMediaRecorder recorder, Camera camera) : this(recorder, default, camera) { }

        /// <summary>
        /// Create a video recording input from one or more game cameras.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="camera">Game cameras to record.</param>
        public BlitCameraInput (IMediaRecorder recorder, IClock clock, Camera camera) : this(CreateInput(recorder), clock, camera) { }

        /// <summary>
        /// Create a video recording input from a game camera.
        /// </summary>
        /// <param name="input">Texture input to receive video frames.</param>
        /// <param name="camera">Game camera to record.</param>
        public BlitCameraInput (ITextureInput input, Camera camera) : this(input, default, camera) { }
        
        /// <summary>
        /// Create a video recording input from one or more game cameras.
        /// </summary>
        /// <param name="input">Texture input to receive video frames.</param>
        /// <param name="clock">Clock for generating timestamps.</param>
        /// <param name="camera">Game cameras to record.</param>
        public BlitCameraInput (ITextureInput input, IClock clock, Camera camera) {
            // Save state
            this.input = input;
            this.clock = clock;
            this.camera = camera;
            this.frameBuffer = new RenderTexture((int)camera.pixelRect.width, (int)camera.pixelRect.height, 24, RenderTextureFormat.ARGB32);
            this.material = new Material(Shader.Find(@"Hidden/NCPX/BlitCopy"));
            this.commandBuffer = new CommandBuffer();
            // Setup camera
            camera.forceIntoRenderTexture = true;
            var tempRT = Shader.PropertyToID("_MainTex");
            commandBuffer.GetTemporaryRT(tempRT, frameBuffer.descriptor, FilterMode.Bilinear);
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, tempRT);
            commandBuffer.Blit(tempRT, frameBuffer, material);
            camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            // Start recording
            if (RenderPipelineManager.currentPipeline != null)
                RenderPipelineManager.endCameraRendering += OnEndRender;
            else
                Camera.onPostRender += OnPostRender;
        }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {
            // Stop recording
            camera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            if (RenderPipelineManager.currentPipeline != null)
                RenderPipelineManager.endCameraRendering -= OnEndRender;
            else
                Camera.onPostRender -= OnPostRender;
            // Release
            Material.Destroy(material);
            input.Dispose();
            frameBuffer.Release();
        }
        #endregion


        #region --Operations--

        private readonly ITextureInput input;
        private readonly IClock clock;
        private readonly Camera camera;
        private readonly RenderTexture frameBuffer;
        private readonly Material material;
        private readonly CommandBuffer commandBuffer;

        private void OnPostRender (Camera cam) {
            if (cam == camera)
                input.CommitFrame(frameBuffer, clock?.timestamp ?? 0L);
        }

        private void OnEndRender (ScriptableRenderContext context, Camera cam) => OnPostRender(cam);

        private static ITextureInput CreateInput (IMediaRecorder recorder) {
            if (SystemInfo.supportsAsyncGPUReadback)
                return new AsyncTextureInput(recorder);
            return new TextureInput(recorder);
        }
        #endregion
    }
}