using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;

namespace Unbound.Core.Utils
{
    public class TableRefHelper
    {
        public readonly static TableReference stringTableCards = (TableReference) "StringTableCards";
        public readonly static TableReference stringTableDefault = (TableReference) "StringTableDefault";
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
        public class TranslationData
        {
            public string en_US, fr, it, de, es, pt_BR, ru, ja, zh;
        }

        public TableRefHelper(string modID)
        {
            this.modID = modID;
        }

        internal static Locales getEnumFormLocal(Locale locale)
        {
            if (!Enum.TryParse(locale.Identifier.Code.Replace("-", "_"), out Locales retrn))
                retrn = (Locales) (-1);
            return retrn;
        }
        public TableRefHelper UpdateSrting(string key, Locales lang, string Translated)
        {
            if (Translated == default) return this;
            if (!overrides.ContainsKey(key))
                overrides.Add(key, new Dictionary<Locales, string>());
            overrides[key][lang] = Translated;
            return this;
        }
        public TableRefHelper UpdateSrting(string key, TranslationData translations )
        {
            return UpdateSrting(key,Locales.en_US,translations.en_US).UpdateSrting(key, Locales.fr, translations.fr)
                .UpdateSrting(key, Locales.it, translations.it).UpdateSrting(key, Locales.de, translations.de)
                .UpdateSrting(key, Locales.es, translations.es).UpdateSrting(key, Locales.pt_BR, translations.pt_BR)
                .UpdateSrting(key, Locales.ru, translations.ru).UpdateSrting(key, Locales.ja, translations.ja)
                .UpdateSrting(key, Locales.zh, translations.zh);
        }


        public TableRefHelper Build(TableReference table)
        {
            if (!built)
            {
                foreach (string key in overrides.Keys)
                {
                    UnboundCore.Instance.StartCoroutine(InjectTableData(table, (TableEntryReference) $"{modID}_{key}", key, this));
                }
                built = true;
            }
            return this;
        }

        public LocalizedString GenerateString(TableReference table, string key)
        {
            return new LocalizedString(table, (TableEntryReference) $"{modID}_{key}");
        }

        private bool built = false;
        private string modID;
        internal Dictionary<string, Dictionary<Locales, string>> overrides = new Dictionary<string, Dictionary<Locales, string>>();
        public static IEnumerator InjectTableData(TableReference Table, TableEntryReference reference, string Data, TableRefHelper tableRef = null)
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
                if (tableRef != null && tableRef.overrides.TryGetValue(Data, out var translations)) if (!translations.TryGetValue(getEnumFormLocal(local), out value)) value = Data;
                if (entry == null)
                    stringTable.AddEntryFromReference(reference, value);
                else
                    entry.Value = value;
            }
            yield return null;
        }
    }
}
