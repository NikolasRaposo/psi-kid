using System;
using UnityEngine;

/// <summary>
/// Drives the player's horizontal movement, jumping, ground check, sprite
/// facing and interaction (elevators). All input comes from <see cref="GameInput"/>
/// (Input System: keyboard, mouse or gamepad, or a scripted source).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [Header("Movement")]
    [Tooltip("Horizontal movement speed, in units per second.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("Vertical impulse applied when the player jumps.")]
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground check")]
    [Tooltip("Transform marking the point used to test for ground contact.")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Radius of the ground-check overlap circle.")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [Tooltip("Layers considered 'ground' for jumping.")]
    [SerializeField] private LayerMask groundLayer;

    /// <summary>Raised when the player requests a teleport (handled by elevators).</summary>
    public event Action<GameObject> OnTeleportRequest;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private float moveInput;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) {
            Debug.LogError("Animator not found in the PlayerController's children.");
        }
    }

    private void Update() {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundLayer);
        moveInput = GameInput.MoveX;

        HandleAnimations();
        FlipSprite();

        if (GameInput.JumpPressed && isGrounded) {
            Jump();
        }
        if (GameInput.InteractPressed) {
            HandleInteraction();
        }
    }

    private void FixedUpdate() {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump() {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Sfx.Play("jump", 0.35f, UnityEngine.Random.Range(0.95f, 1.08f));
    }

    private void HandleInteraction() {
        OnTeleportRequest?.Invoke(gameObject);
    }

    private void HandleAnimations() {
        if (isGrounded) {
            animator.SetBool("isJumping", false);
            animator.SetBool("isWalking", moveInput != 0);
        } else {
            animator.SetBool("isJumping", true);
        }
    }

    private void FlipSprite() {
        if (moveInput > 0) {
            transform.localScale = new Vector3(1, 1, 1);
        } else if (moveInput < 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
