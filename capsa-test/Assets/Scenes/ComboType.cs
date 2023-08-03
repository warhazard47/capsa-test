using UnityEngine;
using System;
using System.Collections.Generic;


public class ComboType : MonoBehaviour
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
}