using UnityEngine;

/// <summary>
/// Handles all player input.
/// Single Responsibility: Input reading and processing.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Enable/disable input (useful for cutscenes, menus, etc.)")]
    public bool inputEnabled = true;

    // Input values
    private float horizontalInput;
    private float verticalInput;
    private bool runInput;
    private bool jumpInput;
    private bool climbInput;

    // Input properties (read-only from outside)
    public float Horizontal => inputEnabled ? horizontalInput : 0f;
    public float Vertical => inputEnabled ? verticalInput : 0f;
    public bool Run => inputEnabled && runInput;
    public bool Jump => inputEnabled && jumpInput;
    public bool Climb => inputEnabled && climbInput;

    /// <summary>
    /// Get the movement input as a Vector2.
    /// </summary>
    public Vector2 MovementInput => new Vector2(Horizontal, Vertical);

    /// <summary>
    /// Check if there is any movement input.
    /// </summary>
    public bool HasMovementInput => Mathf.Abs(Horizontal) > 0.1f || Mathf.Abs(Vertical) > 0.1f;

    private void Update()
    {
        ReadInputs();
    }

    /// <summary>
    /// Read all inputs from the input system.
    /// </summary>
    private void ReadInputs()
    {
        if (!inputEnabled)
        {
            ClearInputs();
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        runInput = Input.GetKey(KeyCode.LeftShift);
        jumpInput = Input.GetButtonDown("Jump");
        climbInput = Input.GetKey(KeyCode.E);
    }

    /// <summary>
    /// Clear all input values.
    /// </summary>
    private void ClearInputs()
    {
        horizontalInput = 0f;
        verticalInput = 0f;
        runInput = false;
        jumpInput = false;
        climbInput = false;
    }

    /// <summary>
    /// Enable input processing.
    /// </summary>
    public void EnableInput()
    {
        inputEnabled = true;
    }

    /// <summary>
    /// Disable input processing.
    /// </summary>
    public void DisableInput()
    {
        inputEnabled = false;
        ClearInputs();
    }

    /// <summary>
    /// Temporarily block specific inputs (e.g., during hook impulse).
    /// </summary>
    public void BlockMovementInput(bool block)
    {
        if (block)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            runInput = false;
            jumpInput = false;
            climbInput = false;
        }
    }
}
