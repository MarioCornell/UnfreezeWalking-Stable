/*
 * This scripts changes the colours of the PickUp every 10 seconds.
 * The points scored will be calculated based on the Material in use.
 * 
 * This Script has been Added to the PickUp Prefab for the ScriptableObject Pro Demo
 */

using System.Collections;
using UnityEngine;

public class SetMaterial : MonoBehaviour
{
	// An Array of Materials to use
	public Material[] materials;

	// Which Material in the Array is currently in use
	// This is used by the Player Controller to calculate points scored
	public int ix;

	// The Renderer to use for changing the material
	MeshRenderer meshRenderer;

	// Awake is used here because Player Controller Start() might need to use the Renderer reference
	private void Awake()
	{
		// Get a reference to the renderer
		meshRenderer = GetComponent<MeshRenderer>();

		// Start the Coroutine so the Material can change every 10 seconds
		StartCoroutine(Cycle());
	}

	IEnumerator Cycle()
	{
		// Only do this if the PickUp is still active otherwise exit the routine
		while (gameObject.activeSelf)
		{
			// Choose a Random Material from the Array
			Set();

			// Wait for 10 Seconds
			yield return new WaitForSeconds(10f);
		}
	}

	public void Set()
	{
		// Set the current material index to a random number based on the length of the array
		ix = Random.Range(0, materials.Length);

		// Set the PickUp material to the choosen one
		Set(ix);
	}

	// This is also called by Player Controller Start() when restoring the state of a saved game
	public void Set(int ix)
	{
		// Change the material of the Renderer to a material from the Array
		meshRenderer.material = materials[ix];
	}
}
