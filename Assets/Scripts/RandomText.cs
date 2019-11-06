using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomText : MonoBehaviour {

  int lastIdx = -1;
  public string[] lines = {
    "But maybe all that glitters is not gold.",
    "Maybe it's just not your day.",
    "Give up already.",
    "Defeat is an easier option to go with.",
    "You might never win actually.",
    "There's a thin line between determination and promise.",
    "Will you ever see how it ends?"
  };
  public TextMeshProUGUI textBox;

  void OnEnable () {
    int idx = -1;
    while (idx == lastIdx)
      idx = Random.Range (0, lines.Length);

    if (idx >= 0 && idx < lines.Length) {
      textBox.SetText (lines[idx]);
      lastIdx = idx;
    }

  }

}