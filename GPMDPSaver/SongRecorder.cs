using GPMDPSaver.Models;
using NAudio.Wave;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPMDPSaver
{
    public class SongRecorder : IDisposable
    {
        private SongInfo currentSong;
        private SongInfo previousSong;
        private string directory;
        private WaveFileWriter currentFileWriter;
        private WaveFileWriter previousFileWriter;
        private WasapiLoopbackCapture recorder;
        private bool recording;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private bool finishing = false;

        public SongRecorder(string directory)
        {
            this.Directory = directory;
            this.recorder = new WasapiLoopbackCapture();
            this.recorder.DataAvailable += Recorder_DataAvailable;
            this.recorder.RecordingStopped += Recorder_RecordingStopped;
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
            }
        }

        public void StopSongRecording()
        {
            Task.Run(() =>
                {
                    this.recorder.StopRecording();

                    if (this.currentFileWriter != null)
                    {
                        this.currentFileWriter.Close();
                        this.currentFileWriter = null;
                    }
                    if(this.previousFileWriter != null)
                    {
                        this.previousFileWriter.Close();
                        this.previousFileWriter = null;
                    }
                });
        }

        public void Dispose()
        {
            if (this.currentFileWriter != null)
            {
                this.currentFileWriter.Dispose();
            }
            if (this.recorder != null)
            {
                this.recorder.Dispose();
            }
        }

        public void FinishSongRecording()
        {
            logger.Debug("FinishSongRecording called");

            if (this.currentSong != null)
            {
                this.finishing = true;

                this.previousFileWriter = this.currentFileWriter;
                this.currentFileWriter = null;

                this.previousSong = this.currentSong;
                this.currentSong = null;

                this.finishing = false;

                logger.Debug("ConvertToMp3() called");
                this.ConvertToMp3(this.previousFileWriter.Filename, this.previousSong);

                this.previousFileWriter.Close();
                this.previousFileWriter = null;

            }
            logger.Debug("FinishSongRecording finished");
        }

        public void StartSongRecording(SongInfo song)
        {
            Task.Run(() =>
           {
               logger.Debug("StartSongRecording() called");
               if (!System.IO.Directory.Exists(this.directory))
               {
                   System.IO.Directory.CreateDirectory(this.directory);
               }

               this.currentSong = new SongInfo()
               {
                   Artist = song.Artist,
                   Title = song.Title
               };

               if (!recording)
               {
                   logger.Debug("Calling StartRecording()");
                   this.recorder.StartRecording();
                   this.recording = true;

                   logger.Debug("Recording started");
               }
           });
        }

        private void ConvertToMp3(string wavFileName, SongInfo songInfo)
        {
            Task.Run(() =>
            {

                string mp3FileName = Path.Combine(this.directory, songInfo.GenerateFileName + ".mp3");

                logger.Debug("Converting wav to mp3");
                Mp3Codec.WaveToMp3(wavFileName, mp3FileName, songInfo.Artist, songInfo.Title);

                logger.Debug("Deleting wav file");

                // Delete the wav file after doing the conversion
                File.Delete(wavFileName);

                logger.Debug("Wav file deleted");
            });
        }


        private void Recorder_DataAvailable(object sender, WaveInEventArgs e)
        {

            while(this.finishing)
            {
                Thread.Sleep(10);
                
            }

            try
            {
                if (this.currentFileWriter == null)
                {
                    string fileName = Path.Combine(this.directory, Path.GetRandomFileName() + ".wav");

                    logger.Debug("Creating file writer with name " + fileName);

                    this.currentFileWriter = new WaveFileWriter(fileName, this.recorder.WaveFormat);
                }

                this.currentFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error writing data to file");

            }

        }

        private void Recorder_RecordingStopped(object sender, StoppedEventArgs e)
        {
            logger.Debug("Recorder_RecordingStopped() called");
            this.recording = false;
        }
    }
}