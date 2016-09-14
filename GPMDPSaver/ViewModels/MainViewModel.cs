using GPMDPSaver.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Input;

namespace GPMDPSaver.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private JsonSongReader songReader;

        private ICommand startStopCommand;

        public MainViewModel()
        {
            this.songReader = new JsonSongReader();
            this.songReader.SongFinished += SongReader_SongFinished;
            this.CurrentSong = this.songReader.CurrentSong;
        }

        private string previousInfo;

        private void SongReader_SongFinished(object sender, SongFinishedEventArgs e)
        {
            if(e.SongInfo != null)
            {
                this.PreviousInfo = e.SongInfo.Artist + " - " + e.SongInfo.Title;
            }
        }

        public SongInfo CurrentSong
        {
            get;
            set;
        }

        public ICommand StartStopCommand
        {
            get
            {
                if (this.startStopCommand == null)
                {
                    this.startStopCommand = new DelegateCommand(() => this.ToggleReading());
                }
                return startStopCommand;
            }
        }

        private void ToggleReading()
        {
            if(this.songReader.Reading)
            {
                this.songReader.StopReading();
            }
            else
            {
                this.songReader.StartReading();
            }
            this.OnPropertyChanged(nameof(this.StartStopText));
        }

        public string StartStopText
        {
            get
            {
                if (this.songReader.Reading)
                {
                    return "Stop";
                }
                else
                {
                    return "Start";
                }
            }
        }

        public string PreviousInfo
        {
            get
            {
                return previousInfo;
            }

            set
            {
                previousInfo = value;
                this.OnPropertyChanged(nameof(this.PreviousInfo));
            }
        }
    }
}