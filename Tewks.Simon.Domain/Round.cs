using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public class Round
    {
        public Round(int roundNumber)
        {
            SetColors(roundNumber);
            RoundNumber = roundNumber;
        }

        public RoundStats Stats { get; set; } = new RoundStats();
        public int RoundNumber { get; set; }

        public int Attempts { get; set; } = 0;

        

        public ButtonColor[] Colors { get; set; }


        public void Start(DeviceManager Manager)
        {
            ShowColors(Manager.Device);
            Manager.CurrentGame.Stats.RoundNumber++;
            Manager.Ready = true;
        }

        public bool SelectColor(ButtonColor color)
        {
            if (Colors[Attempts] != color)
            {
                throw new InvalidColorSelectionException();
            }

            Attempts++;
            return Attempts < RoundNumber;
        }

        private void ShowColors(IDevice device)
        {
            device.ShowColors(Colors);
            //foreach(var color in Colors)
            //{
            //    device.HighlightColor(color);
            //}

        }

        private void SetColors(int roundNumber)
        {
            var random = new Random();

            Colors = new ButtonColor[roundNumber];

            for(int i = 0; i < roundNumber; i++)
            {
                Colors[i] = (ButtonColor)random.Next(1, 4);
            }
        }

    }
}
