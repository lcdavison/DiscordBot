using System;
using System.Collections.Generic;
using System.Text;
using YoutubeExplode.Videos.Streams;

namespace DiscordBot.Data
{
    public class PlaylistSong
    {
        public string Name { get; set; }

        public string FilePath { get; set; }

        public IStreamInfo AudioStreamInfo { get; set; }
    }
}
