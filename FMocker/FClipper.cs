using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.SoundIn;
using CSCore.Codecs.WAV;
using System.Threading;
using System.IO;

namespace FMocker
{
    class FClipper
    {
        const string SaveDirectory = "..\\..\\SavedFClips\\";
        //Record() puts the last 5 seconds of audio into an FClip object
        
        private void Log(string _LogMe)
        {
            Console.WriteLine(_LogMe);
        }
        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long GetCurrentTime()
        {
            return (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
        }

        private class ShiftKeeperArray
        {
            /*
             * [                   ]
             *   ^       
             * [                   ]
             *              ^
             *              
             * make an array that is at least 5 seconds long
             * as new data comes, write it to the new position in the array, sweeping from left to right and then 
             *  restarting at the left again.
             * 
             */

            //taken from https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/unsafe-code-pointers/how-to-use-pointers-to-copy-an-array-of-bytes
            static unsafe void Copy(byte[] source, int sourceOffset, byte[] target,
            int targetOffset, int count)
            {
                // If either array is not instantiated, you cannot complete the copy.
                if ((source == null) || (target == null))
                {
                    throw new System.ArgumentException();
                }

                // If either offset, or the number of bytes to copy, is negative, you
                // cannot complete the copy.
                if ((sourceOffset < 0) || (targetOffset < 0) || (count < 0))
                {
                    throw new System.ArgumentException();
                }

                // If the number of bytes from the offset to the end of the array is 
                // less than the number of bytes you want to copy, you cannot complete
                // the copy. 
                if ((source.Length - sourceOffset < count) ||
                    (target.Length - targetOffset < count))
                {
                    //Console.WriteLine("count=" + count);
                    //Console.WriteLine("firstone="+(source.Length - sourceOffset));
                    //Console.WriteLine("secondone="+(target.Length - targetOffset));
                    //Console.WriteLine(Environment.StackTrace);
                    return;
                    //throw new System.ArgumentException();
                }

                // The following fixed statement pins the location of the source and
                // target objects in memory so that they will not be moved by garbage
                // collection.
                fixed (byte* pSource = source, pTarget = target)
                {
                    // Copy the specified number of bytes from source to target.
                    for (int i = 0; i < count; i++)
                    {
                        pTarget[targetOffset + i] = pSource[sourceOffset + i];
                    }
                }
            }
            public int sweepIndex = 0;

            byte[] sweepdata;

            public ShiftKeeperArray(double ApproxTotalIntervalMillis, int BytesPerSecond)
            {
                sweepdata = new byte[BytesPerSecond * (long)(ApproxTotalIntervalMillis / 1000L)];
                sweepIndex = 0;
            }
            public void Add(byte[] Data,int Offset,int ByteCount)
            {
                int numAdded = ByteCount - Offset;
                //i  think Offset is unused
                Copy(Data, 0, sweepdata, sweepIndex, numAdded);
                sweepIndex += numAdded;
                if (sweepIndex >= sweepdata.Length)
                {
                    sweepIndex = 0;
                }
            }

            public byte[] GetDataInCorrectOrder()
            {
                byte[] correctOrderArray = new byte[sweepdata.Length];

                //get the 2nd part
                Copy(sweepdata, sweepIndex, correctOrderArray, 0, sweepdata.Length - sweepIndex);
                //get the 1st part
                Copy(sweepdata, 0, correctOrderArray, sweepdata.Length - sweepIndex, sweepIndex);


                return correctOrderArray;
            }
            


        }
        private WasapiCapture capture = null;
        private ShiftKeeperArray shifter = null;
        //only run once; thread to actually keep the last5 seconds of sound in memory
        public void StartRecording()
        {
            new Thread(new ThreadStart(() =>
            {

                //https://stackoverflow.com/questions/18812224/c-sharp-recording-audio-from-soundcard
                //store the audio ; the past 5 seconds
                using (capture = new WasapiLoopbackCapture())
                {
                    //if necessary, you can choose a device here
                    //to do so, simply set the device property of the capture to any MMDevice
                    //to choose a device, take a look at the sample here: http://cscore.codeplex.com/

                    //initialize the selected device for recording
                    capture.Initialize();
                    shifter = new ShiftKeeperArray(TimeInterval, capture.WaveFormat.BytesPerSecond);
                    //create a wavewriter to write the data to
                    //using (WaveWriter w = new WaveWriter(SaveDirectory + "dump.wav", capture.WaveFormat))
                    {
                        
                        //setup an eventhandler to receive the recorded data. this is fired 10 times per second
                        capture.DataAvailable += (s, e) =>
                        {
                            //save the recorded audio
                            //Log("e.Data.Length= " + e.Data.Length);//35280
                            //w.Write(e.Data, e.Offset, e.ByteCount);
                            shifter.Add(e.Data, e.Offset, e.ByteCount);
                        };

                        //start recording
                        capture.Start();

                        bool always = true;
                        while (always) ;
                        //stop recording
                        capture.Stop();
                    }
                }
            })).Start();

        }


        //plays the selected sound back through the speaker
        public void Listen(FClip clip)
        {


        }
        long TimeInterval;
        public FClipper(long TimeInterval)
        {
            this.TimeInterval = TimeInterval;
        }
        private bool IsFromExternalFile(string input)
        {
            return input.Contains(":");
        }
        public string Rename(string oldPath, string newPath, bool colonStart, string nameWithoutColon)
        {
            if (colonStart == false)
            {
                LocalDirList.Remove(oldPath.Substring(oldPath.LastIndexOf('\\') + 1));
                LocalDirList.Add(nameWithoutColon);
                return nameWithoutColon;
            }

            string oldShortName = String.Copy(Reverse_MappingOfShortenedNamesToDirectories[oldPath]);
            String newShortName = String.Copy(nameWithoutColon);

            if (colonStart)
                newShortName = ":" + newShortName;

            MappingOfShortenedNamesToDirectories.Remove(oldShortName);
            MappingOfShortenedNamesToDirectories.Add(newShortName, newPath);




            Reverse_MappingOfShortenedNamesToDirectories.Remove(oldPath);
            Reverse_MappingOfShortenedNamesToDirectories.Add(newPath, newShortName);

            return newShortName;

        }
        public string AddAndMapPath(string input)
        {
            if (IsFromExternalFile(input))
            {
                string newName = ":" + input.Substring(input.LastIndexOf('\\') + 1);
                try
                {
                    MappingOfShortenedNamesToDirectories.Add(newName, input);
                    Reverse_MappingOfShortenedNamesToDirectories.Add(input, newName);
                }
                catch (Exception)
                {
                    //catch if it's already been added to the map
                    return null;
                }
                return newName;
            }
            else
            {
                LocalDirList.Add(input);
                return input;
            }
        }

        public List<string> LocalDirList = new List<string>();
        public Dictionary<string, string> MappingOfShortenedNamesToDirectories = new Dictionary<string, string>();
        public Dictionary<string, string> Reverse_MappingOfShortenedNamesToDirectories = new Dictionary<string, string>();

        public string GetPath(string itemName)
        {
            if (IsFromExternalFile(itemName))
            {
                return MappingOfShortenedNamesToDirectories[itemName];
            }
            else return System.IO.Path.GetFullPath(
                                System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                                        SaveDirectory + itemName));
        }

        public List<FClip> clips = new List<FClip>();

        public FClip AddLastXSecondsToList()
        {
            //so easy just take last5 seconds of recording and put into an FClip object and push to tree
            string fileName = ""+GetCurrentTime() + ".wav";
            byte[] byteDataInCorrectOrder;
            using (WaveWriter w = new WaveWriter(SaveDirectory + fileName, capture.WaveFormat))
            {
                byteDataInCorrectOrder = shifter.GetDataInCorrectOrder();
                w.Write(byteDataInCorrectOrder, 0, byteDataInCorrectOrder.Length);
            }

            FClip clip = new FClip(byteDataInCorrectOrder, fileName);
            clips.Add(clip);
            return clip;

        }

        //plays sound thru mic
        public void PlayThroughMic(FClip clip)
        {
            //idk how yet
        }


        public void SaveToFile(FClip clip)
        {
            //selected by UI


        }
        
    }
}
