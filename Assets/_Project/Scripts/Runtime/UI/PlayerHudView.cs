using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Etherean.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHudView : MonoBehaviour
    {
        [SerializeField] private GameObject content;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text itemCountText;
        [SerializeField] private GameObject healingProgressRoot;
        [SerializeField] private TMP_Text healingProgressText;
        [SerializeField] private Image healingProgressFill;
        [SerializeField] private string healthFormat = "HP {0} / {1}";
        [SerializeField] private string itemCountFormat = "Heal {0} / {1}";
        [SerializeField] private string healingProgressFormat = "Healing {0}%";
        private int _health = -1, _maximum = -1, _count = -1, _capacity = -1, _percent = -1;

        public void Show(int health, int maximum, int count, int capacity, bool healing, float progress)
        {
            if (content != null) content.SetActive(true);
            if (healthText != null && (health != _health || maximum != _maximum))
                healthText.text = string.Format(healthFormat, health, maximum);
            if (itemCountText != null && (count != _count || capacity != _capacity))
                itemCountText.text = string.Format(itemCountFormat, count, capacity);
            if (healthFill != null) healthFill.fillAmount = maximum > 0 ? Mathf.Clamp01((float)health / maximum) : 0;
            if (healingProgressRoot != null) healingProgressRoot.SetActive(healing);
            int percent = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100);
            if (healingProgressText != null && percent != _percent)
                healingProgressText.text = string.Format(healingProgressFormat, percent);
            if (healingProgressFill != null) healingProgressFill.fillAmount = healing ? Mathf.Clamp01(progress) : 0;
            _health = health; _maximum = maximum; _count = count; _capacity = capacity; _percent = percent;
        }

        public void Hide()
        {
            if (content != null) content.SetActive(false);
            _health = _maximum = _count = _capacity = _percent = -1;
        }
    }
}
