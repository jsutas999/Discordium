using System;
using System.Collections.Generic;
using System.Text;

namespace Discordium.Models
{
    public class Song
    {

        public string filename;
        public string songname;
        public string duration;

        public Song(string filename)
        {
            this.filename = filename;
        }


    }
}
