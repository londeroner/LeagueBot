using LeagueBot.Domain;
using LeagueBot.Infrastructure.Factories;
using LeagueBot.Infrastructure.Repositories.Interfaces;
using LeagueBot.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueBot.Services
{
    public class LifeTimeCycle : ILifeTimeCycle
    {
        private readonly IGenericRepository<DailyPoll> _repository;
        private readonly IPollRepository _pollRepository;
        private readonly IChatRepository _chatRepository;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly ILeagueBotClient _client;

        private readonly Timer _timer;
        private const int _timerUpdate = 1000 * 60 * 5; // 5 minutes;

        public LifeTimeCycle(ILogger<LifeTimeCycle> logger,
                             ILeagueBotClient client, 
                             IGenericRepository<DailyPoll> repository,
                             IPollRepository pollRepository,
                             IChatRepository chatRepository,
                             IConfiguration config)
        {
            _repository = repository;
            _pollRepository = pollRepository;
            _config = config;
            _client = client;
            _logger = logger;
            _chatRepository = chatRepository;

            TimerCallback tm = new TimerCallback(Update);
            _timer = new Timer(tm, null, 0, _timerUpdate);
        }

        public async void Update(object param)
        {
            try
            {
                var activeChats = _chatRepository.GetAllActiveChats().ToList();

                foreach (var activeChat in activeChats)
                {
                    var dailyPoll = _pollRepository.GetTodayPollByChatId(activeChat.ChatId);

                    if (dailyPoll is not { })
                    {
                        dailyPoll = DailyPollFactory.CreateDailyPoll(
                            activeChat.ChatId,
                            TimeOnly.Parse(_config.GetValue<string>("DefaultTimeToStartVote")),
                            TimeOnly.Parse(_config.GetValue<string>("DefaultTimeToStartGame"))
                        );

                        _repository.Add(dailyPoll);
                    }

                    var timeToStartGameVoteDifference = DateTime.Now.TimeOfDay.TotalMinutes - dailyPoll.TimeToStartGame.ToTimeSpan().TotalMinutes;
                    if (timeToStartGameVoteDifference > 0 && dailyPoll.PollIsStarted && !dailyPoll.PollIsFinished)
                    {
                        var usersToNotify = dailyPoll.PollUserResults.Where(p => p.PollResult == PollResult.Yes).Select(u => u.User.UserName);
                        _client.NotifyPollUsers(dailyPoll.ChatId, usersToNotify);
                        _pollRepository.StopPoll(dailyPoll);
                    }

                    var timeToStartVoteDifference = DateTime.Now.TimeOfDay.TotalMinutes - dailyPoll.TimeToStartVote.ToTimeSpan().TotalMinutes;
                    if (timeToStartVoteDifference > 0 && !dailyPoll.PollIsStarted && !dailyPoll.PollIsFinished)
                    {
                        var pollId = await _client.StartPollAsync(dailyPoll.ChatId);
                        _pollRepository.StartPoll(dailyPoll, pollId);
                    }
                }

                _logger.LogInformation($"LifeTimeCycle completed at {DateTime.Now:T}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
