using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    public class OpusDecoderSafeHandle : SafeHandle
    {
        public OpusDecoderSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_decoder_destroy(handle);
            return true;
        }
    }
}
