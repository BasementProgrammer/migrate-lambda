using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.S3Events;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Function
{
    public class ImageProcessor
    {
        public MemoryStream ProcessImage(MemoryStream imageStream, int width, int height)
        {
            using (var image = Image.Load(imageStream.ToArray()))
            {
                image.Mutate(x => x.Resize(image.Width / 10, image.Height / 10));
                // Save the thumbnail image to the same bucket
                var outMemoryStream = new MemoryStream();
                image.Save(outMemoryStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
                return outMemoryStream;
            }
        }
    }
}
