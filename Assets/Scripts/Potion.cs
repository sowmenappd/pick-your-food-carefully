using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour {

  [Range (1, 3)]
  public int healAmount = 1;

  public AudioClip clip;
  AudioSource src;

  void Awake () {
    if (!src) {
      src = GameObject.Find ("Potion Pickup Source").GetComponent<AudioSource> ();
      if (!clip)
        src.clip = clip;
    }
    GetComponent<Collider2D> ().isTrigger = true;
  }

  void OnTriggerEnter2D (Collider2D col) {
    if (col.tag == "Ground") {
      GetComponent<Rigidbody2D> ().gravityScale = 0;
      GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
    }
    else if (col.tag == "Player") {
      GetComponent<Collider2D> ().enabled = false;
      var p = col.GetComponent<LivingEntity> ();
      int amountToAdd = p.startingHealthPoints - p.HP;
      amountToAdd = Mathf.Clamp (amountToAdd, 0, healAmount);
      if (!GameManager.Instance.GameOver) {
        p.Heal (amountToAdd);
        src.Play ();
      }
      Destroy(gameObject);
    } else if (col.tag != "Untagged"){
      Destroy (gameObject);
    }
  }
}