using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Musixx.Extensions
{
    public static class ByteExtension
    {
        public async static Task<BitmapImage> ToBitmapImage(this byte[] bytes)
        {
            var bitmap = new BitmapImage();
            bitmap.DecodePixelType = DecodePixelType.Logical;
            bitmap.DecodePixelHeight = bitmap.DecodePixelWidth = 70;
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(bytes.AsBuffer());
                stream.Seek(0);
                await bitmap.SetSourceAsync(stream);
            }
            return bitmap;
        }
    }
}
