using Musixx.Models;
using MVVM.Pattern__UWP_.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace Musixx.ViewModels
{
    public class MusicViewModel : ObservableObject, IMusicHOLD
    {
        MusicHOLD music;

        public string Title { get { return music.Title; } }
        public string Artist { get { return music.Artist; } }
        public string Album { get { return music.Album; } }
        public TimeSpan Duration { get { return music.Duration; } }
        public BitmapImage Cover { get { return music.Cover; } }
        public Uri Uri { get { return music.Uri; } }


        public MusicViewModel(MusicHOLD music)
        {
            this.music = music;
            music.MetadataRetreived += Music_MetadataRetreived;
        }

        private async void Music_MetadataRetreived()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Artist));
                OnPropertyChanged(nameof(Album));
                OnPropertyChanged(nameof(Duration));
                OnPropertyChanged(nameof(Cover));
            });
        }
    }
}
