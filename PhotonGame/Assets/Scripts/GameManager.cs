using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool gameEnded = false;        // has the game ended?
    public float timeToWin;               // time a player needs to hold the hat to win
    public float invincibleDuration;      // how long after a player gets the hat, are they invincible
    private float hatPickupTime;          // the time the hat was picked up by the current holder

    [Header("Players")]
    public string playerPrefabLocation;   // path in Resources folder to the Player prefab
    public Transform[] spawnPoints;       // array of all available spawn points
    public PlayerControl[] players;    // array of all the players--> this controls the player class
    public int playerWithHat;             // id of the player with the hat
    private int playersInGame;            // number of players in the game

    // instance
    public static GameManager instance;

    void Awake()
    {
        // instance
        instance = this;
    }
    // Setting up player controller array
    void Start()
    {
        // Initalizing an array with the number of Players present in Photon Network.
        players = new PlayerControl[PhotonNetwork.PlayerList.Length];

        // Notifyiing other player that I have joined the game.
        photonView.RPC("ImInGame", RpcTarget.All);
    }

    /// <summary>
    /// When player joins the game 
    /// </summary>
    [PunRPC]
    void ImInGame()
    {
        // adding to the player counter
        playersInGame++;

        // checking if player counter is equal to no. of player in the photon network.
        // waiting for every player to join the game scene before spawning the players
        if (playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
    }


    /// <summary>
    /// The SpawnPlayer function gets called when all the players are in the game.
    /// It spawns a player 
    /// </summary>
    void SpawnPlayer()
    {
        // instantiate the player across the network
        // Getting random transform, returns a random integer number ----> spawnPoints[Random.Range(0, spawnPoints.Length)].position
        // Random spawning of players
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);        
        // get the player control script from spawned player
        PlayerControl playerScript = playerObj.GetComponent<PlayerControl>();

        // initialize the player
        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// Function that takes in an Id or game object, then returns the corresponding player.
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    // returns the player who has the requested Id
    public PlayerControl GetPlayer(int playerId)
    {
        // Returns the first element of a sequence players.First.
        // lambda expression with one argument. comparing the id to get the player.
        // from players array, filter out the with player id. and return player --> player controller
        return players.First(x => x.id == playerId);
    }

    // LINQ ----> Language integreated query
    /// <summary>
    /// The most common uses for LINQ statements tend to be sorting, searching, and filtering.
    /// t=> Lambda operator. “t” as a reference to each object in the collection.
    /// 
    /// </summary>
    /// <param name="playerObject"></param>
    /// <returns></returns>



    // returns the player of the requested gameObject
    public PlayerControl GetPlayer(GameObject playerObject)
    {
        // from players array find the requested Game object
        return players.First(x => x.gameObject == playerObject);
    }

    /// <summary>
    /// swaps the hat visual between the two players.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="initialGive"></param>
    // called when a player hits the hatted player - giving them the hat
    [PunRPC]
    public void GiveHat(int playerId, bool initialGive)
    {
        // remove the hat from the currently hatted player
        if (!initialGive)
            GetPlayer(playerWithHat).SetHat(false);

        // give the hat to the new player
        playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        hatPickupTime = Time.time;
    }

    // is the player able to take the hat at this current time?
    public bool CanGetHat()
    {
        if (Time.time > hatPickupTime + invincibleDuration)
            return true;
        else
            return false;
    }

    [PunRPC]
    void WinGame(int playerId)
    {
        gameEnded = true;
        PlayerControl player = GetPlayer(playerId);
        // set the UI to show who's won

        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        

        Invoke("GoBackToMenu", 3.0f);
    }

    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
        Destroy(NetworkManager.instance.gameObject);
    }

}
