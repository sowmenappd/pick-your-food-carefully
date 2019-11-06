using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : LivingEntity, IDamageable {

  public AudioClip hurtClip, stunnedClip, jumpClip;
  public AudioSource hurtSrc, stunnedSrc, jumpSrc;
  const string GROUND_TAG = "Ground";

  [Range (1, 5)]
  public int deltaMove = 1;
  [Range (1, 8)]
  public int jumpForce = 1;

  Transform leftBarrier, rightBarrier;
  Transform activeBoss = null;

  //string[] bosses = { "Boss 1", "Boss 2", "Boss 3" };

  Animator animator;
  new Rigidbody2D rigidbody;
  SpriteRenderer playerRenderer;
  GameObject runParticles;

  Vector2 dir = Vector2.zero;

  bool hasPressed = false;
  bool moving = false, jumping = false, flickering = false;
  public bool Flickering {
    get { return flickering; }
  }

  float stunTimer = 1.75f;
  [HideInInspector] public float currentStunTimer = 0f;
  GameObject stunFX;

  [HideInInspector] public bool stunned = false;

  const float damageProtectionTime = 2f;
  float damagedTimer = 0;
  bool attackedRecently = false;

  public Transform throwablesHolder;
  public GameObject pressEpanel;
  public float throwForce;
  public bool holdingBall = false;

  System.Action<int> OnDamage;
  private Spawner spawner;

  private Vector2 onPauseVelocity;

  protected override void Awake () {
    //PlayerPrefs.DeleteAll();
    base.Awake ();
    animator = GetComponent<Animator> ();
    rigidbody = GetComponent<Rigidbody2D> ();
    playerRenderer = GetComponent<SpriteRenderer> ();
    runParticles = transform.GetChild (0).GetChild (0).gameObject;
    stunFX = transform.GetChild (1).gameObject;
    leftBarrier = GameObject.Find ("Sides1").transform;
    rightBarrier = GameObject.Find ("Sides2").transform;
    spawner = FindObjectOfType<Spawner> ();

    hurtSrc = GameObject.Find ("Player Hurt Source").GetComponent<AudioSource> ();
    hurtSrc.clip = hurtClip;

    stunnedSrc = GameObject.Find ("Player Stunned Source").GetComponent<AudioSource> ();
    stunnedSrc.clip = stunnedClip;

    jumpSrc = GameObject.Find ("Player Jump Source").GetComponent<AudioSource> ();
    jumpSrc.clip = jumpClip;

    OnHpChanged += FindObjectOfType<UIController> ().SetHealthStats;

    OnDamage += (hp) => {
      FindObjectOfType<UIController> ().SetHealthStats (hp);
      if (hurtClip) hurtSrc.Play ();
    };

    OnDeath += () => {
      GameManager.Instance.SetGameOver();
      animator.SetBool("moving", false);
    };
  }

  void Update () {
    if (GameManager.Instance.GameOver) return;

    if(GameManager.Instance.Paused) {
      if(onPauseVelocity == Vector2.one * -1f){
        var rb = GetComponent<Rigidbody2D>();
        onPauseVelocity = rb.velocity;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
      } 
      return;
    } else {
      if(onPauseVelocity != Vector2.one * -1f){
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = onPauseVelocity;
        onPauseVelocity = Vector2.one * -1f;
      }
    }

    //if(spawner.Level == 1)
    //get active boss
    if(GetActiveBoss()){
      activeBoss = GetActiveBoss ();
    }

    if (stunned) {
      if (currentStunTimer == 0 && stunnedClip) {
        stunnedSrc.Play ();
      }
      stunFX.SetActive (true);
      if (transform.localScale.x == -1) {
        stunFX.transform.localScale = new Vector3 (-1.3132f, stunFX.transform.localScale.y, 1);
      } else {
        stunFX.transform.localScale = new Vector3 (1.3132f, stunFX.transform.localScale.y, 1);
      }
      animator.enabled = false;
      currentStunTimer += Time.deltaTime;
      if (currentStunTimer >= stunTimer) {
        stunned = false;
        stunFX.SetActive (false);
        animator.enabled = true;
        currentStunTimer = 0;
      }
      return;
    }

    if (!jumping && Input.GetKeyDown (KeyCode.UpArrow)) {
      Jump ();
      jumpSrc.Play ();
    }

    if(throwablesHolder.childCount == 0){
      holdingBall = false;
    } else {
      throwablesHolder.GetChild (0).GetComponent<Rigidbody2D>().velocity = Vector2.zero;
      throwablesHolder.GetChild (0).rotation = Quaternion.Euler(Vector3.zero);
      throwablesHolder.GetChild (0).GetComponent<Rigidbody2D>().isKinematic = true;
      throwablesHolder.GetChild (0).GetComponent<Rigidbody2D> ().gravityScale = 0;
    }

    if (Input.GetKeyDown (KeyCode.E) && !stunned) {
      Transform throwable;
      if (!holdingBall) {
        throwable = GetNearestBall ();
        if(throwable == null){
          throwable = GetNearestBomb();
        }
        if (throwable != null && throwablesHolder.childCount == 0) {
          throwable.parent = throwablesHolder;
          throwable.localPosition = Vector3.zero;
          throwable.rotation = Quaternion.Euler(Vector3.zero); 
          throwable.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
          throwable.GetComponent<Rigidbody2D> ().gravityScale = 0;
          throwable.GetComponent<Rigidbody2D> ().isKinematic = true;
          throwable.GetComponent<Collider2D> ().enabled = false;
          holdingBall = true;
          Physics2D.IgnoreCollision (throwable.GetComponent<Collider2D> (), activeBoss.GetComponent<Collider2D> (), false);
        }
      } else {
        throwable = throwablesHolder.GetChild (0);
        throwable.parent = null;
        throwable.GetComponent<Rigidbody2D> ().isKinematic = false;
        throwable.GetComponent<Rigidbody2D> ().gravityScale = 1;
        throwable.GetComponent<Collider2D> ().enabled = true;
        Vector2 dir = Vector2.zero;
        if (transform.position.x > activeBoss.position.x) {
          if (transform.localScale.x == 1) 
            dir = Vector2.right * throwForce;
          else 
            dir = Vector2.left * throwForce;
        } else if (transform.position.x < activeBoss.position.x) {
            if (transform.localScale.x == 1) 
              dir = Vector2.right * throwForce;
            else 
              dir = Vector2.left * throwForce;
        }
        dir.y = UnityEngine.Random.Range (2f, 4f);
        throwable.GetComponent<Rigidbody2D> ().AddForce (dir, ForceMode2D.Impulse);
        if(throwable.GetComponent<CannonBall>())
          throwable.GetComponent<CannonBall>().thrownBack = true;
        else if(throwable.GetComponent<ThrowableBomb>())
          throwable.GetComponent<ThrowableBomb>().thrownBack = true;
        holdingBall = false;
      }

    }

    if (Input.GetKey (KeyCode.LeftArrow)) {
      dir = Vector2.left * deltaMove * Time.deltaTime;
      moving = true;
      Move (dir);
    } else if (Input.GetKey (KeyCode.RightArrow)) {
      dir = Vector2.right * deltaMove * Time.deltaTime;
      moving = true;
      Move (dir);
    } else if (!jumping) {
      dir = Vector2.zero;
      moving = false;
    }

    if (attackedRecently) {
      damagedTimer += Time.deltaTime;
      if (damagedTimer >= damageProtectionTime) {
        attackedRecently = false;
        damagedTimer = 0f;
      }
    }

    if((GetNearestBall() || GetNearestBomb()) && GetActiveBoss()){
      pressEpanel.SetActive(true);
    } else {
      pressEpanel.SetActive(false);
    }

    ConfigureParticles ();
    ConfigureAnimator ();
    ClampBoundaries ();
  }

  private Transform GetActiveBoss () {
    Transform boss = null;
    
    if(FindObjectOfType<HooklegEnemy> () != null){
      boss = FindObjectOfType<HooklegEnemy> ().transform;
    } else if (FindObjectOfType<BombThrower> () != null){
      boss = FindObjectOfType<BombThrower> ().transform;
    } else if(FindObjectOfType<FinalBoss>() != null){
      boss = FindObjectOfType<FinalBoss> ().transform;
    }

    if (boss == null) {
    }
    return boss;
  }

  private Transform[] GetActiveCannonBalls () {
    var balls = FindObjectsOfType<CannonBall> ();
    Transform[] t = new Transform[balls.Length];
    for (int i = 0; i < t.Length; i++) {
      t[i] = balls[i].transform;
    }
    return t;
  }

  private Transform[] GetActiveBombs () {
    var bombs = FindObjectsOfType<ThrowableBomb> ();
    Transform[] t = new Transform[bombs.Length];
    for (int i = 0; i < t.Length; i++) {
      t[i] = bombs[i].transform;
    }
    return t;
  }

  private Transform GetNearestBall () {
    var balls = GetActiveCannonBalls ();
    int idx = -1;
    float minDist = float.MaxValue;
    for (int i = 0; i < balls.Length; i++) {
      float dist = Vector2.Distance (transform.position, balls[i].position);
      if (dist < 1f && dist < minDist) {
        idx = i;
        minDist = dist;
      }
    }
    if (idx == -1)
      return null;
    return balls[idx];
  }

  private Transform GetNearestBomb () {
    var balls = GetActiveBombs();
    int idx = -1;
    float minDist = float.MaxValue;
    for (int i = 0; i < balls.Length; i++) {
      float dist = Vector2.Distance (transform.position, balls[i].position);
      if (dist < 2f && dist < minDist) {
        idx = i;
        minDist = dist;
      }
    }
    if (idx == -1)
      return null;
    return balls[idx];
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

  private void ConfigureParticles () {
    if (moving) {
      if (!runParticles.GetComponent<ParticleSystem> ().isPlaying) {
        runParticles.GetComponent<ParticleSystem> ().Play ();
      }
      if (dir.x > 0) runParticles.transform.localRotation = Quaternion.Euler (0, 0, 0);
      if (dir.x < 0) runParticles.transform.localRotation = Quaternion.Euler (0, 0, 180);
    } else {
      if (runParticles.GetComponent<ParticleSystem> ().isPlaying)
        runParticles.GetComponent<ParticleSystem> ().Stop ();
    }
  }

  private void Jump () {
    rigidbody.AddForce (Vector2.up * jumpForce, ForceMode2D.Impulse);
    moving = true;
    jumping = true;
    animator.SetBool ("jumping", true);
  }

  private void OnCollisionEnter2D (Collision2D col) {
    if (col.collider.tag == GROUND_TAG || col.collider.tag == "Weapon") {
      jumping = false;
      moving = false;
      animator.SetBool ("jumping", false);
    }
  }

  private void Move (Vector2 dir) {
    float xPos = Mathf.Clamp (transform.position.x + dir.x, leftBarrier.position.x, rightBarrier.position.x);
    float yPos = Mathf.Clamp (transform.position.y + dir.y, leftBarrier.position.y, leftBarrier.position.y + 5f);
    transform.position = new Vector3 (xPos, yPos, transform.position.z);
  }

  private void ConfigureAnimator () {
    bool moving = dir != Vector2.zero;
    animator.SetBool ("moving", moving);

    if (moving) {
      if (dir.x > 0)
        transform.localScale = new Vector3 (1f, transform.localScale.y, transform.localScale.z);
      else if (dir.x < 0)
        transform.localScale = new Vector3 (-1f, transform.localScale.y, transform.localScale.z);
    }
  }

  public void TakeDamage (int amount) {
    if (enabled && !attackedRecently && !flickering && IsAlive) {
      healthPoints -= amount;
      if (OnDamage != null) OnDamage (healthPoints);
      animator.SetTrigger ("attacked");
      StartCoroutine (DamageFlicker ());
      attackedRecently = true;
      if (healthPoints <= 0) {
        Die ();
        if (OnDeath != null) OnDeath ();
      }
    }
  }

  public void Stun (float duration) {
    stunned = true;
    stunTimer = duration;
  }

  private IEnumerator DamageFlicker () {
    int count = 100, i = 0;
    flickering = true;
    while (i++ < count) {
      playerRenderer.enabled = !playerRenderer.enabled;
      yield return new WaitForSeconds (0.03f);
    }
    flickering = false;
  }

  public override void Die () {
    //do death animation and sound
    base.Die ();
  }

  public void SpecificInit () { }

  //code i figured out myself :D :D

  //[SerializeField] private float onInputHoldMoveTime = 1f;
  //float inputHoldTimer = 0;

  // void PlateMove(){
  //   if (Input.GetKey (KeyCode.LeftArrow) && !hasPressed){
  //     dir = Vector2.left * deltaMove;
  //     Move(dir);
  //   }
  //   else if (Input.GetKey (KeyCode.RightArrow) && !hasPressed){
  //     dir = Vector2.right * deltaMove;
  //     Move(dir);
  //   }

  //   if (dir != Vector2.zero){
  //     inputHoldTimer += Time.deltaTime;
  //     hasPressed = true;
  //   }

  //   if (Input.GetKeyUp (KeyCode.LeftArrow) || Input.GetKeyUp (KeyCode.RightArrow)) {
  //     dir = Vector2.zero;
  //     inputHoldTimer = 0;
  //     hasPressed = false;
  //   }

  //   if (inputHoldTimer > onInputHoldMoveTime && hasPressed) {
  //     Move(dir/2.5f);
  //   }
  // }
}