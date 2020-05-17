using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayController : MonoBehaviour
{
	private Text CreditValueText;
	private Text PayoutValueText;
	private GameObject GameOverCanvas;
	private GameObject ContinueButton;

	private CreditManager CreditManagerInstance;
	private PayoutManager PayoutManagerInstance;

    // Start is called before the first frame update
    void Start()
    {
		CreditValueText = GameObject.Find("CreditValueText").GetComponent<Text>();
		PayoutValueText = GameObject.Find("PayoutValueText").GetComponent<Text>();
		GameOverCanvas = GameObject.Find("GameOverCanvas");
		ContinueButton = GameObject.Find("ContinueButton");

		CreditManagerInstance = GameObject.Find("Main Camera").GetComponent<CreditManager>();
		PayoutManagerInstance = GameObject.Find("Main Camera").GetComponent<PayoutManager>();

		CreditManagerInstance.InitCreditText();
		PayoutManagerInstance.InitPayoutText();

		InactivateGameOverCanvas();//最初は非表示
    }

	/*==============================================================================*/
	/* 外部IF																		*/
	/*==============================================================================*/
	public void UpdateCreditText(int newCredit)
	{
		CreditValueText.text = newCredit.ToString();
	}
	public void UpdatePayoutText(int newPayout)
	{
		PayoutValueText.text = newPayout.ToString();
	}
	public void ActivateGameOverCanvas()
	{
		GameOverCanvas.SetActive(true);//ゲームオーバーキャンバスを表示する
	}
	public void InactivateGameOverCanvas()
	{
		GameOverCanvas.SetActive(false);
	}
	public void InactivateContinueButton()
	{
		ContinueButton.SetActive(false);
	}
}
