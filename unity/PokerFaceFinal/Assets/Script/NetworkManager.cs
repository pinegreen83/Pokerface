using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;


public class NetworkManager : MonoBehaviourPunCallbacks
{

    #region 변수 영역

    #region private 변수 영역
    // 게임의 버전
    private readonly string version = "version 0.1.4";
    // 게임 외부 React에서 유저 아이디를 받아오는 함수
    [DllImport("__Internal")]
    private static extern void TakeGameInfoFromReact();

    // 유저의 닉네임
    private string userId = "test";
    // 룸 목록에 대한 데이터를 저장하기 위한 딕셔너리 자료형
    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    // 룸 목록을 표시할 프리팹
    private GameObject roomItemPrefab;
    
    //WebGL에서 사용할 방 이름과 플레이어 이름 저장 변수
    private string userIdWeb;
    private string roomNameWeb;
    #endregion

    #endregion

    void Awake()
    {
        // 60 frame으로 고정
        Application.targetFrameRate = 60;

        // vSync off
        QualitySettings.vSyncCount = 0;

        // 마스터 클라이언트의 씬 자동 동기화 옵션
        PhotonNetwork.AutomaticallySyncScene = true;
        // 게임 버전 설정
        PhotonNetwork.GameVersion = version;

        PlayerPrefs.DeleteAll();
    }

    void Start()
    {
        // 시작할 때 Web에서 플레이어 아이디 및 방 이름 가져오기
        #if !UNITY_EDITOR && UNITY_WEBGL
            TakeGameInfoFromReact();
        #endif

        // 포톤 서버 접속
        if (PhotonNetwork.IsConnected == false)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    #region WebGL 빌드 시 사용하는 함수들

    // Web에서 실행시킬 Unity 내의 함수
    public void ReceiveUnityGameInfo(string gameInfo)
    {
        // Debug.Log(gameInfo);
        GameInfo info = JsonConvert.DeserializeObject<GameInfo>(gameInfo);
        userIdWeb = info.userId;
        roomNameWeb = info.roomName;
        PlayerPrefs.SetString("gameMode", info.gameMode);
        Debug.Log(PlayerPrefs.GetString("gameMode"));
    }

    #endregion

    #region Window 혹은 Mac으로 빌드 시 사용하는 함수들
    // 유저명을 설정하는 로직
    public void SetUserId()
    {
        // 접속 유저의 닉네임 등록
        PhotonNetwork.NickName = userId;
    }

    #endregion

    #region 포톤 서버에서 활용하는 콜백 함수들

    // 포톤 서버에 접속 후 호출되는 콜백 함수
    public override void OnConnectedToMaster()
    {
        // Debug.Log("Connected to Master!");
        // Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby();
    }

    // 로비에 접속했을 때 실행되는 함수
    public override void OnJoinedLobby()
    {
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        PhotonNetwork.NickName = userIdWeb;

        #region // 게임 실행과 동시에 방 입장 테스트 코드
        SetUserId();

        // 룸의 속성 정의
        RoomOptions ro = new RoomOptions() { MaxPlayers = 2, IsOpen = true, IsVisible = true};
        PhotonNetwork.JoinOrCreateRoom(roomNameWeb, ro, null);
        #endregion
    }

    // 룸에 입장한 후 호출되는 콜백 함수
    public override void OnJoinedRoom()
    {
        Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        Debug.Log($"Player Count = {PhotonNetwork.CurrentRoom.PlayerCount}");

        // if (PhotonNetwork.IsMasterClient)
        // {
        //     PhotonNetwork.LoadLevel("Main");
        // }
    }

    #endregion
}