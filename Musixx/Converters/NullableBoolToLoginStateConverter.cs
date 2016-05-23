using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Musixx.Converters
{
    public class NullableBoolToLoginStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? b = (bool?)value;
            if (b.HasValue)
            {
                if (b.Value)
                    return "Se déconnecter";
                else
                    return "Se connecter";
            }
            else
                return "En attente...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
