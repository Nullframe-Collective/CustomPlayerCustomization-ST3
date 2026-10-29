using Il2Cpp;
using Il2CppCodeStage.AntiCheat.Storage;
using UnityEngine;

namespace CustomPlayerCustomization
{
    internal static class CustomizationModels
    {
        private sealed class Model(CustomizationInfo info, Transform transform)
        {
            internal readonly CustomizationInfo Info = info;
            internal readonly Transform Transform = transform;
            internal CharacterCustomization.hat Entry;
        }

        private sealed class Models(Component source, Transform original, Transform root, Dictionary<string, Model> items)
        {
            internal readonly Component Source = source;
            internal readonly Transform Original = original;
            internal readonly Transform Root = root;
            internal readonly Dictionary<string, Model> Items = items;
        }

        private const string SelectionKey = "CustomPlayerCustomization.Hat";
        private static readonly Dictionary<IntPtr, Models> Registered = new();
        private static readonly HashSet<IntPtr> Loading = new();

        internal static bool CustomSelection => ObscuredPrefs.GetString(SelectionKey, "").Length > 0;

        internal static void BeginLoad(CharacterCustomization source)
        {
            source.JDBFCGMAMBH.Clear();
            Loading.Add(source.Pointer);
        }

        internal static void EndLoad(CharacterCustomization source)
        {
            Loading.Remove(source.Pointer);
            string saved = ObscuredPrefs.GetString(SelectionKey, "");

            if (saved.Length == 0 || Resolve(source.JDBFCGMAMBH, Registered.GetValueOrDefault(source.Pointer), saved) >= 0)
            {
                Save(source);
            }
        }

        private static Models For(Component source, Transform original)
        {
            if (Registered.TryGetValue(source.Pointer, out Models models) && models.Root) return models;

            GameObject root = new("CustomPlayerCustomizationHats");
            root.transform.SetParent(original.parent, false);
            root.transform.localPosition = original.localPosition;
            root.transform.localRotation = original.localRotation;
            root.transform.localScale = original.localScale;
            Registered[source.Pointer] = models = new(source, original, root.transform, new());

            return models;
        }

        private static Transform Instantiate(Models models, CustomizationInfo info, int prefab)
        {
            if (models.Items.TryGetValue(info.Id, out Model existing) && existing.Transform) return existing.Transform;

            if (!Core.LoadedBundles.TryGetValue(info.Name, out GameObject[] bundle) || bundle is not { Length: >= 2 } || !bundle[prefab]) return null;

            GameObject instance = UnityEngine.Object.Instantiate(bundle[prefab], models.Root, false);
            instance.name = info.Name;
            instance.SetActive(false);

            if (prefab == 0)
            {
                instance.transform.position = info.MainMenuTransform.Position;
                instance.transform.localPosition = info.MainMenuTransform.LocalPosition;
                instance.transform.localEulerAngles = info.MainMenuTransform.Rotation;
                instance.transform.localScale = info.MainMenuTransform.Scale;
            }
            else if (prefab == 1)
            {
                instance.transform.position = info.InGameTransform.Position;
                instance.transform.localPosition = info.InGameTransform.LocalPosition;
                instance.transform.localEulerAngles = info.InGameTransform.Rotation;
                instance.transform.localScale = info.InGameTransform.Scale;
            }

            models.Items[info.Id] = new(info, instance.transform);

            return instance.transform;
        }

        internal static void Menu(CharacterCustomization source)
        {
            if (!source || !source.LEEICKEPPEB || source.JDBFCGMAMBH == null || source.GKCNMADGAAF == null) return;

            Models models = For(source, source.LEEICKEPPEB);
            AddMenuHats(source, models);
            RemoveBrokenHats(source);

            if (Loading.Contains(source.Pointer))
            {
                RestoreSelection(source, models);
            }

            ClampSelection(source);
            CustomizationCatalog.OnMenuReady(source);
        }

