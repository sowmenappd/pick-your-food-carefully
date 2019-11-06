using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSetter : MonoBehaviour {

  public string trigger;

  void Start () {
    GetComponent<Animator>().SetTrigger(trigger);
  }

}