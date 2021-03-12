using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.IO;
using System.Threading.Tasks;
using static DiscordBot.Data.YouTubeSearchResult;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace DiscordBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private const string YOUTUBE_URI = @"https://www.youtube.com/watch?v=";

        [Command("play")]
        public async Task PlayCommand([Remainder] string input)
        {
            bool isNumber = int.TryParse(input, out int videoIndex);

            if(isNumber)
            {
                if(Videos.Count == 0)
                {
                    await PerformYouTubeSearch(input);

                    await SendVideoResults();
                }
                else
                {
                    var youtubeClient = new YoutubeClient();

                    videoIndex -= 1;

                    var selectedVideo = Videos[videoIndex];
                    var videoStreamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(selectedVideo.ID);

                    var audioStreamInfo = videoStreamManifest.GetAudioOnly().WithHighestBitrate();

                    if(audioStreamInfo is null)
                    {
                        throw new NullReferenceException($"{nameof(audioStreamInfo)} is null");
                    }

                    Console.WriteLine($"Audio Stream URL : {audioStreamInfo.Url}");
                    
                    Videos.Clear();
                }
            }
            else
            {
                Videos.Clear();

                await PerformYouTubeSearch(input);

                await SendVideoResults();
            }
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
            searchRequest.Type = "video";

            var searchResponse = await searchRequest.ExecuteAsync();

            foreach (var searchItem in searchResponse.Items)
            {
                Console.WriteLine(searchItem.Id.VideoId);

                switch (searchItem.Id.Kind)
                {
                    case "youtube#video":
                        AddSearchResult(searchItem.Id.VideoId, searchItem.Snippet.Title);
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

        private void AddSearchResult(string videoId, string videoTitle)
        {
            Videos.Add((ID: videoId, Title: videoTitle));
        }

        private async Task SendVideoResults()
        {
            await Context.Channel.SendMessageAsync(
                $"1 : {Videos[0].Title}\n" +
                $"2 : {Videos[1].Title}\n" +
                $"3 : {Videos[2].Title}\n" +
                $"4 : {Videos[3].Title}\n" +
                $"5 : {Videos[4].Title}");
        }
    }
}
