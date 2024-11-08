using Discord.Audio.Opus.SafeHandlers;
using System.Runtime.InteropServices;
namespace Discord.Audio.Opus
{
    /// <summary>
    /// Native opus handler that directly calls the exported opus functions.
    /// </summary>
    internal static partial class NativeOpus
    {
#if ANDROID
        private const string DllName = "libopus.so";
#elif LINUX
        private const string DllName = "libopus.so.0.10.1";
#elif WINDOWS
        private const string DllName = "opus.dll";
#elif MACOS || IOS || MACCATALYST
        private const string DllName = "__Internal__";
#else
        private const string DllName = "opus";
#endif

        //Encoder
        [LibraryImport(DllName)]
        public static partial int opus_encoder_get_size(int channels);

        [LibraryImport(DllName)]
        public static unsafe partial OpusEncoderSafeHandle opus_encoder_create(int Fs, int application, int* error);

        [LibraryImport(DllName)]
        public static partial int opus_encoder_init(OpusEncoderSafeHandle st, int Fs, int channels, int application);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_encode(OpusEncoderSafeHandle st, short* pcm, int frame_size, byte* data, int max_data_bytes);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_encode_float(OpusEncoderSafeHandle st, float* pcm, int frame_size, byte* data, int max_data_bytes);

        [LibraryImport(DllName)]
        public static partial void opus_encoder_destroy(IntPtr st);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_encoder_ctl(OpusEncoderSafeHandle st, int request, void* data);

        //Decoder
        [LibraryImport(DllName)]
        public static partial int opus_decoder_get_size(int channels);

        [LibraryImport(DllName)]
        public static unsafe partial OpusDecoderSafeHandle opus_decoder_create(int Fs, int channels, int* error);

        [LibraryImport(DllName)]
        public static partial int opus_decoder_init(OpusDecoderSafeHandle st, int Fs, int channels);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_decode(OpusDecoderSafeHandle st, byte* data, int len, short* pcm, int frame_size, int decode_fec);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_decode_float(OpusDecoderSafeHandle st, byte* data, int len, float* pcm, int frame_size, int decode_fec);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_decoder_ctl(OpusDecoderSafeHandle st, int request, void* data);

        [LibraryImport(DllName)]
        public static partial void opus_decoder_destroy(IntPtr st);

        //Dred Decoder
        [LibraryImport(DllName)]
        public static partial int opus_dred_decoder_get_size();

        [LibraryImport(DllName)]
        public static unsafe partial OpusDREDDecoderSafeHandle opus_dred_decoder_create(int* error);

        [LibraryImport(DllName)]
        public static partial int opus_dred_decoder_init(OpusDREDDecoderSafeHandle dec);

        [LibraryImport(DllName)]
        public static partial void opus_dred_decoder_destroy(IntPtr dec);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_dred_decoder_ctl(OpusDREDDecoderSafeHandle dred_dec, int request, void* data);

        //Dred Packet?
        [LibraryImport(DllName)]
        public static partial int opus_dred_get_size();

        [LibraryImport(DllName)]
        public static unsafe partial OpusDREDSafeHandle opus_dred_alloc(int* error);

        [LibraryImport(DllName)]
        public static partial void opus_dred_free(IntPtr dec); //I'M JUST FOLLOWING THE DOCS!

        [LibraryImport(DllName)]
        public static unsafe partial int opus_dred_parse(OpusDREDDecoderSafeHandle dred_dec, OpusDREDSafeHandle dred, byte* data, int len, int max_dred_samples, int sampling_rate, int* dred_end, int defer_processing);

        [LibraryImport(DllName)]
        public static partial int opus_dred_process(OpusDREDDecoderSafeHandle dred_dec, OpusDREDSafeHandle src, OpusDREDSafeHandle dst);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_decoder_dred_decode(OpusDecoderSafeHandle st, OpusDREDSafeHandle dred, int dred_offset, int* pcm, int frame_size);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_decoder_dred_decode_float(OpusDecoderSafeHandle st, OpusDREDSafeHandle dred, int dred_offset, float* pcm, int frame_size);

        //Opus Packet Parsers
        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_parse(byte* data, int len, byte* out_toc, byte* frames, short* size, int* payload_offset);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_get_bandwidth(byte* data);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_get_samples_per_frame(byte* data, int Fs);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_get_nb_channels(byte* data);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_get_nb_frames(byte* packet, int len);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_get_nb_samples(byte* packet, int len, int Fs);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_has_lbrr(byte* packet, int len);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_get_nb_samples(OpusDecoderSafeHandle dec, byte* packet, int len);

