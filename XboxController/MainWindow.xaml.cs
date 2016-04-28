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
    /// //change
    public partial class MainWindow : Window
    {

        DispatcherTimer _timer = new DispatcherTimer();
        KinectSensor _sensor;
        public bool isPaused = false;
        public bool isCalibrated = false;
        public string targetName;
        Material hitColor = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));

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
                            gameView.Position = new Point3D(jointCollection[JointType.Head].Position.X * 5, jointCollection[JointType.Head].Position.Y * 15, 12.2);
                          
                            handsAboveHead(user);
                            
                            // Determines direction of right hand
                            if (jointCollection[JointType.HandRight].Position.X > -0.13 && jointCollection[JointType.HandLeft].Position.X < 0.2)
                            {
                                targetName = "Center";
                                handDirection.Text = "center";
                                
                            }
                            if (jointCollection[JointType.HandRight].Position.X < -0.13)
                            {
                                targetName = "Left";
                                handDirection.Text = "Left";
                            }
                            if (jointCollection[JointType.HandRight].Position.X > 0.2)
                            {
                                targetName = "Right";
                                handDirection.Text = "Right";
                                ArrowRightModel.Material = hitColor;         
                            }
                            
                        }
                    }
                }
            }
        }

        public void startGame()
        {
            StartScreen.Visibility = Visibility.Hidden;
        }
        //Closing Sensor_SkeletonFrameReady method

        //Gesture for pausing game if hands are above head
        public void handsAboveHead(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
            {
                if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
                {
                    pauseMenuVisibility();
                }
            }
        }

        //Xbox controller stuff

        void _timer_Tick(object sender, EventArgs e)
        {
            DisplayControllerInformation();
            MenuControls();
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
            if (targetName == "Center")
            {
                ArrowCenterModel.Material = hitColor;  
            }
            if (targetName == "Right")
            {
                ArrowRightModel.Material = hitColor;
            }
            if (targetName == "Left")
            {
                ArrowLeftModel.Material = hitColor;  
            }
        }


        //Xbox menu controls 
        void MenuControls()
        {
            initalizeController();
            var GPstate = GamePad.GetState(PlayerIndex.One);
            if (PauseMenu.IsVisible)
            {
                if (GPstate.IsButtonDown(Buttons.A))
                {
                    PauseMenu.Visibility = Visibility.Hidden;
                }
                else if (GPstate.IsButtonDown(Buttons.X))
                {
                    OptionsMenu.Visibility = Visibility.Visible;
                    PauseMenu.Visibility = Visibility.Hidden;
                }
                else if (GPstate.IsButtonDown(Buttons.B))
                {
                    Environment.Exit(1);
                }
                else if (GPstate.IsButtonDown(Buttons.Y))
                {
                    startGame(); 
                }
                else if (GPstate.IsButtonDown(Buttons.Start))
                {
                    OptionsMenu.Visibility = Visibility.Hidden;
                    PauseMenu.Visibility = Visibility.Hidden;
                }
            }
            if (StartScreen.IsVisible)
            {
                if (GPstate.IsButtonDown(Buttons.A))
                {
                    StartScreen.Visibility = Visibility.Hidden;
                    startGame();
                }
                else if (GPstate.IsButtonDown(Buttons.X))
                {
                    OptionsMenu.Visibility = Visibility.Visible;
                    StartScreen.Visibility = Visibility.Hidden;
                }
                else if (GPstate.IsButtonDown(Buttons.B))
                {
                    Environment.Exit(1);
                }
                else if (GPstate.IsButtonDown(Buttons.Y))
                {
                    OptionsMenu.Visibility = Visibility.Visible;
                }
                else if (GPstate.IsButtonDown(Buttons.Start))
                {
                    OptionsMenu.Visibility = Visibility.Hidden;
                    PauseMenu.Visibility = Visibility.Hidden;
                    StartScreen.Visibility = Visibility.Hidden;
                    startGame();
                }
            }
            if (OptionsMenu.IsVisible)
            {
                if (GPstate.IsButtonDown(Buttons.A))
                {
                    StartScreen.Visibility = Visibility.Visible;
                    OptionsMenu.Visibility = Visibility.Hidden;
                }
                else if (GPstate.IsButtonDown(Buttons.B))
                {
                    OptionsMenu.Visibility = Visibility.Hidden;
                }
            }
        }

        public void pauseMenuVisibility()
        {
            PauseMenu.Visibility = Visibility.Visible;
        }

        public void calibrateTargeting()
        {
            calibrationScreen.Visibility = Visibility.Visible;
            //Math
            //hand used to aim x and y max and min
            //average the height and width and then divide by 2 to get the center
            //get difference between points at x and y measurements
            //measures of arm movement divided by screen measusurements gets how many meters moved over screen distance
            //

            isCalibrated = true;
            calibrationScreen.Visibility = Visibility.Hidden;
        }
        //Keyboard support
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    MessageBox.Show("Enter");
                    break;
                case Key.Back:
                    MessageBox.Show("Back");
                    break;

            }

        }

        // button click methods
        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            PauseMenu.Visibility = Visibility.Hidden;
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsMenu.Visibility = Visibility.Visible;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            OptionsMenu.Visibility = Visibility.Hidden;
        }

    }
}