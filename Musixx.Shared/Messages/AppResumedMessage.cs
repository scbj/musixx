using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Musixx.Shared.Messages
{
    public class AppResumedMessage : IMessage
    {

        public AppResumedMessage()
        {
            Timestamp = DateTime.Now;
        }

        public AppResumedMessage(DateTime timestamp)
        {
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; set; }

        public void Serialize(ValueSet value)
        {
            value.Add(nameof(Timestamp), Timestamp);
        }

        public void Deserialize(ValueSet value)
        {
            Timestamp = (DateTime)value[nameof(Timestamp)];
        }
    }
}