        [LibraryImport(DllName)]
        public static unsafe partial void opus_pcm_soft_clip(float* pcm, int frame_size, int channels, float* softclip_mem);

        //Repacketizer
        [LibraryImport(DllName)]
        public static partial int opus_repacketizer_get_size();

        [LibraryImport(DllName)]
        public static partial OpusRepacketizerSafeHandle opus_repacketizer_init(OpusRepacketizerSafeHandle rp);

        [LibraryImport(DllName)]
        public static partial OpusRepacketizerSafeHandle opus_repacketizer_create();

        [LibraryImport(DllName)]
        public static partial void opus_repacketizer_destroy(IntPtr rp);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_repacketizer_cat(OpusRepacketizerSafeHandle rp, byte* data, int len);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_repacketizer_out_range(OpusRepacketizerSafeHandle rp, int begin, int end, byte* data, int maxlen);

        [LibraryImport(DllName)]
        public static partial int opus_repacketizer_get_nb_frames(OpusRepacketizerSafeHandle rp);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_repacketizer_out(OpusRepacketizerSafeHandle rp, byte* data, int maxlen);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_pad(byte* data, int len, int new_len);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_packet_unpad(byte* data, int len);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_packet_pad(byte* data, int len, int new_len, int nb_streams);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_packet_unpad(byte* data, int len, int nb_streams);

        //Multistream Encoder
        [LibraryImport(DllName)]
        public static partial int opus_multistream_encoder_get_size(int streams, int coupled_streams);

        [LibraryImport(DllName)]
        public static partial int opus_multistream_surround_encoder_get_size(int channels, int mapping_family);

        [LibraryImport(DllName)]
        public static unsafe partial OpusMSEncoderSafeHandle opus_multistream_encoder_create(int Fs, int channels, int streams, int coupled_streams, byte* mapping, int application, int* error);

        [LibraryImport(DllName)]
        public static unsafe partial OpusMSEncoderSafeHandle opus_multistream_surround_encoder_create(int Fs, int channels, int mapping_family, int* streams, int* coupled_streams, byte* mapping, int application, int* error);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_encoder_init(OpusMSEncoderSafeHandle st, int Fs, int channels, int streams, int coupled_streams, byte* mapping, int application);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_surround_encoder_init(OpusMSEncoderSafeHandle st, int Fs, int channels, int mapping_family, int* streams, int* coupled_streams, byte* mapping, int application);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_encode(OpusMSEncoderSafeHandle st, byte* pcm, int frame_size, byte* data, int max_data_bytes);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_encode_float(OpusMSEncoderSafeHandle st, float* pcm, int frame_size, byte* data, int max_data_bytes);

        [LibraryImport(DllName)]
        public static partial void opus_multistream_encoder_destroy(IntPtr st);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_encoder_ctl(OpusMSEncoderSafeHandle st, int request, void* data);

        //Multistream Decoder
        [LibraryImport(DllName)]
        public static partial int opus_multistream_decoder_get_size(int streams, int coupled_streams);

        [LibraryImport(DllName)]
        public static unsafe partial OpusMSDecoderSafeHandle opus_multistream_decoder_create(int Fs, int channels, int streams, int coupled_streams, byte* mapping, int* error);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_decoder_init(OpusMSDecoderSafeHandle st, int Fs, int channels, int streams, int coupled_streams, byte* mapping);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_decode(OpusMSDecoderSafeHandle st, byte* data, int len, short* pcm, int frame_size, int decode_fec);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_decode_float(OpusMSDecoderSafeHandle st, byte* data, int len, float* pcm, int frame_size, int decode_fec);

        [LibraryImport(DllName)]
        public static unsafe partial int opus_multistream_decoder_ctl(OpusMSDecoderSafeHandle st, int request, void* data);

        [LibraryImport(DllName)]
        public static partial void opus_multistream_decoder_destroy(IntPtr st);

        //Library Information
        [LibraryImport(DllName)]
        public static unsafe partial byte* opus_strerror(int error);

        [LibraryImport(DllName)]
        public static unsafe partial byte* opus_get_version_string();
    }
}