using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Models
{
    public class User
    {
        public User(string name, string pictureUrl)
        {
            Name = name;
            PictureUrl = pictureUrl;
        }

        public string Name { get; private set; }
        public string PictureUrl { get; private set; }
    }
}
