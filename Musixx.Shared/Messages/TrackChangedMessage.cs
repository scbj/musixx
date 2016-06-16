using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Musixx.Shared.Messages
{
    public class TrackChangedMessage : IMessage
    {
        public TrackChangedMessage() { }

        public TrackChangedMessage(Uri trackId)
        {
            TrackId = trackId;
        }

        public Uri TrackId { get; set; }

        public void Serialize(ValueSet value)
        {
            value.Add(nameof(TrackId), TrackId);
        }

        public void Deserialize(ValueSet value)
        {
            TrackId = (Uri)value[nameof(TrackId)];
        }
    }
}