        private static void AddMenuHats(CharacterCustomization source, Models models)
        {
            foreach (CustomizationInfo info in Core.CustomizationList)
            {
                if (info == null || !info.IsAvailable) continue;

                Transform model = Instantiate(models, info, 0);
                if (!model) continue;

                int id = Index(source.JDBFCGMAMBH, model);
                if (id < 0)
                {
                    id = source.JDBFCGMAMBH.Count;
                    source.JDBFCGMAMBH.Add(model);
                }

                Model item = models.Items[info.Id];
                item.Entry ??= new() { hatName = info.Name, hatPrice = info.Price };
                CharacterCustomization.hat entry = item.Entry;
                entry.hatID = id;

                if (!source.GKCNMADGAAF.ToArray().Any(hat => hat != null && hat.Pointer == entry.Pointer))
                {
                    source.GKCNMADGAAF.Add(entry);
                }
            }
        }

        private static void RemoveBrokenHats(CharacterCustomization source)
        {
            for (int index = source.GKCNMADGAAF.Count - 1; index >= 0; index--)
            {
                int hat = source.GKCNMADGAAF[index].hatID;
                if (hat < 0 || hat >= source.JDBFCGMAMBH.Count || !source.JDBFCGMAMBH[hat])
                {
                    source.GKCNMADGAAF.RemoveAt(index);
                }
            }
        }

        private static void RestoreSelection(CharacterCustomization source, Models models)
        {
            string saved = ObscuredPrefs.GetString(SelectionKey, "");
            int hat = Resolve(source.JDBFCGMAMBH, models, saved);
            if (hat >= 0)
            {
                source.NKAOFLPKHIH = hat;
            }

            int selected = -1;
            for (int index = 0; index < source.GKCNMADGAAF.Count; index++)
            {
                if (source.GKCNMADGAAF[index].hatID == source.NKAOFLPKHIH)
                {
                    selected = index;
                }
            }

            source.NNFKNKHLBPO = selected < 0 ? 0 : selected;
        }

        private static void ClampSelection(CharacterCustomization source)
        {
            if (source.NNFKNKHLBPO < 0 || source.NNFKNKHLBPO >= source.GKCNMADGAAF.Count)
            {
                source.NNFKNKHLBPO = 0;
            }

            if (source.GKCNMADGAAF.Count > 0)
            {
                int hat = source.GKCNMADGAAF[source.NNFKNKHLBPO].hatID;
                source.NKAOFLPKHIH = hat >= 0 && hat < source.JDBFCGMAMBH.Count ? hat : 0;
            }
        }

        internal static void Save(CharacterCustomization source)
        {
            if (!source || Loading.Contains(source.Pointer)) return;

            Save(source.JDBFCGMAMBH, source.Pointer, source.NKAOFLPKHIH);
        }

        internal static void Player(SyncCustomization source)
        {
            if (!source || !source.LEEICKEPPEB || source.GKCNMADGAAF == null) return;

            if (source.GKCNMADGAAF.Count == 0)
            {
                for (int index = 0; index < source.LEEICKEPPEB.childCount - 1; index++)
                {
                    source.GKCNMADGAAF.Add(source.LEEICKEPPEB.GetChild(index));
                }
            }

            Models models = For(source, source.LEEICKEPPEB);

            foreach (Model model in models.Items.Values)
            {
                bool available = CustomizationCatalog.Enabled && Core.CustomizationList.Any(info => info.Id == model.Info.Id && info.IsAvailable);
                if (!available && model.Transform)
                {
                    model.Transform.gameObject.SetActive(false);
                }
            }

            if (CustomizationCatalog.Enabled)
            {
                foreach (CustomizationInfo info in Core.CustomizationList)
                {
                    if (info == null || !info.IsAvailable) continue;

                    Transform model = Instantiate(models, info, 1);
                    if (model && Index(source.GKCNMADGAAF, model) < 0)
                    {
                        source.GKCNMADGAAF.Add(model);
                    }
                }
            }

            CustomizationCatalog.OnPlayerReady(source);
        }

        internal static void Local(SyncCustomization source)
        {
            Player(source);
            string saved = ObscuredPrefs.GetString(SelectionKey, "");
            int hat = CustomizationCatalog.Enabled ? Resolve(source.GKCNMADGAAF, Registered.GetValueOrDefault(source.Pointer), saved) : -1;
            int current = ObscuredPrefs.GetInt("HatID", 0);

            if (saved.Length > 0)
            {
                current = hat >= 0 ? hat : 0;
            }

            if (current < 0 || current >= source.GKCNMADGAAF.Count ||
                !CustomizationCatalog.Enabled && !Key(source, current).StartsWith("base:", StringComparison.Ordinal))
            {
                current = 0;
            }

            ObscuredPrefs.SetInt("HatID", current);
        }

        internal static string Key(SyncCustomization source, int hat)
        {
            if (!source || source.GKCNMADGAAF == null || hat < 0 || hat >= source.GKCNMADGAAF.Count) return "";

            Transform model = source.GKCNMADGAAF[hat];
            Models models = Registered.GetValueOrDefault(source.Pointer);
            Model custom = models?.Items.Values.FirstOrDefault(item => item.Transform && model && item.Transform.Pointer == model.Pointer);

            return custom?.Info.Id ?? "base:" + hat;
        }

        internal static int Resolve(SyncCustomization source, string key) =>
            source ? Resolve(source.GKCNMADGAAF, Registered.GetValueOrDefault(source.Pointer), key) : -1;

        private static int Resolve(Il2CppSystem.Collections.Generic.List<Transform> list, Models models, string key)
        {
            if (string.IsNullOrEmpty(key) || list == null) return -1;

            if (key.StartsWith("base:", StringComparison.Ordinal))
            {
                return int.TryParse(key[5..], out int id) && id >= 0 && id < list.Count &&
                    (models == null || !models.Items.Values.Any(model => model.Transform && list[id] && model.Transform.Pointer == list[id].Pointer)) ? id : -1;
            }

            return models != null && models.Items.TryGetValue(key, out Model entry) &&
                Core.CustomizationList.Any(info => info.Id == key && info.IsAvailable) ? Index(list, entry.Transform) : -1;
        }

        internal static void Save(SyncCustomization source, int hat)
        {
            if (source)
            {
                Save(source.GKCNMADGAAF, source.Pointer, hat);
            }
        }

        private static void Save(Il2CppSystem.Collections.Generic.List<Transform> list, IntPtr pointer, int hat)
        {
            Models models = Registered.GetValueOrDefault(pointer);
            string key = models?.Items.Values.FirstOrDefault(item => item.Transform &&
                hat >= 0 && hat < list.Count && list[hat] && item.Transform.Pointer == list[hat].Pointer)?.Info.Id ?? "";
            ObscuredPrefs.SetString(SelectionKey, key);
        }

        private static int Index(Il2CppSystem.Collections.Generic.List<Transform> list, Transform model)
        {
            for (int index = 0; index < list.Count; index++)
            {
                if (list[index] && model && list[index].Pointer == model.Pointer) return index;
            }

            return -1;
        }

        internal static void Update()
        {
            foreach (var pair in Registered.ToArray())
            {
                Models models = pair.Value;

                if (!models.Source || !models.Original || !models.Root)
                {
                    if (models.Root)
                    {
                        UnityEngine.Object.Destroy(models.Root.gameObject);
                    }

                    Registered.Remove(pair.Key);
                    Loading.Remove(pair.Key);

                    continue;
                }

                models.Root.localPosition = models.Original.localPosition;
                models.Root.localRotation = models.Original.localRotation;
                models.Root.localScale = models.Original.localScale;
            }
        }
    }
}
