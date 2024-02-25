using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Tables;
using System.Xml.Linq;
using System.Collections.Generic;
using System;
using UnboundLib;
using Unbound = UnboundLib.Unbound;

namespace Unbound.Extensions
{
    // for testing
    internal static class CardInfoExt
    {
        public static CardInfo SetCardName(this CardInfo info, string name, TableRefHelper tableRef = null)
        {
            var reference = (TableEntryReference) ("CARD_" + info.name);
            var table = (TableReference) ("StringTableCards");
            var locStr = new LocalizedString("StringTableCards", reference);
            UnboundLib.Unbound.Instance.StartCoroutine(TableRefHelper.InjectTableData(table, reference, name, tableRef));
            info.SetFieldValue("m_localizedCardName", locStr);
            return info;
        }
        public static CardInfo SetCardDescription(this CardInfo info, string description, TableRefHelper tableRef = null)
        {
            var reference = (TableEntryReference) ("CARD_" + info.name+ "_DESC");
            var table = (TableReference) ("StringTableCards");
            var locStr = new LocalizedString("StringTableCards", reference);
            UnboundLib.Unbound.Instance.StartCoroutine(TableRefHelper.InjectTableData(table, reference, description, tableRef));
            info.SetFieldValue("m_localizedCardDescription", locStr); 
            return info;
        }
        public static CardInfo SetCardStats(this CardInfo info, CardInfoStat[] cardStats, TableRefHelper tableRef = null)
        {
            int i = 0;
            foreach (CardInfoStat stat in cardStats)
            {
                var reference = (TableEntryReference) ($"STAT({i++})_" + info.name);
                var table = (TableReference) ("StringTableCards");
                var locStr = new LocalizedString("StringTableCards", reference);
                UnboundLib.Unbound.Instance.StartCoroutine(TableRefHelper.InjectTableData(table, reference, stat.stat, tableRef));
                stat.SetFieldValue("m_localizedStat", locStr); 
            }
            info.cardStats = cardStats;
            return info;
        }
    }
    public class TableRefHelper
    {
        public enum Locales
        {
            en_US,
            fr,
            it,
            de,
            es,
            pt_BR,
            ru,
            ja,
            zh
        }
        internal static Locales getEnumFormLocal(Locale locale)
        {
            if(!Enum.TryParse(locale.Identifier.Code.Replace("-","_"), out Locales retrn))
                retrn = (Locales) (-1);
            return retrn;
        }
        public TableRefHelper UpdateSrting(string Normal, Locales lang, string Translated)
        {
            if (!overrides.ContainsKey(Normal))
                overrides.Add(Normal, new Dictionary<Locales, string>());
            overrides[Normal][lang] = Translated; 
            return this;
        }
        internal Dictionary<string,Dictionary<Locales,string>> overrides = new Dictionary<string, Dictionary<Locales, string>> ();
        internal static IEnumerator InjectTableData(TableReference Table, TableEntryReference reference, string Data, TableRefHelper tableRef = null)
        {
            var locals = LocalizationSettings.AvailableLocales.Locales;
            foreach (var local in locals)
            {
                var table = LocalizationSettings.StringDatabase.GetTableAsync(Table, local);
                yield return table;
                Addressables.ResourceManager.Acquire(table);

                var stringTable = table.Result;
                var entry = stringTable.GetEntryFromReference(reference);
                string value = Data;
                if (tableRef != null && tableRef.overrides.TryGetValue(Data, out var translations)) if(!translations.TryGetValue(getEnumFormLocal(local), out value)) value = Data;
                if (entry == null)
                    stringTable.AddEntryFromReference(reference, value);
                else
                    entry.Value = value;
            }
            yield return null;
        }
    }
}
