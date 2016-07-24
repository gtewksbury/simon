using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public class Statistics
    {
        /// <summary>
        /// Gets or sets the number of games played.
        /// </summary>
        public int GamesPlayed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the current round.
        /// </summary>
        public int CurrentRound { get; set; } = 1;

        /// <summary>
        /// Gets or sets the highest round.
        /// </summary>
        public int HighestRound { get; set; } = 1;

        /// <summary>
        /// Gets or sets the total number of hours played
        /// </summary>
        public int TotalHoursPlayed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of minutes played
        /// </summary>
        public int TotalMinutesPlayed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of seconds played
        /// </summary>
        public int TotalSecondsPlayed { get; set; } = 0;

        /// <summary>
        /// Gest or sets the number of minutes for the game.
        /// </summary>
        public int GameMinutesPlayed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of seconds for the game.
        /// </summary>
        public int GameSecondsPlayed { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of times the red button has been clicked.
        /// </summary>
        public int RedClickCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of times the yellow button has been clicked.
        /// </summary>
        public int YellowClickCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of times the white button has been clicked.
        /// </summary>
        public int WhiteClickCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of times the green button has been clicked.
        /// </summary>
        public int GreenClickCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the time of the stats
        /// </summary>
        public DateTime StatsTime { get; set; }

        public async Task Publish()
        {
            await AzureIoTHub.SendDeviceToCloudMessageAsync(this);
        }

        public static Statistics Create(DeviceManager manager)
        {
            var currentRound = manager.CurrentGame == null || manager.CurrentGame.GameOver 
                ? 0 : manager.CurrentGame.RoundNumber;
            var gameDuration = manager.CurrentGame == null || manager.CurrentGame.GameOver 
                ? new TimeSpan(0, 0, 0) : DateTime.Now.Subtract(manager.CurrentGame.StartTime);

            var totalDuration = new TimeSpan(
                0, 
                manager.Stats.HoursPlayed, 
                manager.Stats.MinutesPlayed, 
                manager.Stats.SecondsPlayed);

            var updatedTotalDuration = totalDuration.Add(gameDuration);


            return new Statistics
            {
                GamesPlayed = manager.Stats.GamesPlayed,
                CurrentRound = currentRound,
                StatsTime = DateTime.Now,
                HighestRound = manager.Stats.HighestRound,
                RedClickCount = manager.Stats.RedClickCount,
                GreenClickCount = manager.Stats.GreenClickCount,
                YellowClickCount = manager.Stats.YellowClickCount,
                WhiteClickCount = manager.Stats.WhiteClickCount,
                TotalHoursPlayed = updatedTotalDuration.Hours,
                TotalMinutesPlayed = updatedTotalDuration.Minutes,
                TotalSecondsPlayed = updatedTotalDuration.Seconds,
                GameMinutesPlayed = gameDuration.Minutes,
                GameSecondsPlayed = gameDuration.Seconds
            };
        }
    }
}
