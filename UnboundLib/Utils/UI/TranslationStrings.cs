namespace Unbound.Core.Utils.UI {
    public class TranslationStrings {


        public static TableRefHelper HostTextOverride { get; private set; }
        public static TableRefHelper TeamColorNames { get; private set; }

        internal static void INIT() {
            HostTextOverride = new TableRefHelper("")
                .UpdateSrting("BUTTON_INVITEFRIEND", new TableRefHelper.TranslationData() {
                    en_US = "Host"
                }).Build(TableRefHelper.stringTableDefault);

            #region Colours
            TeamColorNames = new TableRefHelper(UnboundCore.ModId)
            .UpdateSrting("Team_0_Name", new TableRefHelper.TranslationData() {
                en_US = "Orange"
            }).UpdateSrting("Team_1_Name", new TableRefHelper.TranslationData() {
                en_US = "Blue"
            }).UpdateSrting("Team_2_Name", new TableRefHelper.TranslationData() {
                en_US = "Red"
            }).UpdateSrting("Team_3_Name", new TableRefHelper.TranslationData() {
                en_US = "Green"
            }).UpdateSrting("Team_4_Name", new TableRefHelper.TranslationData() {
                en_US = "Yellow"
            }).UpdateSrting("Team_5_Name", new TableRefHelper.TranslationData() {
                en_US = "Purple"
            }).UpdateSrting("Team_6_Name", new TableRefHelper.TranslationData() {
                en_US = "Magenta"
            }).UpdateSrting("Team_7_Name", new TableRefHelper.TranslationData() {
                en_US = "Cyan"
            }).UpdateSrting("Team_8_Name", new TableRefHelper.TranslationData() {
                en_US = "Tangerine"
            }).UpdateSrting("Team_9_Name", new TableRefHelper.TranslationData() {
                en_US = "Light Blue"
            }).UpdateSrting("Team_10_Name", new TableRefHelper.TranslationData() {
                en_US = "Peach"
            }).UpdateSrting("Team_11_Name", new TableRefHelper.TranslationData() {
                en_US = "Lime"
            }).UpdateSrting("Team_12_Name", new TableRefHelper.TranslationData() {
                en_US = "Light Yellow"
            }).UpdateSrting("Team_13_Name", new TableRefHelper.TranslationData() {
                en_US = "Orchid"
            }).UpdateSrting("Team_14_Name", new TableRefHelper.TranslationData() {
                en_US = "Pink"
            }).UpdateSrting("Team_15_Name", new TableRefHelper.TranslationData() {
                en_US = "Aquamarine"
            }).UpdateSrting("Team_16_Name", new TableRefHelper.TranslationData() {
                en_US = "Dark Orange"
            }).UpdateSrting("Team_17_Name", new TableRefHelper.TranslationData() {
                en_US = "Dark Blue"
            }).UpdateSrting("Team_18_Name", new TableRefHelper.TranslationData() {
                en_US = "Dark Red"
            }).UpdateSrting("Team_19_Name", new TableRefHelper.TranslationData() {
                en_US = "Dark Green"
            }).UpdateSrting("Team_20_Name", new TableRefHelper.TranslationData() {
                en_US = "Dark Yellow"
            }).UpdateSrting("Team_21_Name", new TableRefHelper.TranslationData() {
                en_US = "Indigo"
            }).UpdateSrting("Team_22_Name", new TableRefHelper.TranslationData() {
                en_US = "Cerise"
            }).UpdateSrting("Team_23_Name", new TableRefHelper.TranslationData() {
                en_US = "Teal"
            }).UpdateSrting("Team_24_Name", new TableRefHelper.TranslationData() {
                en_US = "Burnt Orange"
            }).UpdateSrting("Team_25_Name", new TableRefHelper.TranslationData() {
                en_US = "Midnight Blue"
            }).UpdateSrting("Team_26_Name", new TableRefHelper.TranslationData() {
                en_US = "Maroon"
            }).UpdateSrting("Team_27_Name", new TableRefHelper.TranslationData() {
                en_US = "Evergreen"
            }).UpdateSrting("Team_28_Name", new TableRefHelper.TranslationData() {
                en_US = "Gold"
            }).UpdateSrting("Team_29_Name", new TableRefHelper.TranslationData() {
                en_US = "Violet"
            }).UpdateSrting("Team_30_Name", new TableRefHelper.TranslationData() {
                en_US = "Ruby"
            }).UpdateSrting("Team_31_Name", new TableRefHelper.TranslationData() {
                en_US = "Dark Cyan"
            }).Build(TableRefHelper.stringTableDefault);
            #endregion
        }

    }
}
