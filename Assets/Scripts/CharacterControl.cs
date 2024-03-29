﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour {

	public float moveSpeed = 5f;
	//public float fallSpeed = 5f;

	public float bounceForce = 150f;

	public float displayControlCueTime = 2.5f;
	public GameObject controlCueObject;

	bool isPlayerMovable = true;

	Transform playerTransform;
	Rigidbody2D playerRigidbody;

	float vx,vy;

	void Awake() {
		playerTransform = GetComponent<Transform>();
		playerRigidbody = GetComponent<Rigidbody2D>();
		if (!playerRigidbody) {
			Debug.LogWarning("Rigidbody not found on player!");
		}
		controlCueObject.SetActive(true);
		StartCoroutine(DisableControlCue());
	}

	IEnumerator DisableControlCue() {
		yield return new WaitForSeconds(displayControlCueTime);
		controlCueObject.SetActive(false);
	}

    // Update is called once per frame
    void Update() {
        if (!isPlayerMovable) {
			return;
		}

		vx = Input.GetAxisRaw("Horizontal") * moveSpeed;
		vy = playerRigidbody.velocity.y;

		// Speed limit
		if (vy < -5) {
			vy = -5;
		}

		playerRigidbody.velocity = new Vector2(vx, vy);
    }

	public void Bounce() {
			//Debug.Log("Bounce!");
			vy = 0f;
			playerRigidbody.AddForce(new Vector2(0, bounceForce));
	}

	public bool GetIsMovable() {
		return isPlayerMovable;
	}

	public void FreezeMotion() {
		isPlayerMovable = false;
        playerRigidbody.velocity = new Vector2(0,0);
		playerRigidbody.isKinematic = true;
	}

	public void UnFreezeMotion() {
		isPlayerMovable = true;
		playerRigidbody.isKinematic = false;
	}

}
