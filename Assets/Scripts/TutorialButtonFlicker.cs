using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonFlicker : MonoBehaviour {
  // Start is called before the first frame update

  Image leftButton, rightButton;

  public Sprite leftOn, leftOff, rightOn, rightOff;

  int counter = 0;

  void OnEnable () {
    if (!leftButton || !rightButton) {
      leftButton = transform.GetChild (1).GetComponent<Image> ();
      rightButton = transform.GetChild (2).GetComponent<Image> ();
    }
    StartCoroutine (StartFlicker ());
  }

  IEnumerator StartFlicker () {
    while (true) {
      if (counter == 0) {
        leftButton.sprite = leftOn;
        rightButton.sprite = rightOff;
      } else {
        leftButton.sprite = leftOff;
        rightButton.sprite = rightOn;
      }
      yield return new WaitForSeconds (.5f);
      counter = (counter + 1) % 2;
    }
  }

  void OnDisable () {
    StopCoroutine (StartFlicker ());
  }

}