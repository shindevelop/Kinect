using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Microsoft.Kinect;

namespace KinectEarthMove
{
    public partial class MainWindow : System.Windows.Window
    {
        private KinectSensor nui;
        private DispatcherTimer timer = new DispatcherTimer();
        private EarthTransform earthTransform = new EarthTransform();
        private byte[] colorPixels;
        private Skeleton[] skeletons = new Skeleton[0];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //タイマーセット
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 30);
            timer.Start();

            //Kinectの初期化
            foreach (var obj in KinectSensor.KinectSensors)
            {
                if (obj.Status == KinectStatus.Connected)
                {
                    this.nui = obj;
                    break;
                }
            }

            var colorParam = ColorImageFormat.RgbResolution640x480Fps30;
            var smoothParam = new TransformSmoothParameters
            {
                Smoothing = 0.75f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };

            //各ストリーム有効化
            this.nui.ColorStream.Enable(colorParam);
            this.nui.SkeletonStream.Enable(smoothParam);

            //イベントの登録
            this.nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            this.nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_ColorFrameReady);

            this.nui.Start();
        }

        //RGB画像更新イベント
        void nui_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imgFame = e.OpenColorImageFrame())
            {
                if (imgFame != null)
                {
                    colorPixels = new byte[imgFame.PixelDataLength];
                    imgFame.CopyPixelDataTo(colorPixels);

                    //取得データの描画
                    video.Source = BitmapSource.Create(
                        imgFame.Width, 
                        imgFame.Height, 
                        96, 
                        96, 
                        PixelFormats.Bgr32, 
                        null, 
                        colorPixels, 
                        imgFame.Width * imgFame.BytesPerPixel
                    );
                }
            }
        }

        //スケルトン更新イベント
        private void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    if (this.skeletons.Length != frame.SkeletonArrayLength)
                    {
                        this.skeletons = new Skeleton[frame.SkeletonArrayLength];
                    }

                    frame.CopySkeletonDataTo(this.skeletons);
                }
            }

            //肩・手
            Vector3D shoulderC = new Vector3D();
            Vector3D handR = new Vector3D();
            Vector3D handL = new Vector3D();
            Vector3D shoulderR = new Vector3D();
            Vector3D shoulderL = new Vector3D();

            // find positions of shoulders and hands
            foreach (var data in this.skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    JointCollection j = data.Joints;
                    shoulderC = ToVector3(j[JointType.ShoulderCenter].Position);
                    handL = ToVector3(j[JointType.HandLeft].Position);
                    handR = ToVector3(j[JointType.HandRight].Position);
                    shoulderL = ToVector3(j[JointType.ShoulderLeft].Position);
                    shoulderR = ToVector3(j[JointType.ShoulderRight].Position);
                    break;
                }
            }

            //両手の中心
            Vector3D pos = new Vector3D((handR.X + handL.X) / 2.0, (handR.Y + handL.Y) / 2.0, (handR.Z + handL.Z) / 2.0);
            // move to the center of both hand
            earthTransform.Translate = new Vector3D(pos.X * tfactor, pos.Y * tfactor, pos.Z);
            // scale
            // find the vector from left hand to right hand
            Vector3D hand = new Vector3D(handR.X - handL.X, handR.Y - handL.Y, handR.Z - handL.Z);
            // find the vector from left shoulder to right shoulder
            Vector3D shoulder = new Vector3D(shoulderR.X - shoulderL.X, shoulderR.Y - shoulderL.Y, shoulderR.Z - shoulderL.Z);
            // scale the earth from the difference of lengths(squared) of inter-shoulders and inter-hands
            // if same length scale to 0.8. longer inter-hand , bigger scale
            earthTransform.Scale = hand.LengthSquared - shoulder.LengthSquared + 0.8;
            // rotataion
            // get the angle and axis of inter-hands vector to rotate the earth
            hand.Normalize();
            earthTransform.Angle = Vector3D.AngleBetween(new Vector3D(1, 0, 0), hand);
            earthTransform.Axis = Vector3D.CrossProduct(new Vector3D(1, 0, 0), hand);
        }

        // Timer Callback to display update
        private void timer_Tick(object sender, EventArgs e)
        {
            //球の自転
            earthTransform.SelfRotation += 1.0;
            myEarthGeometry.Transform = earthTransform.GetTransform3D();
        }

        private readonly double tfactor = 5.0;

        public static Vector3D ToVector3(SkeletonPoint vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //this.nui.Dispose();
            //Environment.Exit(0);
        }
    }

    /// <summary>
    /// Trasnformation Helper Class
    /// </summary>
    public class EarthTransform
    {
        private ScaleTransform3D _scale;
        private RotateTransform3D _rotate;
        private RotateTransform3D _self;
        private double _selfAngle;
        private Vector3D _axis;
        private double _angle;
        private TranslateTransform3D _translate;

        public double Scale
        {
            get
            {
                return _scale.ScaleX;
            }
            set
            {
                _scale = new ScaleTransform3D(value, value, value);
            }
        }

        public double Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                _angle = value;
                _rotate = new RotateTransform3D(new AxisAngleRotation3D(_axis, value));
            }
        }

        public Vector3D Axis
        {
            get
            {
                return _axis;
            }
            set
            {
                _axis = value;
                _rotate = new RotateTransform3D(new AxisAngleRotation3D(value, _angle ));
            }
        }
        public Vector3D Translate
        {
            get
            {
                return new Vector3D(_translate.OffsetX, _translate.OffsetY, _translate.OffsetZ);
            }
            set
            {
                _translate = new TranslateTransform3D(value);
            }
        }

        public double SelfRotation 
        {
            get
            {
                return _selfAngle;
            }
            set
            {
                _selfAngle = value;
                _self = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0,1,0), value));
            }
        }

        public EarthTransform()
        {
            this.Scale = 1.0;
            this.Angle = 0.0;
            this.Axis = new Vector3D(0.0, 1.0, 0.0);
            this.Translate = new Vector3D(0.0, 0.0, 0.0);
            this.SelfRotation = 0.0;
        }

        public Transform3D GetTransform3D()
        {
            Transform3DGroup t3dg = new Transform3DGroup();
            t3dg.Children.Add(_scale);
            t3dg.Children.Add(_self);
            t3dg.Children.Add(_rotate);
            t3dg.Children.Add(_translate);
            return t3dg;
        }

    }
}
