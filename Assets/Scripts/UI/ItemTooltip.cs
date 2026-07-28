using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Minecraft-style hover tooltip. Builds its own overlay canvas at runtime the
// first time something is shown, so no scene/prefab setup is needed.
public class ItemTooltip : MonoBehaviour
{
    private static ItemTooltip instance;

    private RectTransform panelRect;
    private TextMeshProUGUI label;

    // Whoever called Show last; Hide only works for the same owner so a
    // fast hover across two slots can't hide the newer tooltip.
    private object owner;

    public static void Show(string text, object who)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (instance == null)
            instance = CreateInstance();

        instance.owner = who;
        instance.label.text = text;

        Vector2 textSize = instance.label.GetPreferredValues(text);
        instance.panelRect.sizeDelta = textSize + new Vector2(18f, 10f);

        instance.panelRect.gameObject.SetActive(true);
        instance.FollowMouse();
    }

    public static void HideFor(object who)
    {
        if (instance == null || instance.owner != who) return;

        instance.owner = null;
        instance.panelRect.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (panelRect != null && panelRect.gameObject.activeSelf)
            FollowMouse();
    }

    private void FollowMouse()
    {
        if (Mouse.current == null) return;

        // Pivot is top-left, so the tooltip hangs below-right of the cursor.
        Vector2 pos = Mouse.current.position.ReadValue() + new Vector2(18f, -12f);

        pos.x = Mathf.Min(pos.x, Screen.width - panelRect.sizeDelta.x);
        pos.y = Mathf.Max(pos.y, panelRect.sizeDelta.y);

        panelRect.position = pos;
    }

    private static ItemTooltip CreateInstance()
    {
        GameObject root = new GameObject("ItemTooltipCanvas");
        DontDestroyOnLoad(root);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000; // above every other UI canvas

        ItemTooltip tip = root.AddComponent<ItemTooltip>();

        GameObject panel = new GameObject("Tooltip");
        panel.transform.SetParent(root.transform, false);

        tip.panelRect = panel.AddComponent<RectTransform>();
        tip.panelRect.pivot = new Vector2(0f, 1f);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.06f, 0.04f, 0.9f);
        bg.raycastTarget = false; // never block clicks/drags underneath

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(panel.transform, false);

        tip.label = textObj.AddComponent<TextMeshProUGUI>();
        tip.label.fontSize = 22f;
        tip.label.alignment = TextAlignmentOptions.Center;
        tip.label.raycastTarget = false;

        RectTransform textRect = tip.label.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        panel.SetActive(false);
        return tip;
    }
}
