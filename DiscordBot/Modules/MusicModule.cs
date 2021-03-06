using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using static DiscordBot.Data.YouTubeSearchResult;

namespace DiscordBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        [Command("play")]
        public async Task PlayCommand([Remainder] string input)
        {
            if (Videos.Count == 0)
            {
                await PerformYouTubeSearch(input);

                await Context.Channel.SendMessageAsync(
                $"1 : {Videos[0]}\n" +
                $"2 : {Videos[1]}\n" +
                $"3 : {Videos[2]}\n" +
                $"4 : {Videos[3]}\n" +
                $"5 : {Videos[4]}");
            }
            else
            {
                int videoIndex = int.Parse(input);
                videoIndex -= 1;

                Console.WriteLine($"{Videos[videoIndex]}");

                Videos.Clear();
            }

            Console.WriteLine(Videos.Count);
        }

        private async Task PerformYouTubeSearch(string keyword)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = LoadYouTubeAPIToken(),
                ApplicationName = this.GetType().ToString()
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = keyword;
            searchRequest.MaxResults = 5;

            var searchResponse = await searchRequest.ExecuteAsync();

            foreach (var searchItem in searchResponse.Items)
            {
                switch (searchItem.Id.Kind)
                {
                    case "youtube#video":
                        Videos.Add($"{searchItem.Snippet.Title}");
                        break;
                }
            }
        }

        private string LoadYouTubeAPIToken()
        {
            string token;
            using(var streamReader = new StreamReader("youtube-api-token.txt"))
            {
                token = streamReader.ReadLine();
            }

            return token;
        }
    }
}
