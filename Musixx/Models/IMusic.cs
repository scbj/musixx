﻿using System;
using Windows.UI.Xaml.Media.Imaging;

namespace Musixx.Models
{
    public interface IMusic
    {
        string Title { get; }
        string Artist { get; }
        string Album { get; }
        TimeSpan Duration { get; }
        BitmapImage Cover { get; }
        Uri Uri { get; }
    }
}