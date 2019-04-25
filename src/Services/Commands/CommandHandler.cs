﻿#pragma warning disable 1591

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sanakan.Config;
using Sanakan.Extensions;
using Sanakan.Services.Executor;
using Shinden.Logger;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Sanakan.Services.Commands
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private IServiceProvider _provider;
        private CommandService _cmd;
        private IExecutor _executor;
        private ILogger _logger;
        private IConfig _config;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, IConfig config, ILogger logger, IExecutor executor)
        {
            _client = client;
            _config = config;
            _logger = logger;
            _executor = executor;
            _provider = provider;
            _cmd = new CommandService();
        }

        public async Task InitializeAsync()
        {
            await _cmd.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            if (msg == null) return;

            if (msg.Author.IsBot || msg.Author.IsWebhook) return;

            int argPos = 0;
            if (msg.HasStringPrefix(_config.Get().Prefix, ref argPos))
            {
                var context = new SocketCommandContext(_client, msg);
                var res = await _cmd.GetExecutableCommandAsync(context, argPos, _provider);

                if (res.IsSuccess())
                {
                    switch (res.Command.Match.Command.RunMode)
                    {
                        case RunMode.Async:
                            await res.Command.ExecuteAsync(_provider);
                            break;

                        default:
                        case RunMode.Sync:
                            var timer = Stopwatch.StartNew();
                            while (!_executor.TryAdd(res.Command))
                            {
                                await Task.Delay(10);
                                if (timer.ElapsedMilliseconds > 1000)
                                {
                                    await context.Channel.SendMessageAsync("", embed: "Przekroczono czas oczekiwania!".ToEmbedMessage(EMType.Error).Build());
                                    break;
                                }
                            }
                            break;
                    }
                }
                else if (res.Result != null)
                {
                    switch (res.Result.Error)
                    {
                        case CommandError.MultipleMatches:
                            await context.Channel.SendMessageAsync("", embed: "Dopasowano wielu użytkowników!".ToEmbedMessage(EMType.Error).Build());
                            break;

                        case CommandError.ParseFailed:
                        case CommandError.BadArgCount:
                            await context.Channel.SendMessageAsync($"Help need!");
                            break;

                        case CommandError.UnmetPrecondition:
                            if (res.Result.ErrorReason.StartsWith("|IMAGE|"))
                            {
                                var emb = new EmbedBuilder()
                                    .WithImageUrl(res.Result.ErrorReason.Remove(0, 7))
                                    .WithColor(EMType.Error.Color());

                                await context.Channel.SendMessageAsync("", embed: emb.Build());
                            }
                            else await context.Channel.SendMessageAsync("", embed: res.Result.ErrorReason.ToEmbedMessage(EMType.Error).Build());
                            break;

                        default:
                            _logger.Log(res.Result.ErrorReason);
                            break;
                    }
                }
            }
        }
    }
}