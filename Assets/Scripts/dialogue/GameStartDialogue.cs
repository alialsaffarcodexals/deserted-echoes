using UnityEngine;

// fires the game start dialogue once when the scene loads
public class GameStartDialogue : MonoBehaviour
{
    [SerializeField] private DialogueTrigger trigger;

    private void Start()
    {
        if (trigger != null)
        {
            trigger.Fire();
        }
    }
}