using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkestLike.InDungeon.Unit
{
    public class CharacterUnitHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Image healthBar;
        [SerializeField] Image selectImage;

        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }

        public void ActiveSelectImage(bool isActive)
        {
            selectImage.gameObject.SetActive(isActive);
        }
    }
}
