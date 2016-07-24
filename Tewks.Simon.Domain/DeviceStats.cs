using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public class DeviceStats
    {
        public int GamesPlayed { get; set; } = 0;

        public int HighestRound { get; set; } = 1;

        public int HoursPlayed { get; set; } = 0;

        public int MinutesPlayed { get; set; } = 0;

        public int SecondsPlayed { get; set; } = 0;

        public int RedClickCount { get; set; } = 0;

        public int YellowClickCount { get; set; } = 0;

        public int WhiteClickCount { get; set; } = 0;

        public int GreenClickCount { get; set; } = 0;
    }
}
