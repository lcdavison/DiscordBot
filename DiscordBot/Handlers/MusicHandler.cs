using Discord;
using Discord.Audio;
using DiscordBot.Data;
using DiscordBot.Music;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Handlers
{
    public class MusicHandler
    {
        private IAudioClient _audioClient;
        private IMusicPlayer _musicPlayer;

        private YouTubeHandler _youtubeHandler;

        public MusicHandler(YouTubeHandler youtubeHandler)
        {
            _youtubeHandler = youtubeHandler;
            _musicPlayer = new MusicPlayer();
        }

        public async Task JoinVoiceChannel(IVoiceChannel voiceChannel)
        {
            if (_audioClient is null)
            {
                Console.WriteLine($"Connecting To Voice Channel...");

                _audioClient = await voiceChannel.ConnectAsync();

                _audioClient.Disconnected += OnBotDisconnect;

                Console.WriteLine($"Connected To Voice Channel {voiceChannel.Name}");
            }
        }

        private async Task OnBotDisconnect(Exception exception)
        {
            Console.WriteLine($"Disconnected : {exception.Message}");

            await _audioClient.StopAsync();

            _audioClient = null;

            return;
        }

        public void PlaySong(VideoMetadata songVideo)
        {
            if (!_musicPlayer.IsPlaying())
            {
                QueueSong(songVideo);

                PlayPlaylist();
            }
            else
            {
                QueueSong(songVideo);
            }
        }

        private void QueueSong(VideoMetadata songVideo)
        {
            _musicPlayer.QueueSong(new PlaylistSong()
            {
                Name = songVideo.Title,
                DownloadSongTask = _youtubeHandler.DownloadSong(songVideo)
            });
        }

        public async Task PlayPlaylist()
        {
            await CheckAudioClientAndExecute(_musicPlayer.Play);
        }

        public async Task PausePlaylist()
        {
            await CheckAudioClientAndExecute(_musicPlayer.Pause);
        }

        public async Task StopPlaylist()
        {
            await CheckAudioClientAndExecute(_musicPlayer.Stop);
        }

        private async Task CheckAudioClientAndExecute(Func<IAudioClient, Task> musicPlayerAction)
        {
            if(_audioClient is { })
            {
                await musicPlayerAction(_audioClient);
            }
        }

        public PlaylistSong[] GetPlaylistSongs()
        {
            return _musicPlayer.GetPlaylistSongs();
        }
    }
}