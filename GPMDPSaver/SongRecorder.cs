using System;
using System.IO;
using GPMDPSaver.Models;
using NAudio.Wave;

namespace GPMDPSaver
{
    public class SongRecorder : IDisposable
    {
        private SongInfo currentSong;
        private WasapiLoopbackCapture recorder;
        private WaveFileWriter fileWriter;
        private string directory;

        public SongRecorder(string directory)
        {
            this.directory = directory;
            this.recorder = new WasapiLoopbackCapture();
            this.recorder.DataAvailable += Recorder_DataAvailable;
        }

        private void Recorder_DataAvailable(object sender, WaveInEventArgs e)
        {
            if(this.fileWriter != null)
            {
                this.fileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public void StartSongRecording(SongInfo song)
        {
            string fileName = song.Artist + " - " + song.Title + ".wav";

            this.fileWriter = new WaveFileWriter(Path.Combine(this.directory, fileName), this.recorder.WaveFormat);

            this.recorder.StartRecording();
        }

        public void FinishSongRecording(SongInfo song)
        {
            this.recorder.StopRecording();

            this.fileWriter.Close();
            this.fileWriter = null;
        }

        public void CancelRecording()
        {
            this.recorder.StopRecording();

            this.fileWriter.Close();
            this.fileWriter = null;
        }
    }
}