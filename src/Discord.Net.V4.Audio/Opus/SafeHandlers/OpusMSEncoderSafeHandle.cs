using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    public class OpusMSEncoderSafeHandle : SafeHandle
    {
        public OpusMSEncoderSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_multistream_encoder_destroy(handle);
            return true;
        }
    }
}
