using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour {

	public Transform foodHolder;
	public Transform[] spawnPoints;
	public Food[] foodItems;

	public float maxSpawnDelay = 1.25f;
	public float maxWaveBreakDelay = 3f;
	float waveNum = 0; 

	static GameManager gameManager;

	public event System.Action OnWaveCompleted;
	public event System.Action OnGameOver;

	void Start () {
		gameManager = GameManager.Instance;
		OnWaveCompleted += gameManager.IncreaseDifficulty;
		OnGameOver += gameManager.SetGameOver;
		OnGameOver += StopSpawning;
		StartCoroutine("StartSpawning");
	}

	IEnumerator StartSpawning(){
		while(!gameManager.GameOver){
			print("Wave "+ waveNum++);
			yield return new WaitForSeconds(Random.Range(0.1f, maxWaveBreakDelay));
			int lastIndex = -1;
			int numToSpawn = Random.Range(1, (gameManager.Difficulty + 1) * foodItems.Length / 4);
			int i=0;
			while(i < numToSpawn){
				int rndSpawnIndex = Random.Range(0, spawnPoints.Length);
				if(lastIndex == -1 || rndSpawnIndex != lastIndex){
					i++;
					int rndFoodIndex = Random.Range(0, foodItems.Length);
					Food item = Instantiate(foodItems[rndFoodIndex], spawnPoints[rndSpawnIndex].position, Quaternion.identity);
					item.GetComponent<Image>().SetNativeSize();
					item.transform.SetParent(foodHolder);
					yield return new WaitForSeconds(Random.Range(0.1f, maxSpawnDelay));
					lastIndex = rndSpawnIndex;
				}
			}

			if(OnWaveCompleted != null){
				OnWaveCompleted();
			}
		}

		yield return null;
	}

	void StopSpawning(){
		StopCoroutine(StartSpawning());
	}

	Food[] Shuffle(Food[] arr){
		for(int i=0; i<arr.Length; i++){
			int idx = Random.Range(i, arr.Length);
			Food temp = arr[i];
			arr[i] = arr[idx];
			arr[idx] = temp; 
		}

		return arr;
	}
	
}
