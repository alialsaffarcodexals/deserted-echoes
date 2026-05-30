using UnityEngine;

// Attach to any GameObject in Credits.unity.
// Plays the credits video automatically; returns to main-menu when done or on Esc.
public class CreditsScene : MonoBehaviour
{
    private void Start()
    {
        CreditsVideoPlayer.Play(() => SceneLoader.LoadScene("main-menu"));
    }
}
