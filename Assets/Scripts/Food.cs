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
	public Sprite sprite;
	public int healthPoints = 0;

	public event System.Action<int> OnFoodPickedUp, OnFoodMissed;


	public void Start(){
		GetComponent<Image>().sprite = sprite;
	}


	void OnTriggerEnter2D(BoxCollider2D col){
		if(col.tag == "Player") {
			if(OnFoodPickedUp != null){
				OnFoodPickedUp(col.GetComponent<Food>().id);
			}

		} else if(col.tag == "LowerBarrier") {
			if(OnFoodMissed != null){
				OnFoodMissed(col.GetComponent<Food>().id);
			}

		}
	}
}
