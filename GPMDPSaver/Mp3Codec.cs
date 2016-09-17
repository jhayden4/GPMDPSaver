using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMDPSaver
{
    public static class Mp3Codec
    {
        public static void WaveToMp3(string wavefileName, string artist, string title, int bitRate = 320)
        {
            using (WaveFileReader wavReader = new WaveFileReader(wavefileName))
            {
                using (LameMP3FileWriter mp3Writer = new LameMP3FileWriter(wavefileName.Replace(".wav",".mp3"), wavReader.WaveFormat, bitRate, new ID3TagData() { Artist = artist, Title = title }))
                {
                    wavReader.CopyTo(mp3Writer);
                }
            }
        }
    }
}
