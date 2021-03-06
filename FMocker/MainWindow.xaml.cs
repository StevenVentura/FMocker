﻿
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using StevenTTS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

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
            shroud.Value = 1f;
            Go(); 
        }
        public MainWindow()
        {
            InitializeComponent();
            
        }
        Thread automoonbasethread = null;
        private int currentFileIndexer = -1;
        private void VRecogToggle_Checked(object sender, RoutedEventArgs e)
        {
            //
            if ((VRecogToggle.IsChecked == true))
            {
                automoonbasethread = new Thread(new ThreadStart(() =>
                {

                    Thread lastThread = null;
                    while (true)
                    {
                        
                        log("speakers to mic()");
                        
                          currentFileIndexer++;
                          if (currentFileIndexer > 16)
                              currentFileIndexer = 0;
                          SpeakersToMic();


                        if (lastThread != null)
                            lastThread.Join();
                        //log("shitty microsoftboy()");
                        //if (UseShittyMicrosoftAPIToCheckForPresenceOfSound())
                        lastThread = new Thread(new ParameterizedThreadStart((__currentFileIndexer) =>
                        {
                            {
                                log("hack go()");
                                StartSpeechRecogHack((int)__currentFileIndexer);
                            }
                        }));
                        lastThread.Start(currentFileIndexer);
                    }
                }));
                automoonbasethread.Start();
            }
            else
            {
                    automoonbasethread.Abort();
            }
        }

        

        private void SetDeviceToAudio2()
        {
            var enumerator = new MMDeviceEnumerator();
            var defaultrecordingdevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            //https://www.codeproject.com/Articles/31836/Changing-your-Windows-audio-device-programmaticall
            if (!defaultrecordingdevice.FriendlyName.ToLower().Contains("cable output"))
            {

                //switch device LOL
                //ctrl+alt+f7
                keybd_event(VK_LCONTROL, 0, 0, 0);// presses ctrl
                keybd_event(VK_MENU, 0, 0, 0);
                keybd_event(VK_F7, 0, 0, 0);
                keybd_event(VK_LCONTROL, 0, 2, 0);// presses ctrl
                keybd_event(VK_MENU, 0, 2, 0);
                keybd_event(VK_F7, 0, 2, 0);
                Thread.Sleep(2000);
            }
            enumerator.Dispose();



        }
        private FClipper clipper = null;
        private void Go()
        {
            Thread tred = new Thread(new ThreadStart(() =>
            {
                initchromething();
                TextBoxStreamWriter t = new TextBoxStreamWriter(this.Dispatcher, OutputBox);
                clipper = new FClipper(5000);
                clipper.StartRecordingEvery5();
                Console.WriteLine("Recording started.");
            }));
            tred.SetApartmentState(ApartmentState.STA);
            tred.Start();

            
        }
        private static void log(Object o)
        {
            Console.WriteLine(o.ToString());
        }

        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);
        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        public const int VK_LCONTROL = 0xA2;
        public const int VK_MENU = 0x12; //Alt key code
        //https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
        public const int VK_F7 = 0x76;

        //PostMessage(hWnd, WM_LBUTTONDBLCLK, 0, l);
        private void SpeakersToMic()
        {
           
               
        //https://stackoverflow.com/questions/18812224/c-sharp-recording-audio-from-soundcard
        //store the audio ; the past 5 seconds
        using (var capture = new WasapiLoopbackCapture())
        {
                //if necessary, you can choose a device here
                //to do so, simply set the device property of the capture to any MMDevice
                //to choose a device, take a look at the sample here: http://cscore.codeplex.com/


                //initialize the selected device for recording
               
                 
                //Console.WriteLine("tryna make " + "stevenfile" + currentFileIndexer + ".wav");
            //create a wavewriter to write the data to
            using (WaveFileWriter w = new WaveFileWriter("stevenfile" + currentFileIndexer + ".wav", capture.WaveFormat))
            {

                //setup an eventhandler to receive the recorded data. this is fired 10 times per second
                capture.DataAvailable += (s, e) =>
                {
                    //save the recorded audio
                    //Log("e.Data.Length= " + e.Data.Length);//35280
                    w.Write(e.Buffer, 0, e.BytesRecorded);
                                
                };
                    capture.RecordingStopped += (s, e) =>
                    {
                        try
                        {
                            w.Dispose();

                            capture.Dispose();
                        }
                        catch { }
                    };

                    //start recording
                    capture.StartRecording();

                Thread.Sleep(6666);
                    //stop recording
                    capture.StopRecording();
            }
        }
              
            

        }
        private void SetDeviceToAudio1()
        {
            var enumerator = new MMDeviceEnumerator();
            var defaultrecordingdevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            //https://www.codeproject.com/Articles/31836/Changing-your-Windows-audio-device-programmaticall
            log(defaultrecordingdevice.FriendlyName);
            if (defaultrecordingdevice.FriendlyName.ToLower().Contains("cable output"))
            {

                //switch device LOL
                //ctrl+alt+f7
                keybd_event(VK_LCONTROL, 0, 0, 0);// presses ctrl
                keybd_event(VK_MENU, 0, 0, 0);
                keybd_event(VK_F7, 0, 0, 0);
                keybd_event(VK_LCONTROL, 0, 2, 0);// presses ctrl
                keybd_event(VK_MENU, 0, 2, 0);
                keybd_event(VK_F7, 0, 2, 0);
                Thread.Sleep(2000);
            }
            enumerator.Dispose();

        }
        private void initchromething()
        {

            //https://stackoverflow.com/questions/35964955/selenium-c-sharp-interact-with-chrome-microphone-window
            //selenium API
            //https://www.google.com/intl/en/chrome/demos/speech.html
            ChromeOptions chromeOptions = new ChromeOptions();
            //Environment.SetEnvironmentVariable("webdriver.chrome.driver",
            //                "C:\\Users\\Yoloswag\\source\\repos\\FMocker\\packages\\" +
            //                "Selenium.WebDriver.ChromeDriver.2.41.0\\driver\\win32" +
            //                "\\chromedriver.exe");

            //https://peter.sh/experiments/chromium-command-line-switches/
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            chromeOptions.AddArguments("allow-file-access-from-files"
                                       //,"use-fake-device-for-media-stream"
                                       , "use-fake-ui-for-media-stream"
                                       //,"headless"
                                       //"mute-audio"
                                       , "disable-notifications"
            //, "use-file-for-fake-audio-capture='C:\\Users\\Yoloswag\\" +
            //    "source\\repos\\FMocker\\FMocker\\SavedFClips\\" +
            //    "idontlikeplayback.wav'"
            );
            //chromeOptions.addArguments('start-maximized');
            //chromeOptions.addArguments('incognito');
            //chromeOptions.addArguments('headless');
            //chromeOptions.setUserPreferences({'download.default_directory' : '/path/to/your/download/directory'});


            //for (int n = -1; n <  WaveIn.DeviceCount; n++)
            //{
            //    var caps = WaveIn.GetCapabilities(n);
            //    log(caps.ProductName);
            //}

            

            chromeDriver = new ChromeDriver(service, chromeOptions);
            SetDeviceToAudio1();
        }
        private IWebDriver chromeDriver;
        private void StartSpeechRecogHack(int _currentFileIndexer/*yes i am overriding a variable*/)
        {
            
            
            chromeDriver.Navigate().GoToUrl("https://www.google.com/intl/en/chrome/demos/speech.html");
            
            
            var googleHackRecordButton = chromeDriver.FindElement(By.Id("start_img"));
            googleHackRecordButton.Click();

            //playback a file from speaker to CABLE_A(chrome is configured for that)
                using (var audioFile = new AudioFileReader(
                    //"C:\\Users\\Yoloswag\\source\\repos\\FMocker\\FMocker\\SavedFClips\\" +
                    //"idontlikeplayback.wav"
                    "stevenfile" + _currentFileIndexer + ".wav"
                    ))
                {
                    int selDevice = -1;
                    for (int n = -1; n < WaveOut.DeviceCount; n++)
                    {
                        var caps = WaveOut.GetCapabilities(n);
                        if (caps.ProductName.Contains("CABLE-A Input"))
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
                        outputDevice.Volume = 1f;
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }

            log("doneraedingv");
            try
            {
                chromeDriver.FindElement(By.Id("copy_button")).Click();
                log("clicked copybutton for finalizing");
            }
            catch { }
            var googlehackOutputBox = chromeDriver.FindElement(By.Id("final_span"));
            string text = googlehackOutputBox.Text;
            if (text == null || text == "")
                text = chromeDriver.FindElement(By.Id("interim_span")).Text;
            log("text is " + text);
            //now play it back as moonbase alpha kek
            PlayAsMoonbaseAlphaMeme(text);
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
                meme += s + "[n<" + r.Next(800, 800) + "," + r.Next(200, 200) + "]";
            }
            return meme;
        }
        private int fonixFileIndexer = -1;
        private string getFonixFileName()
        {
            return "FonixFile" + fonixFileIndexer + ".wav";
        }
        private void incrementFonixFile()
        {
            fonixFileIndexer++;
            if (fonixFileIndexer > 16)
            {
                fonixFileIndexer = 0;
            }
        }
        private void PlayAsMoonbaseAlphaMeme(string _Hah)
        {
            incrementFonixFile();
            var deepFriedIncrementedString = memetext(_Hah);

            PLEASEffsffff.PLEASE_TalkToWav(getFonixFileName(), deepFriedIncrementedString);
            

            using (var audioFile = new AudioFileReader(getFonixFileName()))
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
        private string randomnamereeee()
        {
            string outboi = "";
            string alphabet = "abcdefghijklmnopqrstuvwxyz";
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                outboi += alphabet.Substring(r.Next(0,alphabet.Length), 1);
            }

            return outboi + ".wav";
        }
        Thread ugh = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            
            switch (button.Name)
            {
                case "StartRecord":
                    {
                        
                        if (clipper.stopbuttonisntpressed == true)
                        {
                            log("its recording already idiot");
                            return;
                        }
                        log("manual recording started.");
                        ugh = new Thread(new ThreadStart(() =>
                        {
                            string randomfilenamepls = randomnamereeee();
                            clipper.StartRecordingSpecial(randomfilenamepls);
                            clipper.recordingSpecialThread.Join();
                            log("manual recording ended. saved to " + randomfilenamepls);
                        }));
                        ugh.Start();
                        
                    }
                    break;
                case "EndRecord":
                    {
                        clipper.StopRecordingSpecial();
                        ugh.Join();
                        FClip clip = clipper.AddRandomCustomFileToList();
                        ListBoxObject.Items.Add(clipper.AddAndMapPath(clip.fileName));
                    }
                    break;
                case "RecordButton":
                    {
                        FClip clip = clipper.AddLastXSecondsToList();
                        ListBoxObject.Items.Add(clipper.AddAndMapPath(clip.fileName));
                        Console.WriteLine(clip.fileName + " added.");
                    }
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
                    try
                    {
                        SetDeviceToAudio2();
                    }
                    catch { }
                    try
                    {
                        chromeDriver.Close();
                    }
                    catch { }
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
