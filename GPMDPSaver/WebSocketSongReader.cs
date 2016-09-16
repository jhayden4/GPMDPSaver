using GPMDPSaver.Models;
using Newtonsoft.Json.Linq;
using System;
using WebSocketSharp;

namespace GPMDPSaver
{
    public class WebSocketSongReader : IDisposable
    {
        private bool reading;
        private bool playing;

        public delegate void SongActionHandler(object sender, SongActionEventArgs e);

        public event SongActionHandler SongAction;

        private WebSocket webSocket;

        public WebSocketSongReader()
        {
            this.CurrentSong = new SongInfo()
            {
                Artist = "(None)",
                Title = "(None)"
            };
        }

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

        public void StartReading()
        {
            this.Reading = true;
            this.webSocket = new WebSocket("ws://localhost:5672");
            this.webSocket.OnMessage += WebSocket_OnMessage;
            this.webSocket.ConnectAsync();
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
                    this.playing = (bool)payload;
                    break;
                case "track":
                    this.ProcessTrackMessage(payload);
                    break;
                case "time":
                    this.ProcessTimeMessage(payload);
                    break;
            }
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

            if (this.playing)
            {
                this.OnSongAction(new SongInfo() { Artist = this.CurrentSong.Artist, Title = this.CurrentSong.Title }, Models.SongAction.Finish);
                this.OnSongAction(new SongInfo() { Artist = artist, Title = title }, Models.SongAction.Start);
            }

            this.CurrentSong.Artist = artist;
            this.CurrentSong.Title = title;
        }

        public void StopReading()
        {
            this.Reading = false;
            this.webSocket.OnMessage -= WebSocket_OnMessage;
            this.webSocket.Close();
            this.webSocket = null;
        }

        protected virtual void OnSongAction(SongInfo songInfo, SongAction action)
        {
            if (this.SongAction != null)
            {
                this.SongAction.Invoke(this, new SongActionEventArgs() { SongInfo = songInfo, Action = action });
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}