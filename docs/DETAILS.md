# CustomPlayerCustomization 1.3.0

Custom hats are registered before the native menu or network player applies a saved HatID. Repeated initialization preserves native ordering and does not append duplicate models. The saved custom selection uses the content ID, so a room which excludes that pack falls back to the default hat instead of selecting another pack at the same numeric index.

On NullFrame servers with this addon enabled, open the pause menu, choose Customization, then the skin page. Hat Slot 1 is the native hat slot. Slots 2–4 can contain other hats or None. Apply Changes updates the preview and broadcasts the selection. Extra slots are saved in UserData/NullFrame/hats.json and are inactive on official servers and in rooms without the addon.

Install both CustomPlayerCustomization.dll 1.3.0 and NullServers.dll 0.0.30 on the host and clients. Existing customization bundles and customization.json files keep their format. The namespace base: is reserved for native hat identifiers in saved selections.

The optional CustomizationCatalog API remains version 1. SelectionApi 1 exposes HatKey, ResolveHat, SaveSelection and CreateHat; SlotCount is 4. CustomizationCatalog.Enabled and the existing content-selection contract remain available to NullServers. Custom ragdolls use stable content IDs to create the required TPS hat prefabs.

Ragdoll snapshots preserve color, skin URLs and all selected hats. Appearance is reapplied after native corpse initialization; face and TV materials are preserved independently of fur/skin material zero.
