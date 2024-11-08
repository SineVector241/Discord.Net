using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    public class OpusMSDecoderSafeHandle : SafeHandle
    {
        public OpusMSDecoderSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_multistream_decoder_destroy(handle);
            return true;
        }
    }
}
