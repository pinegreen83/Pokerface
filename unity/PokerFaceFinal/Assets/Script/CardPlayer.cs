 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class CardPlayer : MonoBehaviourPunCallbacks
{
    public string userName;
    public PhotonView pv;
    private Card hand;
    public GameObject showCard;
    public Animator animCard;
    private int nowChip;

    private void Awake()
    {
        userName = "";
        pv = GetComponent<PhotonView>();
        nowChip = 0;
        showCard = null;
        hand = null;
    }

    // 플레이어의 모든 정보를 보여주는 함수
    public string GetPlayerInfo()
    {
        return userName + " " + pv + " " + hand + " " + showCard + " " + nowChip;
    }

    // 보유하는 칩들의 갯수를 초기화 시키는 함수
    public void SetChips(int chips)
    {
        nowChip = chips;
    }

    // 보유하고 있는 칩을 증감하는 함수
    public void UpdateChips(int chips)
    {
        nowChip += chips;
    }

    // 각 플레이어가 보유하고 있는 칩의 갯수를 알려주는 함수
    public int GetChips()
    {
        return nowChip;
    }

    // 유저 아이디 설정하는 함수
    public void SetUserId(string id)
    {
        userName = id;
    }

    // 유저 아이디 가져오는 함수
    public string GetUserId()
    {
        return userName;
    }

    // 플레이어들에게 카드를 나눠주는 함수
    public void AddCard(Card card)
    {  
        // Debug.Log("In CardPlayer : " + card.GetCardInfo());
        hand = card;
    }

    // 플레이어가 가지고 있는 카드에 대한 정보를 가져옴
    public int GetCardType()
    {
        return hand.GetCardType();
    }

    public string GetCardInfo()
    {
        return hand.GetCardInfo();
    }

    // 플레이어들이 카드를 생성해서 화면에 띄우는 함수
    public IEnumerator CreateCard(string card, Vector3 cardPosition, Quaternion cardRotation, string player)
    {
        showCard = Instantiate(Resources.Load<GameObject>(card), cardPosition, cardRotation);
        animCard = showCard.GetComponent<Animator>();
        animCard.SetTrigger(player);

        yield return new WaitForSeconds(1.5f);
    }

    // 플레이어들이 카드를 생성해서 화면에 띄우는 함수
    public IEnumerator CheckCard(string player)
    {
        animCard.SetTrigger(player);

        yield return new WaitForSeconds(3f);
    }

    public void DestroyMadeCard()
    {
        Destroy(showCard);
    }   
}
