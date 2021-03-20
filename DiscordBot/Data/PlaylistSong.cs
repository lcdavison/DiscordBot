using System;
using System.Collections.Generic;
using System.Text;
using YoutubeExplode.Videos.Streams;
using System.Threading.Tasks;

namespace DiscordBot.Data
{
    public class PlaylistSong
    {
        public string Name { get; set; }

        public TimeSpan CurrentTime { get; set; }

        public Task<string> DownloadSongTask { get; set; }
    }
}
