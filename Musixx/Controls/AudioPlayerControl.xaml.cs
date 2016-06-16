using Musixx.Models;
using MVVM.Pattern__UWP_.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Musixx.Controls
{
    public sealed partial class AudioPlayerControl : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer timer;
        private bool userIsDraggingSlider;

        public AudioPlayerControl()
        {
            DataContext = this;
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += Playing;

            this.slider.AddHandler(Thumb.PointerPressedEvent, new PointerEventHandler(SliderThumbPressed), true);
            this.slider.AddHandler(Thumb.PointerReleasedEvent, new PointerEventHandler(SliderThumbReleased), true);

            player.MediaEnded += (s, e) => PlayEnded?.Invoke();

            PlayPauseCommand = new RelayCommand((s) =>
            {
                if (IsPlaying)
                    Pause();
                else
                    Play();
            });
        }

        private double value;
        public double Value
        {
            get { return value; }
            private set
            {
                this.value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        private double maximum;
        public double Maximum
        {
            get { return maximum; }
            private set
            {
                maximum = value;
                OnPropertyChanged(nameof(Maximum));
            }
        }

        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            private set
            {
                isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public event Action PlayEnded;

        public ICommand PlayPauseCommand { get; set; }

        private void Playing(object sender, object e)
        {
            // Test if it's need to put IsPlaying in condition below
            if (!IsPlaying)
                throw new Exception("So.. add IsPlaying in if condition below :");


            //Search for exact value of duration
            if (player.Source != null && !userIsDraggingSlider)
            {
                Value = player.Position.TotalSeconds;
                slider.Value = value;
            }

        }
        private void SliderThumbPressed(object sender, PointerRoutedEventArgs e) => userIsDraggingSlider = true;

        private void SliderThumbReleased(object sender, PointerRoutedEventArgs e)
        {
            player.Position = TimeSpan.FromSeconds(slider.Value);
            userIsDraggingSlider = false;
        }


        private void Play()
        {
            player.Play();
            IsPlaying = true;

            timer.Start();
        }

        private void Pause()
        {
            player.Pause();
            IsPlaying = false;

            timer.Stop();
        }

        public void Play(IMusic music)
        {
            if (IsPlaying)
                Pause();

            Maximum = music.Duration.TotalSeconds;
            Value = 0;
            title.Text = music.Title;
            artist.Text = music.Artist;
            cover.Source = music.Cover;
            player.Source = music.Uri;
            player.Tag = music;

            Play();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
