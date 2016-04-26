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

namespace XboxController
{
    class Gestures
    {
                readonly int WINDOW_SIZE = 50; //Number of frames for that gesture to last
                int _frameCount = 0; //number of frames we ask for data is called window size
                MainWindow w = new MainWindow();

        //Gesture for pausing game if hands are above head
        public void handsAboveHead(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
            {
                if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
                {
                    MessageBox.Show("Pause");
                }
            }
        }
        public void Reset()
        {
            _frameCount = 0;
        }
    }
}
