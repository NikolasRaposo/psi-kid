using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.2f;
    private bool isGrounded;
    private float moveInput;
    public LayerMask groundLayer;
    public Transform groundCheck; // Ponto de verifica��o de solo

    private Rigidbody2D rb;
    private PlayerState currentState;
    private Animator animator;

    public event Action<GameObject> OnTeleportRequest;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) {
            Debug.LogError("Animator n�o encontrado em objetos filhos do PlayerController.");
        }
    }

    private void Update() {
        // Verifica se o jogador est� no ch�o
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundLayer);

        moveInput = Input.GetAxisRaw("Horizontal");
        // Alterna as anima��es
        HandleAnimations();

        // Inverte a escala do sprite dependendo da dire��o do movimento
        FlipSprite();
        // Realiza o pulo
        if (Input.GetButtonDown("Jump") && isGrounded) {
            Jump();
        }
    }
    private void FixedUpdate() {
        // Movimenta o jogador horizontalmente
        MovePlayer();
    }

    private void MovePlayer() {
        // Move o jogador de acordo com o input
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
    private void OnEnable() {
        PlayerInputHandler.OnJump += Jump;
        PlayerInputHandler.OnInteraction += HandleInteraction;
    }

    private void OnDisable() {
        PlayerInputHandler.OnJump -= Jump;
    }

    private void HandleInteraction() {
        if (OnTeleportRequest != null) {
            OnTeleportRequest.Invoke(gameObject);
        }
    }

    public void Jump() {
        if (isGrounded) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void HandleAnimations() {
        if (isGrounded) {
            animator.SetBool("isJumping", false);
            if (moveInput != 0) {
                // Se o jogador est� se movendo
                animator.SetBool("isWalking", true);
            } else {
                // Se o jogador n�o est� se movendo
                animator.SetBool("isWalking", false);
            }
        } else {
            // Se o jogador est� no ar
            animator.SetBool("isJumping", true);
        }
    }
    private void FlipSprite() {
        // Inverte a dire��o do sprite com base no movimento horizontal
        if (moveInput > 0) {
            transform.localScale = new Vector3(1, 1, 1); // Normal
        } else if (moveInput < 0) {
            transform.localScale = new Vector3(-1, 1, 1); // Invertido
        }
    }
}
