using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProtocol
{
    class TestPacket
    {
        [XField(0)]
        public int TestNumber;

        [XField(1)]
        public double TestDouble;

        [XField(2)]
        public bool TestBoolean;
    }
}
