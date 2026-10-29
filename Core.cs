using Il2Cpp;
using MelonLoader;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UnityEngine;

[assembly: MelonInfo(typeof(CustomPlayerCustomization.Core), "CustomPlayerCustomization", "1.3.3", "Nullframe Collective")]
[assembly: MelonGame("ZeoWorks", "Slendytubbies 3")]
[assembly: MelonColor(255, 155, 89, 182)]

namespace CustomPlayerCustomization
{
    public class Core : MelonMod
    {
        private string _customizationPath = Path.Combine(MelonLoader.Utils.MelonEnvironment.UserDataDirectory, "CustomCustomization");
        public static Dictionary<string, GameObject[]> LoadedBundles = new();
        public static List<CustomizationInfo> CustomizationList = new();
        public static bool IsInitialized { get; private set; }
        public static bool AssetsReady { get; private set; }
        public static string Error { get; private set; }

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            InitializeModSetup();
            LoadBundles();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName) => Refresh();

        private void InitializeModSetup()
        {
            Directory.CreateDirectory(_customizationPath);
            CustomizationList.Clear();
            HashSet<string> ids = new(StringComparer.Ordinal);
            HashSet<string> names = new(StringComparer.Ordinal);

            foreach (string directory in Directory.GetDirectories(_customizationPath))
            {
                if (!File.Exists(Path.Combine(directory, "customization.json")) && !File.Exists(Path.Combine(directory, "bundle"))) continue;

                try
                {
                    CustomizationInfo info = ReadPack(directory, "legacy:" + Path.GetFileName(directory).ToLowerInvariant());

                    if (!ids.Add(info.Id) || !names.Add(info.Name))
                    {
                        throw new InvalidDataException("Duplicate or invalid customization ID/name.");
                    }

                    CustomizationList.Add(info);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
                {
                    Error = $"Cannot load customization {Path.GetFileName(directory)}: {exception.Message}";
                    LoggerInstance.Error(Error);
                }
            }

            CustomizationList.Sort((first, second) => StringComparer.Ordinal.Compare(first.Id, second.Id));
            IsInitialized = true;
        }

        internal static CustomizationInfo ReadPack(string directory, string fallbackId)
        {
            string json = File.ReadAllText(Path.Combine(directory, "customization.json"));
            CustomizationInfo info = JsonSerializer.Deserialize<CustomizationInfo>(json, new JsonSerializerOptions
            {
                Converters = { new VectorJsonConverter() }
            });

            if (info == null || string.IsNullOrWhiteSpace(info.Name))
            {
                throw new InvalidDataException("Invalid customization definition.");
            }

            info.Id = string.IsNullOrEmpty(info.Id) ? fallbackId : info.Id;

            if (!CustomizationCatalog.ValidId(info.Id))
            {
                throw new InvalidDataException("Invalid customization ID.");
            }

            using FileStream bundle = File.OpenRead(Path.Combine(directory, "bundle"));
            using SHA256 algorithm = SHA256.Create();
            string hash = Convert.ToHexString(algorithm.ComputeHash(bundle));
            info.ContentHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash + "\n" + json.Replace("\r\n", "\n"))));
            info.Path = directory;

            return info;
        }

        private void LoadBundles()
        {
            foreach (CustomizationInfo info in CustomizationList)
            {
                string error = info == null ? null : LoadBundle(info);
                if (error != null)
                {
                    Error = error;
                    LoggerInstance.Error(Error);
                }
            }

            UpdateReady();
            CustomizationCatalog.OnReady();
        }

        internal static void Refresh()
        {
            foreach (CustomizationInfo info in CustomizationList)
            {
                string error = info == null ? null : LoadBundle(info);
                if (error != null)
                {
                    MelonLogger.Warning(error);
                }
            }

            UpdateReady();
        }

        internal static bool Loaded(string name) =>
            LoadedBundles.TryGetValue(name, out var prefabs) && prefabs is { Length: >= 2 } && prefabs[0] && prefabs[1];

        internal static string LoadBundle(CustomizationInfo info)
        {
            if (Loaded(info.Name)) return null;

            LoadedBundles.Remove(info.Name);
            string bundlePath = Path.Combine(info.Path, "bundle");
            if (!File.Exists(bundlePath)) return null;

            Il2CppAssetBundle assetBundle = Il2CppAssetBundleManager.LoadFromFile(BundleCache.Resolve(bundlePath));
            if (assetBundle == null)
            {
                return $"Failed to load AssetBundle for customization: {info.Name}";
            }

            GameObject menuPrefab = assetBundle.Load<GameObject>(info.Name);
            GameObject tpsPrefab = assetBundle.Load<GameObject>(info.Name + " (TPS)");
            assetBundle.Unload(false);

            if (!menuPrefab || !tpsPrefab)
            {
                return $"Customization {info.Name} requires menu and TPS prefabs.";
            }

            menuPrefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            tpsPrefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            LoadedBundles[info.Name] = [menuPrefab, tpsPrefab];

            return null;
        }

        internal static void UpdateReady() =>
            AssetsReady = Error == null && CustomizationList.All(info => LoadedBundles.ContainsKey(info.Name));

        public override void OnLateUpdate() => CustomizationModels.Update();

        public static void InstantiateMainMenuCustomization(CharacterCustomization customization) => CustomizationModels.Menu(customization);

        public static void InstantiateInGameCustomization(SyncCustomization customization) => CustomizationModels.Player(customization);
    }
}
