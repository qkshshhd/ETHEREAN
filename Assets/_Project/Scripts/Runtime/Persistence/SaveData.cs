using System;

namespace Etherean.Persistence
{
    public enum ProgressFlag
    {
        WeaponUnlocked, WallTraversalUnlocked, BoarIntroSeen, BoarEncounterCleared,
        DExitOpened, ShortcutCEOpened, ShortcutHBOpened, ForestExitReached
    }

    [Serializable]
    public sealed class PickupSave
    {
        public string id;
        public int remaining;
    }

    [Serializable]
    public sealed class SaveData
    {
        public string signature;
        public int version;
        public string checkpointId;
        public string scenePath;
        public int healingItems;
        public int totalConsumed;
        public string[] flags;
        public PickupSave[] pickups;
    }
}
