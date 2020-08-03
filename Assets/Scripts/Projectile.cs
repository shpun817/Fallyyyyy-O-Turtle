﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public float moveSpeed = 6.5f;

	public float noiseSize = 0.4f;

	Transform player;
	Transform selfTransform;
	Rigidbody2D selfRigidbody;
	Vector3 direction;

	private void Awake() {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
		if (!player) {
			Debug.LogError("Player not found by Projectile " + gameObject.name);
		}
		selfRigidbody = GetComponent<Rigidbody2D>();
		if (!selfRigidbody) {
			Debug.LogError("Rigidbody2D not found on " + gameObject.name);
		}
		selfTransform = GetComponent<Transform>();
		Setup();
	}

	public void Setup() {
		//Debug.Log("Projectile Setup() called on " + gameObject.name);
		// Determine the movement direction
		direction = (player.position - selfTransform.position).normalized;
		direction.z = 0f; // Make sure the z component is zero

		// Introduce noises to the x and y components
		direction.x += Random.Range(-noiseSize, noiseSize);
		direction.y += Random.Range(-noiseSize, noiseSize);

		direction = direction.normalized;

		selfTransform.rotation = Quaternion.FromToRotation(Vector3.right, direction);

		selfRigidbody.velocity = direction * moveSpeed;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Player") {
			//Debug.Log("Projectile hit player!");

			Destroy(gameObject);
		}
	}

}
