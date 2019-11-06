using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour {

  [Range (1, 100)]
  public float deltaMove = 1f;
  public Transform leftBarrier, rightBarrier;

  public float onInputHoldMoveTime = 1f;
  public float inputHoldTimer = 0;

  public Vector2 dir = Vector2.zero;

  bool hasPressed = false;

  void Update () {
    if (Input.GetKey (KeyCode.LeftArrow) && !hasPressed){
      dir = Vector2.left * deltaMove;
      Move(dir);
    }
    else if (Input.GetKey (KeyCode.RightArrow) && !hasPressed){
      dir = Vector2.right * deltaMove;
      Move(dir);
    }

    if (dir != Vector2.zero){
      inputHoldTimer += Time.deltaTime;
      hasPressed = true;
    }

    if (Input.GetKeyUp (KeyCode.LeftArrow) || Input.GetKeyUp (KeyCode.RightArrow)) {
      dir = Vector2.zero;
      inputHoldTimer = 0;
      hasPressed = false;
    }

    if (inputHoldTimer > onInputHoldMoveTime && hasPressed) {
      Move(dir/2.5f);
    }
  }

  void Move (Vector2 dir) {
    float xPos = Mathf.Clamp (transform.position.x + dir.x, leftBarrier.position.x, rightBarrier.position.x);
    transform.position = new Vector3 (xPos, transform.position.y, transform.position.z);
  }

}