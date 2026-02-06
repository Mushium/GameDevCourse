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
    
    [Header("Attack Position")]
    public Transform AttackPoint;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 8f;
    public float gravity = -20f;

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

    [Header("Knockback")]
    public float knockbackForce = 12f;
    public float knockbackDuration = 0.2f;

    [Header("Runtime")]
    public PlayerState currentState = PlayerState.Idle;
    
    Vector2 moveInput;
    bool isGrounded;
    float verticalVelocity;
    
    int jumpsRemaining;

    bool isAttacking = false;
    int comboIndex = 0; // 0 = not attacking, 1..maxCombo = current attack
    float attackTimer = 0f; // time left for the current attack
    float comboTimer = 0f;  // time left to accept next input
    bool queuedNext = false; // whether player queued the next attack

    // roll state
    bool isRolling = false;
    float rollTimer = 0f;

    // knockback state
    bool isKnocked = false;
    float knockbackTimer = 0f;
    float knockbackDirX = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRoot = transform;

        // custom gravity, so disable built-in
        rb.gravityScale = 0f;
        jumpsRemaining = maxJumps;

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
        if (currentState == PlayerState.Die) return;

        if (GameMangerSingleton.Instance.Health <= 0)
        {
            currentState = PlayerState.Die;
            animator.SetInteger("state", (int)currentState);
            return;
        }
        // -------- Ground check --------
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
        
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }

        // -------- Gravity --------
        verticalVelocity += gravity * Time.deltaTime;

        // -------- Knockback timer --------
        if (isKnocked)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnocked = false;
            }
        }

        // -------- Movement & velocity --------
        Vector2 velocity = rb.linearVelocity;

        // X movement
        if (isKnocked)
        {
            // ignore input, just fly away
            velocity.x = knockbackDirX * knockbackForce;
        }
        else
        {
            if (!isRolling)
                velocity.x = moveInput.x * moveSpeed;
            else
                velocity.x = 0f;
        }

        // Y movement from our custom verticalVelocity
        velocity.y = verticalVelocity;

        rb.linearVelocity = velocity;

        // -------- Combo timers --------
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

        // -------- Roll timer --------
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                EndRoll();
            }
        }

        // -------- Animator state --------
        UpdateAnimator();

        // -------- Killzone / respawn --------
        if (transform.position.y <= -10)
        {
            GameMangerSingleton.Instance.RestartScene();
            gameObject.SetActive(false);
        }
    }

    // ---------- INPUT ----------

    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        if (isKnocked) return; // ignore move while knocked

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
        if (!isAttacking && !isRolling && !isKnocked)
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
        if (isKnocked) return; // no jump while knocked

        // If currently attacking and you want to cancel into jump:
        if (isAttacking)
        {
            CancelCombo(); // cancel attack
        }

        // If currently rolling, do not allow jump (you can change this behavior)
        if (isRolling) return;
        
        Collider2D collider = Physics2D.OverlapCircle(
            AttackPoint.position,
            0.1f,
            LayerMask.GetMask("Default")
        );

        if (collider != null)
        {
            verticalVelocity = jumpForce;
            PlayJumpAnimation();
            SetState(PlayerState.Jump);
            return;
        }

        

        if (jumpsRemaining > 0)
        {
            verticalVelocity = jumpForce;
            jumpsRemaining--;

            PlayJumpAnimation();
            SetState(PlayerState.Jump);
        }
    }
    
    
    public void OnInteractInput(InputAction.CallbackContext ctx)
    {
            if (!ctx.performed) return;
            Collider2D[] NPC= Physics2D.OverlapCircleAll(transform.position, 4f, LayerMask.GetMask("NPC"));
            if (NPC.Length == 0) return;
            NPC[0].GetComponent<StartDialog>().OnInteract();
    }

    public void OnAttackInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isKnocked) return; // no attack while knocked

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
        }
    }

    public void OnRollInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (isKnocked) return; // no roll while knocked

        // If currently attacking, cancel the attack and start a roll
        if (isAttacking)
        {
            CancelCombo();
            StartRoll();
            return;
        }

        // If currently rolling, pressing roll again does nothing
        if (isRolling) return;

        // Optionally disallow rolling in mid-air:
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

        // If attacking, rolling, or knocked, don't override their animations
        if (isAttacking || isRolling || isKnocked)
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

    // ---------- COMBAT ----------

    public void Attack(int c)
    {
        Collider2D collider = Physics2D.OverlapCircle(
            AttackPoint.position,
            0.2f,
            LayerMask.GetMask("Enemy")
        );
        
        if (collider == null) return;

        collider.gameObject.GetComponent<Enemy>().TakeDamage(c);
    }

    public void KnockBack(Transform obj)
    {
        // direction away from the source
        Vector2 dir = (transform.position - obj.position).normalized;

        // horizontal knock direction
        knockbackDirX = Mathf.Sign(dir.x);
        if (knockbackDirX == 0)
            knockbackDirX = 1f; // fallback

        // upward kick
        verticalVelocity = 6f; // tweak as needed

        isKnocked = true;
        knockbackTimer = knockbackDuration;

        // Cancel other actions
        CancelCombo();
        isRolling = false;

        // Optional: switch to jump state or add a hit state later
        SetState(PlayerState.Jump);
    }

    // ---------- Gizmos ----------

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (AttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackPoint.position, 0.2f);
        }
    }
}
