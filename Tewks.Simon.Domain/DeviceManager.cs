using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public class DeviceManager
    {
        private volatile Boolean _Ready = false;

        public Game CurrentGame { get; set; }

        public DeviceStats Stats { get; set; }

        public IDevice Device { get; set; }

        public bool Ready
        {
            get { return _Ready; }
            set { _Ready = value; }
        }

        public async Task StartGame(IDevice device)
        {
            await Task.Run(() =>
            {
                Device = device;
                CurrentGame = new Game();
                CurrentGame.Start(this);
            });

            //Stats.GamesPlayed++;
        }

        public async Task SelectColor(ButtonColor color)
        {
            if (CurrentGame.GameOver)
            {
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    if (!CurrentGame.SelectColor(color))
                        CurrentGame.StartNextRound(this);

                    CurrentGame.Stats.TotalCorrect++;
                }
                catch (InvalidColorSelectionException ex)
                {
                    Ready = false;
                    Device.GameOver(ex.CorrectColor);
                }
            });


            // send stats
        }
    }
}
