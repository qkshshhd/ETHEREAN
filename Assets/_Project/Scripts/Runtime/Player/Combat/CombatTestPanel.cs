using UnityEngine;

namespace Etherean.Player
{
    public sealed class CombatTestPanel : MonoBehaviour
    {
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private CombatTestTarget[] targets;

        private void OnGUI()
        {
            if (attack == null) return;
            GUILayout.BeginArea(new Rect(12, 12, 420, 260), GUI.skin.box);
            GUILayout.Label("GREATSWORD / 3 HIT COMBO");
            GUILayout.Label("A/D: move | Space: jump | Enter / left click: attack");
            GUILayout.Label($"Weapon: {attack.WeaponUnlocked} | {attack.Phase} | Move: {attack.MoveMultiplier:F2}");
            GUILayout.Label($"Combo: {attack.ComboStep}/3 | Next queued: {attack.ComboQueued}");
            GUILayout.Label($"Attack selected at start: {(attack.StartedInAir ? "Air" : "Ground")}");
            if (GUILayout.Button("Toggle test weapon lock")) attack.SetWeaponUnlocked(!attack.WeaponUnlocked);
            if (GUILayout.Button("Reset targets"))
                foreach (var target in targets)
                {
                    if (target == null) continue;
                    target.gameObject.SetActive(false);
                    target.gameObject.SetActive(true);
                }
            foreach (var target in targets)
                if (target != null) GUILayout.Label($"{target.name}: HP {target.Health} / Hits {target.HitCount}");
            GUILayout.EndArea();
        }
    }
}
