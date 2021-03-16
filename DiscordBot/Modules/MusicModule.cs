using Discord;
using Discord.Commands;
using DiscordBot.Handlers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly MusicHandler _musicHandler;
        private readonly YouTubeHandler _youTubeHandler;

        private static IMessage _searchResultMessage;

        public MusicModule(MusicHandler musicHandler, YouTubeHandler youtuberHandler)
        {
            _musicHandler = musicHandler;
            _youTubeHandler = youtuberHandler;
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCommand()
        {
            var voiceChannel = (Context.User as IVoiceState).VoiceChannel;

            if(voiceChannel is { })
            {
                _musicHandler.JoinVoiceChannel(voiceChannel);
            }
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        public async Task QueueCommand()
        {
            var playlist = _musicHandler.GetPlaylistSongs();

            var embedBuilder = new EmbedBuilder();
            embedBuilder.Color = new Color(0, 128, 128);
            embedBuilder.Title = "Queued Songs";

            var playlistStringBuilder = new StringBuilder();

            foreach (var song in playlist)
            {
                playlistStringBuilder.AppendLine(song.Name);
            }

            string playlistContent = playlistStringBuilder.ToString();

            embedBuilder.Description = playlistContent;
            var playlistEmbed = embedBuilder.Build();

            await ReplyAsync(embed: playlistEmbed);
        }

        [Command("pause", RunMode = RunMode.Async)]
        public async Task PauseCommand()
        {
            _musicHandler.PausePlaylist();
        }

        [Command("unpause", RunMode = RunMode.Async)]
        [Alias("play", "p")]
        public async Task UnpauseCommand()
        {
            await JoinCurrentVoiceChannel();

            _musicHandler.PlayPlaylist();
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopCommand()
        {
            _musicHandler.StopPlaylist();
        }

        [Command("search", RunMode = RunMode.Async)]
        [Alias("s")]
        public async Task SearchCommand([Remainder] string searchQuery)
        {
            await PerformYouTubeSearch(searchQuery);

            SendVideoResults();
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
                await PerformYouTubeSearch(input);

                SendVideoResults();
            }
        }

        private async Task HandleVideoIndexInput(int videoIndex)
        {
            var videos = _youTubeHandler.SearchResults;

            if (videos.Count == 0)
            {
                await PerformYouTubeSearch(videoIndex.ToString());

                SendVideoResults();
            }
            else
            {
                DeleteSearchMessages();

                var selectedVideo = videos[videoIndex - 1];

                Context.Channel.SendMessageAsync($"Added Song To Queue: {selectedVideo.Title}");

                await JoinCurrentVoiceChannel();

                _musicHandler.PlaySong(selectedVideo);

                videos.Clear();
            }
        }

        private async Task PerformYouTubeSearch(string keyword)
        {
            await _youTubeHandler.SearchForVideos(keyword);
        }

        private async Task SendVideoResults()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Color = new Color(0, 128, 200);
            embedBuilder.Title = "Search Results";

            var resultStringBuilder = new StringBuilder();

            var videos = _youTubeHandler.SearchResults;

            resultStringBuilder.AppendLine($"1 : {videos[0].Title}");
            resultStringBuilder.AppendLine($"2 : {videos[1].Title}");
            resultStringBuilder.AppendLine($"3 : {videos[2].Title}");
            resultStringBuilder.AppendLine($"4 : {videos[3].Title}");
            resultStringBuilder.AppendLine($"5 : {videos[4].Title}");

            var embedContent = resultStringBuilder.ToString();
            embedBuilder.Description = embedContent;

            _searchResultMessage = await ReplyAsync(embed: embedBuilder.Build());
        }

        private async Task DeleteSearchMessages()
        {
            await Context.Message.DeleteAsync();

            await _searchResultMessage.DeleteAsync();
        }

        private async Task JoinCurrentVoiceChannel()
        {
            var voiceChannel = (Context.User as IVoiceState).VoiceChannel;

            if (voiceChannel is { })
            {
                await _musicHandler.JoinVoiceChannel(voiceChannel);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Must be connected to a voice channel.");
                return;
            }
        }
    }
}
