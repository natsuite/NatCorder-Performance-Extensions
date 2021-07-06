/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Inputs {

    using UnityEngine;
    using static Unity.Mathematics.math;

    /// <summary>
    /// Recorder input for recording video frames from textures with cropping.
    /// </summary>
    public sealed class CropTextureInput : ITextureInput { // INCOMPLETE

        #region --Client API--
        /// <summary>
        /// Crop rect in pixel coordinates of the recorder.
        /// </summary>
        public RectInt rect;

        /// <summary>
        /// Crop aspect mode.
        /// </summary>
        public AspectMode aspectMode;

        /// <summary>
        /// Create a crop texture input.
        /// </summary>
        /// <param name="input">Backing texture input to receive cropped frames.</param>
        public CropTextureInput (ITextureInput input) {
            this.input = input;
            this.material = new Material(Shader.Find(@"Hidden/NCPX/CropFilter"));
            this.frameSizeInv = new Vector2(1f / input.frameSize.width, 1f/ input.frameSize.height);
            this.rect = new RectInt(0, 0, input.frameSize.width, input.frameSize.height);
            this.aspectMode = 0;
        }

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public void CommitFrame (Texture texture, long timestamp) {
            var result = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            CommitFrame(texture, result);
            input.CommitFrame(result, timestamp);
            RenderTexture.ReleaseTemporary(result);
        }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {
            input.Dispose();
            Material.Destroy(material);
        }
        #endregion


        #region --Operations--

        private readonly ITextureInput input;
        private readonly Material material;
        private readonly Vector2 frameSizeInv;

        (int, int) ITextureInput.frameSize => input.frameSize;

        private void CommitFrame (Texture source, RenderTexture destination) {
            var (width, height) = input.frameSize;
            var cropOffs = float2(rect.xMin, rect.yMin) / float2(width, height);
            var cropSize = float2(rect.width, rect.height) / float2(width, height);
            var A = float3x3(
                1f, 0f, 1f,
                0f, 1f, 1f,
                1f, 1f, 1f
            );
            var B = float3x3(
                cropOffs.x + cropSize.x, cropOffs.x, cropOffs.x + cropSize.x,
                cropOffs.y, cropOffs.y + cropSize.y, cropOffs.y + cropSize.y,
                1f, 1f, 1f
            );
            var T = mul(B, inverse(A));
            Debug.Log(T);

            /*
            var scale = float2(cropRect.width, cropRect.height) / float2(width, height);
            var scaleFactor = aspectMode == ApsectMode.Fit ? cmin(scale) : cmax(scale);
            */



            /*
            // Offset
            var offset = Matrix4x4.Translate(Vector2.Scale(-cropRect.position, frameSizeInv));
            // Scale

            // Transform
            var forward = offset;
            var inverse = Matrix4x4.identity;

            Debug.Log(forward);

            if (!Matrix4x4.Inverse3DAffine(forward, ref inverse))
                return;
            // Blit
            material.SetMatrix("_Transform", inverse);
            Graphics.Blit(source, destination, material);
            */
        }
        #endregion
    }
}