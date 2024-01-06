using System.Collections.Generic;
using AForge.Video.FFMPEG;

namespace Fractal3d;

// https://stackoverflow.com/questions/9744026/image-sequence-to-video-stream
public static class MovieUtil
{
    public static void CreateMovie(string filename, int width, int height, int frameRate, List<System.Drawing.Bitmap> images)
    {
        using (var vfWriter = new VideoFileWriter())
        {
            vfWriter.Open(filename, width, height, frameRate, VideoCodec.Raw);
            foreach (var image in images)
            {
                vfWriter.WriteVideoFrame(image);
            }

            vfWriter.Close();
        }
    }
}