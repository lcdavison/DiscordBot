using Discord;
using Discord.Commands;
using DiscordBot.Handlers;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static DiscordBot.Data.YouTubeSearchResult;

namespace DiscordBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly MusicHandler _musicHandler;

        private static IMessage _searchResultMessage;

        public MusicModule(MusicHandler musicHandler)
        {
            _musicHandler = musicHandler;
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCommand()
        {
            var voiceChannel = (Context.User as IVoiceState).VoiceChannel;

            _musicHandler.JoinVoiceChannel(voiceChannel);
        }

        [Command("queue", RunMode=RunMode.Async)]
        [Alias("q")]
        public async Task QueueCommand()
        {
            var playlist = await _musicHandler.GetPlaylist();

            var embedBuilder = new EmbedBuilder();
            embedBuilder.Color = new Color(0, 128, 128);
            embedBuilder.Title = "Queued Songs";

            var playlistStringBuilder = new StringBuilder();

            foreach(var song in playlist)
            {
                playlistStringBuilder.Append(song.Name);
            }

            string playlistContent = playlistStringBuilder.ToString();

            embedBuilder.Description = playlistContent;
            var playlistEmbed = embedBuilder.Build();

            await ReplyAsync(embed: playlistEmbed);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        public async Task PlayCommand([Remainder] string input)
        {
            bool isJustNumber = int.TryParse(input, out int videoIndex) && input.Length < 2;

            if (isJustNumber)
            {
                await HandleVideoIndexInput(videoIndex);
            }
            else
            {
                Videos.Clear();

                await PerformYouTubeSearch(input);

                await SendVideoResults();
            }
        }

        private async Task HandleVideoIndexInput(int videoIndex)
        {
            if (Videos.Count == 0)
            {
                await PerformYouTubeSearch(videoIndex.ToString());

                await SendVideoResults();
            }
            else
            {
                await DeleteSearchMessages();

                var selectedVideo = Videos[videoIndex - 1];

                await _musicHandler.QueueSong(selectedVideo.ID);

                JoinAndStartPlaylist();

                Videos.Clear();
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
            using (var streamReader = new StreamReader("youtube-api-token.txt"))
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
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Color = new Color(0, 0, 255);
            embedBuilder.Title = "Search Results";

            var resultStringBuilder = new StringBuilder();

            resultStringBuilder.AppendLine($"1 : {Videos[0].Title}");
            resultStringBuilder.AppendLine($"2 : {Videos[1].Title}");
            resultStringBuilder.AppendLine($"3 : {Videos[2].Title}");
            resultStringBuilder.AppendLine($"4 : {Videos[3].Title}");
            resultStringBuilder.AppendLine($"5 : {Videos[4].Title}");

            var embedContent = resultStringBuilder.ToString();
            embedBuilder.Description = embedContent;

            _searchResultMessage = await ReplyAsync(embed: embedBuilder.Build());
        }

        private async Task DeleteSearchMessages()
        {
            await Context.Message.DeleteAsync();

            await _searchResultMessage.DeleteAsync();
        }

        private async Task JoinAndStartPlaylist()
        {
            var voiceChannel = (Context.User as IVoiceState).VoiceChannel;

            Task<bool> joinVoiceChannel;
            if (voiceChannel is { })
            {
                joinVoiceChannel = _musicHandler.JoinVoiceChannel(voiceChannel);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Must be connected to a voice channel.");
                return;
            }

            bool isAlreadyConnected = await joinVoiceChannel;

            if (isAlreadyConnected)
            {
                return;
            }
            else
            {
                _musicHandler.PlayPlaylist();
            }
        }
    }
}
