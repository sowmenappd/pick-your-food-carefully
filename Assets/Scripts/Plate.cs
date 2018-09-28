using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour {

	[Range(0f, 210f)]
	public float speed;

	public Transform leftBarrier, rightBarrier;

	void Update () {
		Vector2 dir = Vector2.right * Input.GetAxisRaw("Horizontal") * speed; 
		float xPos = Mathf.Clamp(transform.position.x + dir.x, leftBarrier.position.x, rightBarrier.position.x);
		transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
	}
}
