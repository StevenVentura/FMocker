using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        //https://stackoverflow.com/questions/743906/how-to-hide-close-button-in-wpf-window
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void OnThisWindowWasLoaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            Go(); 
        }
        public MainWindow()
        {
            InitializeComponent();
            
        }
        bool isDown = false;
        private FClipper clipper = null;
        private void Go()
        {
            
           

            
            Thread tred = new Thread(new ThreadStart(() =>
            {
                TextBoxStreamWriter t = new TextBoxStreamWriter(this.Dispatcher, OutputBox);
                clipper = new FClipper(5000);
                clipper.StartRecording();
                Console.WriteLine("Recording started.");
                
            }));
            tred.SetApartmentState(ApartmentState.STA);
            tred.Start();

            /*while (true)
            {
                Thread.Sleep(10);
                if (Keyboard.IsKeyDown(Key.OemPlus) && !isDown)
                {
                    isDown = true;
                    Button_Click(PlayButton, null);
                }
                else
                {
                    isDown = false;
                }
            }*/
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "RecordButton":
                    FClip clip = clipper.AddLastXSecondsToList();
                    ListBoxObject.Items.Add(clipper.AddAndMapPath(clip.fileName));
                    Console.WriteLine(clip.fileName + " added.");
                    
                    
                    break;
                case "SaveButton":
                    if (((string)(ListBoxObject.SelectedValue)) == null)
                    {
                        Console.WriteLine("Nothing selected");
                        return;
                    }
                    for (int n = -1; n < WaveOut.DeviceCount; n++)
                    {
                        var caps = WaveOut.GetCapabilities(n);
                        Console.WriteLine(caps.ProductName);
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
                                        if (caps.ProductName.Contains("Headset Earphone"))
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
                            }
                            catch (System.Runtime.InteropServices.COMException e3)
                            { Console.WriteLine(e3); }
                        }));
                        thread.Start(SelectedName);

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
                                            outputDevice.Volume = (float)percentagevolume;
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
                                catch (Exception e4)
                                {
                                    Console.WriteLine(e4);
                                }
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
                case "LoadButton":
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Multiselect = true;
                    ofd.DefaultExt = ".wav";
                    ofd.Filter = "soundfiles (.wav)|*.wav";
                   
                    if (ofd.ShowDialog() == true)
                    {
                        string[] fileNames = new string[ofd.FileNames.Length];
                        string[] pathNames = new string[ofd.FileNames.Length];
                        for (int i = 0; i < fileNames.Length; i++)
                        {
                            fileNames[i] = System.IO.Path.GetFileName(ofd.FileNames[i]);
                            pathNames[i] = System.IO.Path.GetDirectoryName(ofd.FileName);
                            ListBoxObject.Items.Add(clipper.AddAndMapPath(pathNames[i] + "\\" + fileNames[i]));
                            Console.WriteLine("Loaded " + pathNames[i] + "\\" + fileNames[i] + ".");
                        }


                        


                    }
                    break;
                case "RenameButton":
                    {
                        if (((string)(ListBoxObject.SelectedValue)) == null)
                        {

                            Console.WriteLine("Nothing selected");
                            return;
                        }
                        string SelectedName = (string)(ListBoxObject.SelectedValue);

                        RetardBox.Visibility = Visibility.Visible;

                        RenamePath = clipper.GetPath(SelectedName);
                        RenameShortName = String.Copy(SelectedName);

                        string dispName = "";
                        if (SelectedName.StartsWith(":") && SelectedName.Length != 1)
                        {
                            dispName = SelectedName.Substring(1);
                        }
                        else
                            dispName = SelectedName;
                        

                        NameInputHandle.Text = dispName;
                       
                    }
                    break;
                case "YesButton":
                    
                    string textToFix = NameInputHandle.Text;
                    //remove extension from text if there is one
                    if (textToFix.Contains("."))
                    {
                        textToFix = textToFix.Substring(0, textToFix.IndexOf('.'));
                    }
                    
                    //save the old extension
                    string correctExtensionPlusDot = RenamePath.Substring(RenamePath.IndexOf('.'));
                    if (textToFix.StartsWith(":"))
                        return;

                    //append the old extension to the new text
                    string renamedPath = RenamePath.Substring(0, RenamePath.LastIndexOf("\\"))
                                        + "\\" + textToFix + correctExtensionPlusDot;
                    try
                    {
                        System.IO.File.Move(RenamePath, renamedPath);

                        //now fix ui to reflect changes
                        ListBoxObject.Items[ListBoxObject.SelectedIndex] = clipper.Rename(RenamePath, renamedPath,
                                                                        RenameShortName.StartsWith(":"), textToFix + correctExtensionPlusDot);

                        
                    }
                    catch(Exception ere3)
                    {
                        Console.Error.WriteLine("crashmessage=" + ere3.Message + "\r\n" + ere3.StackTrace);
                    }
                    RetardBox.Visibility = Visibility.Hidden;

                    break;
                case "NoButton":
                    RetardBox.Visibility = Visibility.Hidden;
                    break;
            }

        }
        private string RenamePath;
        private string RenameShortName;
        

        private List<Thread> KillableThreads = new List<Thread>();

        public void YesButton_Click(Object sender, EventArgs r)
        {

        }
        public void NoButton_Click(Object sender, EventArgs r)
        {

        }
        double percentagevolume;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            percentagevolume = shroud.Value / 10.0;
        }

    }
}
