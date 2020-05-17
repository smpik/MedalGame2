using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDeleterController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "tag_Ball")
		{
			Destroy(collision.gameObject);
		}
	}
}
