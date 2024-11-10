using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    /// <summary>
    /// Managed wrapper over the OpusDREDDecoder state.
    /// </summary>
    internal class OpusDREDDecoderSafeHandle : SafeHandle
    {
        /// <summary>
        /// Creates a new <see cref="OpusDecoderSafeHandle"/>.
        /// </summary>
        public OpusDREDDecoderSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        /// <inheritdoc/>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_dred_decoder_destroy(handle);
            return true;
        }
    }
}
