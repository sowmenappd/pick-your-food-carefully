using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiscoreDisplayActivator : MonoBehaviour {

  public GameObject hiscorePanel; 

  void OnEnable() {
    int currentHi = PlayerPrefs.GetInt("hiscore", 0);
    print("scored: " + GameManager.Instance.Score);
    if(GameManager.Instance.Score >= currentHi){
      PlayerPrefs.SetInt("hiscore", GameManager.Instance.Score);
      hiscorePanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().SetText(GameManager.Instance.Score.ToString());
      hiscorePanel.SetActive(true);
    } else {
      hiscorePanel.SetActive(false);
    }
  }

}