using Musixx.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Musixx.Shared.Messages
{
    public class UpdatePlaylistMessage : IMessage
    {
        public UpdatePlaylistMessage(List<IMusic> songs)
        {
            Songs = songs;
        }

        public List<IMusic> Songs { get; set; }

        public void Serialize(ValueSet value)
        {
            value.Add(nameof(Songs), Songs);
        }

        public void Deserialize(ValueSet value)
        {
            Songs = (List<IMusic>)value[nameof(Songs)];
        }
    }
}
