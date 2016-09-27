using Musixx.Data;
using Musixx.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace Musixx.Models
{
    public class Song : IMusic
    {
        const string DefaultArtist = "Titre inconnu";
        const string DefaultAlbum = "Album inconnu";
        const string DefaultCover = "ms-appx://Musixx/Assets/default_album.png";

        public string Album { get; set; }
        public string Artist { get; set; }
        public Uri CoverUri { get; set; }
        public TimeSpan Duration { get; set; }
        public string Title { get; set; }
        public Uri Uri { get { return File.Uri; } }
        public IFile File { get; private set; }

        public Song(IFile file)
        {
            File = file;
            Title = System.IO.Path.GetFileNameWithoutExtension(file.Name);

            PopulateDefaultValues();
        }

        private void PopulateDefaultValues()
        {
            Artist = DefaultArtist;
            Album = DefaultAlbum;
            CoverUri = new Uri(DefaultCover);
        }

        public async Task<bool> RetreiveMetadataAsync()
        {
            Debug.WriteLine("Loading " + Title + "...");
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("range", "bytes=0-10");
                var buffer = await client.GetBufferAsync(Uri);
                int? headerLength = ID3.GetID3v2HeaderLength(buffer.ToArray());
                if (!headerLength.HasValue)
                    return false;

                client.DefaultRequestHeaders["range"] = "bytes=0-" + headerLength.Value;
                buffer = await client.GetBufferAsync(Uri);
                ID3.ExtractTagsFromBuffer(buffer, this);

                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                var dialog = new MessageDialog(Title + Environment.NewLine + ex.Message, "Erreur lors de l'extraction des métadonnées");
                await dialog.ShowAsync();
            }

            return false;
        }
    }
}
