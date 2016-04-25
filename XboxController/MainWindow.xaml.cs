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

namespace XboxController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer _timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            DisplayControllerInformation();
        }

        void DisplayControllerInformation()
        {
            initalizeController();
            var GPstate = GamePad.GetState(PlayerIndex.One);


        }

        private void initalizeController()
        {
            Brush defaultColor = new SolidColorBrush(Colors.LightGray);

            GamePad.SetVibration(PlayerIndex.One, 0, 0);
        }

    }
}
