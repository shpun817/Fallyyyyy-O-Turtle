﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, ISetup {

	#region Singleton

		public static GameManager Instance;
		private void Awake() {
			Instance = this;
		}

	#endregion

	public ObjectSpawner objectSpawner;
	public BackgroundCamera backgroundCamera;

	[SerializeField, Range(0, 10)]
	int playerMaxHealth = 4;

	[SerializeField]
	int playerHealth;

	[SerializeField, Range(1, 20)]
	int targetScore = 5;

	[SerializeField]
	int playerScore = 0;

	[SerializeField]
	float playerAltitudeLimit = -500f;

	[SerializeField]
	float flashTime = 1f;

	[SerializeField]
	int maxRestoreHealth = 7;
	[SerializeField]
	int restoreHealthAmount = 0;
	WaitWhile waitWhilePlayerAtMaxHealth;
	IEnumerator restorePlayerHealth;

	public GameObject swordEffect;
	public float swordDuration = 4.5f;

	bool isPlayerFlashing = false;
	bool isHeartShielded = false;
	bool isSwordEffectOn = false;

	public static GameObject Player;

	public AudioSource audioSource;

	Transform playerTransform;
	Rigidbody2D playerRigidbody;
	WaitUntil waitUntilPlayerPositionTooLow;
	public RewindTime playerRewindTime;
	Animator playerAnimator;
	WaitForSeconds waitForFlashTime;

	public Animator heartAnimator;

	Animator swordEffectAnimator;
	WaitForSeconds waitForSwordDuration1SecondLess;
	readonly WaitForSeconds waitFor1Second = new WaitForSeconds(1f);
	IEnumerator stopSwordEffect;

	private void Start() {
		Player = GameObject.FindGameObjectWithTag("Player");
		playerTransform = Player.transform;
		if (!playerTransform) {
			Debug.LogWarning("Player not found by Game Manager.");
		}
		playerRigidbody = Player.GetComponent<Rigidbody2D>();
		if (!playerRigidbody) {
			Debug.LogWarning("Rigidbody2D not found on player.");
		}
		playerRewindTime = Player.GetComponent<RewindTime>();
		if (!playerRigidbody) {
			Debug.LogWarning("RewindTime not found on player.");
		}
		playerAnimator = Player.GetComponent<Animator>();
		if (!playerRigidbody) {
			Debug.LogWarning("Animator not found on player.");
		}
		if (!swordEffect) {
			Debug.LogWarning("Sword Effect not attached to Game Manager!");
		}
		swordEffectAnimator = swordEffect.GetComponent<Animator>();
		waitUntilPlayerPositionTooLow = new WaitUntil(PlayerPositionTooLow);
		waitForFlashTime = new WaitForSeconds(flashTime);
		waitForSwordDuration1SecondLess = new WaitForSeconds(swordDuration - 1f);
		waitWhilePlayerAtMaxHealth = new WaitWhile(PlayerAtMaxHealth);
		Setup();
	}

	public void Setup() {
		playerHealth = playerMaxHealth;
		playerScore = 0;

		StartCoroutine(TeleportPlayerToOrigin());
	}

	private void Update() {
		int gameOverCheck = GameOverCheck();
		if (gameOverCheck != 0) {
			PointsTracker.Instance.SetIsGameOver(true);
			if (gameOverCheck == -1) {
				// LOSE
				//Debug.Log("Player loses.");
				SceneManager.LoadScene("Lose");
			} else if (gameOverCheck == 1) {
				// WIN
				//Debug.Log("Player wins.");
				SceneManager.LoadScene("Win");
			} else {
				Debug.LogWarning("Game Over check receives invalid value.");
			}
		}
	}

	int GameOverCheck() {
		// Return -1 for losing, 1 for winning, 0 for not game over yet

		if (playerHealth <= 0) {
			return -1;
		}

		if (playerScore >= targetScore) {
			return 1;
		}

		return 0;
	}

	bool PlayerPositionTooLow() {
		return (playerTransform.position.y < playerAltitudeLimit);
	}

	IEnumerator TeleportPlayerToOrigin() {
		yield return waitUntilPlayerPositionTooLow;

		Vector3 offset = Vector3.zero - playerTransform.position;
		playerRewindTime.ApplyOffsetTostoredInformation(offset);
		playerTransform.position = Vector3.zero;
		
		objectSpawner.ResetLowY();
		backgroundCamera.ResetLowY();

		StartCoroutine(TeleportPlayerToOrigin());
	}

	public int GetPlayerHealth() {
		return playerHealth;
	}

	public int GetPlayerMaxHealth() {
		return playerMaxHealth;
	}

	public bool GetIsHeartShielded() {
		return isHeartShielded;
	}

	public int GetPlayerScore() {
		return playerScore;
	}

	public int GetTargetScore() {
		return targetScore;
	}

	public float GetPlayerProgress() {
		return (float)playerScore / (float)targetScore;
	}
	
	public void IncreaseScore(int increaseAmount) {
		playerScore += increaseAmount;
	}

	public int GetRestoreHealthAmount() {
		return restoreHealthAmount;
	}

	public int GetMaxRestoreHealth() {
		return maxRestoreHealth;
	}

	public void IncreaseRestoreHealthAmount(int restoreAmount) {
		restoreHealthAmount += restoreAmount;
		if (restoreHealthAmount >= maxRestoreHealth) {
			if (restorePlayerHealth != null) {
				StopCoroutine(restorePlayerHealth);
			}
			restorePlayerHealth = RestorePlayerHealth();
			StartCoroutine(restorePlayerHealth);
		}
	}

	bool PlayerAtMaxHealth() {
		return playerHealth >= playerMaxHealth;
	}

	IEnumerator RestorePlayerHealth() {
		yield return waitWhilePlayerAtMaxHealth;
		playerHealth += Random.Range(1, 3); // Add 1 or 2 (expected 1.5)
		restoreHealthAmount = 0;
	}

	public void PlayerTakeDamage(int damageAmount) {

		if (isPlayerFlashing) {
			return;
		}

		isPlayerFlashing = true;

		// Play sound
		audioSource.Play();

		// Play animation
		playerAnimator.SetTrigger("StartHurt");
		heartAnimator.SetTrigger("StartHurt");
		StartCoroutine(StopHurt());

		if (isHeartShielded) {
			isHeartShielded = false;
		} else {
			playerHealth -= damageAmount;
		}
	}

	IEnumerator StopHurt() {
		yield return waitForFlashTime;

		playerAnimator.SetTrigger("EndHurt");
		heartAnimator.SetTrigger("EndHurt");
		isPlayerFlashing = false;
	}

	public void ShieldHeart() {
		isHeartShielded = true;
	}

	public void EquipSword() {
		if (isSwordEffectOn) {
			if (stopSwordEffect != null)
				StopCoroutine(stopSwordEffect);
		}

		isSwordEffectOn = true;
		swordEffect.SetActive(true);
		swordEffectAnimator.SetBool("isFlashing", false);

		stopSwordEffect = StopSwordEffect();
		StartCoroutine(stopSwordEffect);
	}

	IEnumerator StopSwordEffect() {
		yield return waitForSwordDuration1SecondLess;

		// Make the sword effect flash
		if (swordEffectAnimator) {
			swordEffectAnimator.SetBool("isFlashing", true);
		}

		yield return waitFor1Second;

		if (swordEffectAnimator) {
			swordEffectAnimator.SetBool("isFlashing", false);
		}

		swordEffect.SetActive(false);
		isSwordEffectOn = false;

	}

}
