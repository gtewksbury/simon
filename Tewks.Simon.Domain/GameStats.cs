using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public class GameStats
    {
        public int RoundNumber { get; set; } = 1;

        public int TotalCorrect { get; set; } = 0;
    }
}
