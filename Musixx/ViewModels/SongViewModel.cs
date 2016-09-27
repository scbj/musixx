using Musixx.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.ViewModels
{
    public class SongViewModel
    {
        IMusic music;

        public string Title { get { return music.Title; } }
        public string Artist { get { return music.Artist; } }
        public string Album { get { return music.Album; } }
        public TimeSpan Duration { get { return music.Duration; } }
        public Uri CoverUri { get { return music.CoverUri; } }
        public Uri Uri { get { return music.Uri; } }

        public SongViewModel(IMusic music)
        {
            this.music = music;
        }

        public IMusic GetSong() => music;
    }
}
