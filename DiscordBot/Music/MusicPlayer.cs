using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Audio;
using DiscordBot.Data;
using System.Diagnostics;

namespace DiscordBot.Music
{
    public class MusicPlayer : IMusicPlayer
    {
        private readonly IPlaylist _playlist;

        private PlaylistSong _currentSong;

        private bool _isPlaying;
        private bool _isPaused;

        public event Func<PlaylistSong, Task> OnNextSong;

        public MusicPlayer()
        {
            _playlist = new Playlist();
            _isPlaying = false;
            _isPaused = false;
        }

        public async Task Play(IAudioClient audioClient)
        {
            _isPlaying = true;

            while(_isPlaying)
            {
                if(_isPaused)
                {
                    _isPaused = false;
                }
                else
                {
                    _currentSong = _playlist.NextSong();
                }

                if(_currentSong is { })
                {
                    await PlaySong(audioClient, _currentSong);
                }
                else
                {
                    _isPlaying = false;
                }
            }
        }

        private async Task PlaySong(IAudioClient audioClient, PlaylistSong song)
        {
            string filePath = await song.DownloadSongTask;

            using (var ffmpeg = CreateAudioProcess(filePath))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                await output.CopyToAsync(discord);
                await output.FlushAsync().ConfigureAwait(false);
            }
        }

        private Process CreateAudioProcess(string songFilePath)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{songFilePath}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        public async Task Pause(IAudioClient audioClient)
        {
            //  TODO : Need to figure out how to pause properly


            _isPlaying = false;
            _isPaused = true;
        }

        public async Task Stop(IAudioClient audioClient)
        {
            await audioClient.StopAsync();
            
            _isPlaying = false;
        }

        public void QueueSong(PlaylistSong song)
        {
            _playlist.AddSong(song);
        }

        public PlaylistSong[] GetPlaylistSongs()
        {
            return _playlist.GetSongs();
        }

        public bool IsPlaying()
        {
            return _isPlaying;
        }
    }
}
