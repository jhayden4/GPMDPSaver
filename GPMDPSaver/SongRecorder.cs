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
        private string directory;
        private WaveFileWriter fileWriter;
        private WasapiLoopbackCapture recorder;
        private bool recording;
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

        public void CancelRecording()
        {
            this.recorder.StopRecording();

            this.fileWriter.Close();
            this.fileWriter = null;
        }

        public void Dispose()
        {
            if (this.fileWriter != null)
            {
                this.fileWriter.Dispose();
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
                logger.Debug("record.StopRecording() called");
                this.recorder.StopRecording();

                logger.Debug("fileWriter.Close() called");
                this.fileWriter.Close();
                this.fileWriter = null;

                logger.Debug("ConvertToMp3() called");
                this.ConvertToMp3(this.currentSong.Artist, this.currentSong.Title);

                this.currentSong = null;
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

               logger.Debug("Generating wav file name");
               string fileName = this.GenerateWavFileName(song.Artist, song.Title);

               logger.Debug("Create file wrtier");
               this.fileWriter = new WaveFileWriter(fileName, this.recorder.WaveFormat);

               this.currentSong = new SongInfo()
               {
                   Artist = song.Artist,
                   Title = song.Title
               };

               // Wait until the previous recording has finished before starting the next one

               logger.Debug("Waiting until the previous recording has finished");

               while (recording)
               {
                   Thread.Sleep(10);
               }

               logger.Debug("Calling StartRecording()");
               this.recorder.StartRecording();
               this.recording = true;
               logger.Debug("Recording started");
           });
        }

        private void ConvertToMp3(string artist, string title)
        {
            Task.Run(() =>
            {
                string wavFile = this.GenerateWavFileName(artist, title);

                logger.Debug("Converting wav to mp3");
                Mp3Codec.WaveToMp3(wavFile, artist, title);

                logger.Debug("Deleting wav file");

                // Delete the wav file after doing the conversion
                File.Delete(wavFile);

                logger.Debug("Wav file deleted");
            });
        }

        private string GenerateWavFileName(string artist, string title)
        {
            // Make sure to clean the artist and title for invalid file characters
            string fileName = Path.GetInvalidFileNameChars().Aggregate(artist + " - " + title, (current, c) => current.Replace(c.ToString(), string.Empty)) + ".wav";

            return Path.Combine(this.Directory, fileName);
        }

        private void Recorder_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.fileWriter != null)
            {
                this.fileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void Recorder_RecordingStopped(object sender, StoppedEventArgs e)
        {
            logger.Debug("Recorder_RecordingStopped() called");
            this.recording = false;
        }
    }
}