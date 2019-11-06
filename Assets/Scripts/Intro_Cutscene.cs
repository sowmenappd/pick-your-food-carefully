using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro_Cutscene : MonoBehaviour {

  [Header ("Audio Clips")]
  public AudioClip clip2;
  public AudioClip clip3;

  [Header ("Audio Sources")]
  public AudioSource backgroundLoopSrc;
  public AudioSource doorCloseSrc;

  [Header ("Tutorial")]
  public GameObject leftRightArrowPanel;
  public GameObject pressEtoUsePanel;
  public GameObject pressUptoJumpPanel;

  [Space (5)][Header ("Masking")]
  public SpriteMask maskOverlay;
  public Vector2 maskStartSize, maskSize1;

  [Space (5)]
  public Vector2 captainMaskStartSize;

  [Space (5)][Header ("Dialogue")]
  public GameObject dialoguePanel;
  GameObject[] dialogues;

  [Space (5)][Header ("Animators")]
  public Animator doorAnimator;

  public Animator playerAnimator;
  public Animator playerStartledAnimator;
  public Animator playerSurprisedAnimator;

  public Animator candleAnimator;

  public Animator captainAnimator;
  public Animator captainSurpisedAnimator;

  public Animator enemy1Animator;

  [Space (5)][Header ("Placeholder Positions")]
  public Transform playerXPosition1;
  public Transform playerXPosition2;
  public Transform playerXPosition3;
  public Transform captainTargetPosition1;
  public Transform enemy1TargetPosition1;
  public Transform bombTarget;

  [Space (5)][Header ("Positions")]
  public Transform chandelier;
  public Transform roundWindow;
  public Transform window;
  public Transform candleMask;
  public Transform treasureChest;
  public Transform bomb;
  public Transform key;

  void Awake () {
    dialogues = new GameObject[dialoguePanel.transform.childCount];
    for (int i = 0; i < dialogues.Length; i++)
      dialogues[i] = dialoguePanel.transform.GetChild (i).gameObject;
  }
  void Start () {
    //StartCoroutine (IntroCutscene ());
    FindObjectOfType<Bomb>().OnEvaded += () => {
      key.GetComponent<Rigidbody2D>().isKinematic = false;
      key.GetComponent<Rigidbody2D>().AddForce(new Vector2(-0.75f, 1f) * 8f, ForceMode2D.Impulse);
    };
    FindObjectOfType<Bomb>().OnExploded_Intro += () => {
      StopCoroutine(IntroCutscene());
      StartCoroutine(ClosingMasks());
    };
  }

  public IEnumerator IntroCutscene () {
    yield return new WaitForSeconds (1f);
    while (Vector2.Distance (maskOverlay.transform.localScale, maskStartSize) > 0.001f) {
      maskOverlay.transform.localScale =
        Vector2.MoveTowards (maskOverlay.transform.localScale, maskStartSize, Time.deltaTime);
      yield return null;
    }

    yield return new WaitForSeconds (1f);
    doorAnimator.SetTrigger ("open");
    yield return new WaitForSeconds (1f);

    playerAnimator.SetBool ("moving", true);

    playerAnimator.GetComponent<SpriteRenderer> ().sortingOrder = 10;
    maskOverlay.transform.parent = playerAnimator.transform;
    yield return new WaitForSeconds (.5f);
    playerAnimator.SetBool ("moving", false);
    maskOverlay.transform.localPosition = Vector3.zero;

    yield return new WaitForSeconds (.5f);
    doorCloseSrc.Play ();

    yield return new WaitForSeconds (2.25f);

    backgroundLoopSrc.Play ();

    playerStartledAnimator.SetTrigger ("startled");
    yield return new WaitForSeconds (2f);

    //show dialogue box
    ShowDialogue (dialogues[0]);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    ShowDialogue (null);
    yield return new WaitForSeconds (0.75f);
    playerAnimator.GetComponent<SpriteRenderer> ().flipX = true;
    yield return new WaitForSeconds (0.75f);
    playerAnimator.GetComponent<SpriteRenderer> ().flipX = false;
    yield return new WaitForSeconds (0.75f);

    ShowDialogue (dialogues[1]);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));

    ShowDialogue (null);
    yield return new WaitForSeconds (.5f);

    ShowDialogue (dialogues[2]);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    ShowDialogue (null);

    //show "press left/right arrows to move
    yield return new WaitForSeconds (.5f);
    leftRightArrowPanel.SetActive (true);
    yield return new WaitForSeconds (.5f);

    while (playerAnimator.transform.position.x < playerXPosition1.position.x) {
      float hMove = Input.GetKey (KeyCode.LeftArrow) ? -2 : Input.GetKey (KeyCode.RightArrow) ? 2 : 0;
      Vector2 dir = new Vector2 (hMove, 0);
      playerAnimator.transform.position =
        Vector2.MoveTowards (playerAnimator.transform.position, playerAnimator.transform.position + (Vector3) dir, Time.deltaTime);
      if (dir.x != 0) {
        playerAnimator.SetBool ("moving", true);
        if (dir.x > 0)
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = false;
        else
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = true;
      } else
        playerAnimator.SetBool ("moving", false);
      yield return null;
    }
    playerAnimator.SetBool ("moving", false);
    leftRightArrowPanel.SetActive (false);

    //do something with the candle on the barrel
    //"i can use this candle"
    //press e to use light candle
    //surprise player
    yield return new WaitForSeconds (.5f);
    playerSurprisedAnimator.SetTrigger ("surprised");
    yield return new WaitForSeconds (.25f);
    playerAnimator.GetComponent<SpriteRenderer> ().flipX = true;
    ShowDialogue (dialogues[3]);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    ShowDialogue (null);

    pressEtoUsePanel.SetActive (true);
    yield return new WaitForSeconds (0.5f);

    yield return new WaitUntil (delegate { return Input.GetKeyDown (KeyCode.E); });

    candleAnimator.enabled = true;
    candleMask.gameObject.SetActive (false);
    pressEtoUsePanel.SetActive (false);
    yield return new WaitForSeconds (0.5f);

    roundWindow.GetComponent<SpriteRenderer> ().color = Color.white;
    window.GetComponent<SpriteRenderer> ().color = Color.white;
    chandelier.GetComponent<SpriteRenderer> ().color = Color.white;
    chandelier.GetChild (0).GetComponent<SpriteRenderer> ().color = Color.white;
    playerAnimator.GetComponent<SpriteRenderer> ().flipX = false;
    while (Vector2.Distance (maskOverlay.transform.localScale, maskSize1) > 0.001f) {
      maskOverlay.transform.localScale =
        Vector2.MoveTowards (maskOverlay.transform.localScale, maskSize1, Time.deltaTime);
      yield return null;
    }
    yield return new WaitForSeconds (2f);

    ShowDialogue (dialogues[4]);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    ShowDialogue (null);

    while (playerAnimator.transform.position.x < playerXPosition2.position.x) {
      float hMove = Input.GetKey (KeyCode.LeftArrow) ? -2 : Input.GetKey (KeyCode.RightArrow) ? 2 : 0;
      Vector2 dir = new Vector2 (hMove, 0);
      playerAnimator.transform.position =
        Vector2.MoveTowards (playerAnimator.transform.position, playerAnimator.transform.position + (Vector3) dir, Time.deltaTime);
      if (dir.x != 0) {
        playerAnimator.SetBool ("moving", true);
        if (dir.x > 0)
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = false;
        else
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = true;
      } else
        playerAnimator.SetBool ("moving", false);
      yield return null;
    }

    playerAnimator.SetBool ("moving", false);
    playerSurprisedAnimator.SetTrigger ("surprised");
    yield return new WaitForSeconds (.5f);
    ShowDialogue (dialogues[5]);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    ShowDialogue (null);

    while (playerAnimator.transform.position.x < playerXPosition3.position.x) {
      float hMove = Input.GetKey (KeyCode.LeftArrow) ? -2 : Input.GetKey (KeyCode.RightArrow) ? 2 : 0;
      Vector2 dir = new Vector2 (hMove, 0);
      playerAnimator.transform.position =
        Vector2.MoveTowards (playerAnimator.transform.position, playerAnimator.transform.position + (Vector3) dir, Time.deltaTime);
      if (dir.x != 0) {
        playerAnimator.SetBool ("moving", true);
        if (dir.x > 0)
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = false;
        else
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = true;
      } else
        playerAnimator.SetBool ("moving", false);
      yield return null;
    }
    playerAnimator.SetBool ("moving", false);

    captainAnimator.transform.GetChild (1).gameObject.SetActive (true);
    while (Vector2.Distance (captainAnimator.transform.GetChild (1).localScale, captainMaskStartSize) > 0.001f) {
      captainAnimator.transform.GetChild (1).localScale =
        Vector2.MoveTowards (captainAnimator.transform.GetChild (1).localScale, captainMaskStartSize, 2 * Time.deltaTime);
      yield return null;
    }
    captainAnimator.GetComponent<SpriteRenderer> ().enabled = true;
    yield return new WaitForSeconds (.5f);
    backgroundLoopSrc.clip = clip2;
    backgroundLoopSrc.Play ();
    captainSurpisedAnimator.SetTrigger ("surprised");
    yield return new WaitForSeconds (1f);

    ShowDialogue (dialogues[6]);

    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    ShowDialogue (null);

    yield return new WaitForSeconds (0.5f);
    playerAnimator.GetComponent<SpriteRenderer> ().flipX = true;
    playerSurprisedAnimator.SetTrigger ("surprised");
    yield return new WaitForSeconds (1f);
    ShowDialogue (dialogues[7]);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    ShowDialogue (null);

    for (int i = 8; i < 15; i++) {
      yield return new WaitForSeconds (0.5f);
      ShowDialogue (dialogues[i]);
      yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
      ShowDialogue (null);
    }

    while (captainAnimator.transform.position.x < captainTargetPosition1.position.x) {
      Vector2 dir = new Vector2 (1, 0);
      captainAnimator.transform.position =
        Vector2.MoveTowards (captainAnimator.transform.position, captainAnimator.transform.position + (Vector3) dir, Time.deltaTime);
      yield return null;
    }

    pressUptoJumpPanel.SetActive (true);
    yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
    pressUptoJumpPanel.SetActive (false);

    var bomb = this.bomb.GetComponent<SpriteRenderer> ();
    bomb.sortingOrder = 26;
    bomb.GetComponent<Bomb> ().Ignite ();
    yield return new WaitForSeconds (.25f);
    bomb.GetComponent<Rigidbody2D> ().gravityScale = 1;
    Vector2 bombDir = (bombTarget.position - bomb.transform.position).normalized;
    bombDir.y = .75f;
    bomb.GetComponent<Rigidbody2D> ().AddForce (bombDir * 4f, ForceMode2D.Impulse);
    bomb.GetComponent<Bomb> ().enabled = true;

    backgroundLoopSrc.Pause ();

    while (bomb != null) {
      float hMove = Input.GetKey (KeyCode.LeftArrow) ? -2 : Input.GetKey (KeyCode.RightArrow) ? 2 : 0;
      float vMove = Input.GetKeyDown (KeyCode.UpArrow) ? 5 : 0;
      Vector2 dir = new Vector2 (hMove, 0);
      playerAnimator.transform.position =
        Vector2.MoveTowards (playerAnimator.transform.position, playerAnimator.transform.position + (Vector3) dir, Time.deltaTime);
      if (vMove != 0) {
        playerAnimator.GetComponent<Rigidbody2D> ().AddForce (Vector2.up * 5f, ForceMode2D.Impulse);
      }
      if (dir.x != 0) {
        playerAnimator.SetBool ("moving", true);
        if (dir.x > 0)
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = false;
        else
          playerAnimator.GetComponent<SpriteRenderer> ().flipX = true;
      } else
        playerAnimator.SetBool ("moving", false);
      yield return null;
    }
    playerAnimator.SetBool ("moving", false);

    yield return new WaitUntil (() => this.bomb == null);
    


    if(playerAnimator.GetComponent<PlayerController>().HP != 5) yield break;
    backgroundLoopSrc.Play ();

    for (int i = 15; i < 19; i++) {
      yield return new WaitForSeconds (0.5f);
      ShowDialogue (dialogues[i]);
      yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
      ShowDialogue (null);
    }
    backgroundLoopSrc.Stop ();

    while (enemy1Animator.transform.position.x > enemy1TargetPosition1.position.x) {
      Vector2 dir = new Vector2 (-1, 0);
      enemy1Animator.transform.position =
        Vector2.MoveTowards (enemy1Animator.transform.position, enemy1Animator.transform.position + (Vector3) dir, Time.deltaTime);
      yield return null;
    }
    //enemy1Animator.GetComponent<SpriteRenderer>().sortingOrder = 20;
    yield return new WaitForSeconds (.75f);
    backgroundLoopSrc.clip = clip3;
    backgroundLoopSrc.Play ();

    for (int i = 19; i < 22; i++) {
      yield return new WaitForSeconds (0.5f);
      ShowDialogue (dialogues[i]);
      yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
      ShowDialogue (null);
    }

    //fade audio and all circle masks to darkness
    var captainMask = captainAnimator.transform.GetChild (1);
    while (maskOverlay.transform.localScale.x > 0.001f || captainMask.transform.localScale.x > 0.001f ||
      backgroundLoopSrc.volume > 0f) {
      maskOverlay.transform.localScale =
        Vector3.MoveTowards (maskOverlay.transform.localScale, Vector3.one * 0.001f, Time.deltaTime);
      captainMask.localScale =
        Vector3.MoveTowards (captainMask.localScale, Vector3.one * 0.001f, Time.deltaTime);
      backgroundLoopSrc.volume = Mathf.MoveTowards (backgroundLoopSrc.volume, 0, Time.deltaTime);
      if (maskOverlay.transform.localScale.x < 0.3f) {
        roundWindow.GetComponent<SpriteRenderer> ().color = Color.clear;
        window.GetComponent<SpriteRenderer> ().color = Color.clear;
        chandelier.GetComponent<SpriteRenderer> ().color = Color.clear;
      }
      yield return null;
    }

    //load main menu
    PlayerPrefs.SetInt("introCompleted", 1);
    UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
  }

  public IEnumerator ClosingMasks () {
    yield return new WaitForSeconds(1);
    var captainMask = captainAnimator.transform.GetChild (1);
    while (maskOverlay.transform.localScale.x > 0.001f || captainMask.transform.localScale.x > 0.001f ||
      backgroundLoopSrc.volume > 0f) {
      maskOverlay.transform.localScale =
        Vector3.MoveTowards (maskOverlay.transform.localScale, Vector3.one * 0.001f, Time.deltaTime);
      captainMask.localScale =
        Vector3.MoveTowards (captainMask.localScale, Vector3.one * 0.001f, Time.deltaTime);
      backgroundLoopSrc.volume = Mathf.MoveTowards (backgroundLoopSrc.volume, 0, Time.deltaTime);
      if (maskOverlay.transform.localScale.x < 0.3f) {
        roundWindow.GetComponent<SpriteRenderer> ().color = Color.clear;
        window.GetComponent<SpriteRenderer> ().color = Color.clear;
        chandelier.GetComponent<SpriteRenderer> ().color = Color.clear;
      }
      yield return null;
    }
    UnityEngine.SceneManagement.SceneManager.LoadScene("Intro");
  }

  void ShowDialogue (GameObject dialogueObject) {
    dialoguePanel.SetActive (true);
    for (int i = 0; i < dialogues.Length; i++) {
      if (dialogues[i] == dialogueObject) {
        dialogues[i].SetActive (true);
      } else {
        dialogues[i].SetActive (false);
      }
    }
  }

}