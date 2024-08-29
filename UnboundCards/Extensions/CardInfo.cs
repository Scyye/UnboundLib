using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Unbound.Core;
using Unbound.Core.Utils;

namespace Unbound.Cards.Extensions
{
    // for testing
    internal static class CardInfoExt
    {
        public static CardInfo SetCardName(this CardInfo info, string name, TableRefHelper tableRef = null)
        {
            var reference = (TableEntryReference) ("CARD_" + info.name);
            var locStr = new LocalizedString(TableRefHelper.stringTableCards, reference);
            UnboundCore.Instance.StartCoroutine(TableRefHelper.InjectTableData(TableRefHelper.stringTableCards, reference, name, tableRef));
            info.SetFieldValue("m_localizedCardName", locStr);
            return info;
        }
        public static CardInfo SetCardDescription(this CardInfo info, string description, TableRefHelper tableRef = null)
        {
            var reference = (TableEntryReference) ("CARD_" + info.name+ "_DESC");
            var locStr = new LocalizedString(TableRefHelper.stringTableCards, reference);
            UnboundCore.Instance.StartCoroutine(TableRefHelper.InjectTableData(TableRefHelper.stringTableCards, reference, description, tableRef));
            info.SetFieldValue("m_localizedCardDescription", locStr); 
            return info;
        }
        public static CardInfo SetCardStats(this CardInfo info, CardInfoStat[] cardStats, TableRefHelper tableRef = null)
        {
            int i = 0;
            foreach (CardInfoStat stat in cardStats)
            {
                var reference = (TableEntryReference) ($"STAT({i++})_" + info.name);
                var locStr = new LocalizedString(TableRefHelper.stringTableCards, reference);
                UnboundCore.Instance.StartCoroutine(TableRefHelper.InjectTableData(TableRefHelper.stringTableCards, reference, stat.stat, tableRef));
                stat.SetFieldValue("m_localizedStat", locStr); 
            }
            info.cardStats = cardStats;
            return info;
        }
    }
  
}
