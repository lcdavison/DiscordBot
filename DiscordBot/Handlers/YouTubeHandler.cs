using System;
using System.Collections.Generic;
using System.Text;
using YoutubeExplode;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using Google.Apis.YouTube.v3;
using System.IO;
using Google.Apis.Services;
using DiscordBot.Data;

namespace DiscordBot.Handlers
{
    public class YouTubeHandler
    {
        private const string DOWNLOAD_BASE_PATH = @"G:\YoutubeAudio\";

        private readonly YoutubeClient _youtubeClient;

        private readonly List<VideoMetadata> _searchResults;

        public List<VideoMetadata> SearchResults 
        { 
            get 
            { 
                return _searchResults; 
            }
        }

        public YouTubeHandler()
        {
            _youtubeClient = new YoutubeClient();
            _searchResults = new List<VideoMetadata>();
        }

        public async Task<List<VideoMetadata>> SearchForVideos(string searchQuery)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = LoadYouTubeAPIToken(),
                ApplicationName = this.GetType().ToString()
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = searchQuery;
            searchRequest.MaxResults = 5;
            searchRequest.Type = "video";

            var searchResponse = await searchRequest.ExecuteAsync();

            _searchResults.Clear();

            foreach (var searchItem in searchResponse.Items)
            {
                switch (searchItem.Id.Kind)
                {
                    case "youtube#video":
                        _searchResults.Add(new VideoMetadata()
                        {
                            Id = searchItem.Id.VideoId,
                            Title = searchItem.Snippet.Title
                        });
                        break;
                }
            }

            return _searchResults;
        }

        private string LoadYouTubeAPIToken()
        {
            string token;
            using (var streamReader = new StreamReader("youtube-api-token.txt"))
            {
                token = streamReader.ReadLine();
            }

            return token;
        }

        public async Task<string> DownloadSong(VideoMetadata songVideo)
        {
            var audioStreamInfo = await GetAudioStreamInfo(songVideo.Id);

            string path = DOWNLOAD_BASE_PATH + $"{songVideo.Title}.{audioStreamInfo.Container.Name}";

            Console.WriteLine($"Downloading Song To {path}");

            await _youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, path);

            return path;
        }

        private async Task<IStreamInfo> GetAudioStreamInfo(string videoId)
        {
            var videoStreamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);

            var audioStreamInfo = videoStreamManifest.GetAudioOnly().WithHighestBitrate();

            if (audioStreamInfo is null)
            {
                throw new NullReferenceException($"{nameof(audioStreamInfo)} is null");
            }

            return audioStreamInfo;
        }
    }
}
