using GPMDPSaver.Models;
using NAudio.Wave;
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

        public SongRecorder(string directory)
        {
            this.directory = directory;
            this.recorder = new WasapiLoopbackCapture();
            this.recorder.DataAvailable += Recorder_DataAvailable;
            this.recorder.RecordingStopped += Recorder_RecordingStopped;
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
            if (this.currentSong != null)
            {
                this.recorder.StopRecording();

                this.fileWriter.Close();
                this.fileWriter = null;

                this.ConvertToMp3(this.currentSong.Artist, this.currentSong.Title);

                this.currentSong = null;
            }
        }

        public void StartSongRecording(SongInfo song)
        {
            Task.Run(() =>
           {
               string fileName = this.GenerateWavFileName(song.Artist, song.Title);

               this.fileWriter = new WaveFileWriter(fileName, this.recorder.WaveFormat);

               this.currentSong = new SongInfo()
               {
                   Artist = song.Artist,
                   Title = song.Title
               };

               // Wait until the previous recording has finished before starting the next one

               while (recording)
               {
                   Thread.Sleep(10);
               }

               this.recorder.StartRecording();
               this.recording = true;
           });
        }

        private void ConvertToMp3(string artist, string title)
        {
            Task.Run(() =>
            {
                string wavFile = this.GenerateWavFileName(artist, title);

                Mp3Codec.WaveToMp3(wavFile, artist, title);

                // Delete the wav file after doing the conversion
                File.Delete(wavFile);
            });
        }

        private string GenerateWavFileName(string artist, string title)
        {
            // Make sure to clean the artist and title for invalid file characters
            string fileName = Path.GetInvalidFileNameChars().Aggregate(artist + " - " + title, (current, c) => current.Replace(c.ToString(), string.Empty)) + ".wav";

            return  Path.Combine(this.directory, fileName);           
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
            this.recording = false;
        }
    }
}