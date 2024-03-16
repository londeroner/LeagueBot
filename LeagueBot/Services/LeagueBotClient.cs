using LeagueBot.Infrastructure;
using LeagueBot.Infrastructure.Repositories.Interfaces;
using LeagueBot.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text;

namespace LeagueBot.Services
{
    public class LeagueBotClient : ILeagueBotClient
    {
        private readonly CancellationTokenSource _cts = new ();
        private readonly TelegramBotClient _client;
        private readonly ReceiverOptions _receiverOptions;
        private readonly ILogger<LeagueBotClient> _logger;
        private readonly IGenericRepository<Domain.Chat> _genericRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IConfiguration _configuration;

        private readonly string _leagueBotUsername;
        
        public LeagueBotClient(ILogger<LeagueBotClient> logger, IConfiguration configuration, 
            IGenericRepository<Domain.Chat> repository, IChatRepository chatRepository)
        {
            _configuration = configuration;
            var token = _configuration.GetValue<string>("BotToken");

            _genericRepository = repository;
            _chatRepository = chatRepository;
            _logger = logger;
            _client = new TelegramBotClient(token);
            _leagueBotUsername = configuration.GetValue<string>("BotUsername");
            _receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.PollAnswer }
            };

            _client.StartReceiving(updateHandler: HandleMessageUpdateAsync,
                pollingErrorHandler: HandleErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: _cts.Token);

            _logger.LogInformation("Bot started");
        }

        public async void StartPoll(string chatId)
        {
            await _client.SendPollAsync(
                chatId: chatId,
                question: "Кто играет в лигу сегодня?",
                allowsMultipleAnswers: false,
                options: new[] { "Играю", "Не играю" },
                isAnonymous: false);
        }

        public void NotifyPollUsers(string chatId, IEnumerable<string> userNames)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var user in userNames)
                builder.Append($"@{user} ");

            SendMessage(builder.ToString(), chatId);
        }

        private async Task HandleMessageUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is { } message)
                {
                    var chatId = message.Chat.Id;

                    // Если добавили в новый чат
                    if (message.NewChatMembers != null && message.NewChatMembers.Length > 0)
                    {
                        if (message.NewChatMembers.Select(x => x.Username).Contains(_leagueBotUsername))
                        {
                            await _client.SendTextMessageAsync(chatId,
                                text: $"Привет! Я - ЛигаБот!\nЯ буду помогать вам собирать пати для игры в лигу по вечерам!" +
                                $"\nЕсть несколько команд для взаимодействия со мной! Чтобы отправить команду, начните сообщения с @, укажите меня, и напишите команду через пробел.\n" +
                                $"Например: @{_leagueBotUsername} help",
                                cancellationToken: cancellationToken);

                            CreateChat(chatId.ToString());
                        }
                    }
                    // Если удалили из нового чата
                    else if (message.LeftChatMember != null && message.LeftChatMember.Username == _leagueBotUsername)
                    {
                        DeleteChat(chatId.ToString());
                    }

                    // Если отправили команду боту
                    if (update.Message.Text.StartsWith($"@{_leagueBotUsername}"))
                        HandleCommand(update.Message.Text, chatId.ToString());

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private void HandleCommand(string message, string chatId)
        {
            var str = message.Split(" ");
            if (str.Length > 2)
            {
                SendMessage("Извините, но я пока умею обрабатывать только одну команду за раз!", chatId);
                return;
            }

            var command = str[1];
            switch (command)
            {
                case "help":
                    SendMessage("Вот полный список моих команд:\n\n*help* \\- показывает список доступных команд\n" +
                        "*activate* \\- активирует меня для этого чата, после чего я начну присылать голосования на участие в игре\n" +
                        "*deactivate* \\- выключает меня для этого чата, чтобы прекратить создание голосований", chatId);
                    break;
                case "activate":
                    _chatRepository.ActivateChat(chatId);
                    SendMessage($"Ура, теперь я буду присылать голосования на участие в игре в {_configuration.GetValue<string>("DefaultTimeToStartVote")} " +
                        $"и отмечать проголосовавших \"За\" участников в {_configuration.GetValue<string>("DefaultTimeToStartGame")!}", chatId);
                    break;
                case "deactivate":
                    _chatRepository.DeactivateChat(chatId);
                    SendMessage($"Меня отключили \\:\\(\nДо повторной активации я не буду создавать голосования", chatId);
                    break;
                case "starttestpoll":
                    StartPoll(chatId);
                    break;
                default:
                    SendMessage("Пока я не знаю такой команды.", chatId);
                    break;
            }
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

        private async void SendMessage(string message, string chatId)
        {
            await _client.SendTextMessageAsync(
                    chatId: chatId,
                    parseMode: ParseMode.MarkdownV2,
                    text: message);
        }

        private void CreateChat(string chatId)
        {
            _genericRepository.Add(new Domain.Chat() { ChatId = chatId });
        }

        private void DeleteChat(string chatId)
        {
            _chatRepository.RemoveChatByChatId(chatId);
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
