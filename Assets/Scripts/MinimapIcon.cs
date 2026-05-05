using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    [Header("Icon Settings")]
    public Color iconColor = Color.red;
    public float iconSize = 10f;
    public bool showOnMinimap = true;
    public bool showDirection = false;

    [Header("Icon Type")]
    public IconType type = IconType.Enemy;

    public enum IconType
    {
        Enemy,
        Teammate,
        Objective,
        Loot,
        Custom
    }

    // Reference to the UI icon
    [HideInInspector] public RectTransform uiIcon;

    void Start()
    {
        // Set color based on type if not custom
        if (type != IconType.Custom)
        {
            switch (type)
            {
                case IconType.Enemy:
                    iconColor = Color.red;
                    break;
                case IconType.Teammate:
                    iconColor = Color.green;
                    break;
                case IconType.Objective:
                    iconColor = Color.yellow;
                    break;
                case IconType.Loot:
                    iconColor = new Color(1f, 0.5f, 0f); // Orange
                    break;
            }
        }
    }

    public void UpdateIcon()
    {
        if (uiIcon == null) return;

        uiIcon.sizeDelta = new Vector2(iconSize, iconSize);

        Image img = uiIcon.GetComponent<Image>();
        if (img != null)
            img.color = iconColor;

        uiIcon.gameObject.SetActive(showOnMinimap);
    }
}