using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortRangeThrower : LivingEntity, IEnemy {

  protected SpriteRenderer srenderer;
  protected Animator animator;
  protected Spawner spawner;

  [Range (1, 4)]
  public int moveSpeed;
  public GameObject bombPrefab;

  [Range (1, 5)]
  public int bombThrowForce = 2;

  protected Transform hands;
  protected Transform target;

  bool dirLR = false;
  protected bool thrown = false;
  protected bool attacking = false;
  protected Transform start, mid, end;

  const float moveHaltTimer = .5f;
  protected float haltTimer = 0f;

  protected override void Awake () {
    base.Awake ();
    animator = GetComponent<Animator> ();
    srenderer = GetComponent<SpriteRenderer> ();
    target = FindObjectOfType<PlayerController> ().transform;
    spawner = FindObjectOfType<Spawner> ();
    hands = transform.Find ("Hands");
  }

  protected virtual void Update () {
    if (start && end)
      attacking = true;
    else
      attacking = false;

    if (attacking) {
      FacePlayer ();
      if (!thrown) {
        if (Mathf.Abs (transform.position.x - mid.position.x) > 0.005f){
          transform.position = Vector2.MoveTowards (transform.position, mid.position, moveSpeed * Time.deltaTime);
          animator.SetBool("moving", true);
        }
        else {
          transform.position = mid.position;
          if (haltTimer < moveHaltTimer) {
            haltTimer += Time.deltaTime;
            animator.SetBool ("moving", false);
          } else {
            animator.SetTrigger ("throw");
            haltTimer = 0;
          }
        }
      } else {
        if (Mathf.Abs (transform.position.x - end.position.x) > 0.005f) {
          animator.SetBool ("moving", true);
          transform.position = Vector2.MoveTowards (transform.position, end.position, moveSpeed * Time.deltaTime);
        } else {
          animator.SetBool ("moving", false);
          transform.position = end.position;
          start = null;
          end = null;
        }
      }
    }
  }

  public virtual void AttackPlayer () {
    var bomb = Instantiate (bombPrefab, hands.position, Quaternion.identity);
    bomb.GetComponent<Bomb> ().Ignite ();

    Vector2 hDirToP = (target.position - transform.position);
    hDirToP.y = 0;
    hDirToP.Normalize ();
    ThrowBomb (bomb, hDirToP);
  }

  protected void ThrowBomb (GameObject bomb, Vector2 dir) {
    bomb.GetComponent<Rigidbody2D> ().AddForce (dir * bombThrowForce * 25);
    thrown = true;
  }

  protected virtual void FacePlayer () {
    if (target) {
      if (target.position.x > transform.position.x)
        srenderer.flipX = false;
      else
        srenderer.flipX = true;
    }
  }

  public virtual void GetPath (int windowIndex = 0) {
    dirLR = (Random.Range (0, 2) == 0) ? true : false;
    animator.SetBool ("moving", true);

    if (dirLR) {
      start = spawner.windows[windowIndex].leftPos;
      end = spawner.windows[windowIndex].rightPos;
    } else {
      start = spawner.windows[windowIndex].rightPos;
      end = spawner.windows[windowIndex].leftPos;
    }
    mid = spawner.windows[windowIndex].midPos;

    transform.position = start.position;
    thrown = false;
  }
}