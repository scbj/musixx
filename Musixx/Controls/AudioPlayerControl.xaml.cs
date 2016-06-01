using Musixx.Models;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Musixx.Controls
{
    public sealed partial class AudioPlayerControl : UserControl
    {
        private DispatcherTimer timer;
        private bool userIsDraggingSlider;

        public AudioPlayerControl()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Playing;

            this.slider.AddHandler(Thumb.PointerPressedEvent, new PointerEventHandler(SliderThumbPressed), true);
            this.slider.AddHandler(Thumb.PointerReleasedEvent, new PointerEventHandler(SliderThumbReleased), true);
        }

        public TimeSpan Value { get; private set; }
        public TimeSpan Maximum { get; private set; }
        public bool IsPlaying { get; private set; }

        private void Playing(object sender, object e)
        {
            // Test if it's need to put IsPlaying in condition below
            if (!IsPlaying)
                throw new Exception("So.. add IsPlaying in if condition below :");

            if (player.Source != null)
            {
                Value = player.Position;
            }
        }
        private void SliderThumbPressed(object sender, PointerRoutedEventArgs e) => userIsDraggingSlider = true;

        private void SliderThumbReleased(object sender, PointerRoutedEventArgs e)
        {
            Value = TimeSpan.FromSeconds(slider.Value);
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
            
            Maximum = music.Duration;
            Value = TimeSpan.Zero;
            player.Source = music.Uri;
            player.Tag = music;

            Play();
        }
    }
}
