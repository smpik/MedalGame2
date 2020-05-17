using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGuideStickController : MonoBehaviour
{
	private GameObject Stick1;//BallGuideStickLeft
	private GameObject Stick2;//BallGuideStickRight

	private bool Clockwise;//回転方向
	private float StickAngle;//内部で保持する棒の角度(直接棒の角度を取得すると0=360になったりして使いづらいから)

	private const float ROTATION_SPEED = 1;//棒の回転速度
	private const float ANGLE_LIMIT = 30;//棒の回転角度の上限

    // Start is called before the first frame update
    void Start()
    {
		Stick1 = GameObject.Find("BallGuideStickLeftPivot");
		Stick2 = GameObject.Find("BallGuideStickRightPivot");

		StickAngle = 0f;//初期化
		Clockwise = false;//起動初回時は反時計回りから開始

		Stick1.transform.localEulerAngles = new Vector3(0, 0, StickAngle);//初期化
		Stick2.transform.localEulerAngles = new Vector3(0, 0, StickAngle);
    }

    // Update is called once per frame
    void Update()
    {
		if (StickAngle <= -ANGLE_LIMIT)//時計回り上限に達していれば(左端?)
		{
			Clockwise = false;//回転方向を反時計回りに変更(右向き?)
		}
		if (StickAngle >= ANGLE_LIMIT)//反時計回り上限に達していれば(右端?)
		{
			Clockwise = true;//回転方向を時計回りに変更(左向き)
		}

		if (Clockwise)//回転方向が時計回りなら
		{
			Stick1.transform.Rotate(0, 0, -ROTATION_SPEED);//誘導棒角度更新
			Stick2.transform.Rotate(0, 0, -ROTATION_SPEED);//Rotate(0,0,正)でCCWになるらしい
			StickAngle -= ROTATION_SPEED;//内部で保持している誘導棒の角度も更新
		}
		else//回転方向が反時計回りなら
		{
			Stick1.transform.Rotate(0, 0, ROTATION_SPEED);//誘導棒角度更新
			Stick2.transform.Rotate(0, 0, ROTATION_SPEED);//Rotate(0,0,正)でCCWになるらしい
			StickAngle += ROTATION_SPEED;//内部で保持している誘導棒の角度も更新
		}
	}
}
