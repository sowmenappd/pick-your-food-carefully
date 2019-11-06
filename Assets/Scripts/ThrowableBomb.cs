using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableBomb : Bomb {

  public bool thrownBack = false;

  protected override void Awake () {
    base.Awake ();
    if (GameObject.Find ("Boss 2") != null)
      Physics2D.IgnoreCollision (GetComponent<CircleCollider2D> (), GameObject.Find ("Boss 2").GetComponent<BoxCollider2D> (), false);
    else
      Physics2D.IgnoreCollision (GetComponent<CircleCollider2D> (), GameObject.Find ("Captain").GetComponent<BoxCollider2D> (), false);
  }

  protected override void Start () {
    base.Start ();
    Ignite ();
  }

  protected override void Update () {
    base.Update ();
  }

  protected override void GetFuseTime () {
    onContactExplosionDelay = 5f;
  }

  protected override void OnCollisionEnter2D (Collision2D col) {
    if (col.collider.tag == "Ground" || col.collider.tag == "Environment") {
      fuse = true;
      if (GameObject.Find ("Boss 2") != null){
        Physics2D.IgnoreCollision (GetComponent<CircleCollider2D> (), 
        GameObject.Find ("Boss 2").GetComponent<BoxCollider2D> (), true);
      }
      else
        Physics2D.IgnoreCollision (GetComponent<CircleCollider2D> (), 
        GameObject.Find ("Captain").GetComponent<BoxCollider2D> (), true);

    } else if (col.collider.tag == "Player") {
      var player = col.collider.GetComponent<PlayerController> ();
      fuse = true;
      float pDistX = Mathf.Abs (col.collider.transform.position.x - transform.position.x);
      float pDistY = col.collider.transform.position.y - transform.position.y;
      if (!player.Flickering && pDistX < xHitDelta && pDistY < yHitDelta && pDistY < 0) {
        Stun (col.collider.GetComponent<PlayerController> ());
        col.collider.GetComponent<IDamageable> ().TakeDamage (1);
      }
    } else if (col.collider.tag == "Enemy") {
      timer = onContactExplosionDelay;
    }
  }
}