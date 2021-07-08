using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// This calss manages Mulltiplayer setup in Game. Inherating from MonoBehaviourPunCallbacks.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Components")]
    public PhotonView photonView;

    public static NetworkManager instance;
    void Awake()
    {
        // Checking if the instance is already present.. And when comming back to the scene there will be two NETWORK MANAGERS. Disabling the new one.
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
        }
        else 
        {
            //instantiating the instance
            instance = this;

            //keeping this object when the scene changes
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // connecting to Photon server
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        //base.OnConnectedToMaster();
        Debug.Log("Connected to master server");
        //CreateRoom("FirstRoom");
    }

    // Create a room on server
    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }

    // call back when a room is created.
    public override void OnCreatedRoom()
    {
        Debug.Log("Room is created: " + PhotonNetwork.CurrentRoom.Name);
    }

    // Join a room.
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    // this loads the scene asynchronously pausing the messaging for photon network
    // This function can be called  from RPC network.
    [PunRPC]
    public void ChangeScene(string sceneName) 
    {
        //DontDestroyOnLoad(this);
        PhotonNetwork.LoadLevel(sceneName);
    }

    // call back function ----> when certain things happen we call that function.

}
