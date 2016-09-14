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

        public event SongFinishedHandler SongFinished;

        public delegate void SongFinishedHandler(object sender, SongFinishedEventArgs e);


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
                    this.CurrentSong.Artist = (string)obj["song"]["artist"];
                    this.CurrentSong.Title = (string)obj["song"]["title"];
                    this.CurrentSong.CurrentTime = new TimeSpan(0, 0, 0, 0, (int)obj["time"]["current"]);
                    this.CurrentSong.TotalTime = new TimeSpan(0, 0, 0, 0, (int)obj["time"]["total"]);

                    if(this.CurrentSong.CurrentTime == this.CurrentSong.TotalTime)
                    {
                        SongInfo finishedSong = new SongInfo()
                        {
                            Artist = this.CurrentSong.Artist,
                            Title = this.CurrentSong.Title
                        };

                        this.OnSongFinished(finishedSong);
                    }
                }
            }
        }

        protected virtual void OnSongFinished(SongInfo songInfo)
        {
            if(this.SongFinished != null)
            {
                this.SongFinished.Invoke(this, new SongFinishedEventArgs() { SongInfo = songInfo });
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

    public class SongFinishedEventArgs : EventArgs
    {
        public SongInfo SongInfo
        {
            get;
            set;
        }
    }
}
