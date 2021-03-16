using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Data;

namespace DiscordBot.Music
{
    public interface IPlaylist
    {
        void AddSong(PlaylistSong song);

        PlaylistSong NextSong();

        bool IsPlaylistEmpty();

        PlaylistSong[] GetSongs();
    }
}
