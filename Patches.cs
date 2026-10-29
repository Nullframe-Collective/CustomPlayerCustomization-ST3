using HarmonyLib;
using Il2Cpp;

namespace CustomPlayerCustomization
{
    [HarmonyPatch(typeof(CharacterCustomization), nameof(CharacterCustomization.LoadCharacter))]
    public static class MainMenuCustomizationPatch
    {
        public static void Prefix(CharacterCustomization __instance) => CustomizationModels.BeginLoad(__instance);

        public static void Postfix(CharacterCustomization __instance) => CustomizationModels.EndLoad(__instance);
    }

    [HarmonyPatch(typeof(CharacterCustomization), nameof(CharacterCustomization.changeAppearance))]
    public static class MenuAppearancePatch
    {
        public static void Prefix(CharacterCustomization __instance) => Core.InstantiateMainMenuCustomization(__instance);

        public static void Postfix(CharacterCustomization __instance) => CustomizationModels.Save(__instance);
    }

    [HarmonyPatch(typeof(SyncCustomization), nameof(SyncCustomization.Awake))]
    public static class PlayerAwakePatch
    {
        public static void Postfix(SyncCustomization __instance) => Core.InstantiateInGameCustomization(__instance);
    }

    [HarmonyPatch(typeof(SyncCustomization), nameof(SyncCustomization.OnStartClient))]
    public static class PlayerClientPatch
    {
        public static void Prefix(SyncCustomization __instance) => Core.InstantiateInGameCustomization(__instance);
    }

    [HarmonyPatch(typeof(SyncCustomization), nameof(SyncCustomization.Start))]
    public static class ThirdPersonCustomizationPatch
    {
        public static bool Prefix(SyncCustomization __instance)
        {
            Core.InstantiateInGameCustomization(__instance);

            if (__instance.AMAEKOLBGLP && !__instance.IGDAIDNEJBO)
            {
                __instance.JDCJPLOKLJG();
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(SyncCustomization), nameof(SyncCustomization.JDCJPLOKLJG))]
    public static class LocalCustomizationPatch
    {
        public static void Prefix(SyncCustomization __instance) => CustomizationModels.Local(__instance);
    }

    [HarmonyPatch(typeof(SyncCustomization), nameof(SyncCustomization.MKFLOHBILFG))]
    public static class PlayerAppearancePatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(SyncCustomization __instance) => Core.InstantiateInGameCustomization(__instance);
    }
}
