/*
 * This is the ScriptableObject Pro Class which will be used to Save the Game Data
 */

using UnityEngine;

// Enable the use of ScriptableObject Pro Attributes
using SSSoftware.Attributes;

// Create a menu Item for Creating Scriptable Object Assets
[CreateAssetMenu(fileName = "New DemoSave", menuName = "SOPro DemoSave")]

// Make the Class Mutable. All Public Serializable Fields will be saved
[Mutable]

// Make the Scriptable Object Reset to build values if modified
[ResetOnError]

//[PersistAcrossBuilds] // Uncomment this line to persist the Save Data across builds

// The class must inherit from SSSoftware.SOPro.ScriptableObject
public class SOProObject : SSSoftware.SOPro.ScriptableObject
{
	// Serializable fields to save
	public int played = 0; // Number of Games Played
	public int won = 0; // Number of Games Won
	public int total = 0; // Total number of points scored
	public int highScore = 0; // Highest Score
	public bool saved = false; // Is a Game being saved
	public int points = 0; // Number of point scored so far in the current game

	// These Arrays will contain 1 entry for each PickUp
	// They will be null if no game is being saved
	public bool[] pickups = null; // Is a PickUp active is the current game
	public int[] matIxs = null; // The index of which Material to use for the PickUp
}
