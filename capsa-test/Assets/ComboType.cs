using UnityEngine;
using System;
using System.Collections.Generic;


public class ComboType : IComparable<ComboType>
{
	public enum CombinationType
	{
		Invalid,
		One,
		Pair,
		Triple,
		Straight,
		Flush,
		FullHouse,
		FourOfAKind,
		StraightFlush,
		RoyalFlush,
		Dragon
	}

	private Card key;
	private List<Card> cards;
	private CombinationType combination;

	public ComboType(List<Card> cards, Card key, CombinationType combination)
	{
		this.cards = cards;
		this.key = key;
		this.combination = combination;
	}

	public static ComboType Make(List<Card> cards)
	{
		cards.Sort();
		var key = cards[cards.Count - 1];
		var combination = CombinationType.Invalid;

        if (cards.Count > 5)
        {
            if (Dragon.instance.IsValid(cards, true))
                combination = CombinationType.Dragon;
        }
        else if (cards.Count == 5)
        {
            if (RoyalFlush.instance.IsValid(cards, true))
                combination = CombinationType.RoyalFlush;
            else if (StraightFlush.instance.IsValid(cards, true))
                combination = CombinationType.StraightFlush;
            else if (FourOfAKind.instance.IsValid(cards, true))
                combination = CombinationType.FourOfAKind;
            else if (FullHouse.instance.IsValid(cards, true))
                combination = CombinationType.FullHouse;
            else if (Flush.instance.IsValid(cards, true))
                combination = CombinationType.Flush;
            else if (Straight.instance.IsValid(cards, true))
                combination = CombinationType.Straight;
            else
                combination = CombinationType.Invalid;
        }
        else
        {
            if (Triple.instance.IsValid(cards, true))
                combination = CombinationType.Triple;
            else if (Pair.instance.IsValid(cards, true))
                combination = CombinationType.Pair;
            else if (One.instance.IsValid(cards, true))
                combination = CombinationType.One;
            else
                combination = CombinationType.Invalid;
        }

        return new ComboType(new List<Card>(cards.ToArray()), key, combination);
	}

	public Card Key
	{
		get { return key; }
	}

	public List<Card> Cards
	{
		get { return cards; }
	}

	public CombinationType Combination
	{
		get { return combination; }
	}

	public static bool operator >(ComboType lhs, ComboType rhs)
	{
		return lhs.CompareTo(rhs) > 0;
	}

	public static bool operator <(ComboType lhs, ComboType rhs)
	{
		return lhs.CompareTo(rhs) < 0;
	}

	public int CompareTo(ComboType other)
	{
		if (combination == other.combination)
			return key.CompareTo(other.key);
		else
			return (int)combination - (int)other.combination;
	}
}



