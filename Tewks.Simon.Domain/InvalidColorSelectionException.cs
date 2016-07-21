using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tewks.Simon.Domain
{
    public class InvalidColorSelectionException : Exception
    {
        public ButtonColor CorrectColor { get; set; }
    }
}
