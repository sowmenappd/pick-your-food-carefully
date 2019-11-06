using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour {

  GameManager gm;
  AudioSource backgroundSrc;

  public Transform[] windowsParent, balconiesParent;
  [HideInInspector] public Window[] windows;
  [HideInInspector] public Balcony[] balconies;

  public Potion potionPrefab;
  public Transform potionSpawnHeightTransform;

  int level = 1;
  public int Level {
    get { return level; }
  }
  public int activeBombs = 0;
  public int activeWhales = 0;

  public GameObject dialogPanel;
  public GameObject popupPanel;

  public Animator doorAnimator;
  public AudioSource doorCloseSrc;

  PlayerController controller;

  [Header ("Boss level 1")]
  public GameObject boss1;
  public GameObject cannon;
  public GameObject Boss1EntryDialog;
  public GameObject Boss1ExitDialog;

  [Header ("Boss level 2")]
  public GameObject boss2;
  public GameObject Boss2Slider;
  public GameObject Boss2EntryDialog;
  public GameObject Boss2ExitDialog;
  public AudioClip level2Clip;

  [Header ("Boss level 3")]
  public GameObject boss3;
  public GameObject Boss3Slider;
  public GameObject Boss3EntryDialog;
  public GameObject Boss3Exit1Dialog;
  public GameObject Boss3Exit2Dialog;
  public GameObject chestKey;
  public GameObject chest;
  public GameObject gem;
  public AudioClip level3Clip;
  public AudioClip victoryClip;
  public GameObject pressEPanel;
  public GameObject victoryDialog;
  public AudioClip keyDropClip;
  public AudioClip chestOpenClip;
  public GameObject afterVictory1Dialog;
  public GameObject afterVictory2Dialog;

  public Animator creditsAnimator;

  int lastAccessedWindowIndex = -1;

  float healTimer = 0;
  const float potionSpawnTime = 5;

  System.Action<int> OnNewLevel;
  System.Action OnLevelCleared;

  void Start () {
    gm = GameManager.Instance;
    backgroundSrc = GameObject.Find ("Background Audio Main").GetComponent<AudioSource> ();
    controller = FindObjectOfType<PlayerController> ();
    RegisterEvents ();
    if (windowsParent != null && windowsParent.Length != 0) {
      windows = new Window[windowsParent.Length];
      for (int i = 0; i < windows.Length; i++) {
        var lT = windowsParent[i].GetChild (2).Find ("Left");
        var rT = windowsParent[i].GetChild (2).Find ("Right");
        var mT = windowsParent[i].GetChild (2).Find ("Mid");
        windows[i] = new Window (lT, rT, mT);
      }
    }

    if (balconiesParent != null && balconiesParent.Length != 0) {
      balconies = new Balcony[balconiesParent.Length];
      for (int i = 0; i < balconies.Length; i++) {
        var startT = balconiesParent[i].GetChild (1).Find ("Start");
        var endT = balconiesParent[i].GetChild (1).Find ("End");
        balconies[i] = new Balcony (startT, endT);
      }
    }
    StartCoroutine (InitWaves ());
  }

  void Update () {
    if (healTimer == -1) return;

    healTimer += Time.deltaTime;
  }

  private void RegisterEvents () {
    var ui = FindObjectOfType<UIController> ();
    OnNewLevel += ui.SetLevelText;
    OnLevelCleared += ui.ShowClearedMessage;
  }

  private IEnumerator InitWaves () {
    level = 1;
    while (!gm.GameOver) {
      if (OnNewLevel != null) OnNewLevel (level);
      bool healAvailable = false;
      int remainingHealCount = DifficultyAdjuster.LevelHealCount[level];
      int maxLevelHealCount = remainingHealCount;
      float targetSpawnTime = potionSpawnTime;
      if (DifficultyAdjuster.LevelHealCount[level] > 0) {
        healTimer = 0;
        healAvailable = true;
      } else {
        healTimer = -1;
      }
      yield return new WaitForSeconds (3f);

      if (level == 9) {
        controller.GetComponent<Animator> ().SetBool ("moving", false);
        controller.transform.localScale =
          new Vector3 (-1, controller.transform.localScale.y, controller.transform.localScale.z);
        controller.enabled = false;
        doorAnimator.SetTrigger ("open");
        backgroundSrc.Pause ();
        yield return new WaitForSeconds (1f);

        boss1.SetActive (true);
        cannon.SetActive (true);

        yield return new WaitForSeconds (1.25f);
        doorCloseSrc.Play ();

        dialogPanel.SetActive (true);
        Boss1EntryDialog.SetActive (true);
        yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
        dialogPanel.SetActive (false);
        Boss1EntryDialog.SetActive (false);
        controller.enabled = true;
        backgroundSrc.Play ();
        controller.enabled = true;
        yield return new WaitForSeconds (0.5f);

        boss1.GetComponent<HooklegEnemy> ().enabled = true;

        var boss = boss1.GetComponent<LivingEntity> ();
        while (boss.IsAlive) {
          if (healAvailable) {
            if (healTimer >= targetSpawnTime && remainingHealCount > 0) {
              //spawn potion
              var left = potionSpawnHeightTransform.GetChild (0).position.x;
              var right = potionSpawnHeightTransform.GetChild (1).position.x;
              Vector3 rPos = new Vector3 (Random.Range (left, right), potionSpawnHeightTransform.position.y, potionSpawnHeightTransform.position.z);
              var p = Instantiate (potionPrefab, rPos, Quaternion.identity);
              p.GetComponent<Rigidbody2D> ().gravityScale = 0.2f;
              print (p.transform.position);
              --remainingHealCount;
              healTimer = 0;
              targetSpawnTime = Mathf.Pow (potionSpawnTime, maxLevelHealCount - remainingHealCount + 1);
              print ("next heal spawns after " + targetSpawnTime);
            }
          }
          yield return null;
        }

        boss.GetComponent<Rigidbody2D> ().isKinematic = true;
        boss.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

        dialogPanel.SetActive (true);
        Boss1ExitDialog.SetActive (true);
        controller.enabled = false;
        yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
        dialogPanel.SetActive (false);
        Boss1ExitDialog.SetActive (false);
        controller.enabled = true;
        yield return new WaitForSeconds (0.5f);

        boss1.SetActive (false);
        cannon.SetActive (false);
        if (FindObjectsOfType<CannonBall> () != null) {
          foreach (var b in FindObjectsOfType<CannonBall> ()) {
            Destroy (b.gameObject);
          }
        }

      } else if (level == 21) {
        controller.GetComponent<Animator> ().SetBool ("moving", false);
        controller.transform.localScale =
          new Vector3 (-1, controller.transform.localScale.y, controller.transform.localScale.z);
        controller.enabled = false;
        doorAnimator.SetTrigger ("open");
        backgroundSrc.Stop ();
        yield return new WaitForSeconds (.5f);
        boss2.SetActive (true);
        boss2.GetComponent<Animator> ().SetTrigger ("door_in");

        yield return new WaitForSeconds (1.5f);
        doorCloseSrc.Play ();

        yield return new WaitForSeconds (.5f);

        dialogPanel.SetActive (true);
        Boss2EntryDialog.SetActive (true);
        backgroundSrc.clip = level2Clip;
        backgroundSrc.Play ();
        yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
        dialogPanel.SetActive (false);
        Boss2EntryDialog.SetActive (false);
        Boss2Slider.SetActive (true);
        controller.enabled = true;

        yield return new WaitForSeconds (0.5f);

        boss2.GetComponent<BombThrower> ().enabled = true;

        var boss = boss2.GetComponent<LivingEntity> ();
        while (boss.IsAlive) {
          if (healAvailable) {
            if (healTimer >= targetSpawnTime && remainingHealCount > 0) {
              //spawn potion
              var left = potionSpawnHeightTransform.GetChild (0).position.x;
              var right = potionSpawnHeightTransform.GetChild (1).position.x;
              Vector3 rPos = new Vector3 (Random.Range (left, right), potionSpawnHeightTransform.position.y, potionSpawnHeightTransform.position.z);
              var p = Instantiate (potionPrefab, rPos, Quaternion.identity);
              p.GetComponent<Rigidbody2D> ().gravityScale = 0.2f;
              print (p.transform.position);
              --remainingHealCount;
              healTimer = 0;
              targetSpawnTime = Mathf.Pow (potionSpawnTime, maxLevelHealCount - remainingHealCount + 1);
              print ("next heal spawns after " + targetSpawnTime);
            }
          }
          yield return null;
        }

        boss.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

        dialogPanel.SetActive (true);
        Boss2ExitDialog.SetActive (true);
        yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
        dialogPanel.SetActive (false);
        Boss2ExitDialog.SetActive (false);
        yield return new WaitForSeconds (0.5f);

        boss2.SetActive (false);
      } else if (level == 35) {
        controller.GetComponent<Animator> ().SetBool ("moving", false);
        controller.transform.localScale =
          new Vector3 (-1, controller.transform.localScale.y, controller.transform.localScale.z);
        controller.enabled = false;
        doorAnimator.SetTrigger ("open");
        backgroundSrc.Stop ();
        yield return new WaitForSeconds (.5f);
        boss3.SetActive (true);
        boss3.GetComponent<Animator> ().SetTrigger ("door_in");

        yield return new WaitForSeconds (1.5f);
        doorCloseSrc.Play ();

        yield return new WaitForSeconds (.5f);

        dialogPanel.SetActive (true);
        Boss3EntryDialog.SetActive (true);
        backgroundSrc.clip = level3Clip;
        backgroundSrc.Play ();
        yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
        dialogPanel.SetActive (false);
        Boss3EntryDialog.SetActive (false);
        Boss3Slider.SetActive (true);
        controller.enabled = true;

        yield return new WaitForSeconds (0.5f);

        boss3.GetComponent<FinalBoss> ().enabled = true;

        var boss = boss3.GetComponent<LivingEntity> ();
        while (boss.IsAlive) {
          if (healAvailable) {
            if (healTimer >= targetSpawnTime && remainingHealCount > 0) {
              //spawn potion
              var left = potionSpawnHeightTransform.GetChild (0).position.x;
              var right = potionSpawnHeightTransform.GetChild (1).position.x;
              Vector3 rPos = new Vector3 (Random.Range (left, right), potionSpawnHeightTransform.position.y, potionSpawnHeightTransform.position.z);
              var p = Instantiate (potionPrefab, rPos, Quaternion.identity);
              p.GetComponent<Rigidbody2D> ().gravityScale = 0.2f;
              print (p.transform.position);
              --remainingHealCount;
              healTimer = 0;
              targetSpawnTime = Mathf.Pow (potionSpawnTime, maxLevelHealCount - remainingHealCount + 1);
              print ("next heal spawns after " + targetSpawnTime);
            }
          }
          yield return null;
        }

        if (!gm.GameOver) {
          controller.GetComponent<Animator> ().SetBool ("moving", false);
          controller.enabled = false;
          boss.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

          dialogPanel.SetActive (true);
          Boss3Exit1Dialog.SetActive (true);
          yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
          Boss3Exit1Dialog.SetActive (false);
          yield return new WaitForSeconds (1f);
          Boss3Exit2Dialog.SetActive (true);
          yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
          Boss3Exit2Dialog.SetActive (false);
          dialogPanel.SetActive (false);

          backgroundSrc.Stop ();
          chestKey.transform.parent = null;
          chestKey.GetComponent<BoxCollider2D> ().enabled = true;
          chestKey.GetComponent<Rigidbody2D> ().isKinematic = false;
          chestKey.GetComponent<Rigidbody2D> ().AddForce (Vector2.up * 4.5f, ForceMode2D.Impulse);
          chestKey.transform.rotation = Quaternion.Euler (Vector3.forward * 60f);
          backgroundSrc.clip = keyDropClip;
          backgroundSrc.loop = false;
          backgroundSrc.Play ();
          boss3.SetActive (false);

          yield return new WaitForSeconds (2f);
          controller.enabled = true;
          //get key
          while (Mathf.Abs (controller.transform.position.x - chestKey.transform.position.x) > 0.5f) {
            yield return null;
          }

          controller.enabled = false;
          controller.GetComponent<Animator> ().SetBool ("moving", false);
          pressEPanel.SetActive (true);
          yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.E));
          chestKey.transform.parent = FindObjectOfType<PlayerController> ().transform;
          chestKey.SetActive (false);
          pressEPanel.SetActive (false);
          controller.enabled = true;

          while (Mathf.Abs (controller.transform.position.x - chest.transform.position.x) > 0.75f) {
            yield return null;
          }
          controller.enabled = false;
          controller.GetComponent<Animator> ().SetBool ("moving", false);
          pressEPanel.SetActive (true);
          yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.E));
          pressEPanel.SetActive (false);
          chest.GetComponent<Animator> ().SetTrigger ("open");
          backgroundSrc.clip = chestOpenClip;
          backgroundSrc.Play ();
          gem.SetActive (true);
          gem.GetComponent<SpriteRenderer> ().sortingOrder = 5;
          //make the gem bounce up
          //gem.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 1.5f, ForceMode2D.Impulse);
          yield return new WaitForSeconds(.5f);
          backgroundSrc.clip = victoryClip;
          backgroundSrc.loop = true;

          dialogPanel.SetActive (true);
          afterVictory1Dialog.SetActive (true);
          yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
          afterVictory1Dialog.SetActive (false);
          yield return new WaitForSeconds (.5f);
          afterVictory2Dialog.SetActive (true);
          yield return new WaitUntil (() => Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Space));
          afterVictory2Dialog.SetActive (false);
          yield return new WaitForSeconds (.5f);
          dialogPanel.SetActive (false);

          backgroundSrc.Play ();
          //show some more victory screen
          popupPanel.SetActive (true);
          victoryDialog.SetActive (true);
          yield return new WaitForSeconds (3f);
          popupPanel.SetActive (false);
          victoryDialog.SetActive (false);

          creditsAnimator.SetTrigger ("fade");
          StartCoroutine (ShowCredits ());
          yield return new WaitForSeconds (2f);
          StopCoroutine (InitWaves ());
          break;
        }

      } else {
        int dBombCount = DifficultyAdjuster.GetBombCount (level);
        //int dFoodCount = DifficultyAdjuster.GetFoodCount (level);

        activeBombs = dBombCount;
        int count = 0;

        while (!gm.GameOver && count < activeBombs) {
          //pick a random window
          int index = UnityEngine.Random.Range (0, windows.Length);
          while (index == lastAccessedWindowIndex) {
            index = UnityEngine.Random.Range (0, windows.Length);
          }

          if (healAvailable) {
            if (healTimer >= targetSpawnTime && remainingHealCount > 0) {
              //spawn potion
              var left = potionSpawnHeightTransform.GetChild (0).position.x;
              var right = potionSpawnHeightTransform.GetChild (1).position.x;
              Vector3 rPos = new Vector3 (Random.Range (left, right), potionSpawnHeightTransform.position.y, potionSpawnHeightTransform.position.z);
              var p = Instantiate (potionPrefab, rPos, Quaternion.identity);
              p.GetComponent<Rigidbody2D> ().gravityScale = 0.2f;
              print (p.transform.position);
              --remainingHealCount;
              healTimer = 0;
              targetSpawnTime = Mathf.Pow (potionSpawnTime, maxLevelHealCount - remainingHealCount + 1);
              print ("next heal spawns after " + targetSpawnTime);
            }
          }

          float val = Random.value;
          if (val < 0.02f && activeWhales < 2) {
            var enemyType = FindObjectOfType<WhaleTanker> ();
            index %= 2;
            enemyType.GetPath (index);
          } else {
            var enemyType = FindObjectsOfType<ShortRangeThrower> ().Where (e => e.GetComponent<WhaleTanker> () == null).ElementAt (0);
            enemyType.GetPath (index);
          }

          ++count;
          lastAccessedWindowIndex = index;
          yield return new WaitForSeconds (2f);
        }

        while (!gm.GameOver && FindObjectsOfType<Bomb> ().Length > 0) {
          yield return null;
        }
        count = 0;
        activeBombs = 0;
      }
      ++level;
      if (level == 36) break;
      if (OnLevelCleared != null) OnLevelCleared ();

      yield return new WaitForSeconds (3f);
      yield return null;
    }
  }

  IEnumerator ShowCredits () {
    int len = creditsAnimator.transform.childCount;
    yield return new WaitForSeconds (2f);
    for (int i = 0; i < len; i++) {
      if ((i - 1) >= 0) {
        creditsAnimator.transform.GetChild (i - 1).gameObject.SetActive (false);
      }
      creditsAnimator.transform.GetChild (i).gameObject.SetActive (true);
      yield return new WaitForSeconds (5f);
    }
    creditsAnimator.transform.GetChild (len - 1).gameObject.SetActive (false);
    while (backgroundSrc.volume > 0.001f) {
      backgroundSrc.volume = Mathf.MoveTowards (backgroundSrc.volume, 0, 0.1f * Time.deltaTime);
      yield return null;
    }
    backgroundSrc.volume = 0;
    yield return new WaitForSeconds (1f);
    UnityEngine.SceneManagement.SceneManager.LoadScene ("Main Menu");
  }

}