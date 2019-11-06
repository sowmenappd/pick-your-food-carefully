using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour {

  public Transform muzzle;
  public GameObject cannonBallPrefab;
  AudioSource fireSrc;

  public float fireForce;

  Transform target;

  void Awake () {
    target = FindObjectOfType<PlayerController> ().transform;
    fireSrc = GameObject.Find("Cannon Fire Source").GetComponent<AudioSource>();
  }

  public void FireCannon () {
    var ball = Instantiate (cannonBallPrefab, muzzle.position, Quaternion.identity);
    Vector2 dir = (target.position - muzzle.position);
    dir.y = 1.75f;
    ball.GetComponent<Rigidbody2D> ().AddForce (dir * fireForce * Random.Range (0.6f, 1.2f), ForceMode2D.Impulse);
    fireSrc.Play();
    Physics2D.IgnoreCollision (ball.GetComponent<CircleCollider2D> (), GameObject.Find ("Boss 1").GetComponent<BoxCollider2D> (), true);
  }
}