using System;
using System.Collections;
using System.Collections.Generic;

public class RoyalFlush : Inheritor<RoyalFlush> {
	StraightFlush evaluator = new StraightFlush();
	protected override void PreEvaluate () {
		evaluator.Begin (card, key => key.Nominal == "A");
	}

	public override void Evaluate(int index) {
		base.Evaluate (index);
		evaluator.Evaluate (index);
	}

	protected override void PostEvaluate () {
		evaluator.End ();
		results = evaluator.Results;
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		if (cards.Count != 5)
			return false;
		if (!isSorted)
			cards.Sort ();
		
		
		return Flush.instance.IsValid (cards, isSorted)
			&& cards [0].Nominal == "10"
			&& cards [1].Nominal == "J"
			&& cards [2].Nominal == "Q"
			&& cards [3].Nominal == "K"
			&& cards [4].Nominal == "A";
	}
}

