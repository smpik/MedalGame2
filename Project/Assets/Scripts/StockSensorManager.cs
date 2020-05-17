using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockSensorManager : MonoBehaviour
{
	public enum JOB_PATTERN//Sensorの役割
	{
		UNDER = 0,
		UPPER
	}

	private GameObject[] UpperSensors = new GameObject[10];

	private StopperManager StopperManagerInstance;
	private BingoMasuController BingoMasuControllerInstance;

	// Start is called before the first frame update
	void Start()
    {
		StopperManagerInstance = GameObject.Find("Stopper").GetComponent<StopperManager>();
		BingoMasuControllerInstance = GameObject.Find("BingoMasu").GetComponent<BingoMasuController>();

		getUpperSensorObject();//UpperSensorを変数に格納する
        for(int i=0;i<10;i++)//UpperSensorを無効にしておく(UpperSensorはUnderSensorがボールを検出しているときのみ有効化する)
		{
			UpperSensors[i].SetActive(false);
		}
    }
	private void getUpperSensorObject()
	{
		for(int i=0;i<10;i++)
		{
			UpperSensors[i] = GameObject.Find("StockSensor_" + i + "_Upper");
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public int GetMyId(string sensorObjectName)//各StockSensorにIdを教えてあげる
	{
		int myId = 0;

		switch(sensorObjectName)
		{
			case "StockSensor_0_Under":
			case "StockSensor_0_Upper":
				myId = 0;
				break;
			case "StockSensor_1_Under":
			case "StockSensor_1_Upper":
				myId = 1;
				break;
			case "StockSensor_2_Under":
			case "StockSensor_2_Upper":
				myId = 2;
				break;
			case "StockSensor_3_Under":
			case "StockSensor_3_Upper":
				myId = 3;
				break;
			case "StockSensor_4_Under":
			case "StockSensor_4_Upper":
				myId = 4;
				break;
			case "StockSensor_5_Under":
			case "StockSensor_5_Upper":
				myId = 5;
				break;
			case "StockSensor_6_Under":
			case "StockSensor_6_Upper":
				myId = 6;
				break;
			case "StockSensor_7_Under":
			case "StockSensor_7_Upper":
				myId = 7;
				break;
			case "StockSensor_8_Under":
			case "StockSensor_8_Upper":
				myId = 8;
				break;
			case "StockSensor_9_Under":
			case "StockSensor_9_Upper":
				myId = 9;
				break;
			default:
				Debug.Log("StockSensorのオブジェクト名がおかしいかも");
				break;
		}

		return myId;
	}

	public JOB_PATTERN GetMyJob(string sensorObjectName)//各StockSensorに自分の役割を教えてあげる
	{
		JOB_PATTERN myJob = JOB_PATTERN.UNDER;//エラー回避のため、初期値を持たせる(UNDERであることに意味はない。swich文で正しく設定できてる)

		switch(sensorObjectName)
		{
			case "StockSensor_0_Under":
			case "StockSensor_1_Under":
			case "StockSensor_2_Under":
			case "StockSensor_3_Under":
			case "StockSensor_4_Under":
			case "StockSensor_5_Under":
			case "StockSensor_6_Under":
			case "StockSensor_7_Under":
			case "StockSensor_8_Under":
			case "StockSensor_9_Under":
				myJob = JOB_PATTERN.UNDER;
				break;
			case "StockSensor_0_Upper":
			case "StockSensor_1_Upper":
			case "StockSensor_2_Upper":
			case "StockSensor_3_Upper":
			case "StockSensor_4_Upper":
			case "StockSensor_5_Upper":
			case "StockSensor_6_Upper":
			case "StockSensor_7_Upper":
			case "StockSensor_8_Upper":
			case "StockSensor_9_Upper":
				myJob = JOB_PATTERN.UPPER;
				break;
			default:
				Debug.Log("StockSensorのオブジェクト名がおかしいかも");
				break;
		}

		return myJob;
	}

	public void ReportSensorResult(int sensorId, JOB_PATTERN sensorJob)//各センサーからの報告を受ける
	{
		if (StopperManagerInstance.IsStopperInactiveTimerStop() == true)//StopperInactiveTimerが止まってるとき(=リセットの待ち時間が終わってるとき。待ち時間中は検知してもビンゴを埋めたくないため)
		{
			if (sensorJob == JOB_PATTERN.UNDER)//UnderSensorが検知したとき
			{
				if (StopperManagerInstance.IsSpPocket(sensorId) == false)//sensorIdがSP枠でなければ(SP枠のときはUpperSensor使わない。Stopperも)
				{
					UpperSensors[sensorId].SetActive(true);//報告してきたsensorのインデックスと同じUpperSensorを有効にする
				}
				BingoMasuControllerInstance.NotifyFromStockSensor(sensorId);//ビンゴ埋める(sensorIdを渡す。その枠に割り当てられている役割はビンゴのほうで処理する)
			}
			if (sensorJob == JOB_PATTERN.UPPER)//UpperSensorが検知したとき
			{
				StopperManagerInstance.InactivateAllStopper();//Stopperを無効にする
				InactivateAllUpperSensor();//UpperSensorを無効化する
				BingoMasuControllerInstance.ResetBingo();//ビンゴリセット
			}
		}
	}
	public void InactivateAllUpperSensor()
	{
		for(int i=0;i<10;i++)
		{
			UpperSensors[i].SetActive(false);
		}
	}
}
