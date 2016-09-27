using Musixx.Clouds;
using Musixx.Models;
using Musixx.Views;
using MVVM.Pattern__UWP_.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace Musixx.ViewModels
{
    public class MainViewModel : ObservableObject, IViewModel<MainView>
    {
        object IViewModel.View
        {
            get { return View; }
            set { View = (MainView)value; }
        }
        private GoogleDrive cloud;
        private bool? loginState = false;
        private User user;
        private ObservableCollection<SongViewModel> musics = new ObservableCollection<SongViewModel>();
        private MusicViewModel currentPlaying;

        public MainViewModel()
        {
            cloud = new GoogleDrive();
            LogInOutCommand = new RelayCommand(logInOut, (s) => LoginState.HasValue);
        }

        private async void logInOut(object sender)
        {
            bool temp = loginState.Value;
            LoginState = null;
            if (temp)
                LoginState = !await cloud.Logout();
            else
                LoginState = await cloud.Login();

            if (loginState.Value)
            {
                User = await cloud.GetUser();
                //Musics = new ObservableCollection<MusicViewModel>((await cloud.GetMusics()).OrderBy(m => m.Title).Select(m => new MusicViewModel(m)));
                var songs = await cloud.GetMusic();
                foreach(var s in songs.Cast<Song>())
                {
                    s.RetreiveMetadataAsync().ContinueWith(async (t) =>
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            musics.Add(new SongViewModel(s));
                        });
                    });
                }
            }
        }

        public MainView View { get; set; }

        public ICommand LogInOutCommand { get; set; }

        public bool? LoginState
        {
            get { return loginState; }
            set
            {
                loginState = value;
                OnPropertyChanged(nameof(LoginState));
            }
        }

        public User User
        {
            get { return user; }
            set
            {
                user = value;
                OnPropertyChanged(nameof(User));
            }
        }

        public ObservableCollection<SongViewModel> Musics
        {
            get { return this.musics; }
            set
            {
                this.musics = value;
                OnPropertyChanged(nameof(Musics));
            }
        }

        public MusicViewModel CurrentPlaying
        {
            get { return this.currentPlaying; }
            set
            {
                this.currentPlaying = value;
                OnPropertyChanged(nameof(CurrentPlaying));
            }
        }

    }
}
