using System;
using System.Windows;
using Kinect.Toolkit;
using KinectSabre.Render;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;

namespace KinectSabre
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        RenderGame game;
        KinectSensor kinect;
        private Skeleton[] SkeletonBuffer = null;
        private byte[] colorPixels;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                kinect = KinectSensor.KinectSensors[0];
                kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinect.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
                kinect.SkeletonStream.Enable(new TransformSmoothParameters()
                {
                    Correction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f,
                    Smoothing = 0.5f
                });

                kinect.SkeletonFrameReady += kinect_SkeletonFrameReady;
                kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinect_ColorFrameReady);

                colorPixels = new byte[kinect.ColorStream.FramePixelDataLength];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                kinect = null;
            }

            kinect.Start();

            using (game = new RenderGame())
            {
                game.Exiting += game_Exiting;
                game.Run();
            }
            if (kinect != null)
            {
                kinect.Dispose();
            }
        }

        void kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using ( ColorImageFrame clrFrame = e.OpenColorImageFrame() )
            {
                if (clrFrame != null)
                {
                    clrFrame.CopyPixelDataTo(colorPixels);
                    game.UpdateColorTexture(colorPixels);
                }
            }
        }

        void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool player1 = true;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {

                    SkeletonBuffer = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(SkeletonBuffer);

                    foreach (Skeleton data in SkeletonBuffer)
                    {
                        if (data.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            foreach (Joint joint in data.Joints)
                            {
                                //if (joint.Position.X < 0.8f)
                                //    continue;
                                switch (joint.JointType)
                                {
                                    case JointType.HandLeft:
                                        if (player1)
                                            game.P1LeftHandPosition = Position(data, JointType.HandLeft);
                                        else
                                            game.P2LeftHandPosition = Position(data, JointType.HandLeft);
                                        break;
                                    case JointType.HandRight:
                                        if (player1)
                                            game.P1RightHandPosition = Position(data, JointType.HandRight);
                                        else
                                            game.P2RightHandPosition = Position(data, JointType.HandRight);
                                        break;
                                    case JointType.WristLeft:
                                        if (player1)
                                            game.P1LeftWristPosition = Position(data, JointType.WristLeft);
                                        else
                                            game.P2LeftWristPosition = Position(data, JointType.WristLeft);
                                        break;
                                    case JointType.ElbowLeft:
                                        if (player1)
                                            game.P1LeftElbowPosition = Position(data, JointType.ElbowLeft);
                                        else
                                            game.P2LeftElbowPosition = Position(data, JointType.ElbowLeft);
                                        break;
                                }
                            }

                            if (player1)
                            {
                                player1 = false;
                                game.P1IsActive = true;
                            }
                            else
                            {
                                game.P2IsActive = true;
                                return;
                            }
                        }
                    }

                    if (player1)
                        game.P1IsActive = false;

                    game.P2IsActive = false;
                }
            }
        }

        void game_Exiting(object sender, EventArgs e)
        {
            Close();
        }

        public static Vector3 Position(Skeleton skeleton, JointType jt)
        {
            if (skeleton == null)
            {
                return Vector3.Zero;
            }
            else
            {
                return SkeletonPointToVector3(skeleton.Joints[jt].Position);
            }
        }

        public static Vector3 SkeletonPointToVector3(SkeletonPoint pt)
        {
            return new Vector3(pt.X, pt.Y, pt.Z);
        }

    }
}
