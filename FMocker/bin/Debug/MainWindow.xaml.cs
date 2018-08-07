using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Speech;
using System.Speech.Synthesis;
using SharpTalk;
using System.Speech.Recognition;
using CSCore.CoreAudioAPI;
using CSCore.Codecs.WAV;
using System.IO;

namespace StevenTTS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public static class PLEASEffsffff
    {
        public static void PLEASE_TalkToWav(string fileName, string text)
        {
            using (var tts = new FonixTalkEngine())
            {

                // tts.Voice = (TtsVoice)Enum.Parse(typeof(TtsVoice), VoiceSelector.Text);


                tts.SpeakToWavFile(fileName, text);
            }
        }
    }
    public partial class MainWindow : Window
        
    {
        
        public MainWindow()
        {
           
            InitializeComponent();
            
                begin();
           
        }

        private void begin()
        { 
        TextBoxStreamWriter t = new TextBoxStreamWriter(this.Dispatcher, OutputBoxHandle);
            int index = 0;
            while (File.Exists("dump" + ++index + ".wav"))
            {
                File.Delete("dump" + index + ".wav");
            }

            foreach (var value in Enum.GetValues(typeof(TtsVoice)))
                VoiceSelector.Items.Add(value);
            VoiceSelector.SelectedIndex = 0;
        new Thread(new ThreadStart(() =>
        {
            //start the voice saver
            //new SpeechInputFromSpeakerSaver().begindumpingfiles();

        })).Start();
        }
        class SpeechInputFromSpeakerSaver
        {
            public void begindumpingfiles()
            {
                uint filecounter = 0;
                while (true)
                {
                    using (var capture = new CSCore.SoundIn.WasapiLoopbackCapture())
                    {
                    //if necessary, you can choose a device here
                    //to do so, simply set the device property of the capture to any MMDevice
                    //to choose a device, take a look at the sample here: http://cscore.codeplex.com/
                    
                        //initialize the selected device for recording
                        capture.Initialize();
                        //create a wavewriter to write the data to
                        filecounter++;
                        if (filecounter > 100)
                            filecounter = 0;
                        using (WaveWriter w = new WaveWriter("dump" + filecounter + ".wav", capture.WaveFormat))
                        {

                            //setup an eventhandler to receive the recorded data. this is fired 10 times per second
                            capture.DataAvailable += (s, e) =>
                            {
                            //save the recorded audio
                            //Log("e.Data.Length= " + e.Data.Length);//35280
                            w.Write(e.Data, e.Offset, e.ByteCount);
                            //shifter.Add(e.Data, e.Offset, e.ByteCount);
                        };

                            //start recording
                            capture.Start();
                            //record for 5 seconds
                            Thread.Sleep(5000);
                            //stop recording
                            capture.Stop();
                        }
                    }
                }


            }



        }

        private string memetext(string boring)
        {
            string meme = "";
            //   :nv
            // [:nv] nick[k < 1, 90 >]is[s<1,40>] an[n < 800, 200 >] nibba[r < 900, 400 >]
            //[:nh] HEY[k < 1, 230 >] BEAR[s < 1, 230 >]MAYBE[s < 1, 80 >] DROP[s < 1, 110 >]WITHIN[s < 1, 140 >] FIVE[s < 1, 170 >]MILES[s < 1, 2000 >] OF[s < 1, 230 >]uh[s < 1, 270 >]
            meme = "[:nh]";
            string[] split = boring.Split(' ');
            Random r = new Random();
            foreach (var s in split)
            {
                meme += s + "[n<" + r.Next(800,800) + "," + r.Next(200,200) + "]";
            }
            return meme;
        }
        
        public void Button_Click(object o, RoutedEventArgs e)
        {
            Button b = (Button)o;

            switch(b.Name)
            {
                case "GoButton":
                    string text = TextBoxHandle.Text;
                    
                    
                    TextBoxHandle.Text = "";
                    Console.WriteLine(text);
                    if ((automeme.IsChecked == true))
                    {
                        text = memetext(text);
                    }
                    //save as a sound file
                    const string fileName = "abusedFile.wav";
                    /*using (var reader = new SpeechSynthesizer())
                    {
                        foreach (var x in reader.GetInstalledVoices())
                            Console.WriteLine(x.VoiceInfo.Name);
                        reader.Rate = (int)-2;
                        reader.SetOutputToWaveFile(fileName);
                        //https://stackoverflow.com/questions/16021302/c-sharp-save-text-to-speech-to-mp3-file
                        reader.Speak(text);
                    }*/

                    using (var tts = new FonixTalkEngine())
                    {
                       
                        tts.Voice = (TtsVoice)Enum.Parse(typeof(TtsVoice),VoiceSelector.Text);
                        
                        
                        tts.SpeakToWavFile(fileName, text);
                    }
                    


                    //////////
                    //now output from the sound file
                    using (var audioFile = new AudioFileReader(fileName))
                    {
                        int selDevice = -1;
                        for (int n = -1; n < WaveOut.DeviceCount; n++)
                        {
                            var caps = WaveOut.GetCapabilities(n);
                            if (caps.ProductName.Contains("CABLE Input"))
                            {
                                selDevice = n;
                                break;
                            }
                        }
                        using (var outputDevice = new WaveOutEvent()
                        {
                            DeviceNumber = selDevice
                        })
                        {
                            outputDevice.Init(audioFile);
                            outputDevice.Volume = (float)percentagevolume;
                            outputDevice.Play();
                            while (outputDevice.PlaybackState == PlaybackState.Playing)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }



                    break;
                case "CloseButton":
                    Environment.Exit(0);
                    break;
            }

        }
        double percentagevolume;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            percentagevolume = shroud.Value / 10.0;
        }

        private void VRecogToggle_Checked(object sender, RoutedEventArgs e)
        {
            //
            if ((VRecogToggle.IsChecked == true))
            {
                idiotMockRecogToMBATTS = new Thread(new ThreadStart(() =>
               {
                   Console.WriteLine("idiot mock started.");
                   IdiotMock moron = new IdiotMock();
                   
                   while(true)
                   {
                       //Thread.Sleep(100);
                       moron.cycle();
                   }
                   //listen to voices

               }));
                idiotMockRecogToMBATTS.Start();
            }
            else
            {
                try
                {
                    idiotMockRecogToMBATTS.Abort();
                }
                catch (Exception) { }
            }
        }
        private Thread idiotMockRecogToMBATTS = null;
        private class IdiotMock
        {
            private int localcounter = -1;   
            private void recognizer_SpeechRecognized(SpeechRecognizedEventArgs e)
            {


            }
            //record audio and keep spamming the um thing
            public void cycle()
            {
                
                SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(
                    new System.Globalization.CultureInfo("en-US"));
                
                
                //recognizer.LoadGrammar(new DictationGrammar());
                //while(!File.Exists("dump" + localcounter + ".wav"))
                //{
                //    localcounter++;
                //    if (localcounter > 100)
                //        localcounter = 0;
                //}
                //Console.WriteLine("detecetd file " + "dump" + localcounter + ".wav");

                //Thread.Sleep(5000);
                recognizer.SetInputToDefaultAudioDevice();
                //recognizer.SetInputToWaveFile("dump" + localcounter + ".wav");
                //recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
                RecognitionResult result = recognizer.Recognize();
                try
                {
                    File.Delete("dump" + localcounter + ".wav");
                }
                catch (Exception) { }
                localcounter++;
                string res = result.Text;
                Console.WriteLine("res= " + res);

            }

        }
    }
    class SpeechStreamer : Stream
    {
        private AutoResetEvent _writeEvent;
        private List<byte> _buffer;
        private int _buffersize;
        private int _readposition;
        private int _writeposition;
        private bool _reset;

        public SpeechStreamer(int bufferSize)
        {
            _writeEvent = new AutoResetEvent(false);
            _buffersize = bufferSize;
            _buffer = new List<byte>(_buffersize);
            for (int i = 0; i < _buffersize; i++)
                _buffer.Add(new byte());
            _readposition = 0;
            _writeposition = 0;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return -1L; }
        }

        public override long Position
        {
            get { return 0L; }
            set { }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0L;
        }

        public override void SetLength(long value)
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int i = 0;
            while (i < count && _writeEvent != null)
            {
                if (!_reset && _readposition >= _writeposition)
                {
                    _writeEvent.WaitOne(100, true);
                    continue;
                }
                buffer[i] = _buffer[_readposition + offset];
                _readposition++;
                if (_readposition == _buffersize)
                {
                    _readposition = 0;
                    _reset = false;
                }
                i++;
            }

            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
            {
                _buffer[_writeposition] = buffer[i];
                _writeposition++;
                if (_writeposition == _buffersize)
                {
                    _writeposition = 0;
                    _reset = true;
                }
            }
            _writeEvent.Set();

        }

        public override void Close()
        {
            _writeEvent.Close();
            _writeEvent = null;
            base.Close();
        }

        public override void Flush()
        {

        }
    }
}
