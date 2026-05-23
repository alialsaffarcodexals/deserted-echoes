using UnityEngine;

//script used to trigger dialogue either attahed to zone/trigger or called from other scripts
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Conversation conversation;

    // if true the convo only plays once 
    [SerializeField] private bool firstVisitOnly = true;

    //it fires automatically when the player walks into the trigger zone
   
    [SerializeField] private bool fireOnPlayerEnter = true;

    // call this from other scripts to start the dialogue manually
    public void Fire()
    {
        if (conversation == null) return;

        // skip if the player already saw this one and its a first visit only convo
        if (firstVisitOnly && DialogueManager.HasSeen(conversation.firstVisitKey)) return;

        DialogueManager.Instance.StartConversation(conversation);
    }

    // 2d trigger detection
    // make sure the gameobject has a collider2d with Is Trigger checked
    // and the player has the "Player" tag
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!fireOnPlayerEnter) return;
        if (!other.CompareTag("Player")) return;
        Fire();
    }
}