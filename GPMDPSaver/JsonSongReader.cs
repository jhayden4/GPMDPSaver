using GPMDPSaver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace GPMDPSaver
{
    public class JsonSongReader
    {
        private bool reading;
        private static string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Google Play Music Desktop Player\json_store\playback.json");
        private FileSystemWatcher fileWatcher;

        public event SongActionHandler SongAction;

        public delegate void SongActionHandler(object sender, SongActionEventArgs e);


        public JsonSongReader()
        {
            this.CurrentSong = new SongInfo()
            {
                Artist = "(None)",               
                Title = "(None)"
            };

            this.fileWatcher = new FileSystemWatcher();

            if (!File.Exists(filePath))
            {
                throw new Exception("Google Play Music Desktop Player has not writen to the playback file");
            }

            fileWatcher.Path = Path.GetDirectoryName(filePath);
            fileWatcher.Filter = Path.GetFileName(filePath);

            fileWatcher.Changed += FileWatcher_Changed;
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string fileText = string.Empty;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    fileText = sr.ReadToEnd();
                }
            }

            if (!string.IsNullOrWhiteSpace(fileText))
            {
                JObject obj = null;

                try
                {
                    obj = JObject.Parse(fileText);
                }
                catch(Exception ex)
                {
                    // There was an error parsing the file so just continue on;
                    // TODO: Add logging
                }

                if (obj != null)
                {
                    //// Make sure the song is playing
                    //if ((bool)obj["playing"])
                    //{
                        string artist = (string)obj["song"]["artist"];
                        string title = (string)obj["song"]["title"];
                        int currentTime = (int)obj["time"]["current"];
                        int totalTime = (int)obj["time"]["total"];                      

                        if(currentTime == 0 && artist != this.CurrentSong.Artist && title != this.CurrentSong.Title)
                        {
                            SongInfo startedSong = new SongInfo()
                            {
                                Artist = artist,
                                Title = title
                            };

                            this.OnSongAction(startedSong, GPMDPSaver.Models.SongAction.Start);
                        }
                        else if (currentTime == totalTime)
                        {
                            SongInfo finishedSong = new SongInfo()
                            {
                                Artist = artist,
                                Title = title
                            };

                            this.OnSongAction(finishedSong, GPMDPSaver.Models.SongAction.Finish);
                        }

                        this.CurrentSong.Artist = artist;
                        this.CurrentSong.Title = title;
                      this.CurrentSong.CurrentTime = new TimeSpan(0, 0, 0, 0, currentTime);
                        this.CurrentSong.TotalTime = new TimeSpan(0, 0, 0, 0,totalTime);
                    //}
                }
            }
        }

        protected virtual void OnSongAction(SongInfo songInfo, SongAction action)
        {
            if(this.SongAction != null)
            {
                this.SongAction.Invoke(this, new SongActionEventArgs() { SongInfo = songInfo, Action = action });
            }
        }

        public SongInfo CurrentSong
        {
            get;
            set;
        }

        public bool Reading
        {
            get
            {
                return reading;
            }

            private set
            {
                reading = value;
            }
        }

        public void StartReading()
        {
            this.Reading = true;
            this.fileWatcher.EnableRaisingEvents = true;
        }

        public void StopReading()
        {
            this.Reading = false;
            this.fileWatcher.EnableRaisingEvents = false;
        }
    }

    

    
}
