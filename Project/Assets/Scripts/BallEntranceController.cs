using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallEntranceController : MonoBehaviour
{
	private const float MOVE_LIMIT = 0.15f;//移動量の上限
	private const float MOVE_SPEED = 0.001f;//移動速度


	private float MoveDistance;//移動量
	private bool MoveVector;//trueが正方向。falseが負方向。
	private Vector3 MovePos;//移動座標(MovePositionの引数に使う)

	private Rigidbody RbBallEntrance;

    // Start is called before the first frame update
    void Start()
    {
		MoveDistance = 0;
		MoveVector = true;//最初は正方向から開始
		RbBallEntrance = GameObject.Find("BallEntrance").GetComponent<Rigidbody>();
		MovePos = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
		if (MoveDistance >= MOVE_LIMIT)//正方向上限に達していれば
		{
			MoveVector = false;//移動方向を負方向に変更
		}
		if (MoveDistance <= -MOVE_LIMIT)//負方向上限に達していれば
		{
			MoveVector = true;//移動方向を正方向に変更
		}

		if (MoveVector)//移動方向が正方向なら
		{
			MovePos.x += MOVE_SPEED;//移動座標を更新
			RbBallEntrance.MovePosition(MovePos);//オブジェクトを移動
			MoveDistance += MOVE_SPEED;//内部で覚えておく移動量も更新
		}
		else//移動方向が負方向なら
		{
			MovePos.x -= MOVE_SPEED;//移動座標を更新
			RbBallEntrance.MovePosition(MovePos);//オブジェクトを移動
			MoveDistance -= MOVE_SPEED;//内部で覚えておく移動量も更新
		}
	}

	public float GetMoveDistance()
	{
		return MoveDistance;
	}
}
