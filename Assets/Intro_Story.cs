using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Intro_Story : MonoBehaviour {

  Transform holder;

  void Awake () {
  }

  IEnumerator Start () {
    holder = transform;
    int len = holder.childCount;
    for (int i = 0; i < len; i++) {
      if (i - 1 >= 0) {
        Color c = holder.GetChild (i - 1).GetComponent<TextMeshProUGUI> ().color;
        while (holder.GetChild (i - 1).GetComponent<TextMeshProUGUI> ().color.a > 0f) {
          c.a = Mathf.MoveTowards (c.a, 0f, Time.deltaTime);
          holder.GetChild (i - 1).GetComponent<TextMeshProUGUI> ().color = c;
          yield return null;
        }
        holder.GetChild (i - 1).gameObject.SetActive (false);
      }
      holder.GetChild (i).gameObject.SetActive (true);
      Color _c = holder.GetChild (i).GetComponent<TextMeshProUGUI> ().color;
      while (holder.GetChild (i).GetComponent<TextMeshProUGUI> ().color.a < 1f) {
        _c.a = Mathf.MoveTowards (_c.a, 255f, Time.deltaTime);
        holder.GetChild (i).GetComponent<TextMeshProUGUI> ().color = _c;
        yield return null;
      }
      yield return new WaitForSeconds (2f);
    }

    Color ac = holder.GetChild (len - 1).GetComponent<TextMeshProUGUI> ().color;
    while (holder.GetChild (len - 1).GetComponent<TextMeshProUGUI> ().color.a > 0f) {
      ac.a = Mathf.MoveTowards (ac.a, 0f, Time.deltaTime);
      holder.GetChild (len - 1).GetComponent<TextMeshProUGUI> ().color = ac;
      yield return null;
    }
    holder.GetChild (len - 1).gameObject.SetActive (false);

    GetComponent<Animator> ().SetTrigger ("out");

    yield return new WaitForSeconds (2f);
    FindObjectOfType<Intro_Cutscene> ().StartCoroutine (FindObjectOfType<Intro_Cutscene> ().IntroCutscene ());
  }

}