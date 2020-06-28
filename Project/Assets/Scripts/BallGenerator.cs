using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGenerator : MonoBehaviour
{
	private const float POS_DEFAULT_X = 0.225f;
	private const float POS_DEFAULT_Y = 0.46f;
	private const float POS_DEFAULT_Z = 0.03f;

	private Vector3 DefaultGeneratePos;
	private bool IsPermitGenerate;//ボール生成許可(trueなら生成してよし)

	private CreditManager CreditManagerInstance;
	private BallEntranceController BallEntranceControllerInstance;

    // Start is called before the first frame update
    void Start()
    {
		CreditManagerInstance = GameObject.Find("Main Camera").GetComponent<CreditManager>();
		BallEntranceControllerInstance = GameObject.Find("BallEntrance").GetComponent<BallEntranceController>();
		DefaultGeneratePos = new Vector3(POS_DEFAULT_X, POS_DEFAULT_Y, POS_DEFAULT_Z);
		IsPermitGenerate = true;
    }

	public void GenerateBall()
	{
		if( (CreditManagerInstance.IsCreditZero()==false)
			&& (IsPermitGenerate == true) )
		{
			IsPermitGenerate = false;//ポケットに入った後の一通りのアクションが終わるまでfalse(BingoMasuControllerからセット関数が呼ばれる)
			GameObject ballPrefab = decidePrefab();//ランダムで何色のボールにするか決める
			Vector3 generatePos = decideGeneratePos();//生成座標を決める
			Instantiate(ballPrefab,generatePos,Quaternion.identity);
			CreditManagerInstance.SubtractCredit(1);
		}
	}
	private GameObject decidePrefab()
	{
		GameObject ret = (GameObject)Resources.Load("Prefabs/Ball0");

		int id = Random.Range(0, 9 + 1);//ランダムでインデックスを決める
		ret = (GameObject)Resources.Load("Prefabs/Ball" + id);

		return ret;
	}
	private Vector3 decideGeneratePos()
	{
		Vector3 generatePos = DefaultGeneratePos;

		generatePos.x += BallEntranceControllerInstance.GetMoveDistance();

		return generatePos;
	}

	public void PermitGenerate()
	{
		IsPermitGenerate = true;
	}
}
