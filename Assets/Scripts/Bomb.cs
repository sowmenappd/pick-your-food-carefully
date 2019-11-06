using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bomb : MonoBehaviour {

  public LayerMask damageMask;

  public AudioClip clip;
  protected AudioSource explosionSrc;

  [SerializeField] protected float onContactExplosionDelay;
  [Range (0.25f, 1.25f)]
  [SerializeField] protected float explosionRadius;
  protected int damage = 1;
  protected bool fuse = false;
  protected float timer = 0;

  protected Animator animator;
  protected CircleCollider2D ccollider;

  public System.Action OnEvaded;
  public System.Action OnExploded_Intro;

  protected const float yHitDelta = 0.8f;
  protected const float xHitDelta = 0.75f;

  protected virtual void Awake () {
    animator = GetComponent<Animator> ();
    ccollider = GetComponent<CircleCollider2D> ();
    if (SceneManager.GetActiveScene ().name == "Main")
      GetFuseTime ();
    timer = 0;
    explosionSrc = GameObject.Find ("Bomb Explode Source").GetComponent<AudioSource> ();
    if (explosionSrc) {
      if (!explosionSrc.clip) {
        explosionSrc.clip = clip;
      }
    }
  }

  protected virtual void Start () {
    OnEvaded += () => GameManager.Instance.Score += 100;
  }

  protected virtual void Update () {
    if (fuse && timer < onContactExplosionDelay) {
      timer += Time.deltaTime;
    } else if (fuse && timer >= onContactExplosionDelay) {
      Explode ();
      transform.position += Vector3.up * 0.5f;
    } else if (timer >= onContactExplosionDelay) {
      GetComponent<Collider2D> ().enabled = false;
      GetComponent<Rigidbody2D> ().gravityScale = 0;
      transform.rotation = Quaternion.Euler (0, 0, 0);
      GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
    }
  }

  protected virtual void GetFuseTime () {
    onContactExplosionDelay = DifficultyAdjuster.GetBombFuseTime (FindObjectOfType<Spawner> ().Level);
  }

  public void Ignite () {
    animator.SetTrigger ("ticking");
  }

  private void Explode () {
    fuse = false;
    animator.SetTrigger ("explode");
    explosionSrc.Play ();

    var colliders = Physics2D.OverlapCircleAll (transform.position + (Vector3) ccollider.offset, explosionRadius + 0.375f, damageMask);
    bool pDamaged = false;
    foreach (Collider2D c in colliders) {
      IDamageable damageable = c.GetComponent<IDamageable> ();
      if (damageable != null) {
        damageable.TakeDamage (damage);
        pDamaged = true;
      }
    }
    GetComponent<Collider2D> ().enabled = false;

    if (!pDamaged && OnEvaded != null) {
      OnEvaded ();
    }

    if (pDamaged && gameObject.scene.name == "Intro" && OnExploded_Intro != null)
      OnExploded_Intro ();

    OnEvaded = null;
    Destroy (gameObject, 0.95f);
  }

  protected virtual void OnCollisionEnter2D (Collision2D col) {
    if (col.collider.tag == "Ground" || col.collider.tag == "Environment") {
      fuse = true;
    } else if (col.collider.tag == "Player") {
      var player = col.collider.GetComponent<PlayerController> ();
      fuse = true;
      float pDistX = Mathf.Abs (col.collider.transform.position.x - transform.position.x);
      float pDistY = col.collider.transform.position.y - transform.position.y;
      if (!player.Flickering && pDistX < xHitDelta && pDistY < yHitDelta && pDistY < 0) {
        Stun (col.collider.GetComponent<PlayerController> ());
      }
      timer = onContactExplosionDelay;
    }

    if (gameObject.scene.name == "Intro") {
      if (col.collider.tag == "Player") {
        Explode ();
      }
    }
  }

  protected void Stun (PlayerController controller) {
    controller.Stun(1.75f);
  }

  protected virtual void OnDrawGizmos () {
    if (ccollider != null)
      Gizmos.DrawWireSphere (transform.position + (Vector3) ccollider.offset, explosionRadius);
  }

}