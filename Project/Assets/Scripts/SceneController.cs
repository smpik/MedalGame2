using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

	public void TransitTitleScene()
	{
		SceneManager.LoadScene("Title");//タイトルシーンへ遷移する
	}
	public void TransitMainScene()
	{
		SceneManager.LoadScene("Main");//メインシーンへ遷移する
	}
}
