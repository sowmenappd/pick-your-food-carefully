using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HooklegEnemy : LivingEntity, IEnemy, IDamageable {

  public LayerMask damageMask;
  Transform leftBarrier, rightBarrier;
  [Range (1f, 4f)]
  public float moveSpeed;
  [Range (1f, 8f)]
  public float kickForce;

  public enum State { Chasing, Attacking };
 [SerializeField] State state;

 public UnityEngine.UI.Slider healthSlider;

 public float playerChaseDistance = 2f;
 public float playerAttackRadius = .1f;

 public AudioSource bossHurtSrc;

 public Transform cannonStandPosition;
 public Cannon cannon;
 PlayerController target;

 [SerializeField] bool fireCannons = false;

 public float attackCooldown = 1f, cannonCooldown = 2f;
 float attackTimer = 0, cannonTimer;

 Collider2D _collider;
 Animator animator;
 SpriteRenderer spRenderer;
 new Rigidbody2D rigidbody;

 void OnEnable () {
 base.Awake ();
 SpecificInit ();
  }

  public void Start () {
    healthSlider.value = (1 - HP * 1f / startingHealthPoints);
    animator.SetBool ("moving", true);

    leftBarrier = GameObject.Find ("Sides1").transform;
    rightBarrier = GameObject.Find ("Sides2").transform;
  }

  void Update () {
    if (!IsAlive || GameManager.Instance.Paused) return;
    ClampBoundaries ();

    //if the boss is out of the threshold distance to player target,
    //he's in idle state, target him to go to the cannon
    //and fire cannon balls after a delay
    //after firing a cannon, boss enters a cooldown of 1-2 seconds i.e. idle state
    //if player is detected  between that time inside threshold, he starts chasing the player
    //and attack the player
    // else he fires the cannon again

    Vector3 tPos = new Vector3 (target.transform.position.x, transform.position.y, transform.position.z);
    Vector3 cPos = new Vector3 (cannonStandPosition.position.x, transform.position.y, transform.position.z);

    if (attackTimer > 0) {
      attackTimer -= Time.deltaTime;
    }
    if (cannonTimer > 0 && transform.position.x == cannonStandPosition.position.x) {
      cannonTimer -= Time.deltaTime;
    }

    float targetDist = Vector3.Distance (transform.position, tPos);
    float cannonDist = Vector3.Distance (transform.position, cPos);

    if (targetDist < playerAttackRadius && !target.Flickering) {
      state = State.Attacking;
      fireCannons = false;
    } else if (targetDist < playerChaseDistance && !target.Flickering) {
      state = State.Chasing;
      fireCannons = false;
    } else if (GetActiveCannonBallCount () == 2) {
      state = State.Attacking;
      fireCannons = false;
    } else if (GetActiveCannonBallCount () < 2) {
      state = State.Attacking;
      fireCannons = true;
    }

    if (state == State.Chasing || GetActiveCannonBallCount () > 0)
      FacePlayer ();
    else if (state == State.Attacking && cannonDist < targetDist) {
      FaceCannon ();
    }

    if (state == State.Chasing) {
      //chase player if within threshold
      animator.SetBool ("moving", true);
      transform.position = Vector3.MoveTowards (transform.position, tPos, moveSpeed * Time.deltaTime);
    } else if (state == State.Attacking) {
      if (fireCannons) {
        if (cannonDist < 0.01f) {
          animator.SetBool ("moving", false);
          transform.position = cPos;
          AttackPlayer ();
        } else {
          transform.position =
            Vector3.MoveTowards (transform.position, cPos, moveSpeed * Time.deltaTime);
          animator.SetBool ("moving", true);
        }
      } else {
        if (targetDist < playerAttackRadius &&
          Mathf.Abs (target.transform.position.y - transform.position.y) < 0.1f &&
          target.transform.position.y < -2.6f) {
          AttackPlayer ();
        } else {
          transform.position =
            Vector3.MoveTowards (transform.position, tPos, moveSpeed * Time.deltaTime);
          animator.SetBool ("moving", true);
        }
      }
    }
  }

  public void AttackPlayer () {
    if (fireCannons && cannonTimer <= 0) {
      cannon.transform.GetComponent<Animator> ().SetTrigger ("fire");
      cannonTimer = cannonCooldown;
    } else if (!fireCannons && attackTimer <= 0) {
      animator.SetBool ("moving", false);
      animator.SetTrigger ("attacking");
      attackTimer = attackCooldown;
    }
  }

  public void DoDamage () {
    GetComponent<Rigidbody2D> ().isKinematic = true;
    var colliders = Physics2D.OverlapCircleAll (transform.position, playerAttackRadius, damageMask);
    foreach (Collider2D c in colliders) {
      IDamageable damageable = c.GetComponent<IDamageable> ();
      if (damageable != null && damageable != GetComponent<IDamageable> ()) {
        if (!target.Flickering) {
          damageable.TakeDamage (1);
          Vector2 dir = (transform.position.x > c.transform.position.x) ? new Vector2 (-1, .5f) : new Vector2 (1, .5f);
          c.GetComponent<Rigidbody2D> ().AddForce (dir * kickForce, ForceMode2D.Impulse);
          target.Stun (1f);
        }
      }
    }
    GetComponent<Rigidbody2D> ().isKinematic = false;
  }

  void FacePlayer () {
    if (transform.position.x > target.transform.position.x && target.transform.position.y > -2.6f) {
      spRenderer.flipX = true;
    } else if (transform.position.x < target.transform.position.x && target.transform.position.y > -2.6f) {
      spRenderer.flipX = false;
    }
  }

  void FaceCannon () {
    if (transform.position.x > cannonStandPosition.position.x) {
      spRenderer.flipX = true;
    } else {
      spRenderer.flipX = false;
    }
  }

  public void GetPath (int windowIndex) { }

  public override void Die () {
    base.Die ();
    animator.SetBool ("dead", true);
    GetComponent<Collider2D> ().enabled = false;
    GetComponent<Rigidbody2D> ().isKinematic = true;
    enabled = false;
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
      if (pDistX < .7f && pDistY < .9f && pDistY >= 0 && target.transform.position.y > -2.6f) {
        Vector2 dir = Vector2.up * 3f + ((col.transform.localScale.x == -1) ? Vector2.left * 1.4f : Vector2.right * 1.4f);
        col.collider.GetComponent<Rigidbody2D> ().AddForce (dir, ForceMode2D.Impulse);
      }
    }
  }

  void OnCollisionStay2D (Collision2D col) {
    if (col.collider.tag == "Player") {
      float pDistX = Mathf.Abs (target.transform.position.x - transform.position.x);
      float pDistY = target.transform.position.y - transform.position.y;
      col.collider.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
      if (pDistX < .7f && pDistY < .9f && pDistY >= 0 && target.transform.position.y > -2.6f) {
        Vector2 dir = Vector2.up * 3f + ((col.transform.localScale.x == -1) ? Vector2.left * 1.4f : Vector2.right * 1.4f);
        col.collider.GetComponent<Rigidbody2D> ().AddForce (dir, ForceMode2D.Impulse);
      }
    }
  }

  private int GetActiveCannonBallCount () {
    var count = FindObjectsOfType<CannonBall> ();
    if (count == null) return 0;
    else return count.Length;
  }

  void OnDrawGizmos () {
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere (transform.position, playerChaseDistance);
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere (transform.position, playerAttackRadius);
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
    state = State.Chasing;
    cannonTimer = cannonCooldown;
    target = FindObjectOfType<PlayerController> ();
    _collider = GetComponent<Collider2D> ();
    rigidbody = GetComponent<Rigidbody2D> ();
    animator = GetComponent<Animator> ();
    spRenderer = GetComponent<SpriteRenderer> ();
    OnDeath += () => GameManager.Instance.Score += 1500;

    bossHurtSrc = GameObject.Find ("Boss Hurt Source").GetComponent<AudioSource> ();
  }
}