using UnityEngine;

// can choose either calm or tense music based on level
public enum DialogueMood { Calm, Tense }

//scriptable object that lets me create convos in editor
[CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue/Conversation")]
public class Conversation : ScriptableObject
{
    // unique id used to remember if the player already saw this one
    // leave empty if i want it to play every time
    public string firstVisitKey;

    public DialogueMood mood = DialogueMood.Calm;

    public DialogueLine[] lines;
}