/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Filters {

    using System;
    using UnityEngine;

    /// <summary>
    /// Filter for applying image effects to textures for recording.
    /// </summary>
    public interface ITextureFilter : IDisposable {
        
        /// <summary>
        /// Filter a frame.
        /// </summary>
        /// <param name="source">Source texture.</param>
        /// <param name="destination">Destination texture.</param>
        void FilterFrame (Texture source, RenderTexture destination);
    }
}