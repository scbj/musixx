using Musixx.Models;
using Musixx.ViewModels;
using MVVM.Pattern__UWP_.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace Musixx.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainView : Page, IView<MainViewModel>
    {
        public MainView()
        {
            DataContext = ViewModel = new MainViewModel();
            this.InitializeComponent();
            ViewModel.View = this;
        }

        public MainViewModel ViewModel { get; set; }
        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainViewModel)value; }
        }

        private void listView_Musics_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var music = (MusicViewModel)this.listView_Musics.SelectedItem;
            audioPlayer.Play(music);
        }

        private void Play(MusicViewModel music)
        {
            //slider.Value = 0;
            //slider.Maximum = music.Duration.TotalSeconds;
            //txt_Maximum.Text = music.Duration.ToString(@"m\:ss");
            //mediaElement.Source = music.Uri;
            //mediaElement.Play();
            
            //ViewModel.CurrentPlaying = music;
            //mediaPlayerIsPlaying = true;
        }
    }
}
