using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Data
{
    public class ID3
    {
        public static int? GetID3v2HeaderLength(byte[] bytes)
        {
            if (bytes.Length < 3 || Encoding.UTF8.GetString(bytes, 0, 3) != "ID3")
                return null;

            return 7 + BitConverter.ToInt32(bytes.Skip(6).Take(4).Reverse().ToArray(), 0);
        }
    }
}
