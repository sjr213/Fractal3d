namespace Fractal3d;

using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using FFMediaToolkit;

// https://stackoverflow.com/questions/9744026/image-sequence-to-video-stream
public static class MovieUtil
{
    public static void CreateMovieMp4(string filename, int width, int height, int frameRate, List<Bitmap> images)
    {
        FFmpegLoader.FFmpegPath = @"C:\ffmpeg\bin\";
        var settings = new VideoEncoderSettings(width, height, frameRate, codec: VideoCodec.H264)
            {
                EncoderPreset = EncoderPreset.Medium,
                CRF = 17
            };
        var file = MediaBuilder.CreateContainer(filename).WithVideo(settings).Create();
        foreach (var bitmap in images)
        {
            var rect = new Rectangle(Point.Empty, bitmap.Size);
            var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);
            file.Video.AddFrame(bitmapData); // Encode the frame
            bitmap.UnlockBits(bitLock);
        }

        file.Dispose();
    }
}