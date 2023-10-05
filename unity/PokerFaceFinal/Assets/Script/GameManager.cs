using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Game Object / 게임 내 오브젝트
    // 베팅 시작하는 버튼
    public Button startButton;
    // 게임 레디 버튼(게스트한테만 활성화)
    public Button readyButton;
    // 강제 추방 버튼(호스트한테만 활성화)
    public Button dropUserBtn;
    // 게임 시작 버튼(호스트한테만 활성화)
    public Button gameStartButton;
    // 방에서 나가는 버튼
    public Button exitBtn;
    // 턴을 시작하는 버튼
    public Button startBtn;
    // 게임 종료 시 나타나는 버튼
    public Button gameOverButton;
    // 게임 종료 시 나타나는 재시작 버튼
    public Button restartButton;

    // 베팅 버튼들이 모여있는 판낼
    public GameObject buttonSelcetionPanel;
    // 라운드 정보 판넬
    public GameObject roundInfoPanel;
    // 게임 종료 판넬
    public GameObject gameOverPanel;

    // 플레이어들의 카메라
    public Transform playerCamera;

    // 칩을 얼마나 베팅할 것인지 나타내는 슬라이더
    public Slider chipSlider;

    // 검은색 칩 프리팹
    public GameObject chipBlack;
    // 파란색 칩 프리팹
    public GameObject chipBlue;
    // 초록색 칩 프리팹
    public GameObject chipGreen;
    // 빨간색 칩 프리팹
    public GameObject chipRed;

    // 게임매니저에 붙는 포톤뷰
    public PhotonView pv;

    // 방 이름 텍스트
    public TMP_Text roomName;
    // 방 플레이어 수 확인 텍스트
    public TMP_Text roomPlayerCount;
    // 상대 플레이어 이름
    public TMP_Text enemyName;
    // 현재 몇 라운드인지 나타내는 텍스트
    public TMP_Text roundText;
    // 현재 플레이중인 플레이어
    public TMP_Text nowPlay;
    // 마지막으로 베팅된 것
    public TMP_Text lastBet;
    // 현재 남은 시간 출력
    public TMP_Text timer;
    // 현재 내가 가진 칩
    public TMP_Text myChipText;
    // 현재 상대방이 가진 칩
    public TMP_Text oppoChipText;
    // 현재 라운드에 걸려 있는 칩
    public TMP_Text totalBet;
    // 현재 라운드에 베팅된 칩 갯수
    public TMP_Text roundBetChipText;
    // 현재 내가 베팅한 칩 갯수
    public TMP_Text myBetChipText;
    // 게임 종료 안내 문구
    public TMP_Text gameOverText;
    // 게임 내에서 로그가 어떻게 쌓였는지 확인하는 텍스트
    public TMP_Text logCheckText;

    // 라운드 마다 칩을 생성하여 관리할 오브젝트
    public Transform roundEmptyObject;

    // 셔플된 카드를 넣을 덱
    public List<Card> deck;
    #endregion

    #region Private Class Variables / 클래스 내 변수
    
    #region NetworkManager를 위한 변수
    // 게임의 버전
    private readonly string version = "version 0.1.4";
    // 게임 외부 React에서 유저 아이디를 받아오는 함수
    [DllImport("__Internal")]
    private static extern void TakeGameInfoFromReact();

    // 룸 목록에 대한 데이터를 저장하기 위한 딕셔너리 자료형
    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    // 룸 목록을 표시할 프리팹
    private GameObject roomItemPrefab;
    
    //WebGL에서 사용할 방 이름과 플레이어 이름 저장 변수
    private string userIdWeb;
    private string roomNameWeb;

    #endregion

    #region GameManager를 위한 변수
    // 게임 내에서 유저 강제 퇴장 시 리액트에 알려주는 함수
    [DllImport("__Internal")]
    private static extern void UserDropOutToReact();
    // 게임 내에서 내가 게임을 종료 했을 경우에 리액트에 알려주는 함수
    [DllImport("__Internal")]
    private static extern void UserRoomOutToReact();
    // 게임 내에서 게임 종료 시 리액트에 알려주는 함수
    [DllImport("__Internal")]
    private static extern void GameOverToReact();
    // 게임 내에서 라운드 시작 시 리액트에 감정분석 시작하라고 알려주는 함수
    [DllImport("__Internal")]
    private static extern void StartFaceAPIToReact();

    // 현재 게임 타입
    private string gameMode;
    // 내 플레이어 타입
    private string myPlayerType;
    // 게임이 진행된 라운드
    private int round;
    // 게임 종료 시 보내줘야 할 토큰
    private string sendToReactToken;
    // 게임 종료 리턴 확인
    private bool isResponse;

    // 카드 덱
    private CardDeck cardDeck;

    // 호스트 플레이어
    private CardPlayer hostPlayer;
    // 게스트 플레이어
    private CardPlayer guestPlayer;

    // 현재 라운드에 베팅된 칩
    private int nowRoundBet;
    // 현재 라운드에 내가 베팅한 칩
    private int nowMyBet;

    // 라운드 시작 플레이어
    private string firstBet;
    // 시작 베팅 칩
    private int startBetChip;
    // 내가 갖고 있는 칩
    private int myChip;
    // 전체 놓여 있는 칩
    private int allPlayerChip;

    // UI Ctrl
    private UICtrl uICtrl;
    // 현재 턴이 나의 턴인지
    private bool isMyTurn;
    // 무승부인지 확인하기
    private bool isDraw;
    // 라운드 결과
    private string roundResult;
    // 게임 결과
    private string gameResult;

    // 게임 내 진행 로그를 쌓는 변수
    private string gamePlayLog;
    // 현재 턴 수
    private int nowTurn;

    // 감정 분석이 완료 됐는지 확인
    private bool isFaceDection;
    // 현재 라운드가 종료됐는지 확인
    private bool isRoundActive;
    // 현재 턴이 종료됐는지 확인
    private bool isTurnActive;
    // RPC 함수 호출 여부 확인
    private bool callRPC;
    #endregion

    #endregion

    #region Unity Life Cycle / 유니티 라이프 사이클
    // 이 스크립트가 실행되고 가장 먼저 실행되는 함수
    private void Awake()
    {
        // 60 frame으로 고정
        Application.targetFrameRate = 60;

        // vSync off
        QualitySettings.vSyncCount = 0;

        // 마스터 클라이언트의 씬 자동 동기화 옵션
        PhotonNetwork.AutomaticallySyncScene = true;
        // 게임 버전 설정
        PhotonNetwork.GameVersion = version;

        // 인게임 설정
        Resources.UnloadUnusedAssets();
        PlayerPrefs.DeleteAll();
        roundEmptyObject = new GameObject("EmptyObject").transform;
        uICtrl = GetComponent<UICtrl>();

        // 버튼에 함수 연결
        exitBtn.onClick.AddListener(() => OnExitClick());
        gameOverButton.onClick.AddListener(() => OnExitClick());
        restartButton.onClick.AddListener(() => OnExitClick());
        dropUserBtn.onClick.AddListener(() => HostDropGuest());
    }

    // Awake 다음으로 실행되는 함수
    private void Start()
    {
        // 시작할 때 Web에서 플레이어 아이디 및 방 이름 가져오기
        #if !UNITY_EDITOR && UNITY_WEBGL
            TakeGameInfoFromReact();
        #endif

        userIdWeb = "player";
        roomNameWeb = "한글 방제목 테스트";

        // 포톤 서버 접속
        if (PhotonNetwork.IsConnected == false)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    #endregion

    #region GameManager Part / 게임 매니저와 관련된 함수

    #region Game Waiting / 게임 준비 단계
    // 게임 초기화
    private void GameReset()
    {
        StopAllCoroutines();
        
        // 오브젝트 초기화
        for (int i = roundEmptyObject.childCount - 1; i >= 0; i--)
        {
            Destroy(roundEmptyObject.GetChild(i).gameObject);
        }

        if (hostPlayer != null)
        {
            if (hostPlayer.showCard != null)
            {
                hostPlayer.DestroyMadeCard();
            }
            Destroy(hostPlayer);
        }

        if (guestPlayer != null)
        {
            if (guestPlayer.showCard != null)
            {
                guestPlayer.DestroyMadeCard();
            }
            Destroy(guestPlayer);
        }
        
        // UI 초기화
        buttonSelcetionPanel.SetActive(false);
        roundInfoPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        exitBtn.gameObject.SetActive(true);
        gameStartButton.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        myChipText.text = "My Chip : ";
        oppoChipText.text = "Enemy Chip : ";
        nowPlay.text = "Now Play : ";

        // 덱 초기화
        deck = new List<Card>();

        // 변수 초기화
        firstBet = "host";
        isDraw = false;
        myChip = 0;
        nowMyBet = 0;
        nowRoundBet = 0;
        startBetChip = 1;
        gamePlayLog = "";
    }

    // 게임 시작 준비
    private void InitialSetting()
    {
        // Debug.Log("InitialSetting");
        GameReset();

        // 만약 내가 방장이라면 내 플레이어 타입을 host로 함.
        if (PhotonNetwork.IsMasterClient)
        {
            myPlayerType = "host";
            gameStartButton.gameObject.SetActive(true);
            gameStartButton.interactable = false;
            dropUserBtn.interactable = true;
            isMyTurn = true;
        }
        else
        {
            myPlayerType = "guest";
            readyButton.gameObject.SetActive(true);
            dropUserBtn.interactable = false;
            isMyTurn = false;
        }
    }

    // 카드 세팅
    private void CardSetting()
    {
        // Debug.Log("CardSetting");
        for (int i=0; i<20; i++)
        {
            Card card = new (i);
            card.SetCard();
            deck.Add(card);
        }
        Shuffle(deck);
    }

    // 카드 셔플 알고리즘
    private void Shuffle(List<Card> deck)
    {
        int rand1;
        int rand2;

        // int n = deck.Count;
        // while (n > 1)
        // {
        //     n--;
        //     int k = Random.Range(0, n+1);
        //     (deck[k], deck[n]) = (deck[n], deck[k]);
        // }

        for (int i=0; i<deck.Count; i++)
        {
            rand1 = Random.Range(0, deck.Count);
            rand2 = Random.Range(0, deck.Count);

            (deck[rand1], deck[rand2]) = (deck[rand2], deck[rand1]);
        }
    }

    // 상대 유저에게 덱 전송
    private void SendList()
    {
        string cards = deck[0].GetCardType().ToString();
        int a = 0;
        while (a < 19)
        {
            a++;
            cards += "," + deck[a].GetCardType().ToString();
        }
        pv.RPC("SendDeckToOtherPlayer", RpcTarget.Others, cards);
    }

    // 유저가 방에 들어왔을 때 각각의 유저들을 생성시켜주는 함수
    private void CreatePlayer()
    {
        // 출현 위치 정보를 배열에 저장
        Transform[] points = GameObject.Find("Spawn").GetComponentsInChildren<Transform>();
        int idx = PhotonNetwork.CurrentRoom.PlayerCount;

        // 네트워크상에 캐릭터 생성
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject hostPlayerGameObject = PhotonNetwork.Instantiate("CardPlayerEmpty", points[idx].position, Quaternion.Euler(0, 180, 0), 0);
            hostPlayer = hostPlayerGameObject.GetComponent<CardPlayer>();
            hostPlayer.SetUserId(PhotonNetwork.NickName);
            // Debug.Log("host : " + hostPlayer.GetUserId());
            playerCamera.position = new Vector3(-41, 31, -15);
            playerCamera.rotation = Quaternion.Euler(7.274f, 0, 0);
        }
        else
        {
            GameObject guestPlayerGameObject = PhotonNetwork.Instantiate("CardPlayerEmpty", points[idx].position, points[idx].rotation, 0);
            guestPlayer = guestPlayerGameObject.GetComponent<CardPlayer>();
            guestPlayer.SetUserId(PhotonNetwork.NickName);
            // Debug.Log("guest : " + guestPlayer.GetUserId());
            playerCamera.position = new Vector3(-41, 31, 54);
            playerCamera.rotation = Quaternion.Euler(7.274f, 180, 0);
        }
    }

    // 게스트 준비 완료
    private void GuestReady()
    {
        // Debug.Log("GuestReady");
        readyButton.interactable = false;
        pv.RPC("HostGameStart", RpcTarget.Others, null);
    }

    // RPC 통신을 통해서 양측 플레이어에게 게임 시작을 알리는 함수
    private void OnClickGameStart() 
    {
        // Debug.Log("OnClickGameStart");
        pv.RPC("GameStart", RpcTarget.All, null);
    }
    
    // 게임이 시작되고 실행되는 함수
    private void StartGame()
    {
        gameMode = PlayerPrefs.GetString("gameMode");
        // gameMode = "NORMAL";
        // Debug.Log(gameMode);

        // UI 세팅
        dropUserBtn.gameObject.SetActive(false);
        gameStartButton.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
        roundInfoPanel.SetActive(true);

        // 게임 로그 작성 시작
        round = 0;

        // 현재 존재하는 플레이어 정보 저장
        CardPlayer[] nowPlayer = GameObject.FindObjectsOfType<CardPlayer>();
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (CardPlayer cardplayer in nowPlayer)
            {
                if (!cardplayer.pv.IsMine)
                {
                    guestPlayer = cardplayer;
                }
            }
            foreach(var player in PhotonNetwork.CurrentRoom.Players)
            {
                if (!player.Value.NickName.Equals(PhotonNetwork.NickName))
                {
                    guestPlayer.SetUserId(player.Value.NickName);
                    // Debug.Log("guest : " + guestPlayer.GetUserId());
                    enemyName.text = player.Value.NickName;
                }
            }
        }
        else
        {
            foreach (CardPlayer cardplayer in nowPlayer)
            {
                if (!cardplayer.pv.IsMine)
                {
                    hostPlayer = cardplayer;
                }
            }
            foreach(var player in PhotonNetwork.CurrentRoom.Players)
            {
                if (!player.Value.NickName.Equals(PhotonNetwork.NickName))
                {
                    hostPlayer.SetUserId(photonView.Owner.NickName);
                    // Debug.Log("host : " + hostPlayer.GetUserId());
                    enemyName.text = player.Value.NickName;
                }
            }
        }

        // 현재 존재하는 플레이어 정보 수정
        hostPlayer.SetChips(30);
        guestPlayer.SetChips(30);

        isRoundActive = false;
        callRPC = false;
        // 만약 내가 맨 처음 베팅하는 플레이어면 RPC 호출
        if (isMyTurn)
        {
            pv.RPC("RoundStart", RpcTarget.All, firstBet);
        }
    }

    // 방장이 누를 경우 게스트를 강제 퇴장 시키는 함수
    private void HostDropGuest()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("DropUserRPC", RpcTarget.Others, null);
        }
    }
    #endregion
    
    #region Game Turn / 게임 턴 단계
    // 라운드 시작 전 감정분석 완료 확인 함수
    private void FinishFaceDetection()
    {
        isFaceDection = true;
    }

    // 라운드 마다 변수 초기화
    private IEnumerator RoundReset(string firstBetPlayer)
    {
        // 현재 라운드 표시
        round += 1;
        nowTurn = 0;
        roundText.text = "Round " + round.ToString();

        // 현재 라운드가 *1라운드 라면 카드 덱 세팅
        if (round%10 == 1)
        {
            if (round/10 > 0)
            {
                startBetChip <<= 1;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                CardSetting();
                SendList();
            }

            GameObject objectDeck = Instantiate(Resources.Load<GameObject>("Card_Deck"), new Vector3(-58, 18, 19.2f), Quaternion.Euler(90, -90, 0));
            CardDeck cardDeck = objectDeck.GetComponent<CardDeck>();
            yield return StartCoroutine(cardDeck.Actvate());
        }

        yield return new WaitForSeconds(0.5f);

        // 플레이어가 지닌 칩의 갯수 다시 정리
        SetPlayerChip();

        // 생성된 칩 오브젝트 삭제
        if (!isDraw && roundEmptyObject.childCount != 0)
        {
            for (int i = roundEmptyObject.childCount - 1; i >= 0; i--)
            {
                Destroy(roundEmptyObject.GetChild(i).gameObject);
            }
        }

        // 선공 정하기
        if (myPlayerType.Equals(firstBetPlayer)) 
        {
            startButton.interactable = true;
            isMyTurn = false;
        }
        else
        {
            startButton.interactable = false;
            isMyTurn = true; 
        }

        // 카드 배분
        DealCard();
        yield return StartCoroutine(CardCreate());

        switch (myPlayerType)
        {
            case "host":
                gamePlayLog += "#" + firstBetPlayer + "$" + hostPlayer.GetCardType() + "$" + guestPlayer.GetCardType() + "$" + hostPlayer.GetChips() + "$" + guestPlayer.GetChips();
            break;
        }

        yield return new WaitForSeconds(0.5f);
        
        if (!isDraw)
        {
            int roundChip = Mathf.Min(Mathf.Min(hostPlayer.GetChips(), guestPlayer.GetChips()), startBetChip);
            yield return StartCoroutine(ChipCreate("start", "", roundChip, roundChip));
            gamePlayLog += "$" + roundChip * 2 + "$";
        }
        else
        {
            gamePlayLog += "$" + allPlayerChip + "$";
        }
        logCheckText.text = gamePlayLog;
        yield return new WaitForSeconds(0.5f);


        isFaceDection = false;
        if (gameMode.Equals("EMOTION"))
        {
            #if !UNITY_EDITOR && UNITY_WEBGL
                isFaceDection = false;
                StartFaceAPIToReact();
                yield return new WaitUntil(() => isFaceDection == true);
            #endif
        }

        // 턴 시작
        if(!isMyTurn)
        {
            isTurnActive = false;
            callRPC = false;
            while (!isTurnActive)
            {
                pv.RPC("CheckTurnStart", RpcTarget.Others, null);
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitUntil(() => isTurnActive == true);
            pv.RPC("TurnStart", RpcTarget.All, null);
        }
        else
        {
            isTurnActive = false;
            callRPC = false;
            yield return new WaitUntil(() => callRPC == true);
        }
    }

    // 플레이어의 턴 코루틴 함수
    private IEnumerator PlayerTurn()
    {
        // Debug.Log("PlayerTurn");
        // UI 초기화
        uICtrl.UIReset();
        // 베팅 버튼 판넬 활성화
        buttonSelcetionPanel.SetActive(true);
        // 베팅 시작 버튼 상호작용 불가능 설정
        startButton.interactable = false;

        yield return new WaitForSeconds(0.5f);

        nowTurn += 1;
        if(isMyTurn)
        {
            // Debug.Log(myPlayerType + "'s turn started");
            startButton.interactable = true;
            Button checkButton = uICtrl.CheckButton.GetComponent<Button>();
            Button callButton = uICtrl.CallButton.GetComponent<Button>();
            checkButton.gameObject.SetActive(false);
            callButton.gameObject.SetActive(false);
            roundText.text = "Round " + round + " - " + nowTurn;
            nowPlay.text = "Now Play : Me";
            SetClickCheckOrCallButton();
            SetClickBetButton();
        }
        else
        {
            startButton.interactable = false;
            roundText.text = "Round " + round + " - " + nowTurn;
            nowPlay.text = "Now Play : Enemy";
            // Debug.Log(myPlayerType + " waiting");
        }

        int time = 20;
        while (time > 0)
        {
            timer.text = "Last Time : " + time + "s";
            yield return new WaitForSeconds(1f);
            time--;
        }

        OnClickFoldButton();
        yield return new WaitForSeconds(1f);
    }

    // 카드 분배
    private void DealCard()
    {
        // Debug.Log("DealCard");
        Card hostCard = deck.Last();
        deck.RemoveAt(deck.Count-1);
        Card guestCard = deck.Last();
        deck.RemoveAt(deck.Count-1);
        hostPlayer.AddCard(hostCard);
        guestPlayer.AddCard(guestCard);
    }

    // 카드 생성 애니메이션
    private IEnumerator CardCreate()
    {
        // Debug.Log("CardCreate");
        if (hostPlayer.showCard != null)
        {
            hostPlayer.DestroyMadeCard();
        }
        if (guestPlayer.showCard != null)
        {
            guestPlayer.DestroyMadeCard();
        }

        // hostPlayer.CreateCard(guestCard.GetCardInfo(), new Vector3(-10, 33, 40), Quaternion.Euler(0, 180, 0));
        // guestPlayer.CreateCard(hostCard.GetCardInfo(), new Vector3(-70, 33, 0), Quaternion.Euler(0, 0, 0));
        StartCoroutine(hostPlayer.CreateCard(guestPlayer.GetCardInfo(), new Vector3(-58, 16.9f, 19.2f), Quaternion.Euler(90, 90, 0), "guest"));
        StartCoroutine(guestPlayer.CreateCard(hostPlayer.GetCardInfo(), new Vector3(-58, 16.9f, 19.2f), Quaternion.Euler(90, 90, 0), "host"));
        yield return new WaitForSeconds(1f);
    }

    // UI의 칩 갯수를 수정하는 함수
    private void SetPlayerChip()
    {
        // Debug.Log("SetPlayerChip");
        switch(myPlayerType)
        {
            case "host":
                myChipText.text = "My Chip : " + hostPlayer.GetChips();
                oppoChipText.text = "Enemy Chip : " + guestPlayer.GetChips();
            break;

            case "guest":
                myChipText.text = "My Chip : " + guestPlayer.GetChips();
                oppoChipText.text = "Enemy Chip : " + hostPlayer.GetChips();
            break;
        }
        totalBet.text = "Total Bet : " + allPlayerChip;
        roundBetChipText.text = "Round Bet : " + nowRoundBet;
        myBetChipText.text = "My Bet : " + nowMyBet;
    }

    // 베팅 슬라이더 세팅
    private void SetBetSlider()
    {
        // Debug.Log("SetBetSlider");
        int enemyChip = 0;
        switch(myPlayerType)
        {
            case "host":
                enemyChip = guestPlayer.GetChips();
            break;

            case "guest":
                enemyChip = hostPlayer.GetChips();
            break;
        }
        chipSlider.minValue = nowRoundBet - nowMyBet + 1;
        chipSlider.value = chipSlider.minValue;
        chipSlider.maxValue = myChip > nowRoundBet - nowMyBet + enemyChip ? nowRoundBet - nowMyBet + enemyChip : myChip;
    }

    // 콜 버튼 세팅
    private void SetClickCheckOrCallButton()
    {
        // Debug.Log("SetClickCallButton");
        Button callButton = uICtrl.CallButton.GetComponent<Button>();
        Button cancleButton = uICtrl.BetCancelButton.GetComponent<Button>();
        // 내가 가진 칩이 현재 베팅 칩 보다 많으면 내 이전 베팅 칩과 현재 베팅 칩의 갯수를 맞춘 뒤 라운드를 종료한다.
        if (nowTurn == 1)
        {
            // Debug.Log("Check");
            startBtn.onClick.AddListener(() => uICtrl.OnClickStartToCheck());
            cancleButton.onClick.AddListener(() => uICtrl.OnClickBetCancelButtonCheck());
        }
        else
        {
            if (myChip >= (nowRoundBet - nowMyBet))
            {
                // Debug.Log("Call");
                callButton.interactable = true;
                startBtn.onClick.AddListener(() => uICtrl.OnClickStartToCall());
                cancleButton.onClick.AddListener(() => uICtrl.OnClickBetCancelButtonCall());
            }
            else
            {
                callButton.interactable = false;
                startBtn.onClick.AddListener(() => uICtrl.OnClickStartToCall());
                cancleButton.onClick.AddListener(() => uICtrl.OnClickBetCancelButtonCall());
            }
        }
    }

    // 베팅 버튼 세팅
    private void SetClickBetButton()
    {
        // Debug.Log("SetClickBetButton");
        int enemyChip = 0;
        switch(myPlayerType)
        {
            case "host":
                enemyChip = guestPlayer.GetChips();
            break;

            case "guest":
                enemyChip = hostPlayer.GetChips();
            break;
        }
        // 내가 가진 칩이 현재 베팅 칩 보다 많으면 베팅이 가능하며 내가 가진 칩이 현재 베팅 칩보다 적으면 비활성화 된다.
        Button betButton = uICtrl.BetButton.GetComponent<Button>();
        if (myChip > (nowRoundBet - nowMyBet) && enemyChip != 0)
        {
            // Debug.Log("Round : " + round + " NowTurn : " + nowTurn + " BetButton Interactable : true");
            betButton.interactable = true;
            SetBetSlider();
        }
        else
        {
            // Debug.Log("Round : " + round + " NowTurn : " + nowTurn + " BetButton Interactable : false");
            betButton.interactable = false;
        }
    }

    // 체크 버튼 클릭
    public void OnClickCheckButton()
    {
        // Debug.Log("OnClickCheckButton");
        // 맨 처음 턴일 때 체크를 할 수 있음.
        uICtrl.OnclickTurnEnd();
        pv.RPC("EndRound", RpcTarget.All, myPlayerType, "Check", 0, nowRoundBet);
    }

    // 콜 버튼 클릭
    public void OnClickCallButton()
    {
        // Debug.Log("OnClickCallButton");
        // 상대가 베팅한 금액에 맞춰 칩을 낸 후 라운드를 종료한다.
        uICtrl.OnclickTurnEnd();
        int callBet = nowRoundBet - nowMyBet;
        myChip -= callBet;
        nowMyBet += callBet;
        pv.RPC("EndRound", RpcTarget.All, myPlayerType, "Call", callBet, nowMyBet);
    }

    // 폴드 버튼 클릭
    public void OnClickFoldButton()
    {
        // Debug.Log("OnClickFoldButton");
        // 내가 가진 칩에 상관없이 현재 베팅에서 라운드를 종료한다.
        uICtrl.OnclickTurnEnd();
        pv.RPC("EndRound", RpcTarget.All, myPlayerType, "Fold", 0, nowRoundBet);
    }

    // 베팅 버튼 클릭
    private void OnClickBetButton()
    {
        // Debug.Log("OnClickBetButton");
        // 내가 낼 수 있는 칩의 한도 내에서 베팅을 진행하고 다른 플레이어에게 턴을 넘긴다.
        int betChip = (int)chipSlider.value;
        // Debug.Log(myPlayerType + ", bet chip : " + betChip);
        myChip -= betChip;
        nowMyBet += betChip;
        startButton.interactable = false;
        pv.RPC("EndRound", RpcTarget.All, myPlayerType, "Bet", betChip, nowMyBet);
    }
    
    // 칩 생성
    private IEnumerator ChipCreate(string ctrlType, string betType, int chips, int roundBet)
    {
        // Debug.Log("Chip Create " + ctrlType + " BetType : " + betType + " chips : " + chips + " roundBet : " + roundBet);
        if(!ctrlType.Equals("start")){
            gamePlayLog += betType + "-" + chips + ",";
            logCheckText.text = gamePlayLog;
        }

        switch (ctrlType)
        {
            // 라운드 시작 시 칩 갯수 설정
            case "start":
                hostPlayer.UpdateChips(chips * -1);
                guestPlayer.UpdateChips(chips * -1);
                switch (myPlayerType)
                {
                    case "host":
                        myChip = hostPlayer.GetChips();
                        nowMyBet = chips;
                    break;
                    case "guest":
                        myChip = guestPlayer.GetChips();
                        nowMyBet = chips;
                    break;
                }
                nowRoundBet = roundBet;
                chips *= 2;
                allPlayerChip += chips;
            break;

            // 베팅한 사람이 호스트일 때
            case "host":
                hostPlayer.UpdateChips(chips * -1);
                nowRoundBet = roundBet;
                allPlayerChip += chips;
            break;

            // 베팅한 사람이 게스트일 때
            case "guest":
                guestPlayer.UpdateChips(chips * -1);
                nowRoundBet = roundBet;
                allPlayerChip += chips;
            break;
        }

        // 현재의 칩 갯수 만큼 가운데에 칩 생성
        for(int i = 0; i < chips; i++) {
            int x = Random.Range(-44, -36);
            int z = Random.Range(15, 23);
            int chip = Random.Range(0, 4);

            switch(chip) {
                case 0:
                    GameObject blackChip = Instantiate(chipBlack, new Vector3(x, 23, z), Quaternion.identity);
                    blackChip.transform.SetParent(roundEmptyObject.transform);
                break;

                case 1:
                    GameObject blueChip = Instantiate(chipBlue, new Vector3(x, 23, z), Quaternion.identity);
                    blueChip.transform.SetParent(roundEmptyObject.transform);
                break;

                case 2:
                    GameObject greenChip = Instantiate(chipGreen, new Vector3(x, 23, z), Quaternion.identity);
                    greenChip.transform.SetParent(roundEmptyObject.transform);
                break;

                case 3:
                    GameObject redChip = Instantiate(chipRed, new Vector3(x, 23, z), Quaternion.identity);
                    redChip.transform.SetParent(roundEmptyObject.transform);
                break;
            }
        }
        
        // 변경된 칩들 UI 재설정
        SetPlayerChip();

        // Debug.Log("Chip Create End");
        yield return new WaitForSeconds(0.5f);
        // Debug.Log(myPlayerType + " 칩 생성 중 isMyturn : " + isMyTurn);

        if (ctrlType.Equals("start"))
        {
            // Debug.Log("Chip Create If Start");
            yield return new WaitForSeconds(1f);
        }
        else
        {
            // Debug.Log("Chip Create Else");
            if(betType.Equals("Bet") || betType.Equals("Check"))
            {
                if(isMyTurn)
                {
                    isTurnActive = false;
                    callRPC = false;
                    while (!isTurnActive)
                    {
                        pv.RPC("CheckTurnStart", RpcTarget.Others, null);
                        yield return new WaitForSeconds(1f);
                    }
                    yield return new WaitUntil(() => isTurnActive == true);
                    pv.RPC("TurnStart", RpcTarget.All, null);
                }
                else
                {
                    isTurnActive = false;
                    callRPC = false;
                    yield return new WaitUntil(() => callRPC == true);
                    pv.RPC("CanTurnStart", RpcTarget.Others, null);
                }
            }
        }
    }

    #endregion

    #region Game Result / 게임 결과 단계
    // 폴드 하였을 경우 누가 승자인지 판별
    private string CheckFoldWinner()
    {
        // Debug.Log("CheckFoldWinner");
        string result = "";
        // 내 턴일 때 폴드를 했을 경우
        if (isMyTurn)
        {
            // 내가 호스트이면 라운드 시작을 게스트가 내가 게스트이면 라운드 시작을 호스트가
            switch(myPlayerType)
            {
                case "host":
                    result = "guest";
                break;

                case "guest":
                    result = "host";
                break;
            }
            roundResult = "lose";
        }

        // 상대의 턴일 때 폴드를 했을 경우
        else
        {
            result = myPlayerType;
            roundResult = "win";
        }
        return result;
    }

    // 콜을 하였을 경우 누가 승자인지 판별
    private string CheckCallWinner()
    {
        // Debug.Log("CheckCallWinner");
        int hostNum = (hostPlayer.GetCardType() % 10) + 1;
        int guestNum = (guestPlayer.GetCardType() % 10) + 1;

        // Debug.Log("Host : " + hostNum + "Guest : " + guestNum);
        string result = "";

        // 만약 두 플레이어의 카드 숫자가 같다면 무승부
        if (hostNum == guestNum)
        {
            roundResult = "draw";
            // Debug.Log("draw");
            return firstBet;
        }

        switch (myPlayerType)
        {
            // 만약 내가 호스트이고 내 숫자가 크면 호스트 승리 아니면 게스트 승리
            case "host":
                if (hostNum > guestNum)
                {
                    roundResult = "win";
                    // Debug.Log("host win");
                    result = "host";
                }
                else
                {
                    roundResult = "lose";
                    // Debug.Log("host lose");
                    result = "guest";
                }
            break;

            // 만약 내가 게스트이고 내 숫자가 크면 게스트 승리 아니면 호스트 승리
            case "guest":
                if (hostNum > guestNum)
                {
                    roundResult = "lose";
                    // Debug.Log("guest lose");
                    result = "host";
                }
                else
                {
                    roundResult = "win";
                    // Debug.Log("guest win");
                    result = "guest";
                }
            break;
        }
        return result;
    }

    // 라운드 승자 체크 후 승자에게 칩 전송
    private void CheckRoundWinner()
    {
        // Debug.Log("CheckRoundWinner");
        // 나의 경기 결과를 통해서 베팅 칩 여부 결정
        switch (roundResult)
        {
            // 내 상태가 승리일 때
            case "win":
                // 내가 호스트라면 호스트에게 전체 걸린 칩 전송
                switch(myPlayerType)
                {
                    case "host":
                        hostPlayer.UpdateChips(allPlayerChip);
                    break;

                    case "guest":
                        guestPlayer.UpdateChips(allPlayerChip);
                    break;
                }
                allPlayerChip = 0;
                gamePlayLog += "$WIN";
                // Debug.Log(gamePlayLog);
                isDraw = false;
            break;

            // 내 상태가 무승부일 때
            case "draw":
                gamePlayLog += "$DRAW";
                // Debug.Log(gamePlayLog);
                isDraw = true;
            break;

            // 내 상태가 패배일 때
            case "lose":
                // 내가 호스트라면 게스트에게 전체 걸린 칩 전송
                switch(myPlayerType)
                {
                    case "host":
                        guestPlayer.UpdateChips(allPlayerChip);
                    break;
                    case "guest":
                        hostPlayer.UpdateChips(allPlayerChip);
                    break;
                }
                allPlayerChip = 0;
                gamePlayLog += "$LOSE";
                // Debug.Log(gamePlayLog);
                isDraw = false;
            break;
        }

        // 칩 정리 이후 칩 UI 재설정
        SetPlayerChip();
    }

    // 라운드 종료 후 카드 체크
    private IEnumerator CheckRoundCard(string ctrlType, string betType, int chips, int roundBet)
    {
        // Debug.Log("CheckRoundCard");
        switch (betType)
        {
            case "Call":
                // Debug.Log("Call CheckRoundCard");
                yield return StartCoroutine(ChipCreate(ctrlType, betType, chips, roundBet));
                firstBet = CheckCallWinner();
                // Debug.Log(firstBet);
            break;

            case "Fold":
                gamePlayLog += betType + "-0";
                firstBet = CheckFoldWinner();
            break;
        }

        SetPlayerChip();

        yield return new WaitForSeconds(0.5f);
        // 서로의 카드를 가운데로 가져와 확인하여 승패를 가른다.
        switch(myPlayerType)
        {
            case "host":
                // Debug.Log(hostPlayer.showCard);
                StartCoroutine(hostPlayer.CheckCard("hostguest"));
                StartCoroutine(guestPlayer.CheckCard("hosthost"));
            break;

            case "guest":
                // Debug.Log(hostPlayer.showCard);
                StartCoroutine(hostPlayer.CheckCard("guestguest"));
                StartCoroutine(guestPlayer.CheckCard("guesthost"));
            break;
        }

        yield return new WaitForSeconds(3f);

        // 누가 라운드의 승자인지 확인
        CheckRoundWinner();

        // 게임이 종료되었다면 승자 체크하기 아니면 라운드 시작인 플레이어로 다시 새 라운드 시작
        if (!isDraw && CheckGameOver())
        {
            // Debug.Log(gamePlayLog);

            roundInfoPanel.SetActive(false);
            gameOverPanel.SetActive(true);
            exitBtn.gameObject.SetActive(true);
            gameOverText.text = gameResult;
            // Debug.Log(gameOverText);
            if (PhotonNetwork.IsMasterClient)
            {
                #if !UNITY_EDITOR && UNITY_WEBGL
                    isResponse = false;
                    GameOverToReact();
                #endif
                yield return StartCoroutine(PostData());
            }
            yield return new WaitForSeconds(1f);
        }
        else
        {
            // Debug.Log(myPlayerType + isMyTurn);
            if(isMyTurn)
            {   
                isRoundActive = false;
                callRPC = false;
                while (!isRoundActive)
                {
                    pv.RPC("CheckTurnStart", RpcTarget.Others, null);
                    yield return new WaitForSeconds(1f);
                }
                yield return new WaitUntil(() => isRoundActive == true);
                pv.RPC("RoundStart", RpcTarget.All, firstBet);
            }
            else
            {
                isRoundActive = false;
                callRPC = false;
                yield return new WaitUntil(() => callRPC == true);
                pv.RPC("CanRoundStart", RpcTarget.Others, null);
            }
        }
    }

    // 게임종료 확인
    private bool CheckGameOver()
    {
        // Debug.Log("CheckGameOver");
        if ((hostPlayer.GetChips() == 0) || (guestPlayer.GetChips() == 0))
        {
            switch (myPlayerType)
            {
                case "host":
                    if (hostPlayer.GetChips() == 0)
                    {
                        gameResult = "LOSE";
                    }
                    else
                    {
                        gameResult = "WIN";
                    }
                break;

                case "guest":
                    if (guestPlayer.GetChips() == 0)
                    {
                        gameResult = "LOSE";
                    }
                    else
                    {
                        gameResult = "WIN";
                    }
                break;
            }
            return true;
        }
        return false;
    }

    // 게임 종료 시 토큰 설정
    private void SetAccessToken(string token)
    {
        // Debug.Log("SetAccessToken");
        isResponse = true;
        sendToReactToken = token;
        // Debug.Log(token);
    }

    // 게임 종료 후 백엔드로 게임 로그 전송
    private IEnumerator PostData()
    {
        // Debug.Log("PostData");
        #if !UNITY_EDITOR && UNITY_WEBGL
            // Debug.Log("Get Token");
            yield return new WaitUntil(() => isResponse == true);
        #endif

        string Url = "https://pokerface-server.ddns.net/api/histories";
        // string a = "jungjun@ssafy.com";
        // string b = "daeyoung@ssafy.com";
        string PostData = "{ \"hostEmail\": \"" + hostPlayer.GetUserId() + "\", \"guestEmail\": \"" + guestPlayer.GetUserId() +"\", \"gameMode\": \"" + gameMode + "\" , \"gameLog\": \"" + gamePlayLog + "\", \"result\": \"" + gameResult + "\"}";
        // string PostData = "{ \"hostEmail\": \"" + a + "\", \"guestEmail\": \"" + b +"\", \"gameMode\": \"" + gameMode + "\" , \"gameLog\": \"" + gamePlayLog + "\", \"result\": \"" + gameResult + "\"}";
        Debug.Log(PostData);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(PostData);

        UnityWebRequest postRequest = new UnityWebRequest(Url, UnityWebRequest.kHttpVerbPOST);
        postRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
        postRequest.downloadHandler = new DownloadHandlerBuffer();
        postRequest.SetRequestHeader("Content-Type", "application/json"); // JSON 형식으로 요청 보낼 경우
        // postRequest.SetRequestHeader("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJBY2Nlc3NUb2tlbiIsIm5pY2tuYW1lIjoi7J207KCV7KSAIiwiZXhwIjoxNjkyNzUxNjIwLCJlbWFpbCI6Imp1bmdqdW5Ac3NhZnkuY29tIn0.D5_F8aPzgU--iVXSG7U4750uhZASIDtUvyJgV2C3i80Eigfuq2zLA7TmC5943zS340JPl0i0ym5QSnNXFBedSg");
        postRequest.SetRequestHeader("Authorization", sendToReactToken);

        // Debug.Log("Before send");
        yield return postRequest.SendWebRequest();

        if (postRequest.result != UnityWebRequest.Result.Success)
        {
            // Debug.Log(postRequest.result);
            // Debug.LogError("Error: " + postRequest.error);
            // Debug.LogError("Response Code: " + postRequest.responseCode);
            // Debug.LogError("Error Response Body: " + postRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log("Form upload complete");
        }
    }
    #endregion

    #region Photon RPC Function / 포톤 rpc 함수들
    // 호스트가 게임 시작 전에 게스트 플레이어를 강제로 추방시키는 함수
    [PunRPC]
    public void DropUserRPC()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
            UserDropOutToReact();
        #endif
        // Debug.Log("OnExitClick");
        GameReset();
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }

    // 호스트가 게임을 시작할 때 호출하는 함수
    [PunRPC]
    public void HostGameStart()
    {
        // Debug.Log("HostGameStart");
        gameStartButton.interactable = true;
    }

    // 모든 플레이어에게 게임이 시작되었음을 알리고 게임 중의 오브젝트를 활성화 시키는 함수
    [PunRPC]
    public void GameStart()
    {
        StartGame();
    }
    
    // 한 플레이어가 제작한 덱을 다른 플레이어에게 전송하는 함수
    [PunRPC]
    public void SendDeckToOtherPlayer(string cards)
    {
        // Debug.Log("SendDeckToOtherPlayer start");
        string cardInfo = "";
        string[] cardNums = cards.Split(",");
        for (int i=0; i < cardNums.Length; i++)
        {
            Card card = new (int.Parse(cardNums[i]));
            card.SetCard();
            deck.Add(card);
            cardInfo += card.GetCardInfo() + " ";
        }
    }

    // 라운드를 시작할 수 있는지 체크
    [PunRPC]
    public void CheckRoundStart()
    {
        // Debug.Log("CheckRoundStart");
        callRPC = true;
        pv.RPC("CanRoundStart", RpcTarget.Others, null);
    }

    // 라운드를 시작 가능 전송
    [PunRPC]
    public void CanRoundStart()
    {
        // Debug.Log("CanRoundStart");
        isRoundActive = true;
    }

    // 라운드 시작
    [PunRPC]
    public void RoundStart(string firstBetPlayer)   
    {
        // Debug.Log("RoundStart");
        StartCoroutine(RoundReset(firstBetPlayer));
    }

    // 턴을 시작할 수 있는지 체크
    [PunRPC]
    public void CheckTurnStart()
    {
        // Debug.Log("CheckTurnStart");
        callRPC = true;
        pv.RPC("CanTurnStart", RpcTarget.Others, null);
    }

    // 턴을 시작 가능 전송
    [PunRPC]
    public void CanTurnStart()
    {
        // Debug.Log("CanTurnStart");
        isTurnActive = true;
    }

    // 턴 시작
    [PunRPC]
    public void TurnStart()
    {
        // Debug.Log("TurnStart");
        isMyTurn = !isMyTurn;
        StartCoroutine(PlayerTurn());
    }

    // 라운드 종료 시 호출하는 함수
    [PunRPC]
    public void EndRound(string ctrlType, string betType, int chips, int roundBet)
    {
        StopAllCoroutines();
        // Debug.Log(ctrlType + " 모든 코루틴 정지 " + betType);
        // Debug.Log("EndRound");
        buttonSelcetionPanel.SetActive(false);
        lastBet.text = "Last Bet Type : " + betType;

        // 다음 라운드 시작할 사람 지정
        switch (betType)
        {
            // 플레이어가 폴드를 진행했을 때
            case "Fold":
                StartCoroutine(CheckRoundCard(ctrlType, betType, 0, 0));
            break;

            // 플레이어가 베팅을 진행했을 때
            case "Bet":
                StartCoroutine(ChipCreate(ctrlType, betType, chips, roundBet));
            break;

            // 플레이어가 콜을 진행했을 때
            case "Call":
                StartCoroutine(CheckRoundCard(ctrlType, betType, chips, roundBet));
            break;

            // 플레이어가 체크를 진행했을 때
            case "Check":
                StartCoroutine(ChipCreate(ctrlType, betType, chips, roundBet));
            break;
        }
    }

    #endregion

    #region 포톤 서버에서 활용하는 콜백 함수들

    #region Room Status Function / 방과 관련된 함수들
    // 현재 있는 방에서 로비로 나가기
    public void OnExitClick()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
            UserRoomOutToReact();
        #endif
        // Debug.Log("OnExitClick");
        GameReset();
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }

    // 방의 상태가 변경되었을 때 룸 정보를 변경하는 함수
    private void SetRoomInfo()
    {
        // Debug.Log("SetRoomInfo");
        Room room = PhotonNetwork.CurrentRoom;
        roomName.text = room.Name;
        roomPlayerCount.text = $"({room.PlayerCount}/{room.MaxPlayers})";
    }

    // 방으로 새로운 네트워크 유저가 접속했을 때 호출되는 콜백 함수
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Debug.Log("OnPlayerEnteredRoom");
        SetRoomInfo();
    }

    // 방에서 네트워크 유저가 퇴장했을 때 호출되는 콜백 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Debug.Log("OnPlayerLeftRoom");
        if (hostPlayer != null)
        {
            if (hostPlayer.showCard != null)
            {
                hostPlayer.DestroyMadeCard();
            }
            Destroy(hostPlayer);
        }
        if (guestPlayer != null)
        {
            if (guestPlayer.showCard != null)
            {
                guestPlayer.DestroyMadeCard();
            }
            Destroy(guestPlayer);
        }
        SetRoomInfo();
        InitialSetting();
        CreatePlayer();
    }
    
    #endregion

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
        // Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        PhotonNetwork.NickName = userIdWeb;
        // Debug.Log("NickName : " + PhotonNetwork.NickName);

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
        // Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        // Debug.Log($"Player Count = {PhotonNetwork.CurrentRoom.PlayerCount}");

        SetRoomInfo();
        InitialSetting();
        CreatePlayer();

        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            startButton.interactable = true;
        }
    }

    #endregion

    #endregion
    
    #region NetworkManager Part / 네트워크 매니저와 관련된 함수

    #region WebGL 빌드 시 사용하는 함수들

    // Web에서 실행시킬 Unity 내의 함수
    public void ReceiveUnityGameInfo(string gameInfo)
    {
        // Debug.Log(gameInfo);
        GameInfo info = JsonConvert.DeserializeObject<GameInfo>(gameInfo);
        userIdWeb = info.userId;
        roomNameWeb = info.roomName;
        PlayerPrefs.SetString("gameMode", info.gameMode);
        // Debug.Log(PlayerPrefs.GetString("gameMode"));
        // Debug.Log("userId : " + userIdWeb);
    }

    #endregion

    #region Window 혹은 Mac으로 빌드 시 사용하는 함수들
    // 유저명을 설정하는 로직
    public void SetUserId()
    {
        // 접속 유저의 닉네임 등록
        PhotonNetwork.NickName = userIdWeb;
    }

    #endregion

    #endregion
}

class GameInfo // 게임 정보를 받아오기 위한 클래스
{
    public string userId;
    public string roomName;
    public string gameMode;
}