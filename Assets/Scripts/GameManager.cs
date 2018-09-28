using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

#region Singleton
	private static GameManager instance;
	new private static GameObject gameObject;

	public static GameManager Instance{
		get{ 
			if(instance == null){
				gameObject = new GameObject("GameManager");
				instance = gameObject.AddComponent<GameManager>();
				gameObject.transform.SetSiblingIndex(0);
				print("GM instantiated");
			}
			return instance;
		}
	}

#endregion

	public bool GameOver{
		get; 
		private set;
	}

	public int Difficulty{
		get;
		private set;
	}

	public void LoadLevel(string levelName){
		SceneManager.LoadScene(levelName);
	}

	public void IncreaseDifficulty(){
		Difficulty++;
	}

	void Start(){
		GameOver = false;
		Difficulty = 0;
	}

	public void SetGameOver(){
		GameOver = true;
	}

}
