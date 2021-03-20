using Discord.Audio;
using DiscordBot.Data;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Music
{
    public class MusicPlayer : IMusicPlayer
    {
        private readonly IPlaylist _playlist;

        private PlaylistSong _currentSong;
        private AudioFileReader _currentAudioFileStream;

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
                    await PlaySong(audioClient);
                }
                else
                {
                    _isPlaying = false;
                }
            }
        }

        private async Task PlaySong(IAudioClient audioClient)
        {
            string filePath = await _currentSong.DownloadSongTask;

            _currentAudioFileStream = new AudioFileReader(filePath);

            _currentAudioFileStream.CurrentTime = _currentSong.CurrentTime;

            var outputFormat = new WaveFormat(48000, 2);

            using var resampler = new MediaFoundationResampler(_currentAudioFileStream, outputFormat);

            using (var discord = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                resampler.ResamplerQuality = 60;
                var bufferSize = outputFormat.AverageBytesPerSecond / 60;
                var audioBuffer = new byte[bufferSize];

                int bytesRead;
                while((bytesRead = resampler.Read(audioBuffer, 0, bufferSize)) > 0)
                {
                    if (_isPaused)
                    {
                        return;
                    }

                    if(bytesRead < bufferSize)
                    {
                        for(int i = bytesRead; i < bufferSize; ++i)
                        {
                            audioBuffer[i] = 0;
                        }
                    }

                    await discord.WriteAsync(audioBuffer);
                }
            }
        }

        public async Task Pause(IAudioClient audioClient)
        {
            _currentSong.CurrentTime = _currentAudioFileStream.CurrentTime;
 
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
