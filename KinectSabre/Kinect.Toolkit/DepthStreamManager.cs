using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Kinect.Toolkit
{
    public class DepthStreamManager : Notifier
    {
        byte[] depthFrame32;

        public WriteableBitmap DepthBitmap { get; private set; }

        public int XScanBox { get; private set; }
        public int YScanBox { get; private set; }

        public int HeightScanBox { get; private set; }
        public int WidthScanBox { get; private set; }
        public bool UserDetected { get; private set; }
        private short[] pixelData;
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;
        private static readonly int[] IntensityShiftByPlayerR = { 1, 2, 0, 2, 0, 0, 2, 0 };
        private static readonly int[] IntensityShiftByPlayerG = { 1, 2, 2, 0, 2, 0, 0, 1 };
        private static readonly int[] IntensityShiftByPlayerB = { 1, 0, 2, 2, 0, 2, 0, 2 };

        public void Update(object sender ,DepthImageFrameReadyEventArgs e)
        {
            
            if (depthFrame32 == null)
            {
                depthFrame32 = new byte[e.OpenDepthImageFrame().Width * e.OpenDepthImageFrame().Height * 4];
            }

            this.pixelData = new short[e.OpenDepthImageFrame().PixelDataLength];
            ConvertDepthFrame(this.pixelData, ((KinectSensor)sender).DepthStream);
            if (DepthBitmap == null)
            {
                DepthBitmap = new WriteableBitmap(e.OpenDepthImageFrame().Width, e.OpenDepthImageFrame().Height, 96, 96, PixelFormats.Bgra32, null);
            }

            DepthBitmap.Lock();

            int stride = DepthBitmap.PixelWidth * DepthBitmap.Format.BitsPerPixel / 8;
            Int32Rect dirtyRect = new Int32Rect(0, 0, DepthBitmap.PixelWidth, DepthBitmap.PixelHeight);
            DepthBitmap.WritePixels(dirtyRect, depthFrame32, stride, 0);

            DepthBitmap.AddDirtyRect(dirtyRect);
            DepthBitmap.Unlock();

            RaisePropertyChanged(()=>DepthBitmap);
        }

        private void ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream)
        {
            int tooNearDepth = depthStream.TooNearDepth;
            int tooFarDepth = depthStream.TooFarDepth;
            int unknownDepth = depthStream.UnknownDepth;

            // Test that the buffer lengths are appropriately correlated, which allows us to use only one
            // value as the loop condition.
            if ((depthFrame.Length * 4) != this.depthFrame32.Length)
            {
                //throw new InvalidOperationException();
            }

            for (int i16 = 0, i32 = 0; i32 < this.depthFrame32.Length; i16++, i32 += 4)
            {
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (player == 0 && realDepth == tooNearDepth)
                {
                    // white 
                    this.depthFrame32[i32 + RedIndex] = 255;
                    this.depthFrame32[i32 + GreenIndex] = 255;
                    this.depthFrame32[i32 + BlueIndex] = 255;
                }
                else if (player == 0 && realDepth == tooFarDepth)
                {
                    // dark purple
                    this.depthFrame32[i32 + RedIndex] = 66;
                    this.depthFrame32[i32 + GreenIndex] = 0;
                    this.depthFrame32[i32 + BlueIndex] = 66;
                }
                else if (player == 0 && realDepth == unknownDepth)
                {
                    // dark brown
                    this.depthFrame32[i32 + RedIndex] = 66;
                    this.depthFrame32[i32 + GreenIndex] = 66;
                    this.depthFrame32[i32 + BlueIndex] = 33;
                }
                else
                {
                    // transform 13-bit depth information into an 8-bit intensity appropriate
                    // for display (we disregard information in most significant bit)
                    byte intensity = (byte)(~(realDepth >> 4));

                    // tint the intensity by dividing by per-player values
                    this.depthFrame32[i32 + RedIndex] = (byte)(intensity >> IntensityShiftByPlayerR[player]);
                    this.depthFrame32[i32 + GreenIndex] = (byte)(intensity >> IntensityShiftByPlayerG[player]);
                    this.depthFrame32[i32 + BlueIndex] = (byte)(intensity >> IntensityShiftByPlayerB[player]);
                }
            }
        }
    }
}
