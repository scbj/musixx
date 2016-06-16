using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Shared.Models
{
    public class IFile
    {
        int Id { get; set; }
        string Name { get; set; }
        string MD5Hash { get; set; }
        Uri Uri { get; set; }
    }
}
