using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace Musixx.Shared.Messages
{
    public static class MessageService
    {
        const string MessageType = "MessageType";

        private static ValueSet Pack(this IMessage message)
        {
            var value = new ValueSet();
            value.Add(MessageType, message.GetType());
            message.Serialize(value);

            return value;
        }

        public static IMessage Unpack(ValueSet value)
        {
            Type t = (Type)value[MessageType];
            IMessage message = (IMessage)Activator.CreateInstance(t);
            message.Deserialize(value);

            return message;
        }

        public static void SendMessageToForeground(IMessage message) => BackgroundMediaPlayer.SendMessageToForeground(message.Pack());
        public static void SendMessageToBackground(IMessage message) => BackgroundMediaPlayer.SendMessageToBackground(message.Pack());
    }
}
