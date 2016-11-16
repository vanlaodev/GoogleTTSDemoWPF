using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using WpfMvvmLib;

namespace GoogleTTSDemoWPF
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly string GoogleTTSApiUrl = "https://translate.google.com/translate_tts";
        private static readonly string[] LanguageCodes = { "zh-yue", "ja", "en", "pt", "zh-TW" };
        private readonly WaveOutEvent _waveOutEvent;
        private bool _playInProgress;

        public MainViewModel()
        {
            _waveOutEvent = new WaveOutEvent();
            _waveOutEvent.PlaybackStopped += WaveOutEventOnPlaybackStopped;

            Languages =
                new ObservableCollection<string>(new[] { "Cantonese", "Japanese", "English", "Portugese", "Mandarin" });
            SelectedLangIdx = 0;
            PlayCmd = new RelayCommand(CanPlay, Play);
            StopCmd = new RelayCommand(CanStop, Stop);
            ClosingCmd = new RelayCommand(o => true, Closing);
        }

        private void Closing(object obj)
        {
            _waveOutEvent.Stop();
        }

        public int SelectedLangIdx { get; set; }
        public ObservableCollection<string> Languages { get; set; }
        public string Content { get; set; }
        public RelayCommand ClosingCmd { get; set; }
        public RelayCommand PlayCmd { get; set; }
        public RelayCommand StopCmd { get; set; }

        private void WaveOutEventOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (Application.Current != null)
            {
                var dispatcher = Application.Current.Dispatcher;
                if (dispatcher != null)
                {
                    dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
                }
            }
            lock (_waveOutEvent)
            {
                Monitor.Pulse(_waveOutEvent);
            }
        }

        private bool CanStop(object arg)
        {
            return _waveOutEvent.PlaybackState == PlaybackState.Playing;
        }

        private void Stop(object obj)
        {
            _waveOutEvent.Stop();
        }

        private bool CanPlay(object o)
        {
            return !string.IsNullOrEmpty(Content) && _waveOutEvent.PlaybackState != PlaybackState.Playing;
        }

        private void Play(object o)
        {
            if (_playInProgress) return;
            _playInProgress = true;
            Task.Factory.StartNew(() =>
            {
                var ttsData = RetrieveGoogleTTSData();
                using (var ms = new MemoryStream(ttsData))
                {
                    using (var mp3 = new Mp3FileReader(ms))
                    {
                        _waveOutEvent.Init(mp3);
                        _waveOutEvent.Play();
                        if (Application.Current != null)
                        {
                            var dispatcher = Application.Current.Dispatcher;
                            if (dispatcher != null)
                            {
                                dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
                            }
                        }
                        lock (_waveOutEvent)
                        {
                            if (_waveOutEvent.PlaybackState == PlaybackState.Playing)
                            {
                                Monitor.Wait(_waveOutEvent);
                            }
                        }
                    }
                }
                _playInProgress = false;
            });
        }

        private byte[] RetrieveGoogleTTSData()
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("referer", "https://translate.google.com");
                webClient.Headers.Add("user-agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");

                return
                    webClient.DownloadData(string.Format("{0}?ie=UTF-8&q={1}&tl={2}&client=tw-ob", GoogleTTSApiUrl,
                        Content,
                        LanguageCodes[SelectedLangIdx]));
            }
        }
    }
}