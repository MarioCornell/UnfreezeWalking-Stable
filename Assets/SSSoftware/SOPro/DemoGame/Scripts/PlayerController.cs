/*
 * This Script is from the Unity Roll-A-Ball Asset and has been modified to demonstrate Saving Data using ScriptableObject Pro
 */

using UnityEngine;

// Include the namespace required to use Unity UI
using UnityEngine.UI;

// Include the namespace required to reload the scene
using UnityEngine.SceneManagement;

using System.Collections;

public class PlayerController : MonoBehaviour {
	
	// Create public variables for player speed, and for the Text UI game objects
	public float speed;
	public Text countText;
	public Text winText;

	// Create private references to the rigidbody component on the player, and the count of pick up objects picked up so far
	private Rigidbody rb;
	private int count;

	// Below are additional variables added for the ScriptableObject Pro Demo Game

	// Create variables for the Saved Data to be displayed
	public Text playedText;
	public Text wonText;
	public Text highScoreText;
	public Text totalText;

	// The Save button
	public Button save;

	// The Parent object of all the PickUps
	public GameObject pickups;

	// The ScriptableObject Pro Asset to use for Saving Data
	public SOProObject saveData;

	// At the start of the game..
	void Start ()
	{
		// Assign the Rigidbody component to our private rb variable
		rb = GetComponent<Rigidbody>();

		// Set the count to zero 
		count = 0;

		// Restore Saved Game Data if it exists 
		if (saveData.saved)
		{
			int i = 0;

			// Loop through all of the PickUps
			foreach(Transform t in pickups.transform)
			{
				// Set the PickUp Active or Not depending on the Save Data
				t.gameObject.SetActive(saveData.pickups[i]);
				if (saveData.pickups[i])
				{
					// If the PickUp is Active Restore it's Colour from the Save Data
					SetMaterial setMaterial = t.GetComponent<SetMaterial>();
					setMaterial.Set(saveData.matIxs[i]);
				} 
				else
				{
					// Increment the count of already collected PuckUps
					count++;
				}
				i++;
			}
		}
		else
		{
			// Not Restarting a Save Game so set Points Scored to zero
			saveData.points = 0;
		}

		// Run the SetCountText function to update the UI (see below)
		SetCountText ();

		// Set the text property of our Win Text UI to an empty string, making the 'You Win' (game over message) blank
		winText.text = "";
	}

	// Each physics step..
	void FixedUpdate ()
	{
		// Set some local float variables equal to the value of our Horizontal and Vertical Inputs
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		// Create a Vector3 variable, and assign X and Z to feature our horizontal and vertical float variables above
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		// Add a physical force to our Player rigidbody using our 'movement' Vector3 above, 
		// multiplying it by 'speed' - our public player speed that appears in the inspector
		rb.AddForce (movement * speed);
	}

	// When this game object intersects a collider with 'is trigger' checked, 
	// store a reference to that collider in a variable named 'other'..
	void OnTriggerEnter(Collider other) 
	{
		// ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
		if (other.gameObject.CompareTag ("Pick Up"))
		{

			// Get the number of points scored based on the Material used by the PickUp
			SetMaterial setMaterial = other.GetComponent<SetMaterial>();
			saveData.points += setMaterial.ix + 1;
			saveData.total += setMaterial.ix + 1;

			// Make the other game object (the pick up) inactive, to make it disappear
			other.gameObject.SetActive (false);

			// Add one to the score variable 'count'
			count = count + 1;

			// Run the 'SetCountText()' function (see below)
			SetCountText ();
		}
	}

	// Create a standalone function that can update the 'countText' UI and check if the required amount to win has been achieved
	void SetCountText()
	{
		// Update the text field of our 'countText' variable
		countText.text = "Points Scored : " + saveData.points.ToString ();

		// Check if our 'count' is equal to or exceeded 12
		if (count >= 12) 
		{
			// Set the text value of our 'winText'
			winText.text = "You Win!";

			// Turn off the Save Button because the game cannot be saved
			save.enabled = false;

			// Update the Save Data with the new values
			saveData.won++;
			saveData.played++;
			if (saveData.points > saveData.highScore) saveData.highScore = saveData.points;
			saveData.saved = false;
			saveData.pickups = null;
			saveData.matIxs = null;

			// Write the Save Data
			saveData.Save();
		}

		// Update the Text Fields with the Saved Data
		playedText.text = "Games Played : " + saveData.played.ToString();
		wonText.text = "Games Won : " + saveData.won.ToString();
		highScoreText.text = "High Score : " + saveData.highScore.ToString();
		totalText.text = "Total Points : " + saveData.total.ToString();
	}

	// Called When the Save And Exit Button is pressed
	public void OnSaveClick()
	{
		// We are saving a game so initialise the Arrays
		saveData.saved = true;
		saveData.pickups = new bool[12];
		saveData.matIxs = new int[12];
		int i = 0;

		// Loop throught all of the PickUps
		foreach (Transform t in pickups.transform)
		{
			if (t.gameObject.activeSelf)
			{
				// This PickUp is Active so save its state into the Save Arrays
				saveData.pickups[i] = true;
				SetMaterial setMaterial = t.GetComponent<SetMaterial>();
				saveData.matIxs[i] = setMaterial.ix;
			}
			else
			{
				// This PickUp is NOT Active
				saveData.pickups[i] = false;
			}
			i++;
		}

		// Application Quit will automatically write the Save Data
		Application.Quit();
	}

	// Called when the Restart Button is pressed. This will Start a New Game
	public void OnRestartClick()
	{
		// Only increment Games Played if a game is in progress
		if (count < 12) saveData.played++;

		// Not Saving A Game so no need for the Arrays
		saveData.saved = false;
		saveData.pickups = null;
		saveData.matIxs = null;

		// Write the Save Data
		saveData.Save();

		// Reload the Scene to Start a New Game
		SceneManager.LoadScene("Roll-a-ball");
	}

	// Called when the Exit button is pressed
	public void OnExitClick()
	{
		// Only increment Games Played if a game is in progress
		if (count < 12) saveData.played++;

		// Not Saving A Game so no need for the Arrays
		saveData.saved = false;
		saveData.pickups = null;
		saveData.matIxs = null;

		// Application Quit will automatically write the Save Data
		Application.Quit();
	}
}