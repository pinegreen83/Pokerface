using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class UICtrl : MonoBehaviourPunCallbacks
{

    // 베팅 시작 버튼
    public GameObject StartButton;
    // 폴드 버튼
    public GameObject FoldButton;
    // 체크 버튼
    public GameObject CheckButton;
    // 콜 버튼
    public GameObject CallButton;
    // 베팅 버튼
    public GameObject BetButton;
    // 베팅에서 나가는 버튼
    public GameObject BetCancelButton;
    // 베팅 시작으로 나가는 버튼
    public GameObject BackButton;
    // 베팅 버튼들이 모여있는 판넬
    public GameObject ButtonSelcetionPanel;
    // 베팅 판넬
    public GameObject BetPanel;
    // 현재 베팅하고자 하는 칩을 알려주는 텍스트
    public TMP_Text chipText;

    // UI 초기화
    public void UIReset() 
    {
        StartButton.SetActive(true);
        FoldButton.SetActive(false);
        BetButton.SetActive(false);
        CheckButton.SetActive(false);
        CallButton.SetActive(false);
        BackButton.SetActive(false);
        ButtonSelcetionPanel.SetActive(true);
        BetPanel.SetActive(false);
    }

    // 베팅 시작 버튼을 눌러 폴드, 체크, 베팅 버튼 활성화
    public void OnClickStartToCheck()
    {
        StartButton.SetActive(false);
        FoldButton.SetActive(true);
        CheckButton.SetActive(true);
        CallButton.SetActive(false);
        BetButton.SetActive(true);
        BackButton.SetActive(true);
    }

    // 베팅 시작 버튼을 눌러 폴드, 콜, 베팅 버튼 활성화
    public void OnClickStartToCall()
    {
        StartButton.SetActive(false);
        FoldButton.SetActive(true);
        CheckButton.SetActive(false);
        CallButton.SetActive(true);
        BetButton.SetActive(true);
        BackButton.SetActive(true);
    }

    // 베팅 시작 버튼으로 되돌아가기
    public void OnClickBack() 
    {
        StartButton.SetActive(true);
        FoldButton.SetActive(false);
        CheckButton.SetActive(false);
        CallButton.SetActive(false);
        BetButton.SetActive(false);
        BackButton.SetActive(false);
    }

    // 콜 혹은 폴드 버튼을 눌렀을 때 나머지 베팅 시작버튼만 활성화 후 나머지 버튼 비 활성화
    public void OnClickCheckAndCallAndFold()
    {
        StartButton.SetActive(true);
        FoldButton.SetActive(false);
        CheckButton.SetActive(false);
        CallButton.SetActive(false);
        BetButton.SetActive(false);
        BackButton.SetActive(false);
    }

    // 베팅 버튼을 눌렀을 때 베팅 시작버튼, 베팅 판넬 활성화 후 나머지 버튼, 버튼 선택 판넬 비활성화
    public void OnClickBet() 
    {
        StartButton.SetActive(true);
        FoldButton.SetActive(false);
        CheckButton.SetActive(false);
        CallButton.SetActive(false);
        BetButton.SetActive(false);
        BackButton.SetActive(false);
        ButtonSelcetionPanel.SetActive(false);
        BetPanel.SetActive(true);
    }

    // 베팅 취소 버튼을 눌렀을 때 버튼 선택 판넬 활성화 및 베팅 판넬 비활성화(체크)
    public void OnClickBetCancelButtonCheck()
    {
        StartButton.SetActive(false);
        FoldButton.SetActive(true);
        BetButton.SetActive(true);
        CheckButton.SetActive(true);
        CallButton.SetActive(false);
        BackButton.SetActive(true);
        ButtonSelcetionPanel.SetActive(true);
        BetPanel.SetActive(false);
    }

    // 베팅 취소 버튼을 눌렀을 때 버튼 선택 판넬 활성화 및 베팅 판넬 비활성화(콜)
    public void OnClickBetCancelButtonCall()
    {
        StartButton.SetActive(false);
        FoldButton.SetActive(true);
        BetButton.SetActive(true);
        CheckButton.SetActive(false);
        CallButton.SetActive(true);
        BackButton.SetActive(true);
        ButtonSelcetionPanel.SetActive(true);
        BetPanel.SetActive(false);
    }

    // 버튼 선택 판넬 활성화 및 베팅 판넬 비활성화
    public void OnclickTurnEnd()
    {
        FoldButton.SetActive(false);
        BetButton.SetActive(false);
        CheckButton.SetActive(false);
        CallButton.SetActive(false);
        BackButton.SetActive(false);
        ButtonSelcetionPanel.SetActive(true);
        BetPanel.SetActive(false);
    }

    // 베팅 슬라이드에 따라 현재 베팅하고자 하는 칩 텍스트 업데이트
    public void ValueUpdate(float value)
    {
        chipText.text = "Now Bet : " + value;
    }

}
