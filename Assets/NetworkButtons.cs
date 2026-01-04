using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkButtons : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    string ipAddress = "127.0.0.1";

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            ipAddress = GUILayout.TextField(ipAddress, 100);

            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Client"))
            {
                GetComponent<UnityTransport>().ConnectionData.Address = ipAddress;
                NetworkManager.Singleton.StartClient();
            }
        }

        if (NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("START GAME"))
            {
                gameManager.StartGame();
            }
        }

        GUILayout.EndArea();
    }
}
