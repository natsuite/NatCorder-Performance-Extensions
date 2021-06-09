/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

#if UNITY_WEBGL && !UNITY_EDITOR
    #define BRIDGE
#endif

namespace NatSuite.Recorders.Internal {

    using System;
    using System.Runtime.InteropServices;

    public static class WebBridge {

        private const string Assembly = @"__Internal";

        #if BRIDGE
        [DllImport(Assembly, EntryPoint = @"NCCreateWEBMRecorder")]
        public static extern void CreateWEBMRecorder (
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitRate,
            int audioBitRate,
            out IntPtr recorder
        );

        [DllImport(Assembly, EntryPoint = @"NCDownloadBlob")]
        public static extern void DownloadBlob (
            [MarshalAs(UnmanagedType.LPStr)] string url,
            [MarshalAs(UnmanagedType.LPStr)] string fileName
        );
        #else
        public static void CreateWEBMRecorder (
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitRate,
            int audioBitRate,
            out IntPtr recorder
        ) => recorder = default;

        public static void DownloadBlob (string url, string fileName) { }
        #endif
    }
}