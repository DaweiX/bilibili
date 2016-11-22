using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace bilibili.Methods
{
    public class Guessfuzzy
    {
        WriteableBitmap wb;
        public async void Init()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///a.jpg"));
            var pro = await file.Properties.GetImagePropertiesAsync();
            wb = new WriteableBitmap((int)pro.Width, (int)pro.Height);
            //img.source
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    try
                    {
                        await wb.SetSourceAsync(fileStream);
                    }
                    catch
                    { }
                }
            }
            ApplyFilter(1);
        }

        public async void ApplyFilter(int level)
        {
            WriteableBitmap bmp = await Utility.BitmapClone(wb);
            //myimage
        }
        class Utility
        {
            /// <summary>
            /// WriteableBitmap 的拷贝
            /// </summary>
            /// <param name="bitmap">原</param>
            /// <returns></returns>
            public static async Task<WriteableBitmap> BitmapClone(WriteableBitmap bitmap)
            {
                WriteableBitmap result = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);

                byte[] sourcePixels = Get_WriteableBitmap_bytes(bitmap);

                //byte[] resultPixels = new byte[sourcePixels.Length];

                //sourcePixels.CopyTo(resultPixels, 0);

                using (Stream resultStream = result.PixelBuffer.AsStream())
                {
                    await resultStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                }

                return result;
            }

            public static byte[] Get_WriteableBitmap_bytes(WriteableBitmap bitmap)
            {
                // 获取对直接缓冲区的访问，WriteableBitmap 的每个像素都写入直接缓冲区。
                IBuffer bitmapBuffer = bitmap.PixelBuffer;

                //byte[] sourcePixels = new byte[bitmapBuffer.Length];
                //Windows.Security.Cryptography.CryptographicBuffer.CopyToByteArray(bitmapBuffer, out sourcePixels);
                //bitmapBuffer.CopyTo(sourcePixels);

                using (var dataReader = DataReader.FromBuffer(bitmapBuffer))
                {
                    var bytes = new byte[bitmapBuffer.Capacity];
                    dataReader.ReadBytes(bytes);

                    return bytes;
                }
            }
        }
    }
}