using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float inputDeadzone = 0.15f;

    Rigidbody2D rb2d;
    Animator animator;
    int currentFacing = 0;
    string lastAnimState = "";
    string[] availableStates;
    System.Collections.Generic.Dictionary<string,string> stateCache = new System.Collections.Generic.Dictionary<string,string>();

    PlayerInputActions input;
    Vector2 currentMoveInput = Vector2.zero;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null) rb2d.freezeRotation = true;
        animator = GetComponentInChildren<Animator>();
        input = new PlayerInputActions();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            availableStates = new string[clips.Length];
            for (int i = 0; i < clips.Length; i++) availableStates[i] = clips[i].name;
        }
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();

        float mag = moveInput.magnitude;
        if (mag < inputDeadzone) moveInput = Vector2.zero;
        else moveInput = moveInput.normalized * Mathf.Clamp01(mag);

        if (moveInput.sqrMagnitude > 0f)
        {
            int facing = Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)
                ? (moveInput.x > 0 ? 2 : 1)
                : (moveInput.y > 0 ? 3 : 0);
            currentFacing = facing;
            if (animator) animator.SetInteger("Facing", currentFacing);
        }

        if (animator) animator.SetFloat("Speed", moveInput.magnitude);

        if (animator)
        {
            string dirName = FacingName(currentFacing);
            string logicalState = (moveInput.magnitude > 0f ? "Walk_Shadow_" : "Idle_Shadow_") + dirName;
            string desiredState = ResolveStateName(logicalState);
            if (!string.IsNullOrEmpty(desiredState) && desiredState != lastAnimState)
            {
                int hash = Animator.StringToHash(desiredState);
                int layer = 0;
                animator.CrossFade(hash, 0f, layer, 0f);
                animator.Update(0f);
                lastAnimState = desiredState;
            }
        }
        // store for FixedUpdate movement
        currentMoveInput = moveInput;
    }

    void FixedUpdate()
    {
        Vector2 displacement = currentMoveInput * walkSpeed * Time.fixedDeltaTime;
        if (rb2d != null)
        {
            rb2d.MovePosition(rb2d.position + displacement);
        }
        else
        {
            transform.position += new Vector3(displacement.x, displacement.y, 0f);
        }
    }

    string FacingName(int f)
    {
        switch (f)
        {
            case 0: return "down";
            case 1: return "left";
            case 2: return "right";
            case 3: return "up";
            default: return "down";
        }
    }

    string ResolveStateName(string logical)
    {
        if (string.IsNullOrEmpty(logical)) return null;
        if (stateCache.TryGetValue(logical, out var cached)) return cached;
        if (availableStates == null || availableStates.Length == 0)
        {
            stateCache[logical] = null;
            return null;
        }
        // exact match
        foreach (var s in availableStates)
            if (s == logical) { stateCache[logical] = s; return s; }
        var low = logical.ToLowerInvariant();
        // try contains match
        foreach (var s in availableStates)
            if (s.ToLowerInvariant().Contains(low)) { stateCache[logical] = s; return s; }
        // try partial matches by split
        var parts = low.Split(new[]{'_', ' '}, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in availableStates)
        {
            var sl = s.ToLowerInvariant();
            bool all = true;
            foreach (var p in parts) if (!sl.Contains(p)) { all = false; break; }
            if (all) { stateCache[logical] = s; return s; }
        }
        stateCache[logical] = null;
        Debug.LogWarning($"Animator state not found (tried resolving): {logical}");
        return null;
    }
}
