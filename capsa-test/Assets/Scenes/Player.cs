using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
	public int id = -1;
	List<Card> cards;
	bool isPass = false;

	public bool Artificial
	{
		get { return id != 0; }
	}

	public List<Card> Cards
	{
		get { return cards; }
		set
		{
			cards = value;
			cards.Sort();
		}
	}

	public bool IsPass
	{
		get { return isPass; }
		set
		{
			isPass = value;
		}
	}

	public void OnTurnBegin()
	{
		StartCoroutine("OnTurn");
	}

	IEnumerator OnTurn()
	{
		float time = 15f;
		float lastUpdateTime = Time.time;

		while (time > 0f)
		{
			yield return new WaitForSeconds(0.1f);

			time -= (Time.time - lastUpdateTime);
			lastUpdateTime = Time.time;
		}

	}
	public void OnTurnEnd()
	{
		StopCoroutine("Analyze");
		StopCoroutine("OnTurn");
	}

	public void Deal(List<Card> dealCards)
	{
		if (dealCards.Count == 0)
			return;
	}

	public void Pass()
	{
		isPass = true;
	}

}