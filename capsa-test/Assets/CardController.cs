using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Card : MonoBehaviour, IComparable<Card>
{
	public enum Elements
	{
		Diamond,
		Club,
		Heart,
		Spade
	}
	public static string[] numberOrder = { "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2" };

	Player player;
	public Elements elements;
	public Button button;
	public TextMeshProUGUI numberText;
	public TextMeshProUGUI elementText;
	int _score;
	string _nominal;
	RectTransform rt;

    public Player Player
    {
        get
        {
            if (player == null)
                player = GetComponentInParent<Player>();
            return player;
        }
    }

    public string Nominal
	{
		set
		{
			_nominal = value.ToUpper();
			_score = Array.IndexOf<string>(numberOrder, _nominal);
		}

		get
		{
			return _nominal;
		}
	}

	public int Score
	{
		get { return _score; }
	}

	void Awake()
	{
		button = GetComponent<Button>();
		rt = transform.GetChild(0).GetComponent<RectTransform>();
	}

	void Start()
	{
		numberText.text = _nominal;
		elementText.text = elements.ToString();
	}

	public void ToggleSelect()
	{
		if (rt.anchoredPosition == Vector2.zero)
		{
			rt.anchoredPosition = new Vector2(0, 20);
			Player.OnCardMarked(this);
		}
		else
		{
			rt.anchoredPosition = Vector2.zero;
			Player.OnCardUnmarked(this);
		}
	}

	public void Select(bool flag)
	{
		if (flag)
		{
			rt.anchoredPosition = new Vector2(0, 20);
			Player.OnCardMarked(this);
		}
		else
		{
			rt.anchoredPosition = Vector2.zero;
			Player.OnCardUnmarked(this);
		}
	}

	public static bool operator >(Card lhs, Card rhs)
	{
		return lhs.CompareTo(rhs) > 0;
	}

	public static bool operator <(Card lhs, Card rhs)
	{
		return lhs.CompareTo(rhs) < 0;
	}

	public int CompareTo(Card other)
	{
		if (_score == other._score)
			return (int)this.elements - (int)other.elements;
		else
			return _score - other._score;
	}
}