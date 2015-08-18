/* Kinect V2 Map Visualisation - This project is made using various technology. 
 
Firstly kinectV2 is used for gesture and voice recognition. In this application two types of gesture is recognised one is based on visual gesture builder 
and other one is using bone joints as kinect v2 has the capability to capture small bone in hand so using hand gesture like close , open or lasso. 
After that window app has the capability to speak if speaker is on about what is happening in app otherwise you can see on screen as text too.

Secondly, for the UI interface I have not used the Google earth which is widely used before for Map visualisation. As I like open source so want to use 
cesium which is also for me a great opportunity to learn about WEBgl, new technology of HTML 5 which is widely used for visualisation.
Here we can have a full globe view and also can move globe using gesture, search for the place by speaking and switching to aeroplane and street view by saying
fly and walk respectively. For street view I have used Google street view.

Lastly we can take the snapshot of any state in our app by doing lasso hand gesture.

In the window app I have make the UI such that user not feel alone while using app so user can see himself/herself on the screen and when they do gesture 
they can see bone of the body too. Moreover we do not have to open app again we can close or open kinect as many time as we want in app it will reset the 
application. To reset the cesium just click on home button on it.

I have provided the Instruction on how to use application which can be hide by clicking on hide button.

To run the app things required are mentioned in read me file.
Places we can go - INDIA, AMERICA, SANDIEGO, SANFRANCISCO, MOUNTEVEREST, GRANDCANYON, HANOVER, NEWYORK, DELHI, GERMANY, EUROPE, BANGLORE, MUMBAI, GOA, HAMBURG
HILDESHEIM, BELGIUM, AMSTERDAM, SWITZERLAND, Effiel Tower, Great Pyramid, PARIS, PRAGUE, BERLIN, VENICE, PISA, Taj Mahal

Made By - Ayush Aggarwal- IIT kanpur , www.ayushaggarwal.in
Supervised By - Prof. Dr. Monika Sester

 If you have doubt or want to say thanks feel free to contact me at www.ayushaggarwal.in :)
 */
namespace final2
{
// these are refrences used for the project
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Kinect;  // for the kinect
using Microsoft.Kinect.VisualGestureBuilder; // for the gesture made by gesture builder
using Microsoft.Kinect.Wpf.Controls;
using EO.WebBrowser;    // for the chrome web browser framework used 
using EO.WebBrowser.Wpf;  // to use its tool in wpf
using System.Runtime.InteropServices;
using Microsoft.Speech.AudioFormat;    // for speech recognition by kinect
using System.Speech.Synthesis;
using System.Windows.Interop;
using PeteBrown.ScreenCapture.Win32Api;  // for taking snapshot using the class
///using System.Speech.Recognition; // for speech recognition by kinect using system hard to do by  it
using Microsoft.Speech.Recognition; // for speech recognition by kinect

/// <summary>
/// Interaction logic for Sound
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "In a full-fledged application, the SpeechRecognitionEngine object should be properly disposed. For the sake of simplicity, we're omitting that code in this sample.")]

public partial class MainWindow : Window
    {
        // As me using two type of gesture recognition one by visual gesture builder and one by bone joints 
        // so I have used different sensor and different body and frame reader so as to make it good.
        // I used joints of hand mean  open,close and lasso to learn about kinect v2 propeties.
        KinectSensor sensor;  // this sensor is used for visual gesture builder or  vgb one
        
        ///  Active Kinect sensor 
        private KinectSensor kinectSensor = null;

        ///  Array for the bodies (Kinect will track up to 6 people simultaneously) 
        private Body[] bodies = null;

        ///  Reader for body frames 
        private BodyFrameReader bodyFrameReader = null;


        ///  KinectBodyView object which handles drawing the Kinect bodies to a View box in the UI 
        private KinectBodyView kinectBodyView = null;

        ///  List of gesture detectors, there will be one detector created for each potential body (max of 6) 
        private List<GestureDetector> gestureDetectorList = null;
        SpeechSynthesizer speaker;  // this is variable for taking speech input
        
        /// Stream for 32b-16b conversion.
        private KinectAudioStream convertStream = null;


        /// Speech recognition engine using audio data from Kinect.
        private SpeechRecognitionEngine speechEngine = null;

        // this variables are used for gesture recognition using joints
         #region Members

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;  // multisource is used a swe can get various types of sources from it infrared, color etc as per application
        IList<Body> _bodies;    // containing body

        #endregion

        #region Constructor

        #endregion

        #region Event handlers

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            EO.WebBrowser.Runtime.RemoteDebugPort = 1234;   
            // this is used to use javascript console for html inside window app we can use by going to following address
            // http://localhost:1234/
            // System.Windows.Forms.MessageBox.Show("Welcome");
        }

        // this is the default class used for activating recognizer which language to use and its setup with kinect
        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;

            // This is required to catch the case when an expected recognizer is not installed.
            // By default - the x86 Speech Runtime is always expected. 
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
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) &&
                    "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        // this class is loaded after window is initialise or mainwindow.xaml is loaded
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            KinectRegion.SetKinectRegion(this, this.kinectRegion);      // make the kinect region this is used to show kinect hand activity on screen can see 
            // on window app part at bottom.

            _sensor = KinectSensor.GetDefault();                    // for the joint one

            this.kinectSensor = KinectSensor.GetDefault();          // for the vgb one

            // set IsAvailableChanged event notifier it is used to activate bone stucture when some body is there only otherwise it is blank
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {

        }

        // this class is loaded to save image
        void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);              // for saving image in png
        }

        void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(100,100 , 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        void OnClickphoto(object sender, RoutedEventArgs e)   // called when click on click button 
        {
            CapturedImage.Source = PeteBrown.ScreenCapture.ScreenCapture.CaptureRegion(0, 0, 1500, 800, true); // here we can define the region of the full app which has to be capture 0,0 is top left point

            SaveToPng(MyGrid, "D:\\image.png"); // It saved the image in given path
        }

        // this class is loaded when we want to open kinect after using the app manually
        void OnOpenSensor(object sender, RoutedEventArgs e) // called when the button is clicked to open sensor
        {
            // just to tell kinect started
            speaker = new SpeechSynthesizer();         // Used for getting output voice from computer
            speaker.Speak("App is started"); 
            System.Windows.Forms.MessageBox.Show("Kinect started - Click on Map Now"); 

            // FOR THE VGB ONE

            // open the sensor
            this.kinectSensor.Open();

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // set the BodyFramedArrived event notifier
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // initialize the BodyViewer object for displaying tracked bodies in the UI
            this.kinectBodyView = new KinectBodyView(this.kinectSensor);

            // initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // set our data context objects for display in UI
            // It is usd to show the bone stucture in UI
            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;

            // As Kinect V2 can detect upto 6 bdies and so can detect gesture from all six bodies so we can used that by assigning gesture detector for each body
            // create a gesture detector for each body (6 bodies => 6 detectors) and create content controls to display results in the UI
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount; // if body is six then it goes to check for each.
            
            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView(i, false, false, 0.0f, webView1,gestureState.Text); 
                // i is body number,webview1 is the html control which have to pass to access it in  this method. // others defiend later
                GestureDetector detector = new GestureDetector(this.kinectSensor, result);        
                this.gestureDetectorList.Add(detector);             
            }


            // FOR THE JOINT ONE
            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
            
            
            // FOR THE SOUND
            this.sensor = KinectSensor.GetDefault();

            // open the sensor
            this.sensor.Open();

            // grab the audio stream
            IReadOnlyList<AudioBeam> audioBeamList = this.sensor.AudioSource.AudioBeams;
            System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

            // create the convert stream
            this.convertStream = new KinectAudioStream(audioStream);

            RecognizerInfo ri = TryGetKinectRecognizer();


            // these are engine where we put what to analyse in speech recogniition it can not be done if we can use bing speech recognizer but it is not possible to use now in germany as in beta phase

            this.speechEngine = new SpeechRecognitionEngine(ri.Id);  // speech recognition engine

            var directions = new Choices();                                  // direction is the vaariable used to direct where we have to go
            directions.Add(new SemanticResultValue("ZoomIn", "ZOOMIN"));    // first one is the text which system see in audio and if match then it save the second name in it which we can access to run what we want
            directions.Add(new SemanticResultValue("ZoomIn", "ZOOMIN"));
            directions.Add(new SemanticResultValue("In", "ZOOMIN"));
            directions.Add(new SemanticResultValue("ZoomOut", "ZOOMOUT"));
            directions.Add(new SemanticResultValue("Out", "ZOOMOUT"));
            directions.Add(new SemanticResultValue("ZoomOut", "ZOOMOUT"));
            directions.Add(new SemanticResultValue("Left", "LEFT"));
            directions.Add(new SemanticResultValue("Right", "RIGHT"));
            directions.Add(new SemanticResultValue("Up", "UP"));
            directions.Add(new SemanticResultValue("Down", "DOWN"));

            // places
            directions.Add(new SemanticResultValue("Go India", "INDIA"));
            directions.Add(new SemanticResultValue("india", "INDIA"));
            directions.Add(new SemanticResultValue("Go AMERICA", "AMERICA"));
            directions.Add(new SemanticResultValue("america", "AMERICA"));
            directions.Add(new SemanticResultValue("Go SanDiego", "SANDIEGO"));
            directions.Add(new SemanticResultValue("SanDiego", "SANDIEGO"));
            directions.Add(new SemanticResultValue("MY PLACE", "MYPLACE"));              // this will not work through window app as cant activate gps through eo web browser 
            directions.Add(new SemanticResultValue("San Francisco Bay", "SANFRANCISCO"));
            directions.Add(new SemanticResultValue("Mount Everest", "MOUNTEVEREST"));
            directions.Add(new SemanticResultValue("Grand Canyon", "GRANDCANYON"));
            directions.Add(new SemanticResultValue("hannover", "HANOVER"));
            directions.Add(new SemanticResultValue("newyork", "NEWYORK"));
            directions.Add(new SemanticResultValue("Delhi", "DELHI"));
            directions.Add(new SemanticResultValue("Goa", "GOA"));
            directions.Add(new SemanticResultValue("Mumbai", "MUMBAI"));
            directions.Add(new SemanticResultValue("Banglore", "BANGLORE"));
            directions.Add(new SemanticResultValue("Europe", "EUROPE"));
            directions.Add(new SemanticResultValue("Germany", "GERMANY"));
            directions.Add(new SemanticResultValue("Switzerland", "SWITZERLAND"));
            directions.Add(new SemanticResultValue("Amsterdam", "AMSTERDAM"));
            directions.Add(new SemanticResultValue("Belgium", "BELGIUM"));
            directions.Add(new SemanticResultValue("Hildesheim", "HILDESHEIM"));
            directions.Add(new SemanticResultValue("Hamburg", "HAMBURG"));
            directions.Add(new SemanticResultValue("Berlin", "BERLIN"));
            directions.Add(new SemanticResultValue("Prague", "PRAGUE"));
            directions.Add(new SemanticResultValue("Sylt", "SYLT"));
            directions.Add(new SemanticResultValue("Paris", "PARIS"));
            directions.Add(new SemanticResultValue("Great Pyramid", "GREAT"));
            directions.Add(new SemanticResultValue("Effiel Tower", "Tower"));
            directions.Add(new SemanticResultValue("Taj Mahal", "TAj"));
            directions.Add(new SemanticResultValue("Pisa", "PISA"));
            directions.Add(new SemanticResultValue("Venice", "VENICE"));
            //tools
            directions.Add(new SemanticResultValue("Fly", "FLY"));
            directions.Add(new SemanticResultValue("walk", "WALK"));
            directions.Add(new SemanticResultValue("valk", "WALK"));
            directions.Add(new SemanticResultValue("Back", "BACK"));
            directions.Add(new SemanticResultValue("photo", "PHOTO"));
            directions.Add(new SemanticResultValue("PHOTO", "PHOTO"));
            var gb = new GrammarBuilder { Culture = ri.Culture }; // to run recognizer
            gb.Append(directions);
            gb.AppendWildcard();  // this is used so that second word of saying wil not be count to detect the acccuracy of word

            var grr = new Grammar(gb);

            this.speechEngine.LoadGrammar(grr); // to load the grammer

            this.speechEngine.SpeechRecognized += this.SpeechRecognized;              // called if speech recognized
            this.speechEngine.SpeechRecognitionRejected += this.SpeechRejected;       // called if speech rejected

            // let the convertStream know speech is going active
            this.convertStream.SpeechActive = true;

            this.speechEngine.SetInputToAudioStream(
            this.convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);
      
        }


        void OnCloseSensor(object sender, RoutedEventArgs e)   // called when click on closing button audio stream gesture recognition all stopped
        {
            CapturedImage.Source = null;     // to remove captured image
            System.Windows.Forms.MessageBox.Show("Kinect Closed");
            speaker = new SpeechSynthesizer();         // Used for getting output voice from computer
            speaker.Speak("App is stoped"); 

            // FOR CLOSING VGB ONE
            if (this.bodyFrameReader != null)   // frame is null
            {
                this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                this.bodyFrameReader.Dispose();
            }

            if (this.gestureDetectorList != null) // detector of all six body is null
            {

                this.gestureDetectorList.Clear();
            }

            if (this.kinectSensor != null)       // Sensor is null
            {
                this.kinectSensor.IsAvailableChanged -= this.Sensor_IsAvailableChanged;
                this.kinectSensor.Close();
            }

            // FOR CLOSING JOINT ONE
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }

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

            if (null != this.sensor)
            {
                this.sensor.Close();
            }

        }

        // FOR THE VGB ONE
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    // It means till people did not go away from sensor no new update of body will be required.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            //this happens if we are detecting some bodies.
            if (dataReceived)
            {
                // visualize the new body data on window
                this.kinectBodyView.UpdateBodyFrame(this.bodies);

                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (this.bodies != null)
                {
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            // Thus it considers all the cases 
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }
            }
        }

        // FOR THE JOINT ONE
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color it is used to display the color image of the user
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap(); // to save it in UI
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                // Find the hand states
                                string HandStatee = "-"; // to show the text on user screen

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        { 
                                        switch (body.HandLeftState)
                                        {
                                            case HandState.Open:
                                                HandStatee = "Nothing";
                                                break;
                                            case HandState.Closed:
                                                HandStatee = "Zoom out";       
                                                // this line of code the interaction between Csharp and javascript
                                                // it depend on the chrome framework we are using for eo browser I used this is the syntax
                                                webView1.EvalScript("callfromcs('S');"); 
                                                // here callfromcs is javascript function where we passed the S as a parameter.
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                        }
                                    
                                    case HandState.Closed:
                                        {
                                            switch (body.HandLeftState)
                                            {
                                                case HandState.Open:
                                                    HandStatee = "Zoom out";       
                                                    webView1.EvalScript("callfromcs('S');"); 
                                                    break;
                                                case HandState.Closed:
                                                    HandStatee = "Zoom in";
                                                    webView1.EvalScript("callfromcs('W');"); 
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        }
                                    
                                    case HandState.Lasso:
                                        speaker = new SpeechSynthesizer();         // Used for getting output voice from computer
                                        speaker.Speak("Photo clicked");
                                        HandStatee = "Photo clicked";
                                        CapturedImage.Source = PeteBrown.ScreenCapture.ScreenCapture.CaptureRegion(0, 0, 1500, 800, true); // just for test
                                        SaveToPng(CapturedImage, "D:\\image.png");
                                        break;
                                    default:
                                        break;
                                }
                                switch (body.HandLeftState)
                                {
                                    case HandState.Lasso:
                                        speaker = new SpeechSynthesizer();         // Used for getting output voice from computer
                                        speaker.Speak("Photo Removed");
                                        HandStatee = "Photo Removed";
                                        break;
                                    default:
                                        break;
                                }
                                gestureState.Text = HandStatee;
                                
                            }
                        }
                    }
                }
            }
        }

      #endregion


        // FOR THE SOUND

        private void ClearRecognitionHighlights() // called when speech recognized can be used for various purposes
        {

        }


        private void SpeechRecognitionRejected(object sender, SpeechRecognizedEventArgs e) // called when speech is not recognized can be used for various purposes
        {

        }

        // called when speech is recognized 
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.4;

            var result = e.Result.Semantics.Value.ToString();   // to change voice to text


            this.ClearRecognitionHighlights();

            if (e.Result.Confidence >= ConfidenceThreshold)    
            {
                switch (result)
                {
                    case "ZOOMIN":
                        {
                            speaker = new SpeechSynthesizer();         // Used for getting output voice from computer
                            speaker.Speak("Ready to Go IN");           // these are the ouput voice by computer
                            webView1.EvalScript("callfromcs('W');");    // to call the javascript commond from window app using eo.web browser
                            Console.WriteLine(result);            // to show the text as console
                            break;
                        }

                    case "ZOOMOUT":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go OUT");
                            webView1.EvalScript("callfromcs('S');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "LEFT":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go LEFT");
                            webView1.EvalScript("callfromcs('A');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "RIGHT":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go RIGHT");
                            webView1.EvalScript("callfromcs('D');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "UP":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go UP");
                            webView1.EvalScript("callfromcs('Q');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "DOWN":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go DOWN");
                            webView1.EvalScript("callfromcs('E');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "INDIA":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go INDIA");
                            webView1.EvalScript("searchcallfromcs('india');");    // searchcallfromcs is javascript function to search the india in cesium and google map and return with input which can be used
                            Console.WriteLine(result);
                            break;
                        }

                    case "AMERICA":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMERICA");
                            webView1.EvalScript("searchcallfromcs('america');");
                            Console.WriteLine(result);
                            break;
                        }
                    case "SANDIEGO":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMERICA");
                            webView1.EvalScript("searchcallfromcs('SANDIEGO');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "SANFRANCISCO":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMERICA");
                            webView1.EvalScript("searchcallfromcs('SANFRANCISCO');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "MOUNTEVEREST":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMERICA");
                            webView1.EvalScript("searchcallfromcs('MOUNTEVEREST');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "GRANDCANYON":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMERICA");
                            webView1.EvalScript("searchcallfromcs('GRANDCANYON');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "HANOVER":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMERICA");
                            webView1.EvalScript("searchcallfromcs('HANNOVER');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "NEWYORK":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMERICA");
                            webView1.EvalScript("searchcallfromcs('NEWYORK');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "DELHI":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go DELHI");
                            webView1.EvalScript("searchcallfromcs('DELHI');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "GERMANY":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go GERMANY");
                            webView1.EvalScript("searchcallfromcs('GERMANY');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "EUROPE":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go DELHI");
                            webView1.EvalScript("searchcallfromcs('EUROPE');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "BANGLORE":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go BANGLORE");
                            webView1.EvalScript("searchcallfromcs('BANGLORE');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "MUMBAI":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go MUMBAI");
                            webView1.EvalScript("searchcallfromcs('MUMBAI');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "GOA":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go GOA");
                            webView1.EvalScript("searchcallfromcs('GOA');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "HAMBURG":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go HAMBURG");
                            webView1.EvalScript("searchcallfromcs('HAMBURG');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "HILDESHEIM":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go HILDESHEIM");
                            webView1.EvalScript("searchcallfromcs('HILDESHEIM');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "BELGIUM":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go BELGIUM");
                            webView1.EvalScript("searchcallfromcs('BELGIUM');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "AMSTERDAM":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go AMSTERDAM");
                            webView1.EvalScript("searchcallfromcs('AMSTERDAM');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "SWITZERLAND":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go SWITZERLAND");
                            webView1.EvalScript("searchcallfromcs('SWITZERLAND');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "Tower":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go Effiel Tower");
                            webView1.EvalScript("searchcallfromcs('Effiel Tower');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "GREAT":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go SWITZERLAND");
                            webView1.EvalScript("searchcallfromcs('Great Pyramid');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "PARIS":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go PARIS");
                            webView1.EvalScript("searchcallfromcs('PARIS');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "PRAGUE":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go PRAGUE");
                            webView1.EvalScript("searchcallfromcs('PRAGUE');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "BERLIN":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go BERLIN");
                            webView1.EvalScript("searchcallfromcs('BERLIN');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "VENICE":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go VENICE");
                            webView1.EvalScript("searchcallfromcs('VENICE');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "PISA":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go PISA");
                            webView1.EvalScript("searchcallfromcs('PISA');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "TAj":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Go Taj Mahal");
                            webView1.EvalScript("searchcallfromcs('Taj Mahal');");
                            Console.WriteLine(result);
                            break;
                        }





















                    case "FLY":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to FLY");
                            webView1.EvalScript("movecallfromcs('fly');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "WALK":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to WALK");
                            webView1.EvalScript("movecallfromcs('walk');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "BACK":
                        {
                            speaker = new SpeechSynthesizer();
                            speaker.Speak("Ready to Normal");
                            webView1.EvalScript("movecallfromcs('normal');");
                            Console.WriteLine(result);
                            break;
                        }

                    case "PHOTO": 
                        {
                        speaker = new SpeechSynthesizer();
                        speaker.Speak("Ready to Take photo");
                        CapturedImage.Source = PeteBrown.ScreenCapture.ScreenCapture.CaptureRegion(0, 0, 1500, 800, true); // just for test
                        SaveToPng(CapturedImage, "D:\\image.png");
                        Console.WriteLine(result);
                        break;
                        }
                }
            }

        }

        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            this.ClearRecognitionHighlights();
        }

        // this is the big methos used inside the main window method used for gesture detction of all the six body we run it one by one to see if any body
        // match with any gesture in the database which we loaded on in gesture detector method and checking one by one for all the gesture for all the body 
        // through ti
        public partial class GestureResultView
        {
            
            /// Array of brush colors to use for a tracked body; array position corresponds to the body colors used in the KinectBodyView class 
            private readonly Brush[] trackedColors = new Brush[] { Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };

            ///  Brush color to use as background in the UI 
            private Brush bodyColor = Brushes.Gray;

            ///  The body index (0-5) associated with the current gesture detector 
            private int bodyIndex = 0; // initialise

            ///  Current confidence value reported by the discrete gesture 
            private float confidence = 0.0f; // can be used to show

            ///  True, if the discrete gesture is currently being detected 
            private bool detected = false;

            ///  True, if the body is currently being tracked <
            private bool isTracked = false;

            ///  To pass the webcontrol to nested method we have to define it too
            EO.WebBrowser.WebView webView1 = null;

            ///  To show the output text 
            string gestureState = "--"; // initialise

            public GestureResultView(int bodyIndex, bool isTracked, bool detected, float confidence, EO.WebBrowser.WebView webView1, string gestureState)
            {
                /// Initializes a new instance of the GestureResultView class and sets initial property values
                this.BodyIndex = bodyIndex;
                this.IsTracked = isTracked;
                this.Detected = detected;
                this.Confidence = confidence;
                this.webView1 = webView1;
                this.gestureState = gestureState;
            }

            /// Gets the body index associated with the current gesture detector result 
            public int BodyIndex
            {
                get
                {
                    return this.bodyIndex;
                }

                private set
                {
                    if (this.bodyIndex != value)
                    {
                        this.bodyIndex = value;
                    }
                }
            }

            /// Gets the body color corresponding to the body index for the result // different for each body
            public Brush BodyColor
            {
                get
                {
                    return this.bodyColor;
                }

                private set
                {
                    if (this.bodyColor != value)
                    {
                        this.bodyColor = value;
                    }
                }
            }

            /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
            public bool IsTracked
            {
                get
                {
                    return this.isTracked;
                }

                private set
                {
                    if (this.IsTracked != value)
                    {
                        this.isTracked = value;
                    }
                }
            }

            /// Gets a value indicating whether or not the discrete gesture has been detected
            public bool Detected
            {
                get
                {
                    return this.detected;
                }

                private set
                {
                    if (this.detected != value)
                    {
                        this.detected = value;
                    }
                }
            }

            /// Gets a float value which indicates the detector's confidence that the gesture is occurring for the associated body 
            public float Confidence
            {
                get
                {
                    return this.confidence;
                }

                private set
                {
                    if (this.confidence != value)
                    {
                        this.confidence = value;
                    }
                }
            }

            /// Updates the values associated with the discrete gesture detection result
            /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
            /// <param name="isGestureDetected">True, if the discrete gesture is currently detected for the associated body</param>
            /// <param name="detectionConfidence">Confidence value for detection of the discrete gesture</param>
           
            /// This one is for left gesture detection checking
            public void UpdateGestureResult1(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
            {
                this.IsTracked = isBodyTrackingIdValid;
                this.Confidence = 0.0f;

                if (!this.IsTracked)
                {
                    this.Detected = false;
                    this.BodyColor = Brushes.Gray;
                }
                else
                {
                    this.Detected = isGestureDetected;
                    this.BodyColor = this.trackedColors[this.BodyIndex];

                    if (this.Detected)
                    {
                        Console.WriteLine("left");
                        this.Confidence = detectionConfidence;
                        webView1.EvalScript("callfromcs('A');");
                        gestureState = "left";
                    }
                }
            }

            /// This one is for right gesture detection checking
            public void UpdateGestureResult2(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
            {
                this.IsTracked = isBodyTrackingIdValid;
                this.Confidence = 0.0f;

                if (!this.IsTracked)
                {
                    this.Detected = false;
                    this.BodyColor = Brushes.Gray;
                }
                else
                {
                    this.Detected = isGestureDetected;
                    this.BodyColor = this.trackedColors[this.BodyIndex];

                    if (this.Detected)
                    {
                        Console.WriteLine("right");
                        this.Confidence = detectionConfidence;                     
                        webView1.EvalScript("callfromcs('D');");
                        gestureState = "right";
                    }
                }
            }

            /// This one is for up gesture detection checking
            public void UpdateGestureResult3(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
            {
                this.IsTracked = isBodyTrackingIdValid;
                this.Confidence = 0.0f;

                if (!this.IsTracked)
                {
                    this.Detected = false;
                    this.BodyColor = Brushes.Gray;
                }
                else
                {
                    this.Detected = isGestureDetected;
                    this.BodyColor = this.trackedColors[this.BodyIndex];

                    if (this.Detected)
                    {
                        Console.WriteLine("up");
                        this.Confidence = detectionConfidence;
                        webView1.EvalScript("callfromcs('Q');");
                        gestureState = "up";
                    }
                }
            }

            /// This one is for down gesture detection checking
            public void UpdateGestureResult4(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
            {
                this.IsTracked = isBodyTrackingIdValid;
                this.Confidence = 0.0f;

                if (!this.IsTracked)
                {
                    this.Detected = false;
                    this.BodyColor = Brushes.Gray;
                }
                else
                {
                    this.Detected = isGestureDetected;
                    this.BodyColor = this.trackedColors[this.BodyIndex];

                    if (this.Detected)
                    {
                        Console.WriteLine("down");
                        this.Confidence = detectionConfidence;
                        webView1.EvalScript("callfromcs('E');");
                        gestureState = "down";
                    }
                }
            }

            public void UpdateGestureResult5(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
            {
                this.IsTracked = isBodyTrackingIdValid;
                this.Confidence = 0.0f;

                if (!this.IsTracked)
                {
                    this.Detected = false;
                    this.BodyColor = Brushes.Gray;
                }
                else
                {
                    this.Detected = isGestureDetected;
                    this.BodyColor = this.trackedColors[this.BodyIndex];

                    if (this.Detected)
                    {
                        Console.WriteLine("voice");
                        this.Confidence = detectionConfidence;
                        gestureState = "voice";
                      //  webView1.EvalScript("callfromcs('');");
                    }
                }
            }

        }

        // this is the other big method used inside the main window method used for gesture detction of all the six body we run it one by one to assign body
        // detection for each body through each we call the gesture detection for each gesture for each bosy and check.
        // here we loaoaded our gesture made in visual gesture builder.
        public class GestureDetector
        {
            ///  Path to the gesture database that was trained with VGB 
            private readonly string gestureDatabase = @"Database\static.gbd";

            ///  Name of the discrete gesture in the database that we want to track
            private readonly string leftGestureName = "left";
            private readonly string rightGestureName = "right";
            private readonly string upGestureName = "up";
            private readonly string downGestureName = "down";
            private readonly string voiceGestureName = "voice";

            ///  Gesture frame source which should be tied to a body tracking ID for detection
            private VisualGestureBuilderFrameSource vgbFrameSource = null;

            ///  Gesture frame reader which will handle gesture events coming from the sensor 
            private VisualGestureBuilderFrameReader vgbFrameReader = null;

            /// <summary>
            /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
            /// </summary>
            /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
            /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
            public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
            {
                if (kinectSensor == null)
                {
                    throw new ArgumentNullException("kinectSensor");
                }

                if (gestureResultView == null)
                {
                    throw new ArgumentNullException("gestureResultView");
                }

                this.GestureResultView = gestureResultView;

                // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
                this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
                this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

                // open the reader for the vgb frames
                this.vgbFrameReader = this.vgbFrameSource.OpenReader();
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.IsPaused = true;
                    this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
                }

                // load the gestures from the gesture database
                using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase))
                {
                    vgbFrameSource.AddGestures(database.AvailableGestures);
                }
            }

            ///  Gets the GestureResultView object which stores the detector results for display in the UI 
            public GestureResultView GestureResultView { get; private set; }

            /// Gets or sets the body tracking ID associated with the current detector
            /// The tracking ID can change whenever a body comes in/out of scope
            public ulong TrackingId
            {
                get
                {
                    return this.vgbFrameSource.TrackingId;
                }

                set
                {
                    if (this.vgbFrameSource.TrackingId != value)
                    {
                        this.vgbFrameSource.TrackingId = value;
                    }
                }
            }

            /// Gets or sets a value indicating whether or not the detector is currently paused
            /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
            public bool IsPaused
            {
                get
                {
                    return this.vgbFrameReader.IsPaused;
                }

                set
                {
                    if (this.vgbFrameReader.IsPaused != value)
                    {
                        this.vgbFrameReader.IsPaused = value;
                    }
                }
            }

            /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
            private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
            {
                VisualGestureBuilderFrameReference frameReference = e.FrameReference;
                using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
                {
                    if (frame != null)
                    {
                        // get the discrete gesture results which arrived with the latest frame
                        IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                        if (discreteResults != null)
                        {
                            
                            foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                            {
                                if (gesture.Name.Equals(this.leftGestureName) && gesture.GestureType == GestureType.Discrete)
                                {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        // update the GestureResultView object with new gesture result values
                                        this.GestureResultView.UpdateGestureResult1(true, result.Detected, result.Confidence);
                                    }
                                }
                                if (gesture.Name.Equals(this.rightGestureName) && gesture.GestureType == GestureType.Discrete)
                                {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        // update the GestureResultView object with new gesture result values
                                        this.GestureResultView.UpdateGestureResult2(true, result.Detected, result.Confidence);
                                    }
                                }
                                if (gesture.Name.Equals(this.upGestureName) && gesture.GestureType == GestureType.Discrete)
                                {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        // update the GestureResultView object with new gesture result values
                                        this.GestureResultView.UpdateGestureResult3(true, result.Detected, result.Confidence);
                                    }
                                }
                                if (gesture.Name.Equals(this.downGestureName) && gesture.GestureType == GestureType.Discrete)
                                {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        // update the GestureResultView object with new gesture result values
                                        this.GestureResultView.UpdateGestureResult4(true, result.Detected, result.Confidence);
                                    }
                                }
                                if (gesture.Name.Equals(this.voiceGestureName) && gesture.GestureType == GestureType.Discrete)
                                {
                                    DiscreteGestureResult result = null;
                                    discreteResults.TryGetValue(gesture, out result);

                                    if (result != null)
                                    {
                                        // update the GestureResultView object with new gesture result values
                                        this.GestureResultView.UpdateGestureResult5(true, result.Detected, result.Confidence);
                                    }
                                }
                            }
                        }
                    }
                }
            }

       /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
            private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
            {
                // update the GestureResultView object to show the 'Not Tracked' image in the UI
                this.GestureResultView.UpdateGestureResult1(false, false, 0.0f);
                this.GestureResultView.UpdateGestureResult2(false, false, 0.0f);
                this.GestureResultView.UpdateGestureResult3(false, false, 0.0f);
                this.GestureResultView.UpdateGestureResult4(false, false, 0.0f);
                this.GestureResultView.UpdateGestureResult5(false, false, 0.0f);
            }
       }

    }
}
