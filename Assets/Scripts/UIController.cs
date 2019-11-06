using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
  
  public enum MessageType { Cleared, Lost };

  public AudioClip endingClip;

  public GameObject popupPanel;
  public GameObject levelClearText;
  public GameObject youLostText;

  public TMPro.TextMeshProUGUI scoreText;

  public Slider healthSlider;
  public TMPro.TextMeshProUGUI hpText;
  public HealthUI hpUI;

  public TMPro.TextMeshProUGUI levelText;

  const float popupDisplayTimer = 2.75f;
  float currentTime = 0;

  void Update(){
    if(popupPanel.activeSelf){
      currentTime += Time.deltaTime;
    }

    if(!GameManager.Instance.GameOver && currentTime >= popupDisplayTimer){
      popupPanel.SetActive(false);
      currentTime = 0;
    } else if (GameManager.Instance.GameOver){
      popupPanel.SetActive(true);
      ShowLoseMessage();
    }
  } 

  public void SetLevelText(int level){
    levelText.text = level.ToString();
  }

  public void SetHealthStats(int hp){
    if(hpUI)
      hpUI.SetHpLevel(hp);
  }

  public void SetScore(int score){
    scoreText.text = score.ToString();
  }

  public void ShowClearedMessage(){
    ShowMessage(MessageType.Cleared);
  }

  public void ShowLoseMessage(){
    ShowMessage(MessageType.Lost);
  }

  public void ShowMessage(MessageType msg){
    popupPanel.SetActive(true);
    if(msg == MessageType.Cleared){
      levelClearText.SetActive(true);
      youLostText.SetActive(false);
    } else{
      levelClearText.SetActive(false);
      youLostText.SetActive(true);
    }
  }

}