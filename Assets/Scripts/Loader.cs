using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {
  // Start is called before the first frame update
  void Awake () {
    if (PlayerPrefs.GetInt ("introCompleted", 0) == 1) {
      UnityEngine.SceneManagement.SceneManager.LoadScene ("Main Menu");
    }
  }

}