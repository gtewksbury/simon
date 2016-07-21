using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public interface IDevice
    {
        void GameOver(ButtonColor color);
        void ShowColors(ButtonColor[] colors);
    }
}
