namespace Microsoft.Samples.Kinect.BodyBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.IO.Ports;
    
    
    

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

       
        private static int GestureResultDAta=0;

        SerialPort sp;

      
        private const double HandSize = 30;

        private const double JointThickness = 3;

       
        private const double ClipBoundsThickness = 10;

        
        private const float InferredZPositionClamp = 0.1f;

        
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));


        //Window1 wind1 = new Window1();

        
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

              
        private readonly Brush inferredJointBrush = Brushes.Yellow;

           
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

       
        private DrawingGroup drawingGroup;

        private DrawingImage imageSource;

        private KinectSensor kinectSensor = null;

        private CoordinateMapper coordinateMapper = null;

    
        private BodyFrameReader bodyFrameReader = null;

        private Body[] bodies = null;

        private List<Tuple<JointType, JointType>> bones;

    
        private int displayWidth;

       
        private int displayHeight;

        
        private List<Pen> bodyColors;

        
        private string statusText = null;

        public MainWindow()
        {

            sp = new SerialPort("COM4",9600);
          sp.Open();
            
            this.kinectSensor = KinectSensor.GetDefault();

            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

           
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.bones = new List<Tuple<JointType, JointType>>();

            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            this.bodyColors = new List<Pen>();

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));

            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            this.kinectSensor.Open();

            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            this.drawingGroup = new DrawingGroup();

            this.imageSource = new DrawingImage(this.drawingGroup);

            this.DataContext = this;

            this.InitializeComponent();
        
         //   wind1.Show();
       

        }

       
        public event PropertyChangedEventHandler PropertyChanged;

    
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

   
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
       
          //    sp.WriteLine("" + GestureResultDAta);

            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }


   
        
           
            



        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }

        }

     
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                 
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    int penIndex = 0;
                    foreach (Body body in this.bodies)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];

                        if (body.IsTracked)
                        {
                            this.DrawClippedEdges(body, dc);

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {
                              
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = InferredZPositionClamp;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }

                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            bool isRightHAndOverHEad = IshandOverHEad(body,JointType.HandRight);
                            bool isLEftHAndOverHEad = IshandOverHEad(body, JointType.HandLeft);

                            int RighthandGestures = RightHandGestureDetected(body, JointType.HandRight);
                            int LeftthandGestures = LeftHandGestureDetected(body, JointType.HandLeft);

                            DetectGesture(body);
                            //sp.WriteLine("" + GestureResultDAta);
                            


                            this.DrawHand(LeftthandGestures, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(RighthandGestures, jointPoints[JointType.HandRight], dc);
                        }
                    }

                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        private void DetectGesture(Body body)
        {
            

            var head = body.Joints[JointType.Head];
            var Righthand = body.Joints[JointType.HandRight];
            var Leftthand = body.Joints[JointType.HandLeft];
            var hipCenter = body.Joints[JointType.HipRight];

            if(head.Position.Y<Righthand.Position.Y)
            {
               
                headerText.Text = "Gesture 1 Recognized";

              
                //sp.Write("2");
                GestureResultDAta = 1;
                
               
            }
            else if (head.Position.Y < Leftthand.Position.Y)
            {


                headerText.Text = "Gesture 2 Recognized";

          
                GestureResultDAta = 2;
              //  sp.Write("3");


            }
                 
            else if (hipCenter.Position.Z-Righthand.Position.Z>0.4)
            {
                headerText.Text = "Gesture 3 Recognized";
                //sp.Write("1");
            }

            else if (hipCenter.Position.Z - Leftthand.Position.Z > 0.4)
            {
                headerText.Text = "Gesture 4 Recognized";
                //sp.Write("5");
            }
            else
            {

                headerText.Text = "No gesture recognized";
          //      sp.Write("4");
            }
         

        }

        private int LeftHandGestureDetected(Body body, JointType jointType)
        {
            int detection;

            var head = body.Joints[JointType.Head];
            var hand = body.Joints[jointType];

            if (hand.Position.Y>head.Position.Y)
            {
                detection = 1;
                return detection;

            }



            return 0;
        }

        private int RightHandGestureDetected(Body body, JointType jointType)
        {
            int detection;

            var head = body.Joints[JointType.Head];
            var hand = body.Joints[jointType];

            if (hand.Position.Y > head.Position.Y)
            {
                detection = 2;
                

                return detection;

            }



            return 0;
        }

        private bool IshandOverHEad(Body body, JointType jointType)
        {
            var head=body.Joints[JointType.Head];
            var hand =body.Joints[jointType];

            bool isDetected= hand.Position.Y>head.Position.Y;

            return isDetected;
        }

       

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
     
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

    
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

     
        private void DrawHand(int  isDetected, Point handPosition, DrawingContext drawingContext)
        {
            switch (isDetected)
            {
                case 1 :
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                   
           
                    break;

                case 2:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;
                case 0 :

                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

              
            }
        }

        
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}
