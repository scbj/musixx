using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Models
{
    public class Music
    {
        private string url;

        public Music(string name, string url)
        {
            this.url = url;
            Title = name;
        }

        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public string CoverUrl { get; private set; }
    }
}
