﻿using GPMDPSaver.Models;
using Newtonsoft.Json.Linq;
using System;
using WebSocketSharp;

namespace GPMDPSaver
{
    public class WebSocketSongReader : IDisposable
    {
        private bool playing;
        private bool reading;
        private WebSocket webSocket;

        public WebSocketSongReader()
        {
            this.CurrentSong = new SongInfo();
        }

        public delegate void SongChangeHandler(object sender, SongChangeEventArgs e);

        public event SongChangeHandler SongChange;

        public SongInfo CurrentSong
        {
            get;
            set;
        }

        public bool Reading
        {
            get
            {
                return reading;
            }

            private set
            {
                reading = value;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void StartReading()
        {
            this.webSocket = new WebSocket("ws://localhost:5672");
            this.webSocket.OnMessage += WebSocket_OnMessage;
            this.webSocket.Connect();
            this.Reading = true;
        }

        public void StopReading()
        {
            this.webSocket.OnMessage -= WebSocket_OnMessage;
            this.webSocket.Close();
            this.webSocket = null;
            this.Reading = false;
        }

        protected virtual void OnSongChange(SongInfo oldSongInfo, SongInfo newSongInfo)
        {
            if (this.SongChange != null)
            {
                this.SongChange.Invoke(this, new SongChangeEventArgs() { OldSongInfo = oldSongInfo, NewSongInfo = newSongInfo });
            }
        }

        private void PlayStateMessage(JToken payload)
        {
            this.playing = (bool)payload;
        }

        private void ProcessTimeMessage(JToken payload)
        {
            this.CurrentSong.CurrentTime = new TimeSpan(0, 0, 0, 0, (int)payload["current"]);
            this.CurrentSong.TotalTime = new TimeSpan(0, 0, 0, 0, (int)payload["total"]);
        }

        private void ProcessTrackMessage(JToken payload)
        {
            string artist = (string)payload["artist"];
            string title = (string)payload["title"];


            SongInfo oldSongInfo = new SongInfo()
            {
                Artist = this.CurrentSong.Artist,
                Title = this.CurrentSong.Title
            };

            SongInfo newSongInfo = new SongInfo()
            {
                Artist = artist,
                Title = title
            };


            this.OnSongChange(oldSongInfo, newSongInfo);



            this.CurrentSong.Artist = artist;
            this.CurrentSong.Title = title;
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            JObject obj = null;

            try
            {
                obj = JObject.Parse(e.Data);
            }
            catch (Exception ex)
            {
                // There was an error parsing the file so just continue on;
                // TODO: Add logging
            }

            string channel = (string)obj["channel"];
            JToken payload = obj["payload"];

            switch (channel)
            {
                case "playState":
                    this.PlayStateMessage(payload);
                    break;

                case "track":
                    this.ProcessTrackMessage(payload);
                    break;

                case "time":
                    this.ProcessTimeMessage(payload);
                    break;
            }
        }
    }
}