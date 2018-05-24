using Prism.Mvvm;
using System;
using System.IO;
using System.Linq;

namespace GPMDPSaver.Models
{
    public class SongInfo : BindableBase
    {
        private string artist;       
        private string title;
        private TimeSpan? currentTime;
        private TimeSpan? totalTime;

        /// <summary>
        /// The artist for the song
        /// </summary>
        public string Artist
        {
            get
            {
                return artist;
            }

            set
            {
                artist = value;
                this.OnPropertyChanged(nameof(this.Artist));
            }
        }

        /// <summary>
        /// The time played/total time for the song
        /// </summary>
        public string TimeDisplay
        {
            get
            {
                if(this.currentTime.HasValue && this.totalTime.HasValue)
                {
                    string current = this.currentTime.Value.ToString(@"m\:ss");
                    string total = this.totalTime.Value.ToString(@"m\:ss");

                    return current + " / " + total;
                }

                return "(None)";

            }                  
          
        }

        /// <summary>
        /// The title of the song
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                this.OnPropertyChanged(nameof(this.Title));
            }
        }

        public TimeSpan? CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                currentTime = value;
                this.OnPropertyChanged(nameof(this.TimeDisplay));
            }
        }

        public TimeSpan? TotalTime
        {
            get
            {
                return totalTime;
            }

            set
            {
                totalTime = value;
                this.OnPropertyChanged(nameof(this.TimeDisplay));
            }
        }

        public string GenerateFileName
        {
            get
            {
                return Path.GetInvalidFileNameChars().Aggregate(this.Artist + " - " + this.Title, (current, c) => current.Replace(c.ToString(), string.Empty));
            }
        }
    }
}