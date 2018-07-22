using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace FMocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Go();
        }
        private FClipper clipper = null;
        private void Go()
        {
            new Thread(new ThreadStart(() =>
            {
                TextBoxStreamWriter t = new TextBoxStreamWriter(this.Dispatcher,OutputBox);
                clipper = new FClipper(5000);
                clipper.StartRecording();

            })).Start();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "RecordButton":
                    FClip clip = clipper.AddLastXSecondsToList();
                    ListBoxObject.Items.Add(clip.fileName);
                    Console.WriteLine(clip.fileName + " added.");
                    break;
                case "SaveButton":
                    if (((string)(ListBoxObject.SelectedValue)) == null)
                    {
                        Console.WriteLine("Nothing selected");
                        return;
                    }
                    break;
                case "ListenButton":
                    {
                        if (((string)(ListBoxObject.SelectedValue)) == null)
                        {
                            Console.WriteLine("Nothing selected");
                            return;
                        }
                        string SelectedName = (string)(ListBoxObject.SelectedValue);
                        Console.WriteLine("Listening to " + SelectedName);
                        SoundPlayer simpleSound = new SoundPlayer(clipper.GetPath(SelectedName));
                        simpleSound.Play();
                    }
                    break;
                case "PlayButton":
                    {
                        try
                        {
                            if (((string)(ListBoxObject.SelectedValue)) == null)
                            {

                                Console.WriteLine("Nothing selected");
                                return;
                            }
                            string SelectedName = (string)(ListBoxObject.SelectedValue);
                            Console.WriteLine("Playing " + SelectedName);
                            Thread thread = new Thread(new ParameterizedThreadStart((__SelectedName) =>
                            {
                                string _SelectedName = (string)__SelectedName;
                                //https://github.com/naudio/NAudio
                                try
                                {
                                    using (var audioFile = new AudioFileReader(clipper.GetPath(_SelectedName)))
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
                                }
                                catch (System.Runtime.InteropServices.COMException e3)
                                { Console.WriteLine(e3); }
                            }));
                            thread.Start(SelectedName);
                            KillableThreads.Add(thread);
                        }catch(Exception e2)
                        {
                            Console.WriteLine(e2.StackTrace);
                        }
                    }
                    break;
                case "KillButton":
                    for (int i = 0; i < KillableThreads.Count; i++)
                    {
                        KillableThreads[i].Abort();
                    }
                    KillableThreads.Clear();
                    break;
                case "CloseButton":
                    Environment.Exit(0);
                    break;
            }

        }
        private List<Thread> KillableThreads = new List<Thread>();



       
    }
}
