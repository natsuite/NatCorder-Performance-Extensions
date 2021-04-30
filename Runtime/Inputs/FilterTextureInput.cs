/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Inputs {

    using System;
    using UnityEngine;
    using Filters;

    /// <summary>
    /// Recorder input for recording video frames from textures with image filters applied.
    /// </summary>
    public class FilterTextureInput : ITextureInput {

        #region --Client API--
        /// <summary>
        /// Create a filtered texture input.
        /// </summary>
        /// <param name="recorder">Media recorder to receive video frames.</param>
        /// <param name="filters">Filters to apply to frames before committing.</param>
        public FilterTextureInput (IMediaRecorder recorder, params ITextureFilter[] filters) : this(CreateDefaultInput(recorder), filters) { }

        /// <summary>
        /// Create a filtered texture input.
        /// </summary>
        /// <param name="input">Backing texture input for committing video frames to a recorder.</param>
        /// <param name="filters">Filters to apply to frames before committing.</param>
        public FilterTextureInput (ITextureInput input, params ITextureFilter[] filters) {
            // Check
            if (filters.Length == 0)
                throw new ArgumentException(@"FilterTextureInput requires one or more filters", nameof(filters));
            // Save
            this.input = input;
            this.filters = filters;
        }

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public void CommitFrame (Texture texture, long timestamp) {
            // Get current
            var current = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(texture, current);
            // Filter
            foreach (var filter in filters) {
                var next = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
                filter.FilterFrame(current, next);
                RenderTexture.ReleaseTemporary(current);
                current = next;
            }
            // Commit
            input.CommitFrame(current, timestamp);
            RenderTexture.ReleaseTemporary(current);
        }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {
            input.Dispose();
            foreach (var filter in filters)
                filter.Dispose();
        }
        #endregion


        #region --Operations--

        private readonly ITextureInput input;
        private readonly ITextureFilter[] filters;

        private static ITextureInput CreateDefaultInput (IMediaRecorder recorder) {
            if (SystemInfo.supportsAsyncGPUReadback) 
                return new AsyncTextureInput(recorder);
            else
                return new TextureInput(recorder);
        }
        #endregion
    }
}