using UnityEngine;

// Optional component for item prefabs: set a nice display name for the hover
// tooltip. If an item doesn't have this, a cleaned-up prefab name is shown
// instead (e.g. "LargeHealthPotion Variant" -> "Large Health Potion").
public class ItemInfo : MonoBehaviour
{
    [Tooltip("Name shown in the hover tooltip. Leave empty to auto-generate from the prefab name.")]
    public string displayName;

    public static string GetDisplayName(GameObject item)
    {
        if (item == null) return "";

        ItemInfo info = item.GetComponent<ItemInfo>();
        if (info != null && !string.IsNullOrEmpty(info.displayName))
            return info.displayName;

        return CleanName(item.name);
    }

    // Strips prefab-naming noise and spaces out CamelCase.
    public static string CleanName(string rawName)
    {
        string n = rawName.Replace("(Clone)", "");
        n = n.Replace("Prefab", "");
        n = n.Replace("Variant", "");
        n = n.Trim();

        System.Text.StringBuilder sb = new System.Text.StringBuilder(n.Length + 8);
        for (int i = 0; i < n.Length; i++)
        {
            if (i > 0 && char.IsUpper(n[i]) && !char.IsUpper(n[i - 1]) && n[i - 1] != ' ')
                sb.Append(' ');
            sb.Append(n[i]);
        }

        return sb.ToString();
    }
}
