using Musixx.Models;
using Musixx.Shared;
using Musixx.Shared.BackgroundTask;
using Musixx.ViewModels;
using MVVM.Pattern__UWP_.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Musixx.Shared.Messages;
using Windows.UI.Core;
using System.Diagnostics;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace Musixx.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainView : Page, IView<MainViewModel>
    {
        #region Fields
        private bool _isMyBackgroundTaskRunning;
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
        private AutoResetEvent backgroundAudioTaskStarted = new AutoResetEvent(false);
        #endregion

        public MainViewModel ViewModel { get; set; }
        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainViewModel)value; }
        }

        #region Properties
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (_isMyBackgroundTaskRunning)
                    return true;

                string value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.BackgroundTaskState) as string;
                if (value == null)
                    return false;
                else
                {
                    try
                    {
                        _isMyBackgroundTaskRunning = EnumHelper.Parse<BackgroundTaskState>(value) == BackgroundTaskState.Running;
                    }
                    catch (ArgumentException)
                    {
                        _isMyBackgroundTaskRunning = false;
                    }

                    return _isMyBackgroundTaskRunning;
                }
            }
        }
        private MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

                do
                {
                    try
                    {
                        mp = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask();
                        }
                        else throw;
                    }
                } while (mp == null && --retryCount >= 0);

                if (mp == null)
                    throw new Exception("Failed to get a MediaPlayer instance.");

                return mp;
            }
        }
        #endregion

        public MainView()
        {
            DataContext = ViewModel = new MainViewModel();
            this.InitializeComponent();
            ViewModel.View = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            playlistView.DoubleTapped += PlaylistView_DoubleTapped;

            Application.Current.Suspending += ForegroundApp_Suspending;
            Application.Current.Resuming += ForegroundApp_Resuming;
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());
        }

        private void ForegroundApp_Resuming(object sender, object e)
        {
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            if (IsMyBackgroundTaskRunning)
            {
                AddMediaPlayerEventHandlers();
                MessageService.SendMessageToBackground(new AppResumedMessage());
                UpdateTransportControls(CurrentPlayer.CurrentState);
            }
            else
            {
                audioPlayer.IsPlaying = false;
            }
        }

        private void ForegroundApp_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            if (IsMyBackgroundTaskRunning)
            {
                RemoveMediaPlayerEventHandlers();
                MessageService.SendMessageToBackground(new AppSuspendedMessage());
            }

            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());

            deferral.Complete();
        }

        private void PlaylistView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_isMyBackgroundTaskRunning)
            {
                RemoveMediaPlayerEventHandlers();
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());
            }

            base.OnNavigatedTo(e);
        }

        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _isMyBackgroundTaskRunning = false;
            backgroundAudioTaskStarted.Reset();
            audioPlayer.CanGoPrevious = false;
            audioPlayer.CanGoNext = false;
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Unknow.ToString());
            audioPlayer.IsPlaying = true;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch(Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                    throw new Exception("Failed to get a MediaPlayer instance.");
                else
                    throw;
            }
        }


        #region Background MediaPlayer Event Handlers
        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState;
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateTransportControls(currentState));
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            var message = MessageService.Unpack(e.Data);
            if (message is TrackChangedMessage)
            {
                var trackChangedMessage = (TrackChangedMessage)message;
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (trackChangedMessage.TrackId == null)
                    {
                        audioPlayer.CurrentSong = null;
                        audioPlayer.CanGoPrevious = false;
                        audioPlayer.CanGoNext = false;
                        return;
                    }
                });
            }
        }

        #endregion

        #region Media Playback Helper methods
        private void UpdateTransportControls(MediaPlayerState state)
        {
            audioPlayer.IsPlaying = state == MediaPlayerState.Playing ? true : false;
        }

        private void RemoveMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.Current.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult != RPC_S_SERVER_UNAVAILABLE)
                    throw;
            }
        }

        private void AddMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                    ResetAfterLostBackground();
                else
                    throw;
            }
        }

        private void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            var startResult = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                if (result)
                {
                    var musics = ViewModel.Musics.Select(m => m.GetSong()).ToList();
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(musics));
                    MessageService.SendMessageToBackground(new StartPlaybackMessage());
                }
                else
                    throw new Exception("Background Audio Task didn't start in expected time");
            });
            startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
                Debug.WriteLine("Background Audio Task initialized");
            else if (status == AsyncStatus.Error)
                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
        }
        #endregion
    }
}
