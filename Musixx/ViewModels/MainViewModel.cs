using Musixx.Clouds;
using Musixx.Models;
using Musixx.Views;
using MVVM.Pattern__UWP_.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                View.SetMusics(await cloud.GetMusics());

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

    }
}
