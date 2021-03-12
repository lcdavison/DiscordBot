using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Data
{
    internal static class YouTubeSearchResult
    {
        public static List<(string ID, string Title)> Videos { get; set; } = new List<(string, string)>();
    }
}
