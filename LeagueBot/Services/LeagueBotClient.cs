using LeagueBot.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace LeagueBot.Services
{
    public class LeagueBotClient : ILeagueBot
    {
        private readonly CancellationTokenSource _cts = new ();
        private readonly TelegramBotClient _client;
        private readonly ReceiverOptions _receiverOptions;
        private readonly ILogger<LeagueBotClient> _logger;
        
        public LeagueBotClient(ILogger<LeagueBotClient> logger ,IConfiguration configuration)
        {
            var token = configuration.GetValue<string>("BotToken");

            _logger = logger;
            _client = new TelegramBotClient(token);
            _receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _client.StartReceiving(updateHandler: HandleMessageUpdateAsync,
                pollingErrorHandler: HandleErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: _cts.Token);

            _logger.LogInformation("Bot started");
        }

        private async Task HandleMessageUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            //if (ActivePoll is not { })
            //    ActivePoll = await _client.SendPollAsync(chatId, "Test question", new List<string>() { "variant 1", "variant 2" }, cancellationToken: cancellationToken, isAnonymous: false);

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // Echo received message text
            Message sentMessage = await _client.SendTextMessageAsync(
                chatId: chatId,
                text: $"@{message.From.Username}",
                cancellationToken: cancellationToken);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

#if DEBUG
        public async void WriteTestMessage()
        {
            var me = await _client.GetMeAsync();
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }
#endif
    }
}
