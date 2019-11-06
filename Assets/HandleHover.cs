using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandleHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
  public void OnPointerClick (PointerEventData eventData) {
    UnityEngine.SceneManagement.SceneManager.LoadScene ("Intro");
  }

  public void OnPointerEnter (PointerEventData eventData) {
    GetComponent<TextMeshProUGUI> ().alpha = 1;
    print("here");
  }

  public void OnPointerExit (PointerEventData eventData) {
    GetComponent<TextMeshProUGUI> ().alpha = 0;
  }

}