namespace TorchSharpYoloV3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Drawing;

    internal class ImageHelper
    {
        /// <summary>
        /// load bytes from StorageFile object
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Task<byte[]></returns>
        public async Task<byte[]> LoadBytesAsync(string file)
        {
            return await File.ReadAllBytesAsync(file);
        }
        /// <summary>
        /// Decode Bytes[] to Bitmap
        /// </summary>
        /// <param name="array"></param>
        /// <returns>Task<Bitmap></returns>
        public async Task<Bitmap> DecodeImageBytesBitmap(byte[] array)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(array)) {
                bmp = new Bitmap(ms);
            }
            return await Task.FromResult(bmp);        
        }
        public Bitmap ResizeBitmap(Bitmap image, int width, int height)
        {
            Bitmap result = new Bitmap(width,height);
            using (Graphics g = Graphics.FromImage(result)) { 
                g.DrawImage(image,0,0,width,height);
            }
            return result;
        }




    }
}
