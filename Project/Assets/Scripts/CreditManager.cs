using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditManager : MonoBehaviour
{
	private const int VALUE_DEFAULT_CREDIT = 15;

	private int Credit;

	private DisplayController DisplayControllerInstance;
	private PayoutManager PayoutManagerInstance;

    // Start is called before the first frame update
    void Start()
    {
		PayoutManagerInstance = GameObject.Find("Main Camera").GetComponent<PayoutManager>();
    }
	
	/*==============================================================================*/
	/* 外部IF																		*/
	/*==============================================================================*/
	public void AddCredit(int addValue)
	{
		Credit += addValue;
		DisplayControllerInstance.UpdateCreditText(Credit);
	}
	public void SubtractCredit(int subtractValue)
	{
		if(Credit>=subtractValue)//引けるか
		{
			Credit -= subtractValue;//引く
		}
		DisplayControllerInstance.UpdateCreditText(Credit);
	}
	public bool IsCreditZero()
	{
		bool ret = false;

		if (Credit <= 0)
		{
			ret = true;
		}

		return ret;
	}
	public void InitCreditText()
	{
		Credit = VALUE_DEFAULT_CREDIT;//表示される前に初期化しておく
		DisplayControllerInstance = GameObject.Find("Main Camera").GetComponent<DisplayController>();
		DisplayControllerInstance.UpdateCreditText(Credit);
	}
	public void NotifyBingoActionIsFinished()//BingoMasuControllerからアクションが一通り終わった時に送られてくる通知
	{
		if (GameObject.Find("Ball(Clone)")==false)//未検出のボールがないか(=落下中のボールがないか)
		{
			if ( (IsCreditZero() == true) && (PayoutManagerInstance.GetPayout()<=0) )//クレジットなしも払い出しもなければ(=ゲームオーバーということ)
			{
				DisplayControllerInstance.ActivateGameOverCanvas();//ゲームオーバー画面表示要求
			}
		}
	}
}
