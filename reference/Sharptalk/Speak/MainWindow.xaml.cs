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

namespace StevenTTS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

            foreach (var value in Enum.GetValues(typeof(TtsVoice)))
                VoiceSelector.Items.Add(value);
            VoiceSelector.SelectedIndex = 0;
        new Thread(new ThreadStart(() =>
        {



        })).Start();
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
                       Thread.Sleep(1);
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
        private class IdiotMock
        {
            //record audio and keep spamming the um thing
            public void cycle()
            {
                SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
                Grammar dictationGrammar = new DictationGrammar();
                recognizer.LoadGrammar(dictationGrammar);
                try
                {
                    recognizer.SetInputToDefaultAudioDevice();
                    RecognitionResult result = recognizer.Recognize();
                    string res = result.Text;
                    Console.WriteLine("res= " + res);
                }
                catch (InvalidOperationException exception)
                {
                    Console.Error.WriteLine(String.Format("Could not recognize input from default aduio device. Is a microphone or sound card available?\r\n{0} - {1}.", exception.Source, exception.Message));
                }
                finally
                {
                    recognizer.UnloadAllGrammars();
                }

            }

        private Thread idiotMockRecogToMBATTS = null;
    }
}
