using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchLight : MonoBehaviour
{

    public Light2D torchLight;

    // checks where the plyer is looking/facing
    private Animator playerAnimator;

    // keeps track if the torch is on or off
    bool torchIsOn = false;

    void Start()
    {
        playerAnimator = GetComponentInParent<Animator>();

        if (torchLight != null)
            torchLight.enabled = false;
    }

    void Update()
    {
        // press T to test the torch, remove this when inventory is done
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (torchIsOn) DisableTorch();
            else EnableTorch();
        }

        if (!torchIsOn) return;
        RotateLight();
    }

    void RotateLight()
    {
        // get the mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // work out the direction from the player to the mouse
        Vector2 direction = mousePos - transform.position;

        // convert that direction to an angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // rotate the light to point at the mouse
        torchLight.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // the inventory system will call this when the player picks up the torch
    public void EnableTorch()
    {
        torchIsOn = true;
        if (torchLight != null)
            torchLight.enabled = true;
    }

    // the inventory system will call this when the torch is dropped or runs out
    public void DisableTorch()
    {
        torchIsOn = false;
        if (torchLight != null)
            torchLight.enabled = false;
    }
}