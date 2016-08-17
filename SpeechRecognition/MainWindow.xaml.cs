namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using System;
    using System.Collections.Generic;    
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;    
    using System.Windows;    
    using System.Windows.Documents;
    using System.Windows.Media;
    using Microsoft.Kinect;    
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;
    using System.IO.Ports;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "In a full-fledged application, the SpeechRecognitionEngine object should be properly disposed. For the sake of simplicity, we're omitting that code in this sample.")]
    public partial class MainWindow : Window
    {
       

        SerialPort sp;

  
    

        
      

        private KinectSensor kinectSensor = null;

        
        private KinectAudioStream convertStream = null;

        
        private SpeechRecognitionEngine speechEngine = null;

        
      //  private Direction curDirection = Direction.Up;

        
        private List<Span> recognitionSpans;

       
        public MainWindow()
        {

            sp = new SerialPort("COM4", 9600);
            sp.Open();

           
            this.InitializeComponent();
        }

       
        private enum Direction
        {
            
            Up,

            
            Down,

            
            Left,

            
            Right
        }


        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;
            
         
            try
            {
                recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            }
            catch (COMException)
            {
                return null;
            }

            foreach (RecognizerInfo recognizer in recognizers)
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-CA".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

      
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {


           
            // Only one sensor is supported
            this.kinectSensor = KinectSensor.GetDefault();

            if (this.kinectSensor != null)
            {
               
                this.kinectSensor.Open();

                IReadOnlyList<AudioBeam> audioBeamList = this.kinectSensor.AudioSource.AudioBeams;
                System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

           
                this.convertStream = new KinectAudioStream(audioStream);
            }
            else
            {
              
                return;
            }

            RecognizerInfo ri = TryGetKinectRecognizer();

            if (null != ri)
            {

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                

                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    this.speechEngine.LoadGrammar(g);
                }

                this.speechEngine.SpeechRecognized += this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected += this.SpeechRejected;
        
                // let the convertStream know speech is going active
                this.convertStream.SpeechActive = true;


                this.speechEngine.SetInputToAudioStream(
                    this.convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
            }
        }

 
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != this.convertStream)
            {
                this.convertStream.SpeechActive = false;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= this.SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= this.SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }

            if (null != this.kinectSensor)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

 

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double ConfidenceThreshold = 0.3;

           sp.Write("1");

          


      

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "FORWARD":
                        GestureText.Text = "Forward";
                        sp.Write("1");

                        break;

                    case "BACKWARD":

                        GestureText.Text = "Backward";

                        sp.Write("5");
                        break;

                    case "LEFT":

                        GestureText.Text = "Left";
                        sp.Write("3");
                
                        break;

                    case "RIGHT":

                        GestureText.Text = "Right";

                        sp.Write("2");
                  
                        break;

                    case "STOP":

                        GestureText.Text = "STOP";

                        sp.Write("4");

                        break;

                }
            }
        }

     
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
          //  this.ClearRecognitionHighlights();
        }
    }
}