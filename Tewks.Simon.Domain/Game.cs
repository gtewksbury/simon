using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    /// <summary>
    /// Represents a simon game, providing the logic
    /// required to run the game.
    /// </summary>
    public class Game
    {
        private volatile Boolean _GameOver = false;

        /// <summary>
        /// Gets or sets the stats associated with the game
        /// </summary>
        public GameStats Stats { get; set; } = new GameStats();

        /// <summary>
        /// Gets or sets the date the game started.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets a collection of ordered colors
        /// associated with the current round
        /// </summary>
        public List<ButtonColor> Colors { get; set; } = new List<ButtonColor>();

        /// <summary>
        /// Gets or sets the current round number
        /// </summary>
        public int RoundNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of buttons clicked
        /// by the user in the current round
        /// </summary>
        public int Attempts { get; set; } = 0;

        /// <summary>
        /// Gets or sets the device manager associated
        /// with the game.
        /// </summary>
        public DeviceManager Manager { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game
        /// is over or not
        /// </summary>
        public bool GameOver
        {
            get { return _GameOver; }
            set { _GameOver = value; }
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        /// <param name="manager">The device manager associated with the game.</param>
        public void Start(DeviceManager manager)
        {
            Debug.WriteLine("Game Started");
            Manager = manager;
            AddButtonColor();
            manager.Device.RoundStarted(Colors.ToArray());
            Stats.RoundNumber = 1;
            manager.Ready = true;
        }

        /// <summary>
        /// Starts a new round on the game
        /// </summary>
        /// <param name="manager">The device manager associated with the game.</param>
        public void StartNextRound(DeviceManager manager)
        {
            // precautionary check
            // if the game is over, stop processing the game
            if (GameOver)
            {
                return;
            }

            AddButtonColor();
            Attempts = 0;
            RoundNumber++;
            Manager.Stats.HighestRound = Math.Max(Manager.Stats.HighestRound, RoundNumber);
            manager.Device.RoundStarted(Colors.ToArray());
            manager.Ready = true;
        }

        /// <summary>
        /// Checks the given color against the expected color.
        /// </summary>
        /// <param name="selectedColor">The color selected by the user.</param>
        /// <returns>Returns 'true' if the round isn't over; otherwise,
        /// returns 'false' if the current round is over.</returns>
        public bool SelectColor(ButtonColor selectedColor)
        {
            if (Colors[Attempts] != selectedColor)
            {
                // if the selected color doesn't match the expected color
                // end the game 
                GameOver = true;
                Manager.Ready = false;

                var gameDuration = DateTime.Now.Subtract(StartTime);

                var totalDuration = new TimeSpan(
                        0,
                        Manager.Stats.HoursPlayed,
                        Manager.Stats.MinutesPlayed,
                        Manager.Stats.SecondsPlayed);

                var updatedTotalDuration = totalDuration.Add(gameDuration);

                Manager.Stats.HoursPlayed = updatedTotalDuration.Hours;
                Manager.Stats.MinutesPlayed = updatedTotalDuration.Minutes;
                Manager.Stats.SecondsPlayed = updatedTotalDuration.Seconds;

                throw new InvalidColorSelectionException();
            }

            Attempts++;
            return Attempts < RoundNumber;
        }

        /// <summary>
        /// Adds a new color to the game.
        /// Called when a new round is started.
        /// </summary>
        private void AddButtonColor()
        {
            var random = new Random();
            var color = (ButtonColor)random.Next(1, 5);
            Colors.Add(color);
        }

    }
}
