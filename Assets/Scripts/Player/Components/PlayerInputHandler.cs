using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public float MoveX { get; private set; }
    public float MoveZ { get; private set; } // For 3D movement usually use Vector2 but separate axes are fine
    public bool JumpTriggered { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool ClimbHeld { get; private set; }
    
    // Grapple Inputs (if handled here or by HookInputController)
    // HookInputController seems to handle its own input, but we might want to consolidate.
    // For this refactor, I'll focus on movement inputs.

    private void Update()
    {
        MoveX = Input.GetAxisRaw("Horizontal");
        MoveZ = Input.GetAxisRaw("Vertical");
        JumpTriggered = Input.GetButtonDown("Jump");
        SprintHeld = Input.GetKey(KeyCode.LeftShift);
        ClimbHeld = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.LeftControl); // Adding Ctrl as option
    }
}
