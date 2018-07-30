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

        
        public void Button_Click(object o, RoutedEventArgs e)
        {
            Button b = (Button)o;

            switch(b.Name)
            {
                case "GoButton":
                    string text = TextBoxHandle.Text;
                    
                    
                    TextBoxHandle.Text = "";
                    Console.WriteLine(text);
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
                        var x = new SpeakerParams();
                        
                        tts.SpeakerParams = x;
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

    }
}
