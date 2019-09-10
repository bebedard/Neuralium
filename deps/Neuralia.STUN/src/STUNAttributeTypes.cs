using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuralia.STUN
{
    public enum StunAttributeTypes : ushort
    {
        MappedAddress,
        ResponseAddress,
        ChangeRequest,
        SourceAddress,
        ChangedAddress,
        Username,
        Password,
        MessageIntegrity,
        ErrorCode,
        UnknownAttributes,
        ReflectedFrom,
    }
}
