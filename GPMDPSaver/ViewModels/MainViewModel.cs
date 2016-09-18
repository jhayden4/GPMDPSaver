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
        private WebSocketSongReader songReader;
        private SongRecorder songRecorder;

        private ICommand startStopCommand;

        public MainViewModel()
        {
            this.songReader = new WebSocketSongReader();
            this.songReader.SongChange += SongReader_SongChange;
            this.CurrentSong = this.songReader.CurrentSong;
            this.Log = new ObservableCollection<string>();
            this.StartStopText = "Start";
            this.songRecorder = new SongRecorder(@"C:\Test");
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

        private string startStopText;

        public string StartStopText
        {
            get
            {
                return this.startStopText;
            }
            set
            {
                this.startStopText = value;
                this.OnPropertyChanged(nameof(this.StartStopText));
            }
        }

        private void SongReader_SongChange(object sender, SongChangeEventArgs e)
        {
           
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() =>
                   {
                       if (e.OldSongInfo != null && !string.IsNullOrWhiteSpace(e.OldSongInfo.Artist) && !string.IsNullOrWhiteSpace(e.OldSongInfo.Title))
                       {                           
                           this.songRecorder.FinishSongRecording();
                           this.AddLogText(e.OldSongInfo.Artist + " - " + e.OldSongInfo.Title + " Finished");
                       }

                       if (e.NewSongInfo != null && !string.IsNullOrWhiteSpace(e.NewSongInfo.Artist) && !string.IsNullOrWhiteSpace(e.NewSongInfo.Title))
                       {
                           this.songRecorder.StartSongRecording(e.NewSongInfo);
                           this.AddLogText(e.NewSongInfo.Artist + " - " + e.NewSongInfo.Title + " Started");
                       }                            
                       
                   }));


            
        }

        private void ToggleReading()
        {
            if (this.songReader.Reading)
            {
                this.StartStopText = "Stopping...";
                this.songReader.StopReading();
                this.songRecorder.FinishSongRecording();
                this.StartStopText = "Start";
            }
            else
            {
                this.StartStopText = "Starting...";
                this.songReader.StartReading();
                this.StartStopText = "Stop";
            }
           
        }

        private void AddLogText(string text)
        {
            this.log.Add(DateTime.Now.ToLongTimeString() + " - " + text);
        }
    }
}