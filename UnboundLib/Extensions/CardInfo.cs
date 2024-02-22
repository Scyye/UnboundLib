using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine;
using HarmonyLib;

namespace UnboundLib.Extensions
{
    // for testing
    internal static class CardInfoExt
    {
        public static CardInfo SetCardName(this CardInfo info, string name)
        {
            // Set "m_localizedCardName" and "m_localizedCardDescription" to the card's title and description using reflection
            info.SetFieldValue("m_localizedCardName", name);
            return info;
        }
        public static CardInfo SetCardDescription(this CardInfo info, string description)
        {
            info.SetFieldValue("m_localizedCardDescription", description); // does that work?
            return info;
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
