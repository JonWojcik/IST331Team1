using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Windows.Threading;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Media.Media3D;

namespace XboxController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer _timer = new DispatcherTimer();
        KinectSensor _sensor;
        static Gestures _gesture = new Gestures();

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(0.5) };
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
             _sensor = KinectSensor.KinectSensors.Where(
                s => s.Status == KinectStatus.Connected).FirstOrDefault();
             foreach (KinectSensor sensor in KinectSensor.KinectSensors)
             {
                 if (_sensor != null)
                 {
                     _sensor.ColorStream.Enable();
                     _sensor.DepthStream.Enable();
                     _sensor.SkeletonStream.Enable();
                     _sensor.AllFramesReady += _sensor_AllFramesReady;
                     try
                     {
                         this._sensor.Start();
                     }
                     catch (InvalidOperationException)
                     {
                         this._sensor = null;
                     }
                 }
             }
        }

        //Skeleton Tracking

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                    if (skeletons.Length > 0)
                    {
                        var user = skeletons.Where(u => u.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                        if (user != null)
                        {
                            Console.WriteLine("User Found!");
                            //Debugging information 
                            JointCollection jointCollection = user.Joints; //Debugging information 
                            Console.WriteLine(jointCollection[JointType.ElbowRight].TrackingState.ToString()); //Debugging 
                            Console.WriteLine(jointCollection[JointType.HandRight].TrackingState.ToString()); //Debugging 
                            Canvas.SetLeft(ellipiseHandRight, jointCollection[JointType.HandRight].Position.X * 200); //Debugging 
                            Canvas.SetTop(ellipiseHandRight, jointCollection[JointType.HandRight].Position.Y * -200); //Debugging
                            LeftHandPosition.Text = "LeftHandPosition: x" + jointCollection[JointType.HandLeft].Position.X + "y" + jointCollection[JointType.HandLeft].Position.Y;
                            RightHandPosition.Text = "RightHandPosition: x" + jointCollection[JointType.HandRight].Position.X + "y" + jointCollection[JointType.HandRight].Position.Y;
                            HeadPosition.Text = "HeadPosition: x" + jointCollection[JointType.Head].Position.X + "y" + jointCollection[JointType.Head].Position.Y;
                            camMain.Position = new Point3D(jointCollection[JointType.Head].Position.X * -10, 10, jointCollection[JointType.Head].Position.Y * 20);
                            _gesture.handsAboveHead(user);
                        }
                    }
                }
            }
        } //Closing Sensor_SkeletonFrameReady method


        //Xbox controller stuff

        void _timer_Tick(object sender, EventArgs e)
        {
            DisplayControllerInformation();
        }

        void DisplayControllerInformation()
        {
            initalizeController();
            var GPstate = GamePad.GetState(PlayerIndex.One);
            if (GPstate.IsButtonDown(Buttons.RightTrigger))
                triggerPull();

        }

        private void initalizeController()
        {
            Brush defaultColor = new SolidColorBrush(Colors.LightGray);

            GamePad.SetVibration(PlayerIndex.One, 0, 0);
        }

        void triggerPull()
        {
            MessageBox.Show("trigger");
        }

        public void pauseMenuVisibility(object sender, EventArgs e)
        {
            PauseMenu.Visibility = Visibility.Visible;
        }

        public void calibrateTargeting()
        {
            calibrationScreen.Visibility = Visibility.Visible;
        }

    }
}
