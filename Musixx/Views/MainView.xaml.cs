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
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        public MainView()
        {
            DataContext = ViewModel = new MainViewModel();
            this.InitializeComponent();
            ViewModel.View = this;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
            
            this.slider.AddHandler(Thumb.PointerPressedEvent, new PointerEventHandler(SliderThumbPressed), true);
            this.slider.AddHandler(Thumb.PointerReleasedEvent, new PointerEventHandler(SliderThumbReleased), true);
        }

        private void Timer_Tick(object sender, object e)
        {
            if (mediaElement.Source != null && mediaPlayerIsPlaying && !userIsDraggingSlider)
            {
                slider.Value = mediaElement.Position.TotalSeconds;
                txt_Value.Text = mediaElement.Position.ToString(@"m\:ss");
            }

            if (mediaPlayerIsPlaying)
                btn_PlayPause.Content = '\uE769';
            else
                btn_PlayPause.Content = '\uE768';

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
            Play(music);
        }

        private void Play(MusicViewModel music)
        {
            slider.Value = 0;
            slider.Maximum = music.Duration.TotalSeconds;
            txt_Maximum.Text = music.Duration.ToString(@"m\:ss");
            mediaElement.Source = music.Uri;
            mediaElement.Play();
            
            ViewModel.CurrentPlaying = music;
            mediaPlayerIsPlaying = true;
        }
        
        public void SliderThumbPressed(object sender, PointerRoutedEventArgs e) => userIsDraggingSlider = true;
        public void SliderThumbReleased(object sender, PointerRoutedEventArgs e)
        {
            //Value = (sender as Slider).Value;
            mediaElement.Position = TimeSpan.FromSeconds(slider.Value);
            userIsDraggingSlider = false;
        }

        private void btn_PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
                mediaElement.Pause();
            else
                mediaElement.Play();

            mediaPlayerIsPlaying = !mediaPlayerIsPlaying;
        }
    }
}
