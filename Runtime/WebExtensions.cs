/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders {

    using Internal;

    /// <summary>
    /// Useful extensions for WebGL.
    /// </summary>
    public static class WebExtensions {

        #region --Client API--
        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        public static void DownloadBlob (string url, string fileName = @"recording.webm") { // DEPLOY
            WebBridge.DownloadBlob(url, fileName);
        }
        #endregion
    }
}