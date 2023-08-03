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

public class Flush : Inheritor<Flush>
{
	List<Card> spades = new List<Card>();
	List<Card> hearts = new List<Card>();
	List<Card> clubs = new List<Card>();
	List<Card> diamonds = new List<Card>();

	protected override void PreEvaluate()
	{
		spades.Clear();
		hearts.Clear();
		clubs.Clear();
		diamonds.Clear();
	}

	public override void Evaluate(int index)
	{
		base.Evaluate(index);

		switch (card[index].elements)
		{
			case Card.Elements.Spade:
				spades.Add(card[index]);
				break;
			case Card.Elements.Heart:
				hearts.Add(card[index]);
				break;
			case Card.Elements.Club:
				clubs.Add(card[index]);
				break;
			case Card.Elements.Diamond:
				diamonds.Add(card[index]);
				break;
		}
	}

	protected override void PostEvaluate()
	{
		FilterCards(spades);
		FilterCards(hearts);
		FilterCards(clubs);
		FilterCards(diamonds);

		results.Sort();
	}

	void FilterCards(List<Card> card)
	{
		for (int i = 0; i < Mathf.Max(0, card.Count - 4); ++i)
		{
			List<Card> set = card.GetRange(i, 5);
			if (filter(set[4]))
				results.Add(new ComboType(set, set[4], ComboType.CombinationType.Flush));
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 5)
			return false;

		return cards[0].elements == cards[1].elements
			&& cards[1].elements == cards[2].elements
			&& cards[2].elements == cards[3].elements
			&& cards[3].elements == cards[4].elements;
	}
}

public class StraightFlush : Inheritor<StraightFlush>
{
	List<Card> bucketSpades = new List<Card>();
	List<Card> specialSpades = new List<Card>();

	List<Card> bucketHearts = new List<Card>();
	List<Card> specialHearts = new List<Card>();

	List<Card> bucketClubs = new List<Card>();
	List<Card> specialClubs = new List<Card>();

	List<Card> bucketDiamonds = new List<Card>();
	List<Card> specialDiamonds = new List<Card>();

	protected override void PreEvaluate()
	{
		Straight.PreEvaluateStraight(ref bucketSpades, ref specialSpades);
		Straight.PreEvaluateStraight(ref bucketHearts, ref specialHearts);
		Straight.PreEvaluateStraight(ref bucketClubs, ref specialClubs);
		Straight.PreEvaluateStraight(ref bucketDiamonds, ref specialDiamonds);
	}

	public override void Evaluate(int index)
	{
		switch (card[index].elements)
		{
			case Card.Elements.Spade:
				Straight.EvaluateStraight(ref results, card[index], ref bucketSpades, ref specialSpades, filter);
				break;
			case Card.Elements.Heart:
				Straight.EvaluateStraight(ref results, card[index], ref bucketHearts, ref specialHearts, filter);
				break;
			case Card.Elements.Club:
				Straight.EvaluateStraight(ref results, card[index], ref bucketClubs, ref specialClubs, filter);
				break;
			case Card.Elements.Diamond:
				Straight.EvaluateStraight(ref results, card[index], ref bucketDiamonds, ref specialDiamonds, filter);
				break;
		}
	}

	protected override void PostEvaluate()
	{
		Straight.PostEvaluateStraight(ref results, ref bucketSpades, ref specialSpades);
		Straight.PostEvaluateStraight(ref results, ref bucketHearts, ref specialHearts);
		Straight.PostEvaluateStraight(ref results, ref bucketClubs, ref specialClubs);
		Straight.PostEvaluateStraight(ref results, ref specialDiamonds, ref specialDiamonds);

		results.Sort();
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		return Flush.instance.IsValid(cards, isSorted) && Straight.instance.IsValid(cards, isSorted);
	}
}

public class RoyalFlush : Inheritor<RoyalFlush>
{
	StraightFlush evaluator = new StraightFlush();
	protected override void PreEvaluate()
	{
		evaluator.Begin(card, key => key.Nominal == "A");
	}

	public override void Evaluate(int index)
	{
		base.Evaluate(index);
		evaluator.Evaluate(index);
	}

	protected override void PostEvaluate()
	{
		evaluator.End();
		results = evaluator.Results;
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 5)
			return false;
		if (!isSorted)
			cards.Sort();


		return Flush.instance.IsValid(cards, isSorted)
			&& cards[0].Nominal == "10"
			&& cards[1].Nominal == "J"
			&& cards[2].Nominal == "Q"
			&& cards[3].Nominal == "K"
			&& cards[4].Nominal == "A";
	}
}

public class Dragon : Inheritor<Dragon>
{
	List<Card> dragon = new List<Card>();

	protected override void PreEvaluate()
	{
		dragon.Clear();
		dragon.Add(card[0]);
	}

	public override void Evaluate(int index)
	{
		base.Evaluate(index);

		if (dragon[dragon.Count - 1].Score + 1 == card[index].Score)
		{
			dragon.Add(card[index]);
		}
	}

	protected override void PostEvaluate()
	{
		if (dragon.Count == 13)
			results.Add(new ComboType(dragon, dragon[dragon.Count - 1], ComboType.CombinationType.Dragon));
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 13)
			return false;

		if (!isSorted)
			cards.Sort();

		for (int i = 0; i < cards.Count - 1; ++i)
		{
			if (cards[i].Score + 1 != cards[i + 1].Score)
				return false;
		}

		return true;
	}
}

public class Straight : Inheritor<Straight>
{

	List<Card> bucket = new List<Card>();
	List<Card> special = new List<Card>();

	protected override void PreEvaluate()
	{
		PreEvaluateStraight(ref bucket, ref special);
	}

	public override void Evaluate(int index)
	{
		EvaluateStraight(ref results, card[index], ref bucket, ref special, filter);
	}

	protected override void PostEvaluate()
	{
		PostEvaluateStraight(ref results, ref bucket, ref special);
		results.Sort();
	}

	public static void PreEvaluateStraight(ref List<Card> bucket, ref List<Card> special)
	{
		bucket.Clear();
		special.Clear();
	}

	public static void EvaluateStraight(ref List<ComboType> results, Card card, ref List<Card> bucket, ref List<Card> special, Func<Card, bool> filter)
	{
		if (bucket.Count > 0)
		{
			int step = card.Score - bucket[bucket.Count - 1].Score;

			if (step == 1)
				bucket.Add(card);
			else if (step > 1)
				bucket.Clear();

			// If 5 combination already found, add to result and remove first item for next possibility
			if (bucket.Count == 5)
			{
				if (filter(bucket[4]))
					results.Add(new ComboType(new List<Card>(bucket), bucket[4], ComboType.CombinationType.Straight));
				bucket.RemoveAt(0);
			}
		}
		else
		{
			bucket.Add(card);
		}

		// Special Straight evaluation
		if (special.Count == 0 && card.Nominal == "3")
			special.Add(card);

		if (special.Count > 0)
		{
			var curr_n = card.Nominal;
			var prev_n = special[special.Count - 1].Nominal;
			if (prev_n == "3" && curr_n == "4"
				|| prev_n == "4" && curr_n == "5"
				|| prev_n == "5" && curr_n == "6"
				|| ((prev_n == "5" || prev_n == "6")
				&& (curr_n == "A" || curr_n == "2")))
				special.Add(card);
		}
	}

	public static void PostEvaluateStraight(ref List<ComboType> results, ref List<Card> bucket, ref List<Card> special)
	{
		if (special.Count >= 5 && special[special.Count - 1].Nominal == "2")
		{
			if (special.Count == 5)
			{
				results.Add(new ComboType(special, special[4], ComboType.CombinationType.Straight));
			}
			else if (special.Count == 6)
			{
				var set = new List<Card>();

				// 2-3-4-5-6
				set.Add(special[special.Count - 1]); // 2
				set.AddRange(special.GetRange(0, 4)); // 3-4-5-6
				results.Add(new ComboType(set, set[0], ComboType.CombinationType.Straight));

				// A-2-3-4-5
				set = special.GetRange(special.Count - 2, 2); // A-2
				set.AddRange(special.GetRange(0, 3)); // 3-4-5
				results.Add(new ComboType(set, set[1], ComboType.CombinationType.Straight));
			}
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 5)
			return false;

		if (!isSorted)
		{
			// Special Case
			if (cards[0].Nominal == "2"
				&& cards[1].Nominal == "3"
				&& cards[2].Nominal == "4"
				&& cards[3].Nominal == "5"
				&& cards[4].Nominal == "6")
				return true;

			if (cards[0].Nominal == "A"
				&& cards[1].Nominal == "2"
				&& cards[2].Nominal == "3"
				&& cards[3].Nominal == "4"
				&& cards[4].Nominal == "5")
				return true;

			cards.Sort();
		}
		else
		{
			// Special Case
			if (cards[0].Nominal == "3"
				&& cards[1].Nominal == "4"
				&& cards[2].Nominal == "5"
				&& cards[3].Nominal == "6"
				&& cards[4].Nominal == "2")
				return true;

			if (cards[0].Nominal == "3"
				&& cards[1].Nominal == "4"
				&& cards[2].Nominal == "5"
				&& cards[3].Nominal == "A"
				&& cards[4].Nominal == "2")
				return true;
		}

		// Normal case
		for (int i = 0; i < cards.Count - 1; ++i)
		{
			if (cards[i].Score + 1 != cards[i + 1].Score)
				return false;
		}

		return true;
	}
}

public class One : Inheritor<One>
{
	public override void Evaluate(int index)
	{
		if (filter(card[index]))
		{
			var set = new List<Card>();
			set.Add(card[index]);
			results.Add(new ComboType(set, set[0], ComboType.CombinationType.One));
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		return cards.Count == 1;
	}
}
public class Triple : Inheritor<Triple>
{


	public override void Evaluate(int index)
	{
		// Max index is (n-1) - 2, don't evaluate
		if (index > card.Count - 3)
			return;

		// Compare first and last nominal, since it's already sorted
		Card[] triple = { card[index], card[index + 1], card[index + 2] };
		if (triple[0].Score == triple[1].Score && triple[1].Score == triple[2].Score && filter(triple[2]))
		{
			results.Add(new ComboType(new List<Card>(triple), triple[2], ComboType.CombinationType.Triple));
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 3)
			return false;

		return cards[0].Nominal == cards[1].Nominal
			&& cards[1].Nominal == cards[2].Nominal;
	}
}
public class Pair : Inheritor<Pair>
{

	public override void Evaluate(int index)
	{
		base.Evaluate(index);

		// Max index is (n-1) - 1, don't evaluate
		if (index > card.Count - 2)
			return;

		Card[] pair = { card[index], card[index + 1] };
		if (pair[0].Score == pair[1].Score && filter(pair[1]))
		{
			results.Add(new ComboType(card, pair[1], ComboType.CombinationType.Pair));
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 2)
			return false;

		return cards[0].Score == cards[1].Score;
	}
}

public class FourOfAKind : Inheritor<FourOfAKind>
{
	List<Card> card = new List<Card>();
	public override void Evaluate(int index)
	{
		base.Evaluate(index);

		// 5 card is needed at least and Max index is (n-1) - 3, don't evaluate
		if (card.Count < 5 || index > card.Count - 4)
			return;

		// Compare first and last nominal, since it's already sorted
		Card[] quad = { card[index], card[index + 1], card[index + 2], card[index + 3] };
		if (quad[0].Nominal == quad[3].Nominal && filter(quad[3]))
		{
			var set = new List<Card>();
			set.Add(card[index == 0 ? 4 : 0]);
			set.AddRange(quad);
			results.Add(new ComboType(set, set[4], ComboType.CombinationType.FourOfAKind));
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 5)
			return false;

		if (!isSorted)
			cards.Sort();

		return cards[0].Nominal == cards[3].Nominal
			|| cards[1].Nominal == cards[4].Nominal;
	}
}

public class FullHouse : Inheritor<FullHouse>
{
	public List<ComboType> triples = null;
	public List<ComboType> pairs = null;
	Triple tripleEvaluator = new Triple();
	Pair pairEvaluator = new Pair();

	protected override void PreEvaluate()
	{
		if (triples != null || pairs != null)
			return;
		tripleEvaluator.Begin(card, filter);
		pairEvaluator.Begin(card, filter);
	}

	public override void Evaluate(int index)
	{
		base.Evaluate(index);
		if (triples != null || pairs != null)
			return;

		tripleEvaluator.Evaluate(index);
		pairEvaluator.Evaluate(index);
	}

	protected override void PostEvaluate()
	{
		if (triples == null && pairs == null)
		{
			tripleEvaluator.End();
			pairEvaluator.End();

			triples = tripleEvaluator.Results;
			pairs = pairEvaluator.Results;
		}

		List<Card> fullHouse = new List<Card>(5);
		for (int tIndex = 0; tIndex < triples.Count; ++tIndex)
		{
			fullHouse.Clear();
			for (int pIndex = 0; pIndex < pairs.Count; ++pIndex)
			{
				if (triples[tIndex].Cards[0].Nominal != pairs[pIndex].Cards[0].Nominal)
				{
					fullHouse.AddRange(pairs[pIndex].Cards);
					fullHouse.AddRange(triples[tIndex].Cards);
					results.Add(new ComboType(fullHouse, triples[tIndex].Cards[2], ComboType.CombinationType.FullHouse));
					break;
				}
			}
		}

		triples = null;
		pairs = null;
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false)
	{
		if (cards.Count != 5)
			return false;

		if (!isSorted)
			cards.Sort();

		return cards[0].Nominal == cards[1].Nominal && cards[1].Nominal == cards[2].Nominal && cards[3].Nominal == cards[4].Nominal
			|| cards[0].Nominal == cards[1].Nominal && cards[2].Nominal == cards[3].Nominal && cards[3].Nominal == cards[4].Nominal;
	}
}