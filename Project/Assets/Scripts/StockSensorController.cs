using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockSensorController : MonoBehaviour
{
	private const float LENGTH_RAY = 0.035f;//Rayを飛ばす長さ
	private const string UNSENSORED_BALL = "Ball(Clone)";
	private const string SENSORED_BALL = "Ball(Sensored)";

	private StockSensorManager StockSensorManagerInstance;

	private int MyId;//自分のインデックス(StockSensorControllerを1個のスクリプトで使いまわすため、区別する必要がある)
	private StockSensorManager.JOB_PATTERN MyJob;//自分の役割

	private Vector3 PosStartRay;
	private Vector3 PosEndRay;

	// Start is called before the first frame update
	void Start()
    {
		StockSensorManagerInstance = GameObject.Find("StockSensor").GetComponent<StockSensorManager>();
		MyId = StockSensorManagerInstance.GetMyId(this.gameObject.name);//自分のインデックスをManagerに聞く
		MyJob = StockSensorManagerInstance.GetMyJob(this.gameObject.name);//自分の役割をManagerに聞く

		PosStartRay = this.gameObject.transform.position;
		PosEndRay = new Vector3(-1, 0, 0);//x軸負方向を向けばなんでもいい
	}

	// Update is called once per frame
	void Update()
    {
		bool result = judgeRayResult();//Rayを飛ばし、検知結果を得る
		if(result==true)//検知したら役割に応じた処理
		{
			doMyJob();
		}
		
	}

	private bool judgeRayResult()
	{
		bool rayResult=false;//検知したら上書きする

		RaycastHit hittedObjectInfo;
		//Debug.DrawRay(PosStartRay, PosEndRay, Color.red, LENGTH_RAY);
		if(Physics.Raycast(PosStartRay,PosEndRay, out hittedObjectInfo, LENGTH_RAY))//Rayはオブジェクトの中心から出る
		{
			if(hittedObjectInfo.collider.gameObject.name != SENSORED_BALL)
			{
				rayResult = true;//検知結果を上書き
				hittedObjectInfo.collider.gameObject.name = SENSORED_BALL;//オブジェクト名を検知済みに上書き
			}
		}

		return rayResult;
	}

	private void doMyJob()
	{
		StockSensorManagerInstance.ReportSensorResult(MyId, MyJob);//Managerに検知したことを知らせる(自分のインデックス、役割を伝える)
	}
}
