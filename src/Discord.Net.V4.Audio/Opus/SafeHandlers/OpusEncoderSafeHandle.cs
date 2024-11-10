using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    /// <summary>
    /// Managed wrapper over the OpusEncoder state.
    /// </summary>
    internal class OpusEncoderSafeHandle : SafeHandle
    {
        /// <summary>
        /// Creates a new <see cref="OpusEncoderSafeHandle"/>.
        /// </summary>
        public OpusEncoderSafeHandle(): base(IntPtr.Zero, true)
        {
        }

        /// <inheritdoc/>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_encoder_destroy(handle);
            return true;
        }
    }
}
