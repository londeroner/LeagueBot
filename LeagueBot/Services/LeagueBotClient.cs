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
        private readonly IGenericRepository<Domain.Chat> _genericChatRepository;
        private readonly IGenericRepository<Domain.User> _genericUserRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IConfiguration _configuration;
        private readonly IPollRepository _pollRepository;

        private readonly string _leagueBotUsername;
        
        public LeagueBotClient(ILogger<LeagueBotClient> logger, IConfiguration configuration, 
            IGenericRepository<Domain.Chat> genericChatRepository, IChatRepository chatRepository,
            IGenericRepository<Domain.User> genericUserRepository, IUserRepository userRepository,
            IPollRepository pollRepository)
        {
            _configuration = configuration;
            var token = _configuration.GetValue<string>("BotToken");

            _genericChatRepository = genericChatRepository;
            _genericUserRepository = genericUserRepository;
            _pollRepository = pollRepository;
            _userRepository = userRepository;
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

        public async Task<string> StartPollAsync(string chatId)
        {
            var poll = await _client.SendPollAsync(
                chatId: chatId,
                question: "Кто играет в лигу сегодня?",
                allowsMultipleAnswers: false,
                options: new[] { "Играю", "Не играю" },
                isAnonymous: false);

            return poll.Poll.Id;
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
                                $"\nЕсть несколько команд для взаимодействия со мной! Чтобы отправить команду, начните сообщение с @, укажите меня, и напишите команду через пробел.\n" +
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
                    if (update.Message.Text?.StartsWith($"@{_leagueBotUsername}") ?? false)
                        HandleCommand(update.Message.Text, chatId.ToString());

                }
                if (update.PollAnswer is { } pollAnswer)
                {
                    var user = _userRepository.GetUserByUserName(pollAnswer.User.Username);
                    if (user == null)
                    {
                        user = new Domain.User() { UserName = pollAnswer.User.Username };
                        _genericUserRepository.Add(user);
                    }

                    _pollRepository.AddPollAnswer(user, pollAnswer.OptionIds[0], pollAnswer.PollId);
                    _logger.LogInformation($"Ответ пользователя {user.UserName} с результатом {pollAnswer.OptionIds[0]} был зарегистрирован");
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
                SendMessage("Извините, но я пока умею обрабатывать только команды без параметров!", chatId);
                return;
            }

            var command = str[1];
            switch (command)
            {
                case "help":
                    SendMessageWithParseMode("Вот полный список моих команд:\n\n*help* \\- показывает список доступных команд\n" +
                        "*activate* \\- активирует меня для этого чата, после чего я начну присылать голосования на участие в игре\n" +
                        "*deactivate* \\- выключает меня для этого чата, чтобы прекратить создание голосований\n" +
                        "*version* \\- текущая версия", chatId);
                    break;
                case "activate":
                    _chatRepository.ActivateChat(chatId);
                    SendMessage($"Ура, теперь я буду присылать голосования на участие в игре в {_configuration.GetValue<string>("DefaultTimeToStartVote")} " +
                        $"и отмечать проголосовавших \"За\" участников в {_configuration.GetValue<string>("DefaultTimeToStartGame")} (С задержкой до 5 минут)", chatId);
                    break;
                case "deactivate":
                    _chatRepository.DeactivateChat(chatId);
                    SendMessage($"Меня отключили :(\nДо повторной активации я не буду создавать голосования", chatId);
                    break;
                case "version":
                    SendMessage($"Текущая версия бота 0.1", chatId);
                    break;
                default:
                    SendMessage("Пока я не знаю такой команды", chatId);
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
            try
            {
                await _client.SendTextMessageAsync(
                        chatId: chatId,
                        text: message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        private async void SendMessageWithParseMode(string message, string chatId)
        {
            try
            {
                await _client.SendTextMessageAsync(
                        chatId: chatId,
                        parseMode: ParseMode.MarkdownV2,
                        text: message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        private void CreateChat(string chatId)
        {
            _genericChatRepository.Add(new Domain.Chat() { ChatId = chatId });
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
