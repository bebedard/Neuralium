﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuralia.STUN.Attributes
{
    public class STUNResponseAddressAttribute : STUNEndPointAttribute
    {
        public override string ToString()
        {
            return string.Format("RESPONSE-ADDRESS {0}", EndPoint);
        }
    }
}