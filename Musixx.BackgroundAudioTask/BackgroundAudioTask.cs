using Musixx.Shared;
using Musixx.Shared.BackgroundTask;
using Musixx.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Musixx.Shared.Models;
using Windows.Media.Core;

namespace Musixx.BackgroundAudioTask
{
    public class BackgroundAudioTask : IBackgroundTask
    {
        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string ThumbnailKey = "thumbnail";

        private SystemMediaTransportControls smtc;
        private AppState foregroundAppState;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private MediaPlaybackList playbackList = new MediaPlaybackList();
        private bool playbackStartedPreviously = false;

        Uri GetCurrentTrackId()
        {
            if (playbackList == null)
                return null;

            return GetTrackId(playbackList.CurrentItem);
        }

        Uri GetTrackId(MediaPlaybackItem item)
        {
            if (item == null)
                return null;

            return item.Source.CustomProperties[TrackIdKey] as Uri;
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");

            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += smtc_ButtonPressed;
            smtc.PropertyChanged += smtc_PropertyChanged;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;

            var value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.AppState);
            if (value == null)
                foregroundAppState = AppState.Unknow;
            else
                foregroundAppState = (AppState)value;

            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            if (foregroundAppState != AppState.Suspended)
                MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running);

            deferral = taskInstance.GetDeferral();

            backgroundTaskStarted.Set();

            taskInstance.Task.Completed += Task_Completed;
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine($"{nameof(BackgroundAudioTask)} {sender.TaskId} Completed...");
            deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine($"{nameof(BackgroundAudioTask)} {sender.Task.TaskId} Cancel Requested...");
            try
            {
                backgroundTaskStarted.Reset();

                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, GetCurrentTrackId()?.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Canceled);
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, foregroundAppState);

                if (playbackList != null)
                {
                    playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                    playbackList = null;
                }

                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                smtc.ButtonPressed -= smtc_ButtonPressed;
                smtc.PropertyChanged -= smtc_PropertyChanged;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            deferral.Complete();
            Debug.WriteLine(nameof(BackgroundAudioTask) + " Cancel complete...");
        }

        #region SystemMediaTransportControls related functions and handlers
        private void UpdateSMTCInfosOnNewTrack(MediaPlaybackItem item)
        {
            if (item == null)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                smtc.DisplayUpdater.Update();
                return;
            }

            smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = item.Source.CustomProperties[TitleKey] as string;

            var thumbnailUri = item.Source.CustomProperties[ThumbnailKey] as Uri;
            if (thumbnailUri != null)
                smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(thumbnailUri);
            else
                smtc.DisplayUpdater.Thumbnail = null;

            smtc.DisplayUpdater.Update();
        }

        private void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // If soundlevel turns to muted, app can choose to pause the music
        }

        private void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("SMTC play button pressed");

                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didn't initialize in time");
                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("SMTC play button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("SMTC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("SMTC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }
        #endregion

        private void StartPlayback()
        {
            try
            {
                if (playbackStartedPreviously)
                    BackgroundMediaPlayer.Current.Play();
                else
                {
                    playbackStartedPreviously = true;

                    var currentTrackId = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId);
                    var currentTrackPosition = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.Position);
                    if (currentTrackId == null)
                        BackgroundMediaPlayer.Current.Play();
                    else
                    {
                        var index = playbackList.Items.ToList().FindIndex(item =>
                        GetTrackId(item) == (Uri)currentTrackId);

                        if (currentTrackPosition == null)
                        {
                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            playbackList.MoveTo((uint)index);

                            BackgroundMediaPlayer.Current.Play();
                        }
                        else
                        {
                            TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
                            handler = (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
                            {
                                if (args.NewItem == playbackList.Items[index])
                                {
                                    playbackList.CurrentItemChanged -= handler;

                                    var position = (TimeSpan)currentTrackPosition;
                                    Debug.WriteLine("StartPlayback: Setting Position " + position);
                                    BackgroundMediaPlayer.Current.Position = position;

                                    BackgroundMediaPlayer.Current.Play();
                                }
                            };
                            playbackList.CurrentItemChanged += handler;

                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            playbackList.MoveTo((uint)index);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            var item = args.NewItem;
            Debug.WriteLine("PlaybackList_CurrentItemChanged: " + (item == null ? "null" : GetTrackId(item).ToString()));

            UpdateSMTCInfosOnNewTrack(item);

            Uri currentTrackId = null;
            if (item != null)
                currentTrackId = item.Source.CustomProperties[TrackIdKey] as Uri;

            if (foregroundAppState == AppState.Active)
                MessageService.SendMessageToForeground(new TrackChangedMessage(currentTrackId));
            else
                ApplicationSettingsHelper.SaveSettingsValue(TrackIdKey, currentTrackId);
        }

        private void SkipToPrevious()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MovePrevious();
        }

        private void SkipToNext()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MoveNext();
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            else if (sender.CurrentState == MediaPlayerState.Paused)
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            else if (sender.CurrentState == MediaPlayerState.Closed)
                smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            var message = MessageService.Unpack(e.Data);
            if (message is AppSuspendedMessage)
            {
                Debug.Write("App suspending");
                foregroundAppState = AppState.Suspended;
                var currentTrackId = GetCurrentTrackId();
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, currentTrackId);
            }
            else if (message is AppResumedMessage)
            {
                Debug.Write("App resuming");
                foregroundAppState = AppState.Active;
            }
            else if (message is StartPlaybackMessage)
            {
                Debug.WriteLine("Starting playback");
                StartPlayback();
            }
            else if (message is SkipNextMessage)
            {
                Debug.WriteLine("Skipping to next");
                SkipToNext();
            }
            else if (message is SkipPreviousMessage)
            {
                Debug.WriteLine("Skipping to previous");
                SkipToPrevious();
            }
            else if (message is TrackChangedMessage)
            {
                var trackChangedMessage = (TrackChangedMessage)message;
                var index = playbackList.Items.ToList().FindIndex(i => (Uri)i.Source.CustomProperties[TrackIdKey] == trackChangedMessage.TrackId);
                Debug.WriteLine("Skipping to track " + index);
                smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                playbackList.MoveTo((uint)index);
            }
            else if (message is UpdatePlaylistMessage)
            {
                var updatePlaylistMessage = (UpdatePlaylistMessage)message;
                CreatePlaybackList(updatePlaylistMessage.Songs);
            }
        }

        private void CreatePlaybackList(IEnumerable<IMusic> songs)
        {
            playbackList = new MediaPlaybackList();
            playbackList.AutoRepeatEnabled = true;

            foreach(var song in songs)
            {
                var source = MediaSource.CreateFromUri(song.Uri);
                source.CustomProperties[TrackIdKey] = song.Uri;
                source.CustomProperties[TitleKey] = song.Title;
                source.CustomProperties[ThumbnailKey] = song.CoverUri;
                playbackList.Items.Add(new MediaPlaybackItem(source));
            }

            BackgroundMediaPlayer.Current.AutoPlay = false;

            BackgroundMediaPlayer.Current.Source = playbackList;

            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }
    }
}
