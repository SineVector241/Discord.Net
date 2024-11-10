using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    /// <summary>
    /// Managed wrapper over the OpusDecoder state.
    /// </summary>
    internal class OpusDecoderSafeHandle : SafeHandle
    {
        /// <summary>
        /// Create a new <see cref="OpusDecoderSafeHandle"/>.
        /// </summary>
        public OpusDecoderSafeHandle() : base(IntPtr.Zero, true)
        {
        }
        
        /// <inheritdoc/>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_decoder_destroy(handle);
            return true;
        }
    }
}
