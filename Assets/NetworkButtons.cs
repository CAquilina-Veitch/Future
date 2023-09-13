using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkButtons : MonoBehaviour {

    [SerializeField] Storage storageScript;

    string test = "127.0.01";

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            test = GUILayout.TextField(test, 100);

            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            /*if (GUILayout.Button("Server")) {
                NetworkManager.Singleton.StartServer();
            }*/
            if (GUILayout.Button("Client"))
            {
                GetComponent<UnityTransport>().ConnectionData.Address = test; NetworkManager.Singleton.StartClient();
            }

        }

        if (NetworkManager.Singleton.IsServer)
        {
            if(GUILayout.Button("START GAME!!! ... !!! >...! "))
            {
                storageScript.StartGame();
            }
        }




        GUILayout.EndArea();
    }

    // private void Awake() {
    //     GetComponent<UnityTransport>().SetDebugSimulatorParameters(
    //         packetDelay: 120,
    //         packetJitter: 5,
    //         dropRate: 3);
    // }
}