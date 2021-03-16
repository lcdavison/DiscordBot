using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Data;
using Discord.Audio;

namespace DiscordBot.Music
{
    public interface IMusicPlayer
    {
        Task Play(IAudioClient audioClient);

        Task Pause(IAudioClient audioClient);

        Task Stop(IAudioClient audioClient);

        void QueueSong(PlaylistSong song);

        PlaylistSong[] GetPlaylistSongs();

        bool IsPlaying();
    }
}
