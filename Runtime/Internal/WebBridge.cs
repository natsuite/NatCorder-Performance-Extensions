/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Internal {

    using System;
    using System.Runtime.InteropServices;

    public static class WebBridge {

        private const string Assembly = @"__Internal";

        [DllImport(Assembly, EntryPoint = @"NCCreateWEBMRecorder")]
        public static extern IntPtr CreateWEBMRecorder (int width, int height, float frameRate, int sampleRate, int channelCount, int bitrate);
    }
}