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
using YoutubeExplode.Videos;

namespace DiscordBot.Handlers
{
    public class MusicHandler
    {
        private const string DOWNLOAD_BASE_PATH = @"G:\YoutubeAudio\";

        private IAudioClient _audioClient;
        private YoutubeClient _youtubeClient;
        private ConcurrentQueue<PlaylistSong> _playlist;
        private bool _isPlaying = false;

        public MusicHandler(YoutubeClient youtubeClient)
        {
            _youtubeClient = youtubeClient;
            _playlist = new ConcurrentQueue<PlaylistSong>();
        }

        public async Task JoinVoiceChannel(IVoiceChannel voiceChannel)
        {
            if (_audioClient is { })
            {
                return;
            }
            else
            {
                _audioClient = await voiceChannel.ConnectAsync();
            }
        }

        public async Task QueueSong(string videoId)
        {
            var video = await GetVideoMetadata(videoId);

            _playlist.Enqueue(new PlaylistSong
            {
                Name = video.Title,
                DownloadSongTask = DownloadSong(video.Id, video.Title)
            });
        }

        private async Task<Video> GetVideoMetadata(string videoId)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoId);
            return video;
        }

        private async Task<string> DownloadSong(string videoId, string videoName)
        {
            var audioStreamInfo = await GetAudioStreamInfo(videoId);

            string path = DOWNLOAD_BASE_PATH + $"{videoName}.{audioStreamInfo.Container.Name}";

            await _youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, path);

            return path;
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
            if (!_isPlaying)
            {
                _isPlaying = true;

                while (!_playlist.IsEmpty)
                {
                    if (_playlist.TryDequeue(out var song))
                    {
                        await PlaySong(song);
                    }
                }

                _isPlaying = false;
            }
            else
            {
                Console.WriteLine("Bot is already playing.");
                return;
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
            string filePath = await song.DownloadSongTask;

            using (var ffmpeg = CreateStream(filePath))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = _audioClient.CreatePCMStream(AudioApplication.Music))
            {
                await output.CopyToAsync(discord);
                await output.FlushAsync().ConfigureAwait(false);
            }

            Console.WriteLine("Finished Playing");
        }

        public PlaylistSong[] GetPlaylist()
        {
            return _playlist.ToArray();
        }
    }
}