using System.Runtime.InteropServices;

namespace Discord.Audio.Opus.SafeHandlers
{
    public class OpusRepacketizerSafeHandle : SafeHandle
    {
        public OpusRepacketizerSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            NativeOpus.opus_repacketizer_destroy(handle);
            return true;
        }
    }
}
