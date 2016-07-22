using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    /// <summary>
    /// Allows an object to implement a device, providing methods
    /// for communicating the game states back to the device.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Indicates the game has ended.
        /// </summary>
        /// <param name="color"></param>
        void GameOver(ButtonColor color);

        /// <summary>
        /// Indicates a new round has started
        /// </summary>
        /// <param name="colors">The colors associated with the new round.</param>
        void RoundStarted(ButtonColor[] colors);
    }
}
