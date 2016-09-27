using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Shared
{
    public static class EnumHelper
    {
        public static T Parse<T>(string value) where T : struct => (T)Enum.Parse(typeof(T), value);
    }
}
