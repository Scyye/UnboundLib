using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Tables;
using System.Xml.Linq;
using System.Collections.Generic;
using System;

namespace UnboundLib.Extensions
{
    // for testing
    internal static class CardInfoExt
    {
        public static CardInfo SetCardName(this CardInfo info, string name, TableRefHelper tableRef = null)
        {
            var reference = (TableEntryReference) ("CARD_" + info.name);
            var table = (TableReference) ("StringTableCards");
            var locStr = new LocalizedString("StringTableCards", reference);
            Unbound.Instance.StartCoroutine(TableRefHelper.InjectTableData(table, reference, name, tableRef));
            info.SetFieldValue("m_localizedCardName", locStr);
            return info;
        }
        public static CardInfo SetCardDescription(this CardInfo info, string description, TableRefHelper tableRef = null)
        {
            var reference = (TableEntryReference) ("CARD_" + info.name+ "_DESC");
            var table = (TableReference) ("StringTableCards");
            var locStr = new LocalizedString("StringTableCards", reference);
            Unbound.Instance.StartCoroutine(TableRefHelper.InjectTableData(table, reference, description, tableRef));
            info.SetFieldValue("m_localizedCardDescription", locStr); // does that work?
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
    internal class balsdhwad
    {
        /*
        public void DrawCard(CardInfoStat[] stats, LocalizedString cardName, LocalizedString description = null, Sprite image = null, bool charge = false)
        {
            if (charge)
            {
                chargeObj.SetActive(value: true);
                chargeObj.transform.SetParent(grid.transform, worldPositionStays: true);
            }

            if (description != null)
            {
                m_localizedEffectText.SetReference(description);
                m_localizedEffectText.gameObject.SetActive(value: true);
                m_localizedEffectText.transform.SetParent(grid.transform, worldPositionStays: true);
            }

            m_localizedNameText.SetReference(cardName);
            for (int i = 0; i < stats.Length; i++)
            {
                GameObject obj = Object.Instantiate(statObject, grid.transform.position, grid.transform.rotation, grid.transform);
                obj.SetActive(value: true);
                obj.transform.localScale = Vector3.one;
                UILocalizedString component = obj.transform.GetChild(0).GetComponent<UILocalizedString>();
                UILocalizedString component2 = obj.transform.GetChild(1).GetComponent<UILocalizedString>();
                component.SetReference(stats[i].LocalizedStat);
                OptionsData.SettingsData settingsData = Optionshandler.instance.OptionsData.GetSettingsData("OPTION_SHOWSTATNUMBERS");
                if (stats[i].simepleAmount != 0 && !settingsData.CurrentValueToggle)
                {
                    component2.SetReference(stats[i].GetLocalizedAmount());
                }
                else
                {
                    component2.Text.text = stats[i].amount;
                }

                component2.Text.color = (stats[i].positive ? positiveColor : negativeColor);
            }

            if ((bool) image)
            {
                icon.sprite = image;
            }

            m_localizedEffectText.transform.position += Vector3.up * 0.3f;
        }*/
    }
}
