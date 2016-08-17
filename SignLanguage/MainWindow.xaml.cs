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


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        private const double HandSize = 30;

        
        private const double JointThickness = 3;

        
        private const double ClipBoundsThickness = 10;

        
        private const float InferredZPositionClamp = 0.1f;

        
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        
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


                            SignDetection(body);

                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                        }
                    }

                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        private void SignDetection(Body body)
        {
            var head = body.Joints[JointType.Head];
            var Righthand = body.Joints[JointType.HandRight];
            var Leftthand = body.Joints[JointType.HandLeft];
            var hipCenter = body.Joints[JointType.HipRight];
            var leftElbow = body.Joints[JointType.ElbowLeft];
            var rightElbow = body.Joints[JointType.ElbowRight];
            var leftShoulder = body.Joints[JointType.ShoulderLeft];
            var rightShoulder = body.Joints[JointType.ShoulderRight];

            if((Righthand.Position.Y-head.Position.Y>0.005)&&(Righthand.Position.Y-head.Position.Y<0.1))
            {
                headerText.Text = "hello";
            }
            else if ((head.Position.Y-Leftthand.Position.Y<0.1)&&(head.Position.Y>Leftthand.Position.Y))
            {
                headerText.Text = "phone";

            }
            else if ((hipCenter.Position.Z - Righthand.Position.Z > 0.4) && (hipCenter.Position.Z- Leftthand.Position.Z<0.2))
            {
                headerText.Text = "Stop";
            }

            else if ((hipCenter.Position.Z - Leftthand.Position.Z > 0.4)&&(hipCenter.Position.Z - Righthand.Position.Z < 0.2))
            {
                headerText.Text = "Peace";

            }

            else if ((hipCenter.Position.Z - Righthand.Position.Z > 0.4)&&(hipCenter.Position.Z - Leftthand.Position.Z > 0.4))
            {
                headerText.Text = "hug";
            }
            else if((rightShoulder.Position.Y-Righthand.Position.Y <0.1)&&(rightShoulder.Position.Y>Righthand.Position.Y)
                     && (leftShoulder.Position.Y - Leftthand.Position.Y < 0.1) && (leftShoulder.Position.Y > Leftthand.Position.Y))
            {
                headerText.Text = "welcome";
            }
            else if((Righthand.Position.Y-head.Position.Y>0.2)&&(Leftthand.Position.Y<head.Position.Y))
            {
                headerText.Text = "high five";
            }
            else if ((Leftthand.Position.Y - head.Position.Y > 0.2) && (Righthand.Position.Y < head.Position.Y))
            {
                headerText.Text = "Loser";
            }
            else if ((Righthand.Position.Y-head.Position.Y>0.2)&&(Leftthand.Position.Y-head.Position.Y>0.2))
            {
                headerText.Text = "we did it :D";
            }
            else if((Righthand.Position.Y-rightElbow.Position.Y>0.005)&&(Righthand.Position.Y-rightElbow.Position.Y <0.09)
                    &&(Righthand.Position.X- rightElbow.Position.X<0.2)&&
                    (Leftthand.Position.Y - rightElbow.Position.Y > 0.005) && (Leftthand.Position.Y - rightElbow.Position.Y < 0.09))
            {
                headerText.Text = "want";
            }
            
            else if((Righthand.Position.X-body.Joints[JointType.Neck].Position.X >0.005)&&
                (Righthand.Position.X-body.Joints[JointType.Neck].Position.X <0.2)
                && (body.Joints[JointType.Neck].Position.Y>Righthand.Position.Y)
                && (body.Joints[JointType.Neck].Position.Y-Righthand.Position.Y<0.3))
            {
                headerText.Text = "I";
            }
            else if(Righthand.Position.X- Leftthand.Position.X<0.08)
            {
                headerText.Text = "Burger";
            }
            else
            {
                headerText.Text = "no sign recognized";

            }

           
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

        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
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
