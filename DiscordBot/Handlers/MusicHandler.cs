using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Discord.Audio;
using Discord;
using System.Threading.Tasks;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Diagnostics;
using DiscordBot.Data;

namespace DiscordBot.Handlers
{
    public class MusicHandler
    {
        private const string DOWNLOAD_BASE_PATH = @"G:\YoutubeAudio\";

        private IAudioClient _audioClient;
        private YoutubeClient _youtubeClient;
        private ConcurrentQueue<PlaylistSong> _playlist;

        public MusicHandler(YoutubeClient youtubeClient)
        {
            _youtubeClient = youtubeClient;
            _playlist = new ConcurrentQueue<PlaylistSong>();
        }

        public async Task<bool> JoinVoiceChannel(IVoiceChannel voiceChannel)
        {
            bool isAlreadyConnected = false;

            if (_audioClient is { })
            {
                isAlreadyConnected = true;
            }
            else
            {
                _audioClient = await voiceChannel.ConnectAsync();
            }

            return isAlreadyConnected;
        }

        public async Task QueueSong(string videoId)
        {
            var videoTitle = await GetVideoName(videoId);

            var audioStreamInfo = await GetAudioStreamInfo(videoId);

            _playlist.Enqueue(new PlaylistSong
            {
                Name = videoTitle,
                AudioStreamInfo = audioStreamInfo
            });
        }

        private async Task<string> GetVideoName(string videoId)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoId);
            return video.Title;
        }

        private async Task<IStreamInfo> GetAudioStreamInfo(string videoId)
        {
            var videoStreamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);

            var audioStreamInfo = videoStreamManifest.GetAudioOnly().WithHighestBitrate();

            if (audioStreamInfo is null)
            {
                throw new NullReferenceException($"{nameof(audioStreamInfo)} is null");
            }

            return audioStreamInfo;
        }

        public bool IsPlaylistEmpty()
        {
            return _playlist.IsEmpty;
        }

        public async Task PlayPlaylist()
        {
            while(_playlist.Count > 0)
            {
                if(_playlist.TryDequeue(out var song))
                {
                    await PlaySong(song);
                }
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        private async Task PlaySong(PlaylistSong song)
        {
            var streamInfo = song.AudioStreamInfo;

            string path = DOWNLOAD_BASE_PATH + $"audio.{streamInfo.Container.Name}";
            await _youtubeClient.Videos.Streams.DownloadAsync(streamInfo, path);

            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = _audioClient.CreatePCMStream(AudioApplication.Music))
            {
                await output.CopyToAsync(discord);
                await output.FlushAsync().ConfigureAwait(false);
            }
        }

        public async Task<PlaylistSong[]> GetPlaylist()
        {
            return _playlist.ToArray();
        }
    }
}