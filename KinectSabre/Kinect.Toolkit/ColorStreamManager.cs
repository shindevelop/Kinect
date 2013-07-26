using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Kinect.Toolkit
{
    public class ColorStreamManager : Notifier
    {
        public BitmapSource ColorBitmap { get; private set; }

        public void Update(ColorImageFrameReadyEventArgs e)
        {
            ColorImageFrame Image = e.OpenColorImageFrame();

           // ColorBitmap = BitmapSource.Create(Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, byte, Image.Width * Image.BytesPerPixel);

            RaisePropertyChanged(()=>ColorBitmap);
        }
    }
}
