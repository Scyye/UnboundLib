using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnboundLib.Extensions
{
    // for testing
    internal static class CardInfoExt
    {
        public static CardInfo SetCardName(this CardInfo info, string name)
        {
            info.SetFieldValue("cardName", name); // does that work?
            return info;
        }
        public static CardInfo SetCardDescription(this CardInfo info, string description)
        {
            info.SetFieldValue("cardDestription", description); // does that work?
            return info;
        }
    }
}
