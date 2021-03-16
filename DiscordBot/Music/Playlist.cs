using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Data;
using System.Collections.Concurrent;

namespace DiscordBot.Music
{
    public class Playlist : IPlaylist
    {
        private readonly ConcurrentQueue<PlaylistSong> _songQueue;

        public Playlist()
        {
            _songQueue = new ConcurrentQueue<PlaylistSong>();
        }

        public void AddSong(PlaylistSong song)
        {
            _songQueue.Enqueue(song);
        }

        public PlaylistSong NextSong()
        {
            if(_songQueue.TryDequeue(out var song))
            {
                return song;
            }
            else
            {
                return null;
            }
        }

        public bool IsPlaylistEmpty()
        {
            return _songQueue.IsEmpty;
        }

        public PlaylistSong[] GetSongs()
        {
            return _songQueue.ToArray();
        }
    }
}
