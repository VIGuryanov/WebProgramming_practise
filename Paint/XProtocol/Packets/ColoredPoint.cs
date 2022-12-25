using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProtocol.Serializator;

namespace XProtocol.Packets
{
    public class ColoredPoint
    {
        [XField(0)]
        public int Color;
        [XField(1)]
        public int X;
        [XField(2)]
        public int Y;
    }
}
