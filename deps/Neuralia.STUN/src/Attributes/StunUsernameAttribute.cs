using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuralia.STUN.Attributes
{
    public class STUNUsernameAttribute : STUNAnsiTextAttribute
    {
        public override string ToString()
        {
            return string.Format("USERNAME \"{0}\"", Text);
        }
    }
}
