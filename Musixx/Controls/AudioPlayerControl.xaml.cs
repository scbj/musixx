using Musixx.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Windows.Input;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Musixx.Controls
{
    public sealed partial class AudioPlayerControl : UserControl, INotifyPropertyChanged
    {
        #region Fields
        private SongViewModel currentSong;
        private bool userIsDraggingSlider;
        private double value;
        private double maximum;
        private bool isPlaying;
        private bool canGoPrevious;
        private bool canGoNext;

        public event Action PlayEnded;
        public ICommand PlayPauseCommand { get; set; }
        #endregion

        #region Properties
        public SongViewModel CurrentSong
        {
            get { return currentSong; }
            set
            {
                this.currentSong = value;
                OnPropertyChanged(nameof(CurrentSong));
            }
        }
        public bool UserIsDraggingSlider { get; set; }
        public double Value
        {
            get { return value; }
            private set
            {
                this.value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        public double Maximum
        {
            get { return maximum; }
            private set
            {
                maximum = value;
                OnPropertyChanged(nameof(Maximum));
            }
        }
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }
        public bool CanGoPrevious
        {
            get { return canGoPrevious; }
            set
            {
                canGoPrevious = value;
                OnPropertyChanged(nameof(CanGoPrevious));
            }
        }
        public bool CanGoNext
        {
            get { return canGoNext; }
            set
            {
                canGoNext = value;
                OnPropertyChanged(nameof(CanGoNext));
            }
        }
        #endregion

        public AudioPlayerControl()
        {
            DataContext = this;
            this.InitializeComponent();

            this.slider.AddHandler(Thumb.PointerPressedEvent, new PointerEventHandler((s,e) => UserIsDraggingSlider = true), true);
            this.slider.AddHandler(Thumb.PointerReleasedEvent, new PointerEventHandler((s, e) => UserIsDraggingSlider = false), true);
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
