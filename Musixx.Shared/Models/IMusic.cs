using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Shared.Models
{
    public interface IMusic
    {
        string Title { get; set; }
        string Artist { get; set; }
        string Album { get; set; }
        TimeSpan Duration { get; set; }
        Uri CoverUri { get; set; }
        Uri Uri { get; }
        IFile File { get; }
    }
}
