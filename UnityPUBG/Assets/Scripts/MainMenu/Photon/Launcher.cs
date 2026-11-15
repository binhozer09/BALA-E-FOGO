using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

namespace UnityPUBG.Scripts.MainMenu
{
    public class Launcher : Photon.PunBehaviour
    {
        public static Launcher Instance { get; private set; }

        public PhotonLogLevel Loglevel = PhotonLogLevel.Full;
        [Tooltip("Maximum players per room.")]
        public byte MaxPlayersPerRoom = 4;
        [Tooltip("Panel containing the connect controls.")]
        public GameObject controlPanel;
        [Tooltip("UI shown while connecting.")]
        public GameObject progressLabel;
        [Tooltip("Displays the current room player count.")]
        public UnityEngine.UI.Text PlayerCount;

        private const float ConnectionTimeoutSeconds = 25f;
        private string _gameVersion = "1";
        private bool isConnecting;
        private Coroutine connectionTimeoutRoutine;

        private void Awake()
        {
            PhotonNetwork.logLevel = Loglevel;
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            Instance = this;
        }

        private void Start()
        {
            SetProgressVisible(false);
        }

        public void Connect()
        {
            if (isConnecting)
                return;

            isConnecting = true;
            SetProgressVisible(true);
            SetStatus("Connecting to server...");

            GameObject titleImage = GameObject.Find("TitleImage");
            if (titleImage != null)
                titleImage.SetActive(false);
            if (controlPanel != null)
                controlPanel.SetActive(false);

            StartConnectionTimeout();

            if (PhotonNetwork.connected)
                PhotonNetwork.JoinRandomRoom();
            else
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Launcher: authenticated with Photon Master Server.");
            if (isConnecting)
            {
                SetStatus("Searching for a room...");
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            Debug.LogError("Photon initial connection failed: " + cause);
            FinishConnectionWithError("Connection failed: " + cause);
        }

        public override void OnConnectionFail(DisconnectCause cause)
        {
            Debug.LogError("Photon connection lost: " + cause);
            FinishConnectionWithError("Connection lost: " + cause);
        }

        public override void OnDisconnectedFromPhoton()
        {
            if (isConnecting)
                FinishConnectionWithError("Disconnected from Photon. Check the network and App ID.");
            Debug.LogWarning("Launcher: OnDisconnectedFromPhoton() called by PUN.");
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("No available room. Creating a new room.");
            SetStatus("Creating a room...");
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }

        public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            string detail = codeAndMsg != null && codeAndMsg.Length > 1
                ? codeAndMsg[1].ToString()
                : "Unknown Photon room error";
            Debug.LogError("Photon could not create a room: " + detail);
            FinishConnectionWithError("Could not create room: " + detail);
        }

        public override void OnJoinedRoom()
        {
            isConnecting = false;
            StopConnectionTimeout();

            if (PlayerCount != null)
            {
                PlayerCount.text = "Players : " + PhotonNetwork.playerList.Length + "/" + MaxPlayersPerRoom;
                PlayerCount.gameObject.SetActive(true);
            }

            if (progressLabel != null && progressLabel.transform.parent != null)
                progressLabel.transform.parent.gameObject.SetActive(false);
            else
                SetProgressVisible(false);

            Debug.Log("Launcher: successfully joined room. Players: " + PhotonNetwork.playerList.Length);

#if UNITY_EDITOR
            if (PhotonNetwork.playerList.Length == MaxPlayersPerRoom)
                Lobby.PTManager.Instance.StartGame();
            else
                Debug.Log("PlayerCount: " + PhotonNetwork.playerList.Length);
#endif
        }

        private void StartConnectionTimeout()
        {
            StopConnectionTimeout();
            connectionTimeoutRoutine = StartCoroutine(ConnectionTimeout());
        }

        private IEnumerator ConnectionTimeout()
        {
            yield return new WaitForSeconds(ConnectionTimeoutSeconds);
            if (isConnecting)
            {
                Debug.LogError("Photon connection timed out after " + ConnectionTimeoutSeconds + " seconds. State: " + PhotonNetwork.connectionStateDetailed);
                FinishConnectionWithError("Connection timed out. Verify internet access and Photon App ID.");
                if (PhotonNetwork.connected)
                    PhotonNetwork.Disconnect();
            }
        }

        private void StopConnectionTimeout()
        {
            if (connectionTimeoutRoutine != null)
            {
                StopCoroutine(connectionTimeoutRoutine);
                connectionTimeoutRoutine = null;
            }
        }

        private void FinishConnectionWithError(string message)
        {
            isConnecting = false;
            StopConnectionTimeout();
            SetStatus(message);
            SetProgressVisible(false);
            if (controlPanel != null)
                controlPanel.SetActive(true);
        }

        private void SetProgressVisible(bool visible)
        {
            if (progressLabel != null)
                progressLabel.SetActive(visible);
        }

        private void SetStatus(string message)
        {
            if (progressLabel == null)
                return;

            UnityEngine.UI.Text label = progressLabel.GetComponent<UnityEngine.UI.Text>();
            if (label == null)
                label = progressLabel.GetComponentInChildren<UnityEngine.UI.Text>();
            if (label != null)
                label.text = message;
        }

        private void OnDestroy()
        {
            StopConnectionTimeout();
            if (Instance == this)
                Instance = null;
        }
    }
}
