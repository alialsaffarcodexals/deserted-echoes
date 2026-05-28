// ─────────────────────────────────────────────────────────────
// BlinkText.cs
// Deserted Echoes | IT8101 Games Development | Group 3
// Description: Blinks a UI Text's alpha on/off. Used by the
//              opening cinematic's "PRESS ANY KEY" prompt.
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class BlinkText : MonoBehaviour
{
    [SerializeField] private float interval = 0.6f;

    private Text _text;
    private float _timer;
    private bool _on = true;

    private void Awake() => _text = GetComponent<Text>();

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < interval) return;
        _timer = 0f;
        _on = !_on;
        Color c = _text.color;
        c.a = _on ? 1f : 0.35f;
        _text.color = c;
    }
}
