using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Card
{
    private readonly int cardType;
    private string cardNum;
    private string cardSuit;

    // 생성자를 추가하여 객체 생성 시 초기화를 수행합니다.
    public Card(int type)
    {
        cardType = type;
    }

    public int GetCardType()
    {
        return cardType;
    }

    // 필요한 메서드들을 추가합니다.
    public string GetCardInfo()
    {
        // return cardNum + " of " + cardSuit;
        return cardSuit + cardNum;
    }

    // 카드 정보 세팅
    public void SetCard()
    {
        int cardNumTemp = (cardType % 10) + 1;
        if (cardNumTemp == 1)
        {
            cardNum = "Ace";
        }
        else
        {
            cardNum = cardNumTemp.ToString();
        }
        int cardSuitNum = (cardType / 10);
        switch (cardSuitNum)
        {
            case 0:
                cardSuit = "Card_Diamond";
            break;

            case 1:
                cardSuit = "Card_Spade";
            break;
        }
    }
}
