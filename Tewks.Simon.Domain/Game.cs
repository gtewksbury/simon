using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public class Game
    {
        private volatile Boolean _GameOver = false;

        public GameStats Stats { get; set; } = new GameStats();

        public Round CurrentRound { get; set; }

        public List<ButtonColor> Colors { get; set; } = new List<ButtonColor>();

        public int RoundNumber { get; set; } = 1;

        public int Attempts { get; set; } = 0;

        public DeviceManager Manager { get; set; }

        public bool GameOver
        {
            get { return _GameOver; }
            set { _GameOver = value; }
        }


        public Random Random { get; set; } = new Random();
        public void Start(DeviceManager manager)
        {
            //CurrentRound = new Round(1);
            //CurrentRound.Start(manager);
            Debug.WriteLine("Game Started");
            Manager = manager;
            AddButtonColor();
            manager.Device.ShowColors(Colors.ToArray());
            Stats.RoundNumber = 1;
            manager.Ready = true;
        }

        public void StartNextRound(DeviceManager manager)
        {
            if (GameOver)
            {
                return;
            }

            AddButtonColor();
            Attempts = 0;
            RoundNumber++;
            manager.Device.ShowColors(Colors.ToArray());
            //Stats.RoundNumber = CurrentRound.RoundNumber;
            manager.Ready = true;
        }

        public bool SelectColor(ButtonColor color)
        {
            if (Colors[Attempts] != color)
            {
                GameOver = true;
                Manager.Ready = false;
                throw new InvalidColorSelectionException();
            }

            Attempts++;
            return Attempts < RoundNumber;
        }

        private void AddButtonColor()
        {
            var random = new Random();
            var color = (ButtonColor)random.Next(1, 5);
            Colors.Add(color);
        }

    }
}
