using GPMDPSaver.Models;
using NLog;
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
        private string directory;
        private bool running;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ICommand startStopCommand;

        public MainViewModel()
        {
            logger.Debug("Program started");
            this.Directory = Properties.Settings.Default.Directory;
            this.songReader = new WebSocketSongReader();
            this.songReader.SongChange += SongReader_SongChange;
            this.CurrentSong = this.songReader.CurrentSong;
            this.Log = new ObservableCollection<string>();
            this.StartStopText = "Start";
            this.songRecorder = new SongRecorder(this.Directory);
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

        public string Directory
        {
            get
            {
                return directory;
            }

            set
            {
                directory = value;
                if (this.songRecorder != null)
                {
                    this.songRecorder.Directory = directory;
                }
                Properties.Settings.Default.Directory = directory;
                Properties.Settings.Default.Save();
                this.OnPropertyChanged(nameof(this.Directory));
            }
        }

        public bool Running
        {
            get
            {
                return running;
            }

            set
            {
                running = value;
                this.OnPropertyChanged(nameof(this.Running));
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
                this.Running = false;
            }
            else
            {
                this.StartStopText = "Starting...";
                this.songReader.StartReading();
                this.StartStopText = "Stop";
                this.Running = true;
            }
           
        }

        private void AddLogText(string text)
        {
            this.log.Add(DateTime.Now.ToLongTimeString() + " - " + text);
        }
    }
}