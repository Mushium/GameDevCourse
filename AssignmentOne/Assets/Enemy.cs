using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int Health = 3;

    public enum EnemyState
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Attack = 3,
        Die = 4
    }

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public Vector2 patrolBoxSize = new Vector2(15f, 1f);
    public float directionChangeIntervalMin = 1.5f;
    public float directionChangeIntervalMax = 3f;
    public Transform AttackPoint;

    [Header("Player Detection")]
    public LayerMask playerLayer;
    public float attackRange = 1.0f;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public EnemyState currentState = EnemyState.Idle;

    // for random walk
    private float directionTimer;
    private int moveDirection = 0;   // -1 = left, 0 = idle, 1 = right

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ResetDirectionTimer();
    }

    void Update()
    {
        if (currentState == EnemyState.Die)
        {
            // optionally zero velocity when dead
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // 1) Check for player in detection box
        Collider2D player = Physics2D.OverlapBox(
            transform.position,
            patrolBoxSize,
            0f,
            playerLayer
        );

        if (player != null)
        {
            HandleChaseAndAttack(player.transform);
        }
        else
        {
            HandleRandomWalk();
        }
    }

    void HandleRandomWalk()
    {
        // Randomly change direction / idle
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            // -1 (left), 0 (idle), 1 (right)
            moveDirection = Random.Range(-1, 2);
            ResetDirectionTimer();
        }

        if (moveDirection == 0)
        {
            SetState(EnemyState.Idle);
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            SetState(EnemyState.Walk);

            if (rb != null)
                rb.linearVelocity = new Vector2(moveDirection * walkSpeed, rb.linearVelocity.y);
            
            if (moveDirection > 0)
                transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            else
                transform.localScale = new Vector3(-1.2f, 1.2f, 1f);
        }
    }

    void HandleChaseAndAttack(Transform player)
    {
        Vector2 toPlayer = player.position - transform.position;
        float distance = Mathf.Abs(toPlayer.x);

        // If close enough horizontally -> Attack
        if (distance <= attackRange)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            SetState(EnemyState.Attack);
            return;
        }

        // Otherwise, run toward player
        SetState(EnemyState.Run);

        int dir = toPlayer.x > 0 ? 1 : -1;

        if (rb != null)
            rb.linearVelocity = new Vector2(dir * runSpeed, rb.linearVelocity.y);

        // Flip sprite
        if (spriteRenderer != null)
            if (dir > 0)
                transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            else
                transform.localScale = new Vector3(-1.2f, 1.2f, 1f);
    }

    void ResetDirectionTimer()
    {
        directionTimer = Random.Range(directionChangeIntervalMin, directionChangeIntervalMax);
    }
    
    public void Attack()
    {
        Collider2D collider = Physics2D.OverlapCircle(AttackPoint.position, 0.5f, LayerMask.GetMask("Player"));
        
        if (collider == null) return;
        
        
        collider.gameObject.GetComponent<PlayerMovement2D>().KnockBack(transform);
        GameMangerSingleton.Instance.Health -= 1;


    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        StartCoroutine(Flash());

        if (Health <= 0)
        {
            Health = 0;
            SetState(EnemyState.Die);
        }
    }

    IEnumerator Flash()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    public void SetState(EnemyState newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        if (animator != null)
            animator.SetInteger("State", (int)currentState);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, patrolBoxSize);
        Gizmos.DrawWireSphere(AttackPoint.position, 0.5f);

    }
}
