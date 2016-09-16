using GPMDPSaver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMDPSaver
{
    public class SongActionEventArgs : EventArgs
    {
        public SongInfo SongInfo
        {
            get;
            set;
        }

        public SongAction Action
        {
            get;
            set;
        }
    }
}
