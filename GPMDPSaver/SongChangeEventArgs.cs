using GPMDPSaver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMDPSaver
{
    public class SongChangeEventArgs : EventArgs
    {
        public SongInfo OldSongInfo
        {
            get;
            set;
        }

        public SongInfo NewSongInfo
        {
            get;
            set;
        }
    }
}
