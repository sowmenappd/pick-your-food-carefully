using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

  #region Singleton
  private static GameManager instance;
  new private static GameObject gameObject;

  public static GameManager Instance {
    get {
      if (instance == null) {
        gameObject = new GameObject ("GameManager");
        instance = gameObject.AddComponent<GameManager> ();
        gameObject.transform.SetSiblingIndex (0);
        print ("GM instantiated");
      }
      return instance;
    }
  }

  #endregion
  AudioSource backgroundAudioSrc;

  public int Score {
    get { return score; }
    set {
      score = value;
      FindObjectOfType<UIController> ().SetScore (Score);
    }
  }
  int score = 0;

  public bool GameOver {
    get; private set;
  }
  public bool Paused {
    get; private set;
  }

  private Transform pausePanel;

  public void LoadLevel (string levelName) {
    SceneManager.LoadScene (levelName);
  }

  void Start () {
    Instance.GameOver = false;
    Instance.Score = 0;
    Instance.backgroundAudioSrc = GameObject.Find ("Background Audio Main").GetComponent<AudioSource> ();

    pausePanel = GameObject.Find("Pause Panel").transform;
    pausePanel.GetChild(1).GetComponent<UnityEngine.UI.Button>().onClick.AddListener( () => {
      pausePanel.gameObject.SetActive(false);
      GameManager.Instance.Paused = false;
    });
    pausePanel.GetChild(2).GetComponent<UnityEngine.UI.Button>().onClick.AddListener( () => {
      UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    });
    pausePanel.gameObject.SetActive(false);
  }

  void Update(){
    if(!GameOver){
      if(Input.GetKeyDown(KeyCode.Escape)){
        Paused = !Paused;
        if(Paused){
          print("Paused");
          pausePanel.gameObject.SetActive(true);
        }
        else{
          print("Unpaused");
          pausePanel.gameObject.SetActive(false);
        }
      }
    }
  }

  public void SetGameOver () {
    Instance.GameOver = true;
    StartCoroutine (FadeToEndingMusic ());
    FindObjectOfType<UIController> ().ShowLoseMessage ();
    StartCoroutine(RestartOnTime());
  }

  IEnumerator RestartOnTime(){
    while(true){
      if(Input.GetKeyDown(KeyCode.R)){
        break;
      }
      yield return null;
    }
    SceneManager.LoadScene("Main");
  }

  IEnumerator FadeToEndingMusic () {
    float speed = 2f;

    float volume = 0.61f;
    while (backgroundAudioSrc.volume > 0) {
      backgroundAudioSrc.volume = Mathf.MoveTowards (backgroundAudioSrc.volume, 0f, speed * Time.deltaTime);
      yield return null;
    }
    backgroundAudioSrc.clip = FindObjectOfType<UIController> ().endingClip;
    backgroundAudioSrc.Play();
    backgroundAudioSrc.volume = volume;
    // while (backgroundAudioSrc.volume < volume) {
    //   backgroundAudioSrc.volume = Mathf.MoveTowards (backgroundAudioSrc.volume, volume, speed * Time.deltaTime);
    //   yield return null;
    // }
  }

}