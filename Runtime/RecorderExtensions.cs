/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders {

    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Useful extensions on recorders.
    /// </summary>
    public static class RecorderExtensions {

        #region --Client API--
        /// <summary>
        /// Finish writing.
        /// </summary>
        /// <param name="recordingName">Desired recording name. This MUST include the file extension.</param>
        /// <param name="overwrite">Should any existing file be overwritten?</param>
        /// <returns>Path to recorded media file.</returns>
        public static async Task<string> FinishWriting (this IMediaRecorder recorder, string recordingName, bool overwrite = false) {
            // Get source and destination paths
            var src = await recorder.FinishWriting();
            var dst = Path.Combine(new FileInfo(src).Directory.FullName, recordingName);
            var directory = File.GetAttributes(src).HasFlag(FileAttributes.Directory); // src and dst are same type
            var exists = File.Exists(dst) || Directory.Exists(dst);
            // Delete existing file
            if (exists && overwrite)
                if (directory)
                    Directory.Delete(dst, true);
                else
                    File.Delete(dst);
            // Move
            try {
                if (directory)
                    Directory.Move(src, dst);
                else
                    File.Move(src, dst);
            }
            // Delete source
            catch (IOException ex) {
                if (directory)
                    Directory.Delete(src, true);
                else
                    File.Delete(src);
                throw ex;
            }
            // Return new path
            return dst;
        }

        /// <summary>
        /// Convert the recorder to an asynchronous recorder which commits frames on a worker thread.
        /// </summary>
        /// <returns>Async recorder which wraps this recorder.</returns>
        public static AsyncRecorder ToAsync (this IMediaRecorder recorder) => new AsyncRecorder(recorder);
        #endregion
    }
}