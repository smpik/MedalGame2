using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayoutManager : MonoBehaviour
{
	private int Payout;

	private DisplayController DisplayControllerInstance;
	private CreditManager CreditManagerInstance;
	private BingoMasuController BingoMasuControllerInstance;
	private StopperManager StopperManagerInstance;
	private StockSensorManager StockSensorManagerInstance;
    // Start is called before the first frame update
    void Start()
    {
		initPayout();

		CreditManagerInstance = GameObject.Find("Main Camera").GetComponent<CreditManager>();
		BingoMasuControllerInstance = GameObject.Find("BingoMasu").GetComponent<BingoMasuController>();
		StopperManagerInstance = GameObject.Find("Stopper").GetComponent<StopperManager>();
		StockSensorManagerInstance = GameObject.Find("StockSensor").GetComponent<StockSensorManager>();
    }
	private void initPayout()
	{
		Payout = 0;
	}

	/*==============================================================================*/
	/* 非周期処理																		*/
	/*==============================================================================*/
	private void convertBingoToPayout(int amountBingo)
	{
		if(amountBingo<8)
		{
			Payout = amountBingo * 5;
		}
		else//全埋めのとき
		{
			Payout = 99;
		}
	}
	/*==============================================================================*/
	/* 外部IF																		*/
	/*==============================================================================*/
	public void UpdatePayout(int amountBingo)//払い出し枚数の更新
	{
		convertBingoToPayout(amountBingo);//ビンゴ数を払い出し枚数に換算
		DisplayControllerInstance.UpdatePayoutText(Payout);//テキストの更新
	}
	public void InitPayoutText()
	{
		initPayout();//表示される前に初期化しておく
		DisplayControllerInstance = GameObject.Find("Main Camera").GetComponent<DisplayController>();
		DisplayControllerInstance.UpdatePayoutText(0);
	}
	public void TapPayoutButton()
	{
		if (Payout > 0)//払い出しできるなら
		{
			CreditManagerInstance.AddCredit(Payout);//クレジットに払い出し枚数を加算
			initPayout();//払い出し枚数をリセット
			StockSensorManagerInstance.InactivateAllUpperSensor();//UpperSensorの無効化
			StopperManagerInstance.InactivateAllStopper();//ストッパー解除
			BingoMasuControllerInstance.ResetBingo();//ビンゴをリセット
		}
	}
	public int GetPayout()
	{
		return Payout;
	}
}
