using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    /// <summary>
    /// Allows a device to interact with the simon gaming logic
    /// </summary>
    public class DeviceManager
    {
        private volatile Boolean _Ready = false;

        /// <summary>
        // Gets or sets the current game.
        /// </summary>
        public Game CurrentGame { get; set; }


        /// <summary>
        // Gets or sets the current game's statistics
        /// </summary>
        public DeviceStats Stats { get; set; }


        /// <summary>
        // Gets or sets the device the device the user interacts with.
        /// </summary>
        public IDevice Device { get; set; }

        /// <summary>
        // Gets or sets a value indicating whether the device is ready
        // for user input.
        /// </summary>
        public bool Ready
        {
            get { return _Ready; }
            set { _Ready = value; }
        }

        /// <summary>
        /// Starts a new game on the given device.
        /// </summary>
        /// <param name="device">The device on which the game is played.</param>
        /// <returns></returns>
        public async Task StartGame(IDevice device)
        {
            await Task.Run(() =>
            {
                Device = device;
                CurrentGame = new Game();
                CurrentGame.Start(this);
            });

            // increment the number of games played on the device
            Stats.GamesPlayed++;

            // publish the stats
        }

        /// <summary>
        /// Called when a user selects a color on a device.
        /// </summary>
        /// <param name="color">The color selected by the user</param>
        /// <returns></returns>
        public async Task SelectColor(ButtonColor color)
        {
            // precautionary check
            // if the game has ended, don't allow the user to select a color
            if (CurrentGame.GameOver)
            {
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    // check if the selected color was correct
                    if (!CurrentGame.SelectColor(color))
                    {
                        // if they selected the correct color
                        // and it was the last color for the current round,
                        // start a new round
                        CurrentGame.StartNextRound(this);
                    }

                    CurrentGame.Stats.TotalCorrect++;
                }
                catch (InvalidColorSelectionException ex)
                {
                    Ready = false;
                    Device.GameOver(ex.CorrectColor);
                }
            });

            // publish statistics
        }
    }
}
