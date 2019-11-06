using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale : Bomb {

  public int jumpForce;
  [Range (0.7f, 1.5f)]
  public float attackRadius;
  [Range (0.75f, 2f)]
  public float jumpMoveDelay;

  public AudioSource whaleKilledSrc; 

  Transform target;
  Transform leftBarrier, rightBarrier, groundBarrier;
  Rigidbody2D rb;
  SpriteRenderer srenderer;

  bool jumping = false;
  bool alive = true;
  const float attackCooldown = 1.25f;
  float cooldownTimer = 0, timeSinceBirth = 0;

  const float yHitDelta = 0.8f;
  const float xHitDelta = 0.5f;

  System.Action OnKilledByPlayer;

  void OnEnable () {
    target = FindObjectOfType<PlayerController> ().transform;
    rb = GetComponent<Rigidbody2D> ();
    srenderer = GetComponent<SpriteRenderer> ();
    leftBarrier = GameObject.Find ("Sides1").transform;
    rightBarrier = GameObject.Find ("Sides2").transform;
    groundBarrier = GameObject.Find ("Ground Collider").transform;
    jumping = true;
  }

  protected override void Awake () {
    base.Awake ();
    whaleKilledSrc = GameObject.Find("Whale Killed Source").GetComponent<AudioSource>();
    OnKilledByPlayer += () => {
      whaleKilledSrc.Play();
      GameManager.Instance.Score += 200;
      };
  }

  protected override void Update () {
    if (alive) {
      timeSinceBirth += Time.deltaTime;
      if (timer < jumpMoveDelay)
        timer += Time.deltaTime;

      if (cooldownTimer < attackCooldown) {
        cooldownTimer += Time.deltaTime;
      }

      if (cooldownTimer >= attackCooldown) {
        if (!jumping && timer >= jumpMoveDelay && Vector2.Distance (target.position, transform.position) < attackRadius) {
          animator.SetBool ("jumping", true);
          animator.SetTrigger ("bite");
          Bite ();
          cooldownTimer = 0;
        }
      }

      if (!jumping && timer >= jumpMoveDelay) {
        Jump ();
        animator.SetBool ("jumping", true);
        timer = 0;
      }
      if (timeSinceBirth >= 3f) {
        float xPos = Mathf.Clamp (transform.position.x, leftBarrier.position.x, rightBarrier.position.x);
        float yPos = Mathf.Clamp (transform.position.y, groundBarrier.position.y, rightBarrier.position.y);
        transform.position = new Vector3 (xPos, yPos, 0);
      }
    }
  }

  void Bite () {
    FacePlayer ();

    Vector2 dirToP = (target.position - transform.position).x > 0 ? new Vector2 (1, 1) : new Vector2 (-1, 1);
    rb.AddForce (dirToP.normalized * jumpForce, ForceMode2D.Impulse);
  }

  void FacePlayer () {
    if (target) {
      if (target.position.x > transform.position.x)
        srenderer.flipX = true;
      else
        srenderer.flipX = false;
    }
  }

  public void GetDamageables () {
    if (!alive) return;

    var colliders = Physics2D.OverlapCircleAll (transform.position, attackRadius, damageMask);
    foreach (Collider2D c in colliders) {
      IDamageable damageable = c.GetComponent<IDamageable> ();
      if (damageable != null) {
        damageable.TakeDamage (damage);
      }
    }
    rb.velocity = Vector2.zero;
  }

  void Jump () {
    FacePlayer ();
    jumping = true;
    Vector2 dirToP = (target.position - transform.position).x > 0 ? new Vector2 (1, 1) : new Vector2 (-1, 1);
    rb.AddForce (dirToP.normalized * jumpForce, ForceMode2D.Impulse);
  }

  protected override void OnCollisionEnter2D (Collision2D col) {
    if (alive) {
      jumping = false;
      animator.SetBool ("jumping", false);

      if (col.collider.tag == "Player") {
        float pDistX = Mathf.Abs (target.position.x - transform.position.x);
        float pDistY = target.position.y - transform.position.y;
        if (pDistX < xHitDelta && pDistY < yHitDelta && pDistY > 0) {
          alive = false;
          animator.SetTrigger ("dead");
          --FindObjectOfType<Spawner> ().activeWhales;
          col.collider.GetComponent<Rigidbody2D> ().AddForce (Vector2.up * jumpForce, ForceMode2D.Impulse);
          GetComponent<Collider2D>().enabled = false;
          GetComponent<Rigidbody2D>().gravityScale = 0;
          if (OnKilledByPlayer != null) OnKilledByPlayer ();
          OnKilledByPlayer = null;
          Destroy (gameObject, 1.05f);
        } else if (pDistX < xHitDelta && pDistY < yHitDelta && pDistY <= 0) {
          Bite ();
        }
      }
    }
  }

  protected override void OnDrawGizmos () {
    Gizmos.DrawWireSphere (transform.position, attackRadius);
  }
}