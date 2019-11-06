using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonFlickerSimple : MonoBehaviour {

  new public Image renderer;
  public Sprite buttonOn, buttonOff;

  public bool startWithOffStage = true;
  float counter = 0;
  public float flickerDelay = 1f;

  public bool mirror = true;

  void Start () {
    if (startWithOffStage)
      renderer.sprite = buttonOff;
    else
      renderer.sprite = buttonOn;

    if(name == "E panel") mirror = true;
    else mirror = false;
  }

  void Update () {
    if (!buttonOff || !buttonOn) return;

    if (mirror) {
      if (FindObjectOfType<PlayerController> ().transform.localScale.x == -1) {
        transform.localScale = new Vector3 (-1, 1, 1);
      } else if (FindObjectOfType<PlayerController> ().transform.localScale.x == 1) {
        transform.localScale = new Vector3 (1, 1, 1);
      }

    }

    counter += Time.deltaTime;

    if (counter > flickerDelay) {
      counter = 0;

      if (renderer.sprite == buttonOff) {
        renderer.sprite = buttonOn;
      } else {
        renderer.sprite = buttonOff;
      }
    }
  }
}