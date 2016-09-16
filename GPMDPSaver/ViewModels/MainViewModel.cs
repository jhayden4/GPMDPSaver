using GPMDPSaver.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GPMDPSaver.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private ObservableCollection<string> log;
        private string previousInfo;
        private WebSocketSongReader songReader;

        private ICommand startStopCommand;

        public MainViewModel()
        {
            this.songReader = new WebSocketSongReader();
            this.songReader.SongAction += SongReader_SongAction;
            this.CurrentSong = this.songReader.CurrentSong;
            this.Log = new ObservableCollection<string>();
        }

        public SongInfo CurrentSong
        {
            get;
            set;
        }

        public ObservableCollection<string> Log
        {
            get
            {
                return log;
            }

            set
            {
                log = value;
            }
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

        private void SongReader_SongAction(object sender, SongActionEventArgs e)
        {
            if (e.SongInfo != null)
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() =>
                   {
                       if (e.Action == SongAction.Start)
                       {
                           log.Add(e.SongInfo.Artist + " - " + e.SongInfo.Title + " Started");
                       }
                       else if (e.Action == SongAction.Finish)
                       {
                           log.Add(e.SongInfo.Artist + " - " + e.SongInfo.Title + " Finished");
                       }
                   }));


            }
        }

        private void ToggleReading()
        {
            if (this.songReader.Reading)
            {
                this.songReader.StopReading();
            }
            else
            {
                this.songReader.StartReading();
            }
            this.OnPropertyChanged(nameof(this.StartStopText));
        }
    }
}