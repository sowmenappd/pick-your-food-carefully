using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour {

  int damage = 1;
  bool touchedGround = false;
  [HideInInspector] public bool thrownBack = false;

  Transform leftBarrier, rightBarrier;
  new Rigidbody2D rigidbody;

  private void OnEnable () {
    rigidbody = GetComponent<Rigidbody2D> ();
    leftBarrier = GameObject.Find ("Sides1").transform;
    rightBarrier = GameObject.Find ("Sides2").transform;
  }

  void OnCollisionEnter2D (Collision2D col) {
    if(GameManager.Instance.Paused) return;
    if (!touchedGround) {
      if (col.collider.tag == "Player") {
        if (col.collider.GetComponent<PlayerController> ().Flickering)
          return;
        col.collider.GetComponent<IDamageable> ().TakeDamage (damage);
        Destroy (gameObject);
      }
      if (col.collider.tag == "Ground") {
        touchedGround = true;
        thrownBack = false;
        Physics2D.IgnoreCollision (GetComponent<Collider2D> (), FindObjectOfType<PlayerController> ().GetComponent<Collider2D> (), true);
      }
    } else {
      if (col.collider.tag == "Enemy") {
        if (rigidbody.velocity != Vector2.zero && thrownBack) {
          col.collider.GetComponent<IDamageable> ().TakeDamage (damage);
          Destroy (gameObject);
        }
      }
    }

  }
}