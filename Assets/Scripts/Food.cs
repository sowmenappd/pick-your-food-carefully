using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Food : MonoBehaviour {
	public enum Rarity{ common, special, rare, very_rare, legendary };
	public enum Effect{ good, bad, poison };
	new public string name;
	public int id;
	public Rarity rarity;
	public Effect effect;
	public int healthPoints = 0;
	
	Sprite sprite;
	float startingVelocity = 60f;

	public event System.Action<int> OnFoodPickedUp, OnFoodMissed;

	void Start(){
		GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -(startingVelocity + (startingVelocity / 2 * GameManager.Instance.Difficulty)));
	}

	void OnTriggerEnter2D(Collider2D col){
		if(col.tag == "Player") {
			print(name);
			if(OnFoodPickedUp != null){
				OnFoodPickedUp(col.GetComponent<Food>().id);
			}

		} else if(col.tag == "LowerBarrier") {
			if(OnFoodMissed != null){
				OnFoodMissed(col.GetComponent<Food>().id);
			}
		}
		Destroy(gameObject);
	}
}
