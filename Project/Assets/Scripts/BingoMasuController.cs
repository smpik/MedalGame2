using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BingoMasuController : MonoBehaviour
{
	private enum ACTION_STATE
	{
		DEFAULT = 0,
		ROULETTE,
		FLASH,
		RESET
	}
	private enum EVENT_FLASH
	{
		NONE = 0,
		FLASH_START,
		FLASH_CHANGE,
		FLASH_END
	}
	private enum EVENT_ROULETTE
	{
		NONE = 0,
		ROULETTE_START,
		MY_TURN_COMING,
		MY_TURN_FINISH,
		ROULETTE_END
	}
	private enum EVENT_RESET
	{
		NONE = 0,
		RESET_START,
		RESET_END
	}
	private enum V
	{
		BOTTOM = 0,
		CENTER,
		TOP
	}
	private enum H
	{
		RIGHT = 0,
		CENTER,
		LEFT
	}
	private struct BingoMasuRequestStruct
	{
		public bool Flash;
		public bool Roulette;
		public bool Reset;
	}
	private struct BingoMasuEventStruct
	{
		public EVENT_FLASH Flash;
		public EVENT_ROULETTE Roulette;
		public EVENT_RESET Reset;
	}
	private struct BingoMasuInfoStruct
	{
		public GameObject MasuOnObject;
		public GameObject MasuOffObject;
		public ACTION_STATE ActionState;
		public bool DisplayState;
		public BingoMasuEventStruct Event;
	}
	private const int BINGO_MASU_MAX = 9;//ビンゴマスの数
	private const int REPS_FLASH_CHANGE_MAX = 5;//点滅切り替え回数
	private const int TIME_FLASH_CHANGE = 20;//点滅切り替え待ち時間
	private const int TIME_ROULETTE_RANDOM_UPPER_LIMIT = 300;
	private const int TIME_ROULETTE_RANDOM_UNDER_LIMIT = 200;
	private const int TIME_ROULLETE_MASU_CHANGE = 20;//ルーレット選択中マスを切り替えるまでの時間
	private const int UNSTOCKED = -1;//通知のストック処理に用いる。ストックしてないことを示す

	private BingoMasuRequestStruct BingoMasuRequest;
	private BingoMasuInfoStruct[] BingoMasuInfo;
	private bool[,] BingoTable = new bool[3,3];//[横,縦]ビンゴの状況を覚えておくためのRAM。各アクション終了時に更新する
	private int FlashMasuId;//点滅させるマスのID
	private int FlashChangeReps;//点滅切り替え回数
	private int FlashChangeWaitTimer;//点滅切り替え待ち時間タイマ
	private int RouletteTimer;//ルーレット継続時間を保持するタイマ
	private int RouletteMasuChangeTimer;//ルーレット選択中マスを切り替えるまでの時間を保持するタイマ
	private int RouletteMasuId;//ルーレット選択中マス
	private int[] NotifyStock = new int[10];//通知のストック。通知されてきたIDを覚えておく。アクション中に通知が来た場合はストックし、アクション終了後に自分で通知しなおす。10は適当
	private int NotifyStockNumber;//現在の通知ストック数。
	private int ResetSequence;//リセットシーケンス(0が初期値、1がリセット開始時、2がリセット完了時を表わす。リセットアクションの終了判定に用いる)

	private PayoutManager PayoutManagerInstance;
	private CreditManager CreditManagerInstance;
	/*==============================================================================*/
	/* 初期化処理																		*/
	/*==============================================================================*/
	void Start()
    {
		generateStructInstance();
		initRequest();
		initBingoMasuInfo();
		for(int masu=0;masu<BINGO_MASU_MAX;masu++)
		{
			updateBingoMasuObjectDisplayState(masu);//DisplayStateの初期化をGameObjectのsetActiveにも反映させる
		}
		initBingoTable();
		initNotifyStock();

		PayoutManagerInstance = GameObject.Find("Main Camera").GetComponent<PayoutManager>();
		CreditManagerInstance = GameObject.Find("Main Camera").GetComponent<CreditManager>();
	}
	private void generateStructInstance()
	{
		BingoMasuRequest = new BingoMasuRequestStruct();

		BingoMasuInfo = new BingoMasuInfoStruct[BINGO_MASU_MAX];
		for (int masu = 0; masu < BINGO_MASU_MAX; masu++)
		{
			BingoMasuInfo[masu].Event = new BingoMasuEventStruct();
		}
	}
	private void initRequest()
	{
		BingoMasuRequest.Flash = false;
		BingoMasuRequest.Roulette = false;
		BingoMasuRequest.Reset = false;
	}
	private void initBingoMasuInfo()
	{
		for(int masu=0;masu<BINGO_MASU_MAX;masu++)
		{
			BingoMasuInfo[masu].MasuOnObject = GameObject.Find("BingoMasu_" + masu + "_ON");
			BingoMasuInfo[masu].MasuOffObject = GameObject.Find("BingoMasu_" + masu + "_OFF");
			BingoMasuInfo[masu].ActionState = ACTION_STATE.DEFAULT;
			BingoMasuInfo[masu].DisplayState = false;
			//Eventは初期化しなくても毎回NONEにしてから上書きしているので大丈夫
		}
	}
	private void initBingoTable()
	{
		 for (int horizontal = 0; horizontal < 3; horizontal++)
		{
			for (int vertical = 0; vertical < 3; vertical++)
			{
				BingoTable[horizontal, vertical] = false;
			}
		}
	}
	private void initNotifyStock()
	{
		for(int i=0;i<10;i++)
		{
			NotifyStock[i] = UNSTOCKED;
		}
		NotifyStockNumber = 0;
	}

	/*==============================================================================*/
	/* 周期処理																		*/
	/*==============================================================================*/
	void Update()
    {
        if ((BingoMasuRequest.Flash==true)//何かしらの要求がセットされていれば
			|| (BingoMasuRequest.Roulette==true)
			|| (BingoMasuRequest.Reset==true) )
		{
			decideInput();//inputの確定
			for(int masu=0;masu<BINGO_MASU_MAX;masu++)
			{
				fireEvent(masu);//イベント発火
				transitState(masu);//状態遷移
				updateBingoMasuObjectDisplayState(masu);//出力
			}
			finishAction();//アクション状態終了時の処理
		}
	}
	/*==========================================*/
	/*	inputの確定								*/
	/*==========================================*/
	private void decideInput()
	{
		decideInputByFlash();//点滅アクション状態のINPUT確定処理
		decideInputByRoulette();//ルーレットアクション状態のINPUT確定処理
		decideInputByReset();
	}
	private void decideInputByFlash()
	{
		if (FlashChangeReps > 0)//点滅切り替え回数が残ってるなら
		{
			if (FlashChangeWaitTimer > 0)//点滅切り替え待ちタイマが稼働中なら
			{
				FlashChangeWaitTimer--;//タイマの更新
			}
			else//タイムアップしてるなら
			{
				FlashChangeReps--;//点滅切り替え回数のデクリメント
				FlashChangeWaitTimer = TIME_FLASH_CHANGE;//タイマのリセット
			}
		}
		else//点滅切り替え回数が0なら
		{
			//何もしない(要求のクリアはアクション状態終了時の処理にて行う)
		}
	}
	private void decideInputByRoulette()
	{
		if (RouletteTimer > 0)//ルーレットタイマが稼働中なら
		{
			RouletteTimer--;//ルーレットタイマを更新
			if (RouletteMasuChangeTimer > 0)//ルーレット選択中マス切り替えタイマが稼働中なら
			{
				RouletteMasuChangeTimer--;//ルーレット選択中マス切り替えタイマを更新
			}
			else//ルーレット選択中マス切り替えタイマがタイムアップしてるなら
			{
				updateRouletteMasuId();//ルーレット選択中マスを更新
				RouletteMasuChangeTimer = TIME_ROULLETE_MASU_CHANGE;//ルーレット選択中マス切り替えタイマをセット
			}
		}
		else//ルーレットタイマがタイムアップしてるなら
		{
			//何もしない(要求のクリアはアクション状態終了時の処理にて行う)
		}
	}
	private void updateRouletteMasuId()//ルーレット選択中マスを更新する
	{
		bool restartFlag = false;//無限ループしないように検索やり直しを1回だけにするためのフラグ

		for (int masu = RouletteMasuId; masu < BINGO_MASU_MAX; masu++)//次の埋まってないマスを探す(選択中マスから検索開始するが選択中マスは点灯しているはずなので問題なし)
		{
			if (GameObject.Find("BingoMasu_" + masu + "_ON") == false)//埋まってないマスがあれば
			{
				RouletteMasuId = masu;
				break;
			}
			if (masu == (BINGO_MASU_MAX - 1))//最後まで検索したら(ルーレット要求セット時に埋まってるマスが2つ以上あることは確認しているため、最初から数えなおせばあるはず)
			{
				if (restartFlag == false)//検索やり直しをまだしてなければ
				{
					masu = 0-1;//強制的に先頭から検索をやり直す
					restartFlag = true;//検索やり直しをしたことを覚えておく
				}
				else
				{
					break;
				}
			}
		}
	}
	private void decideInputByReset()
	{
		ResetSequence++;//リセットシーケンスを更新(0が初期値、1がリセット開始時、2がリセット完了時)
	}
	/*==========================================*/
	/*	イベント発火								*/
	/*==========================================*/
	private void fireEvent(int masu)
	{
		if (BingoMasuRequest.Flash == true)//点滅要求がセットされてるなら
		{
			fireEventByFlash(masu);//点滅アクション状態のイベント発火
		}
		if (BingoMasuRequest.Roulette == true)//ルーレット要求がセットされているなら
		{
			fireEventByRoulette(masu);//ルーレットアクション状態のイベント発火
		}
		if (BingoMasuRequest.Reset == true)//リセット要求がセットされているとき
		{
			fireEventByReset(masu);//リセットアクション状態のイベント発火
		}
	}
	private void fireEventByFlash(int masu)
	{
		BingoMasuInfo[masu].Event.Flash = EVENT_FLASH.NONE;//イベントがあれば上書きされる

		if ((FlashMasuId == masu) && (BingoMasuInfo[masu].ActionState == ACTION_STATE.DEFAULT))//点滅マスが自分のマス、かつ自分のマスのアクション状態が通常状態のとき
		{
			BingoMasuInfo[masu].Event.Flash = EVENT_FLASH.FLASH_START;//点滅開始イベント発火
		}
		if ((FlashChangeReps > 0) && (FlashChangeWaitTimer == 0))//点滅切り替え回数が残っていて、点滅切り替え待ちタイマがタイムアップしてるとき
		{
			BingoMasuInfo[masu].Event.Flash = EVENT_FLASH.FLASH_CHANGE;//点滅切り替えイベント発火
		}
		if (FlashChangeReps == 0)//点滅切り替え回数がなくなったとき
		{
			BingoMasuInfo[masu].Event.Flash = EVENT_FLASH.FLASH_END;//点滅終了イベント発火
		}
	}
	private void fireEventByRoulette(int masu)
	{
		BingoMasuInfo[masu].Event.Roulette = EVENT_ROULETTE.NONE;//イベントがあれば上書きされる

		if ((BingoMasuInfo[masu].ActionState == ACTION_STATE.DEFAULT) && (BingoMasuInfo[masu].DisplayState == false))
		{//自分のマスが通常アクション状態、かつ非表示のとき
			BingoMasuInfo[masu].Event.Roulette = EVENT_ROULETTE.ROULETTE_START;//ルーレット開始イベント発火
		}
		if ((BingoMasuInfo[masu].ActionState == ACTION_STATE.ROULETTE) && (BingoMasuInfo[masu].DisplayState == false) && (RouletteMasuId == masu))
		{//自分のマスがルーレット状態、かつ非表示、かつルーレット選択中マスが自分のとき
			BingoMasuInfo[masu].Event.Roulette = EVENT_ROULETTE.MY_TURN_COMING;//ルーレット自分の番来たイベント発火
		}
		if ((BingoMasuInfo[masu].ActionState == ACTION_STATE.ROULETTE) && (BingoMasuInfo[masu].DisplayState == true) && (RouletteMasuId != masu))
		{//自分のマスがルーレット状態、かつ表示、かつルーレット選択中マスが自分じゃないとき
			BingoMasuInfo[masu].Event.Roulette = EVENT_ROULETTE.MY_TURN_FINISH;//ルーレット自分の番終わりイベント発火
		}
		if ((BingoMasuInfo[masu].ActionState == ACTION_STATE.ROULETTE) && (RouletteTimer == 0))
		{//自分のマスがルーレット状態、かつルーレットタイマがタイムアップしてるとき
			BingoMasuInfo[masu].Event.Roulette = EVENT_ROULETTE.ROULETTE_END;//ルーレット終了イベント発火
		}
	}
	private void fireEventByReset(int masu)
	{
		BingoMasuInfo[masu].Event.Reset = EVENT_RESET.NONE;

		if (ResetSequence == 1)//リセットシーケンスが1のとき(すべてのマスをリセット状態にするため、DisplayStateやActionStateを限定しない)
		{//(消灯しているマスもリセット状態にする(消灯していても点滅中かもしれないから)。ほかのアクション状態のときにもイベント発火できるよう、通常状態のとき」に限定しない)
			BingoMasuInfo[masu].Event.Reset = EVENT_RESET.RESET_START;//リセット開始イベント発火
		}
		if ((BingoMasuInfo[masu].ActionState == ACTION_STATE.RESET) && (BingoMasuInfo[masu].DisplayState == false) && (ResetSequence == 2))
		{//自分のマスの状態がリセット状態、かつ消灯、かつリセットシーケンスが2のとき
			BingoMasuInfo[masu].Event.Reset = EVENT_RESET.RESET_END;//リセット終了イベント発火
		}
	}
	/*==========================================*/
	/*	状態遷移									*/
	/*==========================================*/
	private void transitState(int masu)
	{
		switch(BingoMasuInfo[masu].ActionState)//マスの状態が
		{
			case ACTION_STATE.DEFAULT://通常アクション状態なら
				transitStateByDefault(masu);//通常アクション状態の状態遷移処理
				break;
			case ACTION_STATE.FLASH://点滅アクション状態なら
				transitStateByFlash(masu);//点滅アクション状態の状態遷移処理
				break;
			case ACTION_STATE.ROULETTE://ルーレットアクション状態なら
				transitStateByRoulette(masu);//ルーレットアクション状態の状態遷移処理
				break;
			case ACTION_STATE.RESET://リセットアクション状態なら
				transitStateByReset(masu);//リセットアクション状態の状態遷移
				break;
		}
	}
	private void transitStateByDefault(int masu)
	{
		if (BingoMasuInfo[masu].Event.Flash == EVENT_FLASH.FLASH_START)//自分のマスに点滅開始イベントが発火してるとき
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.FLASH;//自分のマスのアクション状態を点滅状態に設定
		}
		if (BingoMasuInfo[masu].Event.Roulette == EVENT_ROULETTE.ROULETTE_START)//自分のマスにルーレット開始イベントが発火してるとき
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.ROULETTE;//自分のマスのアクション状態をルーレット状態に設定
		}
		if(BingoMasuInfo[masu].Event.Reset == EVENT_RESET.RESET_START)//自分のマスにリセット開始イベントが発火しているとき
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.RESET;//リセット状態に設定
			BingoMasuInfo[masu].DisplayState = false;//消灯
		}
	}
	private void transitStateByFlash(int masu)
	{
		if (BingoMasuInfo[masu].Event.Flash == EVENT_FLASH.FLASH_CHANGE)//自分のマスに点滅切り替えイベントが発火してるとき
		{
			BingoMasuInfo[masu].DisplayState = !(BingoMasuInfo[masu].DisplayState);//自分のマスの表示状態を反転
		}
		if (BingoMasuInfo[masu].Event.Flash == EVENT_FLASH.FLASH_END)//自分のマスに点滅終了イベントが発火してるとき
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.DEFAULT;//自分のマスのアクション状態を通常状態に設定
		}
		if (BingoMasuInfo[masu].Event.Reset == EVENT_RESET.RESET_START)//自分のマスにリセット開始イベントが発火しているとき
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.RESET;//リセット状態に設定
			BingoMasuInfo[masu].DisplayState = false;//消灯
		}
	}
	private void transitStateByRoulette(int masu)
	{
		if (BingoMasuInfo[masu].Event.Roulette == EVENT_ROULETTE.MY_TURN_COMING)//自分のマスにルーレット自分の番来たイベントが発火してるとき
		{
			BingoMasuInfo[masu].DisplayState = true;//自分のマスの表示状態をONに設定
		}
		if (BingoMasuInfo[masu].Event.Roulette == EVENT_ROULETTE.MY_TURN_FINISH)//自分のマスにルーレット自分の番終わりイベントが発火してるとき
		{
			BingoMasuInfo[masu].DisplayState = false;//自分のマスの表示状態をOFFに設定
		}
		if (BingoMasuInfo[masu].Event.Roulette == EVENT_ROULETTE.ROULETTE_END)//自分のマスにルーレット終了イベントが発火してるとき
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.DEFAULT;//自分のマスのアクション状態を通常状態に設定
		}
		if (BingoMasuInfo[masu].Event.Reset == EVENT_RESET.RESET_START)//自分のマスにリセット開始イベントが発火しているとき
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.RESET;//リセット状態に設定
			BingoMasuInfo[masu].DisplayState = false;//消灯
		}
	}
	private void transitStateByReset(int masu)
	{
		if(BingoMasuInfo[masu].Event.Reset == EVENT_RESET.RESET_END)//自分のマスにリセット終了イベントが発生しているなら
		{
			BingoMasuInfo[masu].ActionState = ACTION_STATE.DEFAULT;//通常状態に設定
		}
	}
	/*==========================================*/
	/* 出力										*/
	/*==========================================*/
	private void updateBingoMasuObjectDisplayState(int masu)
	{
		BingoMasuInfo[masu].MasuOnObject.SetActive(BingoMasuInfo[masu].DisplayState);//ONマスにDisplayStateでSetActive
		BingoMasuInfo[masu].MasuOffObject.SetActive(!(BingoMasuInfo[masu].DisplayState));//OFFマスに!DisplayStateでSetActive
	}
	/*==========================================*/
	/* アクション状態の終了時の処理					*/
	/*==========================================*/
	private void finishAction()
	{
		if((BingoMasuRequest.Flash==true)&&(FlashChangeReps==0))//点滅アクションの終了時
		{
			BingoMasuRequest.Flash = false;
			commonFunctionAfterActionFinished();
		}
		if((BingoMasuRequest.Roulette==true)&&(RouletteTimer==0))//ルーレットアクションの終了時
		{
			BingoMasuRequest.Roulette = false;
			commonFunctionAfterActionFinished();
		}
		if((BingoMasuRequest.Reset==true)&&(ResetSequence==2))//リセットアクションの終了時
		{
			BingoMasuRequest.Reset = false;
			commonFunctionAfterActionFinished();
		}
	}
	private void commonFunctionAfterActionFinished()//各アクション終了後に共通して行う処理
	{
		updateBingoTable();//ビンゴテーブルの更新(埋めた結果を内部変数にも反映する)
		PayoutManagerInstance.UpdatePayout(calculateAmountBingo());
		bool isNotifyStockExist = pickUpNotifyStock();//通知のストックがあれば処理する
		if (isNotifyStockExist == false)//通知のストックがなければ
		{
			CreditManagerInstance.NotifyBingoActionIsFinished();//アクションが一通り終わったことを通知する
		}
	}
	private void updateBingoTable()
	{
		if(GameObject.Find("BingoMasu_0_ON")==true)//左上
		{
			BingoTable[(int)H.LEFT,(int)V.TOP] = true;
		}
		if (GameObject.Find("BingoMasu_1_ON") == true)//中上
		{
			BingoTable[(int)H.CENTER,(int)V.TOP] = true;
		}
		if (GameObject.Find("BingoMasu_2_ON") == true)//右上
		{
			BingoTable[(int)H.RIGHT, (int)V.TOP] = true;
		}
		if (GameObject.Find("BingoMasu_3_ON") == true)//左中
		{
			BingoTable[(int)H.LEFT, (int)V.CENTER] = true;
		}
		if (GameObject.Find("BingoMasu_4_ON") == true)//中中
		{
			BingoTable[(int)H.CENTER, (int)V.CENTER] = true;
		}
		if (GameObject.Find("BingoMasu_5_ON") == true)//右中
		{
			BingoTable[(int)H.RIGHT, (int)V.CENTER] = true;
		}
		if (GameObject.Find("BingoMasu_6_ON") == true)//左下
		{
			BingoTable[(int)H.LEFT, (int)V.BOTTOM] = true;
		}
		if (GameObject.Find("BingoMasu_7_ON") == true)//中下
		{
			BingoTable[(int)H.CENTER, (int)V.BOTTOM] = true;
		}
		if (GameObject.Find("BingoMasu_8_ON") == true)//右下
		{
			BingoTable[(int)H.RIGHT, (int)V.BOTTOM] = true;
		}
	}
	private bool pickUpNotifyStock()//通知のストックがあるか確認し、あれば処理し、trueを返す。
	{
		bool isNotifyStockExist = false;//戻り値。通知のストックがあるか

		if (NotifyStock[0] != UNSTOCKED)//通知ストック配列の先頭を確認しストックがたまってれば
		{
			isNotifyStockExist = true;
			NotifyFromStockSensor(NotifyStock[0]);//自分で要求する

			for (int i = 0; i < 10; i++)//通知のストックを詰める
			{
				if (i < 9)//詰めてくる要素があるとき
				{
					NotifyStock[i] = NotifyStock[i + 1];
				}
				else//詰めてくる要素がないとき
				{
					NotifyStock[i] = UNSTOCKED;
				}
			}

			NotifyStockNumber--;//通知のストック数を更新する
		}

		return isNotifyStockExist;
	}
	/*==============================================================================*/
	/* 非周期処理																		*/
	/*==============================================================================*/
	private void setFlashRequest(int sensorId)
	{
		if(GameObject.Find("BingoMasu_"+sensorId+"_ON")==false)//まだビンゴを埋めていないなら
		{//(数字枠のUnderSensorから通知がきても、SPによってビンゴが埋められている可能性があるため)
			BingoMasuRequest.Flash = true;//点滅要求をセット
			FlashMasuId = sensorId;//点滅マスを数字枠の数字のマスに設定
			FlashChangeReps = REPS_FLASH_CHANGE_MAX;//点滅切り替え回数をセット
			FlashChangeWaitTimer = TIME_FLASH_CHANGE;//点滅切り替え待ち時間をセット
		}
	}
	private void setRouletteRequest()
	{
		/* 埋まってないマスがいくつあるか確認	*/
		int inactiveMasuCount = 0;
		int lastInactiveMasuId = 0;//for文で一番最後に見つかった埋まってないマスのID(埋まってないマスが1つしかないときに、点滅マスのIDとするため)
		for(int masu=0;masu<BINGO_MASU_MAX;masu++)
		{
			if(GameObject.Find("BingoMasu_"+masu+"_ON")==false)
			{
				lastInactiveMasuId = masu;
				inactiveMasuCount++;
			}
		}
		if(inactiveMasuCount>=2)//埋まってないマスが2つ以上あれば(0ならもちろん、1こでもルーレットはできないから)
		{
			BingoMasuRequest.Roulette = true;//ルーレット要求をセット
			RouletteTimer = Random.Range(TIME_ROULETTE_RANDOM_UNDER_LIMIT, TIME_ROULETTE_RANDOM_UPPER_LIMIT);//ルーレットタイマをセット
			RouletteMasuChangeTimer = TIME_ROULLETE_MASU_CHANGE;//ルーレット選択中マス切り替えタイマをセット
			/* 最初のルーレット選択中マスを決定	*/
			for (int masu = 0; masu < BINGO_MASU_MAX; masu++)
			{
				if (GameObject.Find("BingoMasu_" + masu + "_ON") == false)//埋まってないマスが見つかった場合
				{
					RouletteMasuId = masu;
					break;
				}
			}
		}
		if(inactiveMasuCount==1)//埋まってないマスが1つのみのとき(ルーレットはできないので点滅にする)
		{
			setFlashRequest(lastInactiveMasuId);//検索中に見つかった埋まってないマスのIDを渡す
		}
		if(inactiveMasuCount==0)//埋まってないマスがなければ
		{
			//なにもしない
		}
	}
	private void setResetRequest()
	{
		ResetSequence = 0;//リセットシーケンスを初期化
		initRequest();//要求をリセット(ほかのアクションしててもリセットアクションを最優先する)
		BingoMasuRequest.Reset = true;//リセット要求をセット
	}
	private int calculateAmountBingo()
	{
		int amountBingo = 0;

		amountBingo = isRow0Bingo()			//1行目のチェック
					+ isRow1Bingo()			//2行目のチェック
					+ isRow2Bingo()			//3行目のチェック
					+ isColumn0Bingo()		//1列目のチェック
					+ isColumn1Bingo()		//2列目のチェック
					+ isColumn2Bingo()		//3列目のチェック
					+ isSlashBingo()		//「\」のチェック
					+ isBackSlashBingo();	//「/」のチェック

		return amountBingo;
	}
	private int isRow0Bingo()//左下、中下、右下が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.LEFT, (int)V.BOTTOM]) && (BingoTable[(int)H.CENTER, (int)V.BOTTOM]) && (BingoTable[(int)H.RIGHT, (int)V.BOTTOM]))
		{
			ret = 1;
		}
		return ret;
	}
	private int isRow1Bingo()//左中、中中、右中が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.LEFT, (int)V.CENTER]) && (BingoTable[(int)H.CENTER, (int)V.CENTER]) && (BingoTable[(int)H.RIGHT, (int)V.CENTER]))
		{
			ret = 1;
		}
		return ret;
	}
	private int isRow2Bingo()//左上、中上、右上が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.LEFT, (int)V.TOP]) && (BingoTable[(int)H.CENTER, (int)V.TOP]) && (BingoTable[(int)H.RIGHT, (int)V.TOP]))
		{
			ret = 1;
		}
		return ret;
	}
	private int isColumn0Bingo()//右下、右中、右上が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.RIGHT, (int)V.BOTTOM]) && (BingoTable[(int)H.RIGHT, (int)V.CENTER]) && (BingoTable[(int)H.RIGHT, (int)V.TOP]))
		{
			ret = 1;
		}
		return ret;
	}
	private int isColumn1Bingo()//中下、中中、中上が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.CENTER, (int)V.BOTTOM]) && (BingoTable[(int)H.CENTER, (int)V.CENTER]) && (BingoTable[(int)H.CENTER, (int)V.TOP]))
		{
			ret = 1;
		}
		return ret;
	}
	private int isColumn2Bingo()//左下、左中、左上が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.LEFT, (int)V.BOTTOM]) && (BingoTable[(int)H.LEFT, (int)V.CENTER]) && (BingoTable[(int)H.LEFT, (int)V.TOP]))
		{
			ret = 1;
		}
		return ret;
	}
	private int isSlashBingo()//右上、中中、左下が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.RIGHT, (int)V.TOP]) && (BingoTable[(int)H.CENTER, (int)V.CENTER]) && (BingoTable[(int)H.LEFT, (int)V.BOTTOM]))
		{
			ret = 1;
		}
		return ret;
	}
	private int isBackSlashBingo()//左上、中中、右下が埋まっていたら1を返す
	{
		int ret = 0;
		if ((BingoTable[(int)H.LEFT, (int)V.TOP]) && (BingoTable[(int)H.CENTER, (int)V.CENTER]) && (BingoTable[(int)H.RIGHT, (int)V.BOTTOM]))
		{
			ret = 1;
		}
		return ret;
	}
	private bool isRequestExist()
	{//一つでも要求があればtrueを返す(アクション実行中ならtrueを返す)
		bool ret = true;//全部falseだった時だけfalseに上書きして返す

		if((BingoMasuRequest.Flash==false)
			&& (BingoMasuRequest.Roulette==false)
			&& (BingoMasuRequest.Reset==false))
		{
			ret = false;
		}

		return ret;
	}

	/*==============================================================================*/
	/* 外部IF																		*/
	/*==============================================================================*/
	public void NotifyFromStockSensor(int sensorId)
	{
		if(isRequestExist()!=true)//現在、アクションを実行中でなければ、受け取ったIDに基づきアクションを開始する
		{
			switch (sensorId)
			{
				case 1:
				case 2:
				case 4:
				case 5:
				case 7:
				case 8://数字枠のIDなら
					setFlashRequest(sensorId);//点滅要求セット
					break;
				case 0:
				case 3:
				case 6:
				case 9://SP枠のIDなら
					setRouletteRequest();//ルーレット要求セット
					break;
				default:
					break;
			}
		}
		else//アクションを実行中なら
		{
			NotifyStock[NotifyStockNumber] = sensorId;//通知されてきたIDをストックする
			NotifyStockNumber++;//通知のストック数を更新する
		}
	}

	public void ResetBingo()
	{
		setResetRequest();//リセットアクション要求
		initBingoTable();//ビンゴテーブルのリセット
		initNotifyStock();//通知のストックのリセット
		//ビンゴサークルを無効化
	}
}
