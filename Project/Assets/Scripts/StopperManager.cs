using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopperManager : MonoBehaviour
{
	private const int TIME_WAIT_ACTIVATE = 30;//Stopperが無効化されたとき、再度有効化されるまでの待ち時間

	private GameObject[] Stoppers = new GameObject[10];
	private int StopperInactiveTimer;//Stopperが無効化されたとき、再度有効化されるまでの時間を保持するタイマ

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0;i<10;i++)//Stopperを格納
		{
			if(IsSpPocket(i)==false)//SP枠のIDじゃないなら
			{
				Stoppers[i] = GameObject.Find("Stopper_" + i);
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
		timerCount();
		activateStopper();
    }
	private void timerCount()
	{
		if(StopperInactiveTimer>0)//タイマが動いてるなら
		{
			StopperInactiveTimer--;//カウントダウンする
		}
	}
	private void activateStopper()
	{//無効化されているStopperの待ち時間が終わっていれば有効化する
		for(int i=0;i<10;i++)
		{
			if(IsSpPocket(i)==false)
			{
				if ((GameObject.Find("Stopper_" + i) == false) && (StopperInactiveTimer == 0))//Stopperが無効化、かつタイマが0のとき(=待ち時間が終わった時)
				{
					Stoppers[i].SetActive(true);
				}
			}
		}
	}
	
	public bool IsSpPocket(int id)
	{//渡されてきたIDがSP枠のIDであればtrueを返す
		bool ret = false;

		switch (id)
		{
			case 0:
			case 3:
			case 6:
			case 9:
				ret = true;
				break;
			default:
				break;
		}

		return ret;
	}
	public void InactivateAllStopper()
	{//Stopperを無効化し、タイマをセットする
		for(int i=0;i<10;i++)
		{
			if(IsSpPocket(i)==false)
			{
				Stoppers[i].SetActive(false);//Stopper無効化
			}
		}
		StopperInactiveTimer = TIME_WAIT_ACTIVATE;
	}

	public bool IsStopperInactiveTimerStop()
	{//StopperInactiveTimerが稼働していなければtrueを返す
		bool ret = false;

		if(StopperInactiveTimer<=0)
		{
			ret = true;
		}

		return ret;
	}
}
