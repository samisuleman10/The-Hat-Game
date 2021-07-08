using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerControl : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;

    [Header("info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param> it is the local player called from GM
    [PunRPC]
    public void Initialize(Player player) 
    {
        photonPlayer = player;
        id = player.ActorNumber;
        
        // setting the players array in side the GM here. So every initialized player is assinged to the players array in GM. 
        GameManager.instance.players[id - 1] = this;

        // Give first player the hat.
        if (id == 1)
            GameManager.instance.GiveHat(id, true);

        // if this isn't our local player, disable physics as that's
        // controlled by the user and synced to all other clients
        if (!photonView.IsMine)
            rig.isKinematic = true;
    }

    void Update()
    {
        // Since we have the master client checking each player's cur hat time
        // we need to sync our hat time across the network. This could be done by an RPC every frame(costly)
        // the host will check if the player has won
        if (PhotonNetwork.IsMasterClient)
        {
            if (curHatTime >= GameManager.instance.timeToWin && !GameManager.instance.gameEnded)
            {
                GameManager.instance.gameEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            // track the amount of time we're wearing the hat
            if (hatObject.activeInHierarchy)
                curHatTime += Time.deltaTime;
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        // if rigid body is attached move the object with rig.velocity not transform.position.
        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump()
    {
        // creates an array downwards
        Ray ray = new Ray(transform.position, Vector3.down);

        // checks if the array is 
        if (Physics.Raycast(ray, 0.7f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // sets the player's hat active or not
    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    void OnCollisionEnter(Collision collision)
    {
        //if  this is not our player. We will return the function.
        if (!photonView.IsMine)
            return;

        // did we hit another player?
        if (collision.gameObject.CompareTag("Player"))
        {
            // do they have the hat?
            if (GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
            {
                // can we get the hat?
                if (GameManager.instance.CanGetHat())
                {
                    // give us the hat
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    // writing the value to the server so all the clients can download it.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // sending information
        if (stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        // receiving information.
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }
}
