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
        DispatcherTimer game_time = new DispatcherTimer();

        KinectSensor _sensor;
        public bool isPaused = false;
        public string targetName;
        int xcam = -2;
        int ycam = 2;
        int zcam = 4;
        int game_frames = 0;
        int game_score;
        int game_level;
        int level_points;
        int interval = 160;
        int looped = 0;

        Material hitColor = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
        Material missColor = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
        Material shootColor = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
        Material noColor = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(0.5) };
            _timer.Tick += _timer_Tick;
            _timer.Start();
            game_time = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            game_time.Tick += game_time_Tick;
            

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
            if (StartScreen.Visibility == Visibility.Visible)
            {
                gameView.Position = new Point3D(xcam, ycam, zcam);
                gameView.LookDirection = new Vector3D(4, 1, -22);
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
            Hud.Visibility = Visibility.Visible;
            game_score = 0;
            game_level = 1;
            level_points = 400;
            game_time.Start();
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
        void game_time_Tick(object sender, EventArgs e)
        {
            game_frames++;
            looped++;
            int seconds;
            DateTime random = DateTime.Now;
            int random_number = random.Millisecond/100;
            seconds = game_frames / 80;
            GameFrame.Text = "Frames:" + game_frames;
            if (game_frames < 300)
            {
                ArrowCenterModel.Material = noColor;
                ArrowLeftModel.Material = noColor;
                ArrowRightModel.Material = noColor;
            }
            switch(seconds)
            {
                case 0:
                    gameText.Text = "3";
                    break;
                case 1:
                    gameText.Text = "2";
                    break;
                case 2:
                    gameText.Text = "1";
                    break;
                case 3:
                    gameText.Text = "GO!";
                    break;
                case 4:
                    gameText.Text = "";
                    break;
            }
            if (looped < interval)
            {
                RandomSeconds.Text = "Seconds: " + random_number;
                
                if (looped == (interval - 1))
                {
                    looped = 0;
                    random_number = random.Millisecond / 100;
                    if (random_number == 1 || random_number == 3 || random_number == 5 || random_number == 8)
                    {
                        ArrowLeftModel.Material = shootColor;
                        ArrowCenterModel.Material = noColor;
                        ArrowRightModel.Material = noColor;
                    }
                    if (random_number == 2 || random_number == 4 || random_number == 6 || random_number == 9)
                    {
                        ArrowCenterModel.Material = shootColor;
                        ArrowLeftModel.Material = noColor;
                        ArrowRightModel.Material = noColor;
                    }
                    if (random_number == 0 || random_number == 3 || random_number == 5 || random_number == 7)
                    {
                        ArrowRightModel.Material = shootColor;
                        ArrowCenterModel.Material = noColor;
                        ArrowLeftModel.Material = noColor;
                    }
                }
            }
            Score.Text = "Score: " + game_score;
            Level.Text = "Level: " + game_level;
            if (game_score > level_points)
            {
                game_level++;
                level_points = level_points + level_points;
                interval = interval - 10;
            }
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
                if (ArrowCenterModel.Material == shootColor)
                {
                    ArrowCenterModel.Material = hitColor;
                    game_score = game_score + 50;
                }
                
            }
            if (targetName == "Right")
            {
                if (ArrowRightModel.Material == shootColor)
                {
                    ArrowRightModel.Material = hitColor;
                    game_score = game_score + 50;
                }
            }
            if (targetName == "Left")
            {
                if (ArrowLeftModel.Material == shootColor)
                {
                    ArrowLeftModel.Material = hitColor;
                    game_score = game_score + 50;
                } 
            }
        }


        //Xbox menu controls 
        void MenuControls()
        {
            initalizeController();
            var GPstate = GamePad.GetState(PlayerIndex.One);
            if (PauseMenu.IsVisible)
            {
                game_time.Stop();
                if (GPstate.IsButtonDown(Buttons.A))
                {
                    PauseMenu.Visibility = Visibility.Hidden;
                    game_time.Start();
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
                    game_time.Start();
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

        private void startBtn(object sender, RoutedEventArgs e)
        {
            startGame();
        }
        public void quitGame()
        {
            Environment.Exit(1);
        }

        private void mainQuitBtn(object sender, RoutedEventArgs e)
        {
            quitGame();
        }

    }
}