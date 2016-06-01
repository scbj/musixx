using Musixx.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Musixx.Models
{
    public class Music : IMusic
    {
        private string name;
        private string id;
        private string md5;
        private long size;

        public event Action MetadataRetreived;

        public Music(string name, string url, long size)
        {
            this.name = name;
            this.size = size;
            Title = Path.GetFileNameWithoutExtension(name);
            Uri = new Uri(url);
            System.Diagnostics.Debug.WriteLine(url);

            RetreiveMetadata();
        }

        public string Title { get; private set; }
        public string Artist { get; private set; } = "Artiste inconnu";
        public string Album { get; private set; } = "Album inconnu";
        public TimeSpan Duration { get; private set; }
        public BitmapImage Cover { get; private set; }
        public Uri Uri { get; private set; }

        private async void RetreiveMetadata()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("range", "bytes=0-1000000");
                var buffer = await client.GetBufferAsync(Uri);
                using (var stream = new MemoryStream(buffer.ToArray()))
                {
                    var bytes = buffer.ToArray();
                    var tagFile = TagLib.File.Create(new StreamFileAbstraction(name, stream, stream));
                    var id3 = tagFile.GetTag(TagTypes.Id3v2);

                    Title = id3.Title;
                    if (id3.FirstPerformer != null)
                        Artist = id3.FirstPerformer;
                    if (id3.Album != null)
                        Album = id3.Album;
                    if (id3.Album != null)
                        Cover = await id3.Pictures[0].Data.Data.ToBitmapImage();
                    int bitrate = tagFile.Properties.AudioBitrate * 125;
                    Duration = TimeSpan.FromSeconds(size / bitrate);
                }

                MetadataRetreived?.Invoke();
            }
            catch
            {

            }
        }
    }
}
