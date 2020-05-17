using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGenerator : MonoBehaviour
{
	private const float POS_DEFAULT_X = 0.225f;
	private const float POS_DEFAULT_Y = 0.46f;
	private const float POS_DEFAULT_Z = 0.03f;

	private Vector3 DefaultGeneratePos;

	private GameObject BallPrefab;

	private CreditManager CreditManagerInstance;
	private BallEntranceController BallEntranceControllerInstance;

    // Start is called before the first frame update
    void Start()
    {
		BallPrefab = (GameObject)Resources.Load("Prefabs/Ball");
		CreditManagerInstance = GameObject.Find("Main Camera").GetComponent<CreditManager>();
		BallEntranceControllerInstance = GameObject.Find("BallEntrance").GetComponent<BallEntranceController>();
		DefaultGeneratePos = new Vector3(POS_DEFAULT_X, POS_DEFAULT_Y, POS_DEFAULT_Z);
    }

	public void GenerateBall()
	{
		if(CreditManagerInstance.IsCreditZero()==false)
		{
			//ランダムで何色のボールにするか決める
			Vector3 generatePos = decideGeneratePos();//生成座標を決める
			Instantiate(BallPrefab,generatePos,Quaternion.identity);
			CreditManagerInstance.SubtractCredit(1);
		}
	}
	private Vector3 decideGeneratePos()
	{
		Vector3 generatePos = DefaultGeneratePos;

		generatePos.x += BallEntranceControllerInstance.GetMoveDistance();

		return generatePos;
	}
}
