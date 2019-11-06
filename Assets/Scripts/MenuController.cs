using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour {

  public Animator bomb, mask;
  public AudioSource bombSrc;

  public void CallStart () {
    bombSrc.Play();
    StartCoroutine (_Start ());
  }

  public void CallExit () {
    bombSrc.Play();
    StartCoroutine (_Exit ());
  }
  IEnumerator _Exit () {
    bomb.SetTrigger ("explode");
    yield return new WaitForSeconds (.5f);
    Application.Quit ();
  }

  IEnumerator _Start () {
    bomb.SetTrigger ("explode");
    mask.SetTrigger ("expand");
    yield return new WaitUntil (() => mask.transform.localScale.x == 10);
    UnityEngine.SceneManagement.SceneManager.LoadScene ("Main");
  }

  IEnumerator _Intro () {
    bomb.SetTrigger ("explode");
    mask.SetTrigger ("expand");
    yield return new WaitUntil (() => mask.transform.localScale.x == 10);
    UnityEngine.SceneManagement.SceneManager.LoadScene ("Intro");
  }

  public void CallIntro () {
    bombSrc.Play();
    StartCoroutine (_Intro ());
  }


}