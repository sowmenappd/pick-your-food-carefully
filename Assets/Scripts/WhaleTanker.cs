using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhaleTanker : ShortRangeThrower {

  [Range (0.25f, 1.5f)]
  public float whaleThrowInterval = 1.75f;
  const float moveHaltTimer = 1f;

  protected override void Awake () {
    base.Awake ();
  }

  protected override void Update () {
    if(GameManager.Instance.Paused) return;

    if (start && end) {
      attacking = true;
      srenderer.enabled = true;
    } else {
      attacking = false;
      srenderer.enabled = false;

    }

    if (attacking) {
      FacePlayer ();
      if (!thrown) {
        if (Mathf.Abs (transform.position.x - end.position.x) > 0.005f) {
          animator.SetBool ("moving", true);
          transform.position = Vector2.MoveTowards (transform.position, end.position, moveSpeed * Time.deltaTime);
        } else {
          transform.position = end.position;

          if (haltTimer < moveHaltTimer) {
            haltTimer += Time.deltaTime;
            animator.SetBool ("moving", false);
          } else {
            animator.SetTrigger ("attacking");
            haltTimer = 0;
          }
        }
      } else {
        if (Mathf.Abs (transform.position.x - end.position.x) > 0.005f) {
          animator.ResetTrigger ("attacking");
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

  protected override void FacePlayer () {
    if (target) {
      if (target.position.x > transform.position.x)
        srenderer.flipX = true;
      else
        srenderer.flipX = false;
    }
  }

  public override void GetPath (int balconyIndex = 0) {
    int bIndex = (Random.Range (0, 2) == 0) ? 0 : 1;
    start = spawner.balconies[bIndex].entry;
    end = spawner.balconies[bIndex].outside;

    transform.position = start.position;
    thrown = false;
  }

  public override void AttackPlayer () {
    if (spawner.activeWhales == 0) {
      StartCoroutine (ThrowWhales ());
    }
  }

  IEnumerator ThrowWhales () {
    for (int i = 1; i < 3; i++) {
      var whale = Instantiate (bombPrefab, hands.position, Quaternion.identity);
      spawner.activeWhales++;
      Vector2 hDirToP = (target.position - transform.position).x > 0 ? Vector2.one : new Vector2 (-1, 1);
      whale.GetComponent<Rigidbody2D> ().AddForce (i * hDirToP.normalized * bombThrowForce * 5, ForceMode2D.Impulse);
      whale.GetComponent<Whale> ().enabled = false;
      yield return new WaitForSeconds (whaleThrowInterval);
      whale.GetComponent<Whale> ().enabled = true;
    }
    yield return null;
    thrown = true;

    var t = start;
    start = end;
    end = t;
  }

}