using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : LivingEntity, IEnemy, IDamageable {

  public LayerMask damageMask;
  Transform leftBarrier, rightBarrier;
  [Range (1f, 4f)]
  public float moveSpeed;

  public float jumpForce = 3f, jumpCooldown = 2f;
  float jumpTimer = 0;
  bool jumping = false;

  [Range (1f, 8f)]
  public float throwForce;
  public float preferredAttackRadius;

  public enum State { Idle, Attacking };
 public bool isDodging = false;
 [SerializeField] State state;

 public UnityEngine.UI.Slider healthSlider;

 public AudioSource bossHurtSrc;

 public GameObject bombPrefab;

 public float attackCooldown = 1f;
 float attackTimer = 0;

 [Range (3f, 7f)]
 public float bombDetectionRadius = 3f;

 const int minAttackStreak = 1;
 const int maxAttackStreak = 3;
 float allowedAttackStreak = 0;
 float currentAttackStreak = 0;

 Vector2 idlePoint = Vector2.zero;
 Vector2 resetPoint = new Vector2 (-999, -999);

 const float minIdleTime = 1f;
 const float maxIdleTime = 3f;
 float idleStateDuration = 0;
 float idleStateTimer = 0;

 Collider2D _collider;
 Animator animator;
 SpriteRenderer spRenderer;
 new Rigidbody2D rigidbody;
 Transform bombHolder;

 PlayerController target;

 protected override void Awake () {
 base.Awake ();
 SpecificInit ();
  }

  public void Start () {
    healthSlider.value = (1 - HP * 1f / startingHealthPoints);
    animator.SetBool ("moving", false);

    leftBarrier = GameObject.Find ("Sides1").transform;
    rightBarrier = GameObject.Find ("Sides2").transform;
    NewState ();
  }

  private void Jump (Vector2 dir) {
    GetComponent<Rigidbody2D> ().AddForce (dir * jumpForce, ForceMode2D.Impulse);
    jumping = true;
    animator.SetTrigger ("jump");
    jumpTimer = jumpCooldown;
  }

  void Update () {
    if (!IsAlive) return;
    ClampBoundaries ();

    if (!jumping) {
      if (jumpTimer > 0)
        jumpTimer -= Time.deltaTime;
      else if (jumpTimer <= 0 && BombWithinJumpRadius ()) {
        AvoidBomb ();
      }
    }

    // after entrance, the enemy takes a random chance to either attack or stay idle
    // another random chance decides how much time to spend on the chosen state
    // attacking state: 3 - 5 seconds
    // idle state: 5 - 7 seconds
    // also has a dodging state, whether enemy reacts by jumping away when player throws a bomb
    // has a 50-50 chance to dodge the bomb
    // enemy can dodge bombs during any time

    if (Mathf.Abs (target.transform.position.x - transform.position.x) < bombDetectionRadius &&
      TargetCanThrow ()) {
      isDodging = true;
    } else {
      isDodging = false;
      animator.SetBool ("dodging", false);
    }

    if (!isDodging) {
      if (state == State.Idle) {
        if (idleStateTimer > 0) {
          MoveToIdlePoint ();
          idleStateTimer -= Time.deltaTime;
        } else {
          NewState ();
        }
      } else if (state == State.Attacking) {
        idlePoint = resetPoint;
        if (currentAttackStreak < allowedAttackStreak) {
          FacePlayer ();
          if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
          } else {
            AttackPlayer ();
            ++currentAttackStreak;
            attackTimer = attackCooldown;
          }
        } else {
          while (state == State.Attacking) {
            NewState ();
          }
        }
      }
    } else {
      state = State.Idle;
      if (PlayerCloserToLeftWall ()) {
        idlePoint = new Vector2 (rightBarrier.position.x - 0.25f, transform.position.y);
      } else {
        idlePoint = new Vector2 (leftBarrier.position.x + 0.25f, transform.position.y);
      }
      if (Mathf.Abs (transform.position.x - idlePoint.x) > 0.1f) {
        FaceIdlePoint ();
        MoveToIdlePoint ();
        animator.SetBool ("dodging", true);
      } else {
        FacePlayer ();
        animator.SetBool ("moving", false);
      }
    }

    if (Mathf.Abs (target.transform.position.x - transform.position.x) < 0.75f) {
      animator.SetBool ("attacking", true);
    } else {
      animator.SetBool ("attacking", false);
    }
  }

  private bool BombWithinJumpRadius () {
    var cols = Physics2D.OverlapCircleAll (transform.position, bombDetectionRadius);
    foreach (var col in cols) {
      if (col.GetComponent<ThrowableBomb> () != null && col.GetComponent<ThrowableBomb> ().thrownBack) {
        print ("JUMP");
        return true;
      }
    }
    return false;
  }

  private void CalculateNextIdlePoint () {
    //get random point between player and enemy
    float interp = UnityEngine.Random.Range (0.3f, 0.75f);
    float a = transform.position.x, b = target.transform.position.x - preferredAttackRadius;
    if (a > b) {
      float temp = b;
      b = a;
      a = temp;
    }

    float xPos;
    if (b - a < preferredAttackRadius) {
      if (PlayerCloserToLeftWall ()) {
        b = rightBarrier.position.x;
      } else {
        a = leftBarrier.position.x;
      }
    }
    xPos = Mathf.Lerp (a, b, interp);
    this.idlePoint = new Vector2 (Mathf.Clamp (xPos, leftBarrier.position.x + 0.5f, rightBarrier.position.x - 0.5f), transform.position.y);
  }

  private void MoveToIdlePoint () {
    animator.SetBool ("moving", true);
    FaceIdlePoint ();
    transform.position = Vector2.MoveTowards (transform.position, idlePoint, moveSpeed * Time.deltaTime);
    AdjustHealthSliderDirection ();
  }

  private bool TargetCanThrow () {
    return target.holdingBall;
  }

  private void AvoidBomb () {
    float x = PlayerCloserToLeftWall () ? -3f : 3f;
    Jump (Vector2.up * 6f + Vector2.right * x);
  }

  private void AdjustHealthSliderDirection () {
    if (transform.localScale.x == -1) {
      healthSlider.transform.localScale =
        new Vector3 (-0.02085576f, healthSlider.transform.localScale.y, healthSlider.transform.localScale.z);
    } else {
      healthSlider.transform.localScale =
        new Vector3 (0.02085576f, healthSlider.transform.localScale.y, healthSlider.transform.localScale.z);
    }
  }

  private void FaceIdlePoint () {
    float xDir = idlePoint.x - transform.position.x;
    if (xDir > 0) {
      transform.localScale = new Vector3 (1f, transform.localScale.y, transform.localScale.z);
    } else if (xDir < 0) {
      transform.localScale = new Vector3 (-1f, transform.localScale.y, transform.localScale.z);
    }
  }

  private bool PlayerCloserToLeftWall () {
    float distL = Mathf.Abs (target.transform.position.x - leftBarrier.position.x);
    float distR = Mathf.Abs (target.transform.position.x - rightBarrier.position.x);
    return distL < distR;
  }

  // private bool IsInRange (float value, float min, float max) {
  //   return (value >= min && value <= max);
  // }

  private void NewState () {
    if (UnityEngine.Random.Range (1, 101) < 51) { //idle
      state = State.Idle;
      CalculateNextIdlePoint ();
      idleStateDuration = Mathf.Clamp (idleStateDuration, minIdleTime, maxIdleTime);
      idleStateTimer = idleStateDuration;
    } else { //attacking
      state = State.Attacking;
      allowedAttackStreak = UnityEngine.Random.Range (minAttackStreak, maxAttackStreak + 1);
      currentAttackStreak = 0;
      idlePoint = resetPoint;
    }
  }

  public void AttackPlayer () {
    animator.SetBool ("moving", false);
    var ball = Instantiate (bombPrefab, bombHolder.position, Quaternion.identity);
    Physics2D.IgnoreCollision (ball.GetComponent<CircleCollider2D> (), transform.GetComponent<BoxCollider2D> (), true);
    Vector2 dir = (target.transform.position - bombHolder.position);
    dir.y = UnityEngine.Random.Range (1.75f, 3.25f);
    ball.GetComponent<Rigidbody2D> ().AddForce (dir * throwForce * UnityEngine.Random.Range (0.7f, 1f), ForceMode2D.Impulse);
  }

  void FacePlayer () {
    float xDir = target.transform.position.x - transform.position.x;
    if (xDir > 0 && target.transform.position.y > -2.7f) {
      transform.localScale = new Vector3 (1f, transform.localScale.y, transform.localScale.z);
    } else if (xDir < 0 && target.transform.position.y > -2.7f) {
      transform.localScale = new Vector3 (-1f, transform.localScale.y, transform.localScale.z);
    }
    AdjustHealthSliderDirection ();
  }

  public void GetPath (int windowIndex) {
    MoveToIdlePoint ();
  }

  public void DoDamage () {
    GetComponent<Rigidbody2D> ().isKinematic = true;
    var colliders = Physics2D.OverlapCircleAll (transform.position, 0.75f, damageMask);
    foreach (Collider2D c in colliders) {
      IDamageable damageable = c.GetComponent<IDamageable> ();
      if (damageable != null && damageable != GetComponent<IDamageable> ()) {
        if (!target.Flickering) {
          damageable.TakeDamage (1);
          Vector2 dir = (transform.position.x > c.transform.position.x) ? new Vector2 (-1, .5f) : new Vector2 (1, .5f);
          c.GetComponent<Rigidbody2D> ().AddForce (dir * 3f, ForceMode2D.Impulse);
          target.Stun (1f);
        }
      }
    }
    GetComponent<Rigidbody2D> ().isKinematic = false;
  }

  public override void Die () {
    base.Die ();
    animator.SetBool ("moving", false);
    animator.SetBool ("attacking", false);
    animator.ResetTrigger ("jump");
    animator.SetTrigger ("dead");
  }

  private void ClampBoundaries () {
    float xPos = Mathf.Clamp (transform.position.x + rigidbody.velocity.x * Time.deltaTime, leftBarrier.position.x, rightBarrier.position.x);
    float yPos = Mathf.Clamp (transform.position.y + rigidbody.velocity.y * Time.deltaTime, leftBarrier.position.y, leftBarrier.position.y + 5f);
    transform.position = new Vector3 (xPos, yPos, transform.position.z);

    if (rigidbody.velocity.y > 5) {
      float yLerp = Mathf.Lerp (rigidbody.velocity.y, 0, 1000 * Time.deltaTime);
      rigidbody.velocity = new Vector2 (rigidbody.velocity.x, yLerp);
    }
  }

  void OnCollisionEnter2D (Collision2D col) {
    if (col.collider.tag == "Player") {
      float pDistX = Mathf.Abs (target.transform.position.x - transform.position.x);
      float pDistY = target.transform.position.y - transform.position.y;
      if (pDistX < 1.5f && pDistY < 1.5f && pDistY >= 0 && target.transform.position.y > -2.7f) {
        TakeDamage (1);
        Vector2 dir = Vector2.up * 3f + ((col.transform.localScale.x == -1) ? Vector2.left * 1.4f : Vector2.right * 1.4f);
        col.collider.GetComponent<Rigidbody2D> ().AddForce (dir, ForceMode2D.Impulse);
      }
    }
    jumping = false;
  }

  void OnCollisionStay2D (Collision2D col) {
    if (col.collider.tag == "Player") {
      float pDistX = Mathf.Abs (target.transform.position.x - transform.position.x);
      float pDistY = target.transform.position.y - transform.position.y;
      col.collider.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
      if (pDistX < 1.5f && pDistY < 1.5f && pDistY >= 0 && target.transform.position.y > -2.7f) {
        Vector2 dir = Vector2.up * 3f + ((col.transform.localScale.x == -1) ? Vector2.left * 1.4f : Vector2.right * 1.4f);
        col.collider.GetComponent<Rigidbody2D> ().AddForce (dir, ForceMode2D.Impulse);
      }
    }
    jumping = false;
  }

  private int GetActiveCannonBallCount () {
    var count = FindObjectsOfType<CannonBall> ();
    if (count == null) return 0;
    else return count.Length;
  }

  void OnDrawGizmos () {
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere (transform.position, preferredAttackRadius);
    Gizmos.DrawWireSphere (transform.position, bombDetectionRadius);
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere (idlePoint, 1f);
  }

  public void TakeDamage (int amount) {
    if (IsAlive) {
      healthPoints -= amount;
      healthSlider.value = (1 - healthPoints * 1f / startingHealthPoints);
      animator.SetTrigger ("attacked");
      bossHurtSrc.Play ();
      if (healthPoints <= 0) {
        Die ();
      }
    }
  }

  public void SpecificInit () {
    state = State.Idle;
    target = FindObjectOfType<PlayerController> ();
    _collider = GetComponent<Collider2D> ();
    rigidbody = GetComponent<Rigidbody2D> ();
    animator = GetComponent<Animator> ();
    bombHolder = transform.GetChild (0);
    spRenderer = GetComponent<SpriteRenderer> ();
    OnDeath += () => GameManager.Instance.Score += 1500;
    idlePoint = resetPoint;
    bossHurtSrc = GameObject.Find ("Boss Hurt Source").GetComponent<AudioSource> ();
  }
}