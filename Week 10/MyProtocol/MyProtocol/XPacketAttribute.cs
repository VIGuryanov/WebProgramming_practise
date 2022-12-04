using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProtocol
{
    [AttributeUsage(AttributeTargets.Field)]
    public class XFieldAttribute : Attribute
    {
        public byte FieldID { get; }

        public XFieldAttribute(byte fieldId)
        {
            FieldID = fieldId;
        }
    }
}
