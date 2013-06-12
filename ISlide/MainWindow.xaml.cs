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
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Animation;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.SwipeGestureRecognizer;
using System.Configuration;

namespace ISlide
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Recognizer activeRecognizer;
        private static readonly JointType[][] SkeletonSegmentRuns = new JointType[][]
        {
            new JointType[] 
            { 
                JointType.Head, JointType.ShoulderCenter, JointType.HipCenter 
            },
            new JointType[] 
            { 
                JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
                JointType.ShoulderCenter,
                JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight
            },
            new JointType[]
            {
                JointType.FootLeft, JointType.AnkleLeft, JointType.KneeLeft, JointType.HipLeft,
                JointType.HipCenter,
                JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight
            }
        };
        private KinectSensor nui = null;
        private bool isDisconnectedField = true;
        private string disconnectedReasonField;
        private Skeleton[] skeletons = new Skeleton[0];
        private DateTime highlightTime = DateTime.MinValue;
        private int highlightId = -1;
        private int nearestId = -1;
        private StickMen stickMen = new StickMen();
        private Process pro = null;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.activeRecognizer = this.CreateRecognizer();            
        }

        private void win_ges_Loaded(object sender, RoutedEventArgs e)
        {
            KinectStart();
            StartColorFrame();
            this.txt_filename.Text = ConfigurationSettings.AppSettings["FileName"];
        }

        private void StartColorFrame()
        {
            if (this.nui != null)
            {
                slider_angle.Value = nui.ElevationAngle;
                this.nui.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_ColorFrameReady);
            }
        }

        void nui_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }
                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);
                int stride = colorFrame.Width * 4;
                img_color.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height,
                    96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }
        }
        
        private void win_ges_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.UninitializeNui();
            stickMen.Close();
            if (pro != null && !pro.HasExited)
            {
                pro.Kill();
                pro.Close();
                pro.Dispose();
                pro = null;
            }
            
        }

        private void btn_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.DefaultExt = "All Files（*.*）|*.*";
            fileDlg.Filter = "PowerPoint放映（*.ppsx）|*.ppsx|PowerPoint 97-2003 放映（*.pps）|*.pps|EXE文件（*.exe）|*.exe|All Files（*.*）|*.*";
            if (fileDlg.ShowDialog() == true)
            {
                txt_filename.Text = fileDlg.FileName;                
            }
        }

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (pro != null && !pro.HasExited)
            {
                MessageBox.Show("当前已有一个实例在运行，不能重复启动！");
                return;
            }
            if (this.nui == null)
            {
                MessageBox.Show("Kinect未能正确初始化，请检查连接是否正确！");
                return;
            }
            try
            {
                pro = Process.Start(txt_filename.Text);
                Thread.Sleep(1000);
                stickMen.Show();
                StartSkeletonFrame();
                Mouse.MoveTo(new Point(SystemParameters.PrimaryScreenWidth, 0));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void KinectStart()
        {
            this.InitializeNui();
            KinectSensor.KinectSensors.StatusChanged += (s, ee) =>
            {
                switch (ee.Status)
                {
                    case KinectStatus.Connected:
                        if (nui == null)
                        {
                            Debug.WriteLine("New Kinect connected");
                            InitializeNui();
                        }
                        else
                        {
                            Debug.WriteLine("Existing Kinect signalled connection");
                        }
                        break;
                    default:
                        if (ee.Sensor == nui)
                        {
                            Debug.WriteLine("Existing Kinect disconnected");
                            UninitializeNui();
                        }
                        else
                        {
                            Debug.WriteLine("Other Kinect event occurred");
                        }
                        break;
                }
            };
        }

        private void UninitializeNui()
        {
            if (this.nui != null)
            {
                this.nui.Stop();
                this.nui = null;
            }
            this.IsDisconnected = true;
            this.DisconnectedReason = null;
        }
        
        private void InitializeNui()
        {
            this.UninitializeNui();
            var index = 0;
            while (this.nui == null && index < KinectSensor.KinectSensors.Count)
            {
                try
                {
                    this.nui = KinectSensor.KinectSensors[index];
                    this.nui.Start();
                    this.IsDisconnected = false;
                    this.DisconnectedReason = null;
                }
                catch (IOException ex)
                {
                    this.nui = null;
                    this.DisconnectedReason = ex.Message;
                }
                catch (InvalidOperationException ex)
                {
                    this.nui = null;
                    this.DisconnectedReason = ex.Message;
                }
                index++;
            }
        }

        private void StartSkeletonFrame()
        {
            if (this.nui != null)
            {
                this.nui.SkeletonStream.TrackingMode = checkBox_seated.IsChecked == true ? SkeletonTrackingMode.Seated : SkeletonTrackingMode.Default;

                this.nui.SkeletonStream.Enable();

                this.nui.SkeletonFrameReady += this.OnSkeletonFrameReady;
            }
        }
        private void StopSkeletonFrame()
        {
            if (this.nui != null)
            {
                this.nui.SkeletonFrameReady -= this.OnSkeletonFrameReady;
            }            
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (pro == null || pro.HasExited)
            {
                stickMen.cav_stickmen.Children.Clear();
                stickMen.Hide();
                StopSkeletonFrame();
                return;
            }
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    if (this.skeletons.Length != frame.SkeletonArrayLength)
                    {
                        this.skeletons = new Skeleton[frame.SkeletonArrayLength];
                    }
                    frame.CopySkeletonDataTo(this.skeletons);
                    var newNearestId = -1;
                    var nearestDistance2 = double.MaxValue;
                    foreach (var skeleton in this.skeletons)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            var distance2 = (skeleton.Position.X * skeleton.Position.X) +
                                (skeleton.Position.Y * skeleton.Position.Y) +
                                (skeleton.Position.Z * skeleton.Position.Z);
                            if (distance2 < nearestDistance2)
                            {
                                newNearestId = skeleton.TrackingId;
                                nearestDistance2 = distance2;
                            }
                        }
                    }
                    if (this.nearestId != newNearestId)
                    {
                        this.nearestId = newNearestId;
                    }
                    this.activeRecognizer.Recognize(sender, frame, this.skeletons);
                    this.DrawStickMen(this.skeletons);
                }
            }
        }

        public bool IsDisconnected
        {
            get
            {
                return this.isDisconnectedField;
            }
            private set
            {
                if (this.isDisconnectedField != value)
                {
                    this.isDisconnectedField = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("IsDisconnected"));
                    }
                }
            }
        }

        public string DisconnectedReason
        {
            get
            {
                return this.disconnectedReasonField;
            }
            private set
            {
                if (this.disconnectedReasonField != value)
                {
                    this.disconnectedReasonField = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("DisconnectedReason"));
                    }
                }
            }
        }

        private Recognizer CreateRecognizer()
        {
            var recognizer = new Recognizer();
            recognizer.SwipeRightDetected += (s, e) =>
            {
                 // if (e.Skeleton.TrackingId == nearestId)
                {
                    Keyboard.Press(Key.Right);
                    Keyboard.Release(Key.Right);
                    var storyboard = Resources["LeftAnimate"] as Storyboard;
                    if (storyboard != null)
                    {
                        storyboard.Begin();
                    }
                    HighlightSkeleton(e.Skeleton);
                }
            };

            recognizer.SwipeLeftDetected += (s, e) =>
            {
                 // if (e.Skeleton.TrackingId == nearestId)
                {
                    Keyboard.Press(Key.Left);
                    Keyboard.Release(Key.Left);
                    var storyboard = Resources["RightAnimate"] as Storyboard;
                    if (storyboard != null)
                    {
                        storyboard.Begin();
                    }
                    HighlightSkeleton(e.Skeleton);
                }
            };
            return recognizer;
        }

        private void HighlightSkeleton(Skeleton skeleton)
        {
            this.highlightTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);
            this.highlightId = skeleton.TrackingId;
        }

        private void DrawStickMen(Skeleton[] skeletons)
        {
            stickMen.cav_stickmen.Children.Clear();

            foreach (var skeleton in skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    this.DrawStickMan(skeleton, Brushes.WhiteSmoke, 7);
                }
            }

            foreach (var skeleton in skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    var brush = DateTime.UtcNow < this.highlightTime && skeleton.TrackingId == this.highlightId ? Brushes.Red :
                        skeleton.TrackingId == this.nearestId ? Brushes.Black : Brushes.Gray;
                    this.DrawStickMan(skeleton, brush, 3);
                }
            }
        }

        private void DrawStickMan(Skeleton skeleton, Brush brush, int thickness)
        {
            Debug.Assert(skeleton.TrackingState == SkeletonTrackingState.Tracked, "The skeleton is being tracked.");
            foreach (var run in SkeletonSegmentRuns)
            {
                var next = this.GetJointPoint(skeleton, run[0]);
                for (var i = 1; i < run.Length; i++)
                {
                    var prev = next;
                    next = this.GetJointPoint(skeleton, run[i]);

                    var line = new Line
                    {
                        Stroke = brush,
                        StrokeThickness = thickness,
                        X1 = prev.X,
                        Y1 = prev.Y,
                        X2 = next.X,
                        Y2 = next.Y,
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round
                    };
                    stickMen.cav_stickmen.Children.Add(line);
                }
            }
        }

        private Point GetJointPoint(Skeleton skeleton, JointType jointType)
        {
            var joint = skeleton.Joints[jointType];
            var point = new Point
            {
                X = (stickMen.cav_stickmen.Width / 2) + (stickMen.cav_stickmen.Height * joint.Position.X / 3),
                Y = (stickMen.cav_stickmen.Width / 2) - (stickMen.cav_stickmen.Height * joint.Position.Y / 3)
            };
            return point;
        }

        private void btn_set_Click(object sender, RoutedEventArgs e)
        {
            if (this.nui != null)
            {
                this.nui.ElevationAngle = Convert.ToInt32(slider_angle.Value); 
            }
        }

        private void slider_angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slider_angle.Value != Convert.ToInt32(slider_angle.Value))
            {
                slider_angle.Value = Convert.ToInt32(slider_angle.Value);
            }
            textBlock_angle.Text = slider_angle.Value.ToString();
        }

        private void win_ges_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((pro != null && !pro.HasExited) && (e.Key == Key.Left || e.Key == Key.Right))
            {
                Mouse.MoveTo(new Point(this.Left - 5, this.Top - 5));
                Mouse.Click(MouseButton.Left);
                Mouse.MoveTo(new Point(SystemParameters.PrimaryScreenWidth, 0));
            }
        }

    }
}
