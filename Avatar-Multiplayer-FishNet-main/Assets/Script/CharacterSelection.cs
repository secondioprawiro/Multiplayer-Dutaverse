using FishNet.Connection;
using FishNet.Object;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class CharacterSelection : NetworkBehaviour
{
    [SerializeField] private List<GameObject> character = new List<GameObject>(); // List for boy and girl characters
    [SerializeField] private GameObject characterSelectorPanel; // Panel to select character
    [SerializeField] private GameObject canvasObject; // Canvas containing character selection
    [SerializeField] private GameObject vrPlayerPrefab; // VR player prefab

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            string deviceModel = SystemInfo.deviceModel;
            bool isVR = deviceModel.Contains("Pico");

            if (isVR)
            {
                // Spawn VR character if device is VR (e.g., Pico)
                SpawnRequest(1, LocalConnection);
            }
            else
            {
                // Show character selection canvas for non-VR devices
                canvasObject.SetActive(true);
            }
        }
        else
        {
            // Hide canvas if the client is not the owner
            canvasObject.SetActive(false);
        }
    }

    // Called when the "Boy" button is clicked
    public void SpawnBoy()
    {
        characterSelectorPanel.SetActive(false);
        SpawnCharacter(0);
    }

    // Called when the "Girl" button is clicked
    public void SpawnGirl()
    {
        characterSelectorPanel.SetActive(false);
        SpawnCharacter(1);
    }

    // Sends a request to spawn a character on the server
    private void SpawnCharacter(int spawnIndex)
    {
        if (IsOwner)
        {
            SpawnRequest(spawnIndex, Owner);
        }
    }

    // Server-side logic to spawn the character based on index
    [ServerRpc(RequireOwnership = false)]
    private void SpawnRequest(int spawnIndex, NetworkConnection conn)
    {
        GameObject prefabToSpawn = (spawnIndex == 1 && vrPlayerPrefab != null) ? vrPlayerPrefab : character[spawnIndex];

        // Ensure the prefab and spawn point are valid
        if (prefabToSpawn == null || SpawnPoint.instance == null)
        {
            Debug.LogError("Invalid prefab or spawn point is not defined.");
            return;
        }

        // Instantiate and spawn the selected character on the network
        GameObject playerInstance = Instantiate(
            prefabToSpawn,
            SpawnPoint.instance.transform.position,
            quaternion.identity
        );
        Spawn(playerInstance, conn);
        Debug.Log($"Spawned {prefabToSpawn.name} for connection {conn.ClientId}.");
    }
}


