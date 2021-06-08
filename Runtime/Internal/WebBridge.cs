/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Internal {

    using System;
    using System.Runtime.InteropServices;

    public static class WebBridge {

        private const string Assembly = @"__Internal";

        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"NCCreateWEBMRecorder")]
        public static extern IntPtr CreateWEBMRecorder (
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitRate
        );
        #else
        public static IntPtr CreateWEBMRecorder (
            int width,
            int height,
            float frameRate,
            int sampleRate,
            int channelCount,
            int videoBitRate
        ) => IntPtr.Zero;
        #endif
    }
}