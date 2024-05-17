using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    private bool isConnecting;
    public InputField input;

    private void Start()
    {
    }

    public void ConnectToPhoton()
    {
        isConnecting = true;
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            Debug.Log("Successfully connected to Photon Master Server.");
            isConnecting = false;
            string name = input.text;
            input.text = "";
            PhotonNetwork.JoinOrCreateRoom(name, new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined the room.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected from Photon. Cause: " + cause);
    }
}
