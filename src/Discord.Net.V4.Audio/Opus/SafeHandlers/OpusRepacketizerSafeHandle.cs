using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    /// <summary>
    /// Managed wrapper over the OpusRepacketizer state.
    /// </summary>
    internal class OpusRepacketizerSafeHandle : SafeHandle
    {
        /// <summary>
        /// Creates a new <see cref="OpusRepacketizerSafeHandle"/>.
        /// </summary>
        public OpusRepacketizerSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        /// <inheritdoc/>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_repacketizer_destroy(handle);
            return true;
        }
    }
}
