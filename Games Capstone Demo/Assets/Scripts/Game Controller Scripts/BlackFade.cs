using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackFade : MonoBehaviour
{
	//Vars
	public Image blackFader;
	public float maxFade;
	public float fadeDist;

	private GameObject closest;
	private float closeDist = 25;
	private GameObject player;
	private RaycastHit hit;

	public void Start()
	{
		player = GameObject.FindWithTag("Player");

		if (player == null)
		{
			Debug.LogError("Can't find player");
		}
	}

	public void LateUpdate()
	{
		//Raycast to get the true distance to the closest object (so size is accounted for)
		//if (player != null)
		//{
  //          player = GameObject.FindWithTag("Player");
  //      }

  //      if (player == null)
  //      {
  //          Debug.LogError("Wtf is this");
  //      }

        if (closest != null)
		{
			Debug.DrawRay(player.transform.position, -player.transform.right * 25);
			//Close to a boundary
			Physics.Raycast(player.transform.position, -player.transform.right,
				out hit, fadeDist, LayerMask.GetMask("Boundary"),UnityEngine.QueryTriggerInteraction.Collide);
			if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Boundary"))
			{
				closeDist = Vector3.Distance(player.transform.position, hit.point);
				FadeAmount(closeDist);
			}
			else
			{
				closest = null;
				closeDist = 25;
			}
			//Close to other fade triggers
			/*
			Physics.Raycast(player.transform.position, closest.transform.position-player.transform.position,
				out hit, fadeDist);
			if (hit.collider)
			{
				closeDist = Vector3.Distance(player.transform.position, hit.point);
				FadeAmount(closeDist);
			}
			else
			{
				closest = null;
				closeDist = 25;
			}
			*/
		}
	}

	//Set the fade 0-100% of maxFade
	public void SetFade(GameObject obj, float dist)
	{
		//Listen to only the closest object for the fade
		if (closest != obj && dist <= fadeDist && dist < closeDist)
		{
			closest = obj;
			closeDist = dist;
		}
		if (closest == obj && closeDist <= fadeDist)
		{
			FadeAmount(dist);
		}
	}

	//Reset the fade amount
	public void ResetFade()
	{
		blackFader.color = new Color(0, 0, 0, 0);
	}

	public void FadeAmount(float amount)
	{
		float percent = Mathf.InverseLerp(
			0, 255, (((((amount / fadeDist) - 1) * -1) * 100) / 100) * maxFade); //Quick Maff
		blackFader.color = new Color(0, 0, 0, percent);
	}
}
