using Unbound.Core.Utils;
using UnityEngine.Localization;

namespace Unbound.Networking.UI
{
    public class LocalizedStrings
    {
        private static readonly TableRefHelper translationTable = new TableRefHelper(UnboundNetworking.ModId)
            .UpdateSrting("LetsGoText", new TableRefHelper.TranslationData()
            {
                en_US = "LETS GOO!"
            })
            .UpdateSrting("WaittingForHostText", new TableRefHelper.TranslationData()
            {
                en_US = "WAITING FOR{hostName}"
            }, true)
            .Build(TableRefHelper.stringTableDefault);

        public static LocalizedString LetsGoText { get { return translationTable.GenerateString(TableRefHelper.stringTableDefault, "LetsGoText"); } }
        public static LocalizedString WaittingForHostText { get { return translationTable.GenerateString(TableRefHelper.stringTableDefault, "WaittingForHostText"); } }

    }
}
