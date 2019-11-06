using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour {

  public Sprite heartFull, heartEmpty;
  public UnityEngine.UI.Image[] icons = new UnityEngine.UI.Image[5];

  public void SetHpLevel (int hp) {
    for (int i = 0; i < icons.Length; i++) {
      if (i < hp) {
        icons[i].sprite = heartFull;
      } else {
        if (icons[i].sprite == heartFull) {
          var falling = Instantiate (icons[i].gameObject, icons[i].transform.position, Quaternion.identity, GameObject.Find ("Holder").transform);
          icons[i].sprite = heartEmpty;
          falling.GetComponent<Rigidbody2D> ().gravityScale = 100;
          int dir = (Random.Range (0, 2) == 0) ? -1 : 1;
          falling.GetComponent<Rigidbody2D> ().AddForce (Vector2.one * Random.value * 10000 * dir);
          Destroy (falling.gameObject, 2f);
          break;
        }
      }
    }
  }

}