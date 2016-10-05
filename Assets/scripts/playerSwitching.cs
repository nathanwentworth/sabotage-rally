using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// remove scenemanagement later!
using UnityEngine.SceneManagement;

public class PlayerSwitching : MonoBehaviour
{

    private int
        currentIndex = 0,
        nextIndex,
        totalPlayers;
    public int
        currentPlayer;
    public float
        timer,
        turnTime = 7;
    private bool
        randomPlayerOrder;
    public bool
        playerWin,
        DEBUG_MODE;
    public DataManager data;
    List<PlayerList> playerList = new List<PlayerList>();

    private void Start()
    {
        // randomPlayerOrder = data.RandomPlayerOrder;
        randomPlayerOrder = false;
        // creates a player array that's the length of the number of players
        for (int i = 0; i < Input.GetJoystickNames().Length; i++)
        {
            if (Input.GetJoystickNames()[i] != "")
            {
                playerList.Add(new PlayerList(i, "Player " + (i + 1)));
                Debug.Log("Added " + playerList[i].readableName + " to player list");
            }
            else
            {
                Debug.LogWarning("WARNING: Empty controller found.");
            }
        }
        totalPlayers = playerList.Count;
        // only randomize players if that option is on
        if (randomPlayerOrder)
        {
            // ShuffleArray(players);
        }
        // Start the timer
        timer = turnTime;
    }

    private void Update()
    {
        if (Input.GetButton("Pause")) {
            SceneManager.LoadScene(0);
        }
        if (!DEBUG_MODE)
        {
            if (!playerWin)
            {
                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                }
                else
                {
                    SwitchPlayer();
                }
            }
            else
            {
                Debug.Log(playerList[0].readableName + " wins!");
            }
        }
    }

    private void SwitchPlayer()
    {
        if (!playerWin)
        {
            // increment the index up one
            nextIndex = currentIndex + 1;
            // if the next index is past the array length, loop it back to zero
            if (nextIndex >= totalPlayers)
            {
                // maybe put the shuffle in here so that it randomizes constantly?
                // only real downside is that it could have people go more than once in a row
                // but hey that might actually be fun!
                nextIndex = 0;
            }
            // set the current index from the next index var
            currentIndex = nextIndex;
            currentPlayer = playerList[currentIndex].playerNum;
            // delete later, debug prints
            Debug.Log("Switching controls to " + playerList[currentPlayer].readableName);
            // once the indexer runs through, start the timer again
            timer = turnTime;
        }
    }

    public static void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = Random.Range(0, i);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }

    public void RemovePlayer()
    {
        if (totalPlayers > 1)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].playerNum == currentPlayer)
                {
                    playerList.RemoveAt(i);
                    totalPlayers = playerList.Count;
                    Debug.Log("Removed player " + i + 1);
                    Debug.Log("Total players remaining: " + totalPlayers);
                }
            }
        }
        if (totalPlayers == 1)
        {
            playerWin = true;
        }

    }

}
