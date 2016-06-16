using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Musixx.Shared.Messages
{
    public class SkipNextMessage : IMessage
    {
        public void Serialize(ValueSet value) { }
        public void Deserialize(ValueSet value) { }
    }
}
