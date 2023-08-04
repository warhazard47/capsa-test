using UnityEngine;
using System;
using System.Collections.Generic;

public class Inheritor<T> where T : class, new()
{
	public static T instance = new T();

	protected List<Card> card = null;
	protected Func<Card, bool> filter = null;

	protected List<ComboType> results = new List<ComboType>();
	public List<ComboType> Results
	{
		get
		{
			return results;
		}
	}

	public void Begin(List<Card> cards, Func<Card, bool> filterFunction = null)
	{
		results.Clear();
		card = cards;
		filter = filterFunction != null ? filterFunction : any => true;

		if (card == null)
			Debug.LogException(new Exception("Card is null"));
		if (card.Count == 0)
			Debug.LogException(new Exception("Cannot evaluate empty Card"));

		PreEvaluate();
	}

	protected virtual void PreEvaluate() { }

	public virtual void Evaluate(int index)
	{
		if (card == null)
		{
			Debug.LogException(new Exception("Evaluate is Called before Begin()"));
		}
	}

	protected virtual void PostEvaluate() { }

	public void End()
	{
		PostEvaluate();

		card = null;
		filter = null;
	}

	public List<ComboType> LazyEvaluator(List<Card> cards, bool all = false, Func<Card, bool> filterFunction = null)
	{
		Begin(cards, filterFunction);
		for (int i = 0; i < cards.Count; ++i)
		{
			Evaluate(i);
		}
		End();
		return results;
	}

	public virtual bool IsValid(List<Card> cards, bool isSorted = false)
	{
		return cards.Count > 0;
	}

}