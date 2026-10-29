using Il2Cpp;

namespace CustomPlayerCustomization
{
    public static class CustomizationCatalog
    {
        public static int ApiVersion => 1;
        public static int SelectionApi => 1;
        public static int SlotCount => 4;

        public static string HatKey(SyncCustomization source, int hat) => CustomizationModels.Key(source, hat);

        public static int ResolveHat(SyncCustomization source, string key) => CustomizationModels.Resolve(source, key);

        public static void SaveSelection(SyncCustomization source, int hat) => CustomizationModels.Save(source, hat);

        public static UnityEngine.GameObject CreateHat(UnityEngine.Transform parent, string id)
        {
            CustomizationInfo info = Find(id);
            if (!Enabled || !parent || info == null || !info.IsAvailable) return null;
            if (!Core.LoadedBundles.TryGetValue(info.Name, out var prefabs) || prefabs is not { Length: >= 2 } || !prefabs[1]) return null;

            UnityEngine.GameObject model = UnityEngine.Object.Instantiate(prefabs[1], parent, false);
            model.name = info.Name;
            model.SetActive(true);

            return model;
        }

        public static bool Enabled { get; set; } = true;

        public static bool IsReady => Core.IsInitialized && Core.AssetsReady && Core.Error == null;

        public static IReadOnlyList<CustomizationInfo> Items => Core.CustomizationList.AsReadOnly();

        public static event Action Ready;
        public static event Action<CharacterCustomization> MenuReady;
        public static event Action<SyncCustomization> PlayerReady;

        public static CustomizationInfo Find(string id) =>
            Core.CustomizationList.FirstOrDefault(info => info.Id == id || info.Name == id);

        public static bool ValidId(string id) =>
            id is { Length: > 2 and <= 128 } &&
            id.Contains(':') &&
            !id.StartsWith("base:", StringComparison.Ordinal) &&
            id.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or ':' or '-' or '_' or '.');

        public static bool TryGetHatId(SyncCustomization source, string id, out int hat)
        {
            hat = -1;
            CustomizationInfo info = Find(id);
            if (!source || info == null || !info.IsAvailable) return false;

            for (int index = 0; index < source.GKCNMADGAAF.Count; index++)
            {
                if (source.GKCNMADGAAF[index] && source.GKCNMADGAAF[index].name == info.Name)
                {
                    hat = index;
                    return true;
                }
            }

            return false;
        }

        public static void Refresh() => Core.Refresh();

        internal static void OnMenuReady(CharacterCustomization source) => MenuReady?.Invoke(source);

        internal static void OnReady() => Ready?.Invoke();

        internal static void OnPlayerReady(SyncCustomization source) => PlayerReady?.Invoke(source);

        public static IDisposable Mount(string directory, string id)
        {
            CustomizationInfo info = Core.ReadPack(directory, id);
            if (info.Id != id)
            {
                throw new System.IO.InvalidDataException("The pack ID does not match the requested ID.");
            }

            CustomizationInfo[] replaced = Core.CustomizationList.Where(item => item.Id == info.Id || item.Name == info.Name).ToArray();
            foreach (CustomizationInfo item in replaced)
            {
                Core.CustomizationList.Remove(item);
                Core.LoadedBundles.Remove(item.Name);
            }

            Core.CustomizationList.Add(info);
            Core.CustomizationList.Sort((first, second) => StringComparer.Ordinal.Compare(first.Id, second.Id));
            Mounted mounted = new(info, replaced);

            string error = Core.LoadBundle(info);
            if (error != null)
            {
                mounted.Dispose();
                throw new System.IO.InvalidDataException(error);
            }

            Core.UpdateReady();
            return mounted;
        }

        private sealed class Mounted(CustomizationInfo info, CustomizationInfo[] replaced) : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed) return;

                _disposed = true;
                Core.CustomizationList.Remove(info);
                Core.LoadedBundles.Remove(info.Name);
                Core.CustomizationList.AddRange(replaced);
                Core.CustomizationList.Sort((first, second) => StringComparer.Ordinal.Compare(first.Id, second.Id));

                foreach (CustomizationInfo item in replaced)
                {
                    Core.LoadBundle(item);
                }

                Core.UpdateReady();
            }
        }
    }
}
