using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    public enum PlayerState
    {
        Idle = 0,
        Run = 1,
        Jump = 2,
        Attack1 = 3,
        Attack2 = 4,
        Attack3 = 5,
        Roll = 6,
        Die = 7
    }

    private Animator animator;
    private Rigidbody2D rb;
    private Transform spriteRoot;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 8f;
    public float gravity = -20f; // manual gravity

    [Header("Jumping")]
    public int maxJumps = 2;

    [Header("Combo / Attacks")]
    public int maxCombo = 3;
    public float comboWindow = 0.25f;
    public float[] attackDurations = new float[] { 0.45f, 0.45f, 0.6f };
    public string[] attackStateNames = new string[] { "Attack1", "Attack2", "Attack3" };

    [Header("Rolling")]
    public float rollDuration = 0.5f;
    public string rollStateName = "Roll";

    [Header("Runtime")]
    public PlayerState currentState = PlayerState.Idle;

    // inputs + runtime
    Vector2 moveInput;
    bool isGrounded;
    float verticalVelocity;

    // jump tracking
    int jumpsRemaining;

    // combo state
    bool isAttacking = false;
    int comboIndex = 0; // 0 = not attacking, 1..maxCombo = current attack
    float attackTimer = 0f; // time left for the current attack
    float comboTimer = 0f;  // time left to accept next input
    bool queuedNext = false; // whether player queued the next attack

    // roll state
    bool isRolling = false;
    float rollTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRoot = transform;

        // control gravity manually
        rb.gravityScale = 0f;

        // initialize jumps
        jumpsRemaining = maxJumps;

        // safety: ensure arrays are correct length
        if (attackDurations == null || attackDurations.Length < maxCombo)
        {
            attackDurations = new float[maxCombo];
            for (int i = 0; i < maxCombo; i++) attackDurations[i] = 0.5f;
        }

        if (attackStateNames == null || attackStateNames.Length < maxCombo)
        {
            attackStateNames = new string[maxCombo];
            for (int i = 0; i < maxCombo; i++) attackStateNames[i] = "Attack" + (i + 1);
        }
    }

    void Update()
    {
        // ---- GROUND CHECK (2D) ----
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Reset jumps when grounded
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;

            // Keep grounded
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }

        // Gravity
        verticalVelocity += gravity * Time.deltaTime;

        // Movement: allowed while attacking, blocked only while rolling
        Vector2 velocity = rb.linearVelocity;
        if (!isRolling)
        {
            velocity.x = moveInput.x * moveSpeed;
        }
        else
        {
            velocity.x = 0f;
        }
        velocity.y = verticalVelocity;
        rb.linearVelocity = velocity;

        // Update combo timers
        if (isAttacking)
        {
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;

            if (comboTimer > 0f)
                comboTimer -= Time.deltaTime;

            // If current attack finished
            if (attackTimer <= 0f)
            {
                if (queuedNext && comboIndex < maxCombo)
                {
                    // progress to next attack
                    comboIndex++;
                    PlayAttackAnimation(comboIndex);
                    attackTimer = attackDurations[Mathf.Clamp(comboIndex - 1, 0, attackDurations.Length - 1)];
                    comboTimer = comboWindow;
                    queuedNext = false;

                    // update state integer to corresponding AttackN
                    SetState((PlayerState)(PlayerState.Attack1 + (comboIndex - 1)));
                }
                else
                {
                    // end combo
                    EndCombo();
                }
            }
        }

        // Update roll timer
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                EndRoll();
            }
        }

        UpdateAnimator();

        // killzone / respawn
        if (transform.position.y <= -10)
        {
            GameMangerSingleton.Instance.RestartScene();
            gameObject.SetActive(false);
        }
    }

    // ---------- INPUT ----------

    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();

        // Flip sprite in 2D by scaling X - disabled while rolling (allowed while attacking)
        if (!isRolling && Mathf.Abs(moveInput.x) > 0.01f)
        {
            if (moveInput.x > 0)
                spriteRoot.localScale = new Vector3(1f, 1f, 1f);
            else
                spriteRoot.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Only update Run/Idle if not attacking or rolling
        if (!isAttacking && !isRolling)
        {
            if (isGrounded)
            {
                if (Mathf.Abs(moveInput.x) > 0.01f)
                    SetState(PlayerState.Run);
                else
                    SetState(PlayerState.Idle);
            }
        }
    }

    public void OnJumpInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // If currently attacking and you want to cancel into jump:
        if (isAttacking)
        {
            CancelCombo(); // cancel attack
            // allow normal jump after cancel
        }

        // If currently rolling, do not allow jump (you can change this behavior)
        if (isRolling) return;

        if (jumpsRemaining > 0)
        {
            verticalVelocity = jumpForce;
            jumpsRemaining--;

            PlayJumpAnimation();
            SetState(PlayerState.Jump);
        }
    }

    public void OnAttackInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // If currently rolling, don't allow attack
        if (isRolling) return;

        // If not currently attacking, start the combo
        if (!isAttacking)
        {
            StartCombo();
        }
        else
        {
            // If we are attacking and within combo window, queue the next attack
            if (comboTimer > 0f && comboIndex < maxCombo)
            {
                queuedNext = true;
            }
            // If out of combo window, the press does nothing
        }
    }

    public void OnRollInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // If currently attacking, cancel the attack and start a roll
        if (isAttacking)
        {
            CancelCombo();
            StartRoll();
            return;
        }

        // If currently rolling, pressing roll again does nothing
        if (isRolling) return;

        // Optionally, disallow rolling in mid-air:
        // if (!isGrounded) return;

        StartRoll();
    }

    // ---------- Combo control ----------

    void StartCombo()
    {
        isAttacking = true;
        comboIndex = 1;
        queuedNext = false;

        PlayAttackAnimation(comboIndex);

        attackTimer = attackDurations[Mathf.Clamp(comboIndex - 1, 0, attackDurations.Length - 1)];
        comboTimer = comboWindow;

        SetState((PlayerState)(PlayerState.Attack1 + (comboIndex - 1)));
    }

    void EndCombo()
    {
        isAttacking = false;
        comboIndex = 0;
        attackTimer = 0f;
        comboTimer = 0f;
        queuedNext = false;

        // return to appropriate state
        if (isGrounded)
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
                SetState(PlayerState.Run);
            else
                SetState(PlayerState.Idle);
        }
        else
        {
            SetState(PlayerState.Jump);
        }
    }

    void CancelCombo()
    {
        // Stop any combo/timers and reset variables
        isAttacking = false;
        comboIndex = 0;
        attackTimer = 0f;
        comboTimer = 0f;
        queuedNext = false;
    }

    void PlayAttackAnimation(int attackNumber)
    {
        if (animator == null) return;

        int stateIndex = Mathf.Clamp(attackNumber - 1, 0, attackStateNames.Length - 1);
        string stateName = attackStateNames[stateIndex];

        // Set numeric state to match your enum (Attack1, Attack2, Attack3)
        if (attackNumber == 1)
            animator.SetInteger("state", (int)PlayerState.Attack1);
        else if (attackNumber == 2)
            animator.SetInteger("state", (int)PlayerState.Attack2);
        else if (attackNumber == 3)
            animator.SetInteger("state", (int)PlayerState.Attack3);

        // Force replay the exact animation state
        animator.Play(stateName, 0, 0f);
    }

    // ---------- Roll control ----------

    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;

        // lock movement, set state and force-play roll animation
        SetState(PlayerState.Roll);
        if (animator != null)
            animator.Play(rollStateName, 0, 0f);
    }

    void EndRoll()
    {
        isRolling = false;
        rollTimer = 0f;

        // return to appropriate state
        if (isGrounded)
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
                SetState(PlayerState.Run);
            else
                SetState(PlayerState.Idle);
        }
        else
        {
            SetState(PlayerState.Jump);
        }
    }

    // ---------- STATE / ANIMATION ----------

    public void SetState(PlayerState newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        if (animator != null)
            animator.SetInteger("state", (int)currentState);
    }

    void PlayJumpAnimation()
    {
        if (animator == null) return;

        animator.SetInteger("state", (int)PlayerState.Jump);
        animator.Play("Jump", 0, 0f); // restart jump animation (name must match)
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // If attacking, attack playback controls Animator (we used animator.Play already).
        if (isAttacking)
            return;

        // If rolling, roll playback controls Animator
        if (isRolling)
            return;

        if (isGrounded)
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
                SetState(PlayerState.Run);
            else
                SetState(PlayerState.Idle);
        }
        else
        {
            SetState(PlayerState.Jump);
        }
    }

    // Just to help visualize ground check in the Scene view
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
