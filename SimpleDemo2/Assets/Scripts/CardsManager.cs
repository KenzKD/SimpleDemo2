using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum GridPattern
{
    Four,
    Six,
    Twelve,
    Sixteen,
    Thirty
}

[Serializable]
public class GridVariations
{
    public GridPattern gridPattern;
    public Vector2Int gridSize;
}

[Serializable]
public class CardData
{
    public int cardIndex;
    public int cardPairValue;
    public bool matched;
}

public class CardsManager : MonoBehaviour
{
    // Sprites
    public List<Sprite> cardSprites;

    // UI
    [SerializeField] private TextMeshProUGUI matchesTMP;
    [SerializeField] private TextMeshProUGUI turnsTMP;
    [SerializeField] private GameObject restartButton;

    // Grid Generation
    [SerializeField] private List<GridVariations> gridVariations;
    private GridVariations _selectedGridVariation;
    [SerializeField] private Transform difficultyToggleGroup;

    // Board Generation
    [SerializeField] private Transform boardParent;
    [SerializeField] private RectTransform cardColumn;
    [SerializeField] private GameObject cardPrefab;

    // Card related Variables
    private List<Card> _cards = new();
    private Card _firstCard;
    private Card _secondCard;

    // Singleton instance for easy access
    public static CardsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        _firstCard = null;
        _secondCard = null;

        SettingsManager.OnSettingsLoaded += OnSettingsLoaded;
    }

    private void OnSettingsLoaded(Settings settings)
    {
        SilentSelectGridPattern((int)settings.gridPattern);
        UpdateNoOfMatches();
        UpdateNoOfTurns();
        CheckIfAlreadyWon();
    }

    public void LoadToggleGroup()
    {
        difficultyToggleGroup.GetChild((int)SettingsManager.Instance.settings.gridPattern).GetComponent<Toggle>().isOn =
            true;
    }

    public void SelectGridPattern(int index)
    {
        SilentSelectGridPattern(index);
        AudioManager.Instance.PlayMenuSfx(GameSfx.Click);
    }

    private void SilentSelectGridPattern(int index)
    {
        _selectedGridVariation = gridVariations[index];
        SettingsManager.Instance.settings.gridPattern = _selectedGridVariation.gridPattern;
        SettingsManager.Instance.SaveSettingsFile();
        LoadToggleGroup();
    }

    public void CreatePairings()
    {
        ClearGrid();
        // cardSprites = cardSprites.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < _selectedGridVariation.gridSize.y * _selectedGridVariation.gridSize.x / 2; i++)
        for (int j = 0; j < 2; j++)
        {
            Card card = Instantiate(cardPrefab).GetComponent<Card>();
            card.forwardSprite = cardSprites[i];
            card.cardPairValue = i;
            _cards.Add(card);
        }

        _cards = _cards.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].cardIndex = i;
            SettingsManager.Instance.settings.cardData.Add(new CardData
                { cardIndex = i, cardPairValue = _cards[i].cardPairValue, matched = false });
        }

        SavePairings();
        ArrangeCards();
    }

    public void LoadPairings()
    {
        foreach (CardData data in SettingsManager.Instance.settings.cardData)
        {
            Card card = Instantiate(cardPrefab).GetComponent<Card>();
            card.forwardSprite = cardSprites[data.cardPairValue];
            card.cardPairValue = data.cardPairValue;
            _cards.Add(card);
        }

        ArrangeCards();
    }

    public void ArrangeCards()
    {
        restartButton.SetActive(true);
        for (int columnIndex = 0; columnIndex < _selectedGridVariation.gridSize.y; columnIndex++)
        {
            Transform currentColumn = Instantiate(cardColumn, boardParent);
            for (int rowIndex = 0; rowIndex < _selectedGridVariation.gridSize.x; rowIndex++)
            {
                int index = columnIndex * _selectedGridVariation.gridSize.x + rowIndex;

                _cards[index].transform.SetParent(currentColumn);
                _cards[index].StartingPosition();

                if (!SettingsManager.Instance.settings.cardData[index].matched)
                    _cards[index].UnFlipCard();
            }
        }

        GameManager.Instance.SetGameIsStarted(true);
    }

    public void ClearGrid()
    {
        foreach (Transform child in boardParent) Destroy(child.gameObject);

        SettingsManager.Instance.settings.noOfMatches = 0;
        UpdateNoOfMatches();

        SettingsManager.Instance.settings.noOfTurns = 0;
        UpdateNoOfTurns();

        SettingsManager.Instance.settings.cardData.Clear();
        _cards.Clear();
        restartButton.SetActive(true);
        SavePairings();
    }

    private void SavePairings()
    {
        SettingsManager.Instance.SaveSettingsFile();
    }

    public void CheckCardMatch(Card card)
    {
        SettingsManager.Instance.settings.noOfTurns++;
        UpdateNoOfTurns();

        if (_firstCard == null)
        {
            _firstCard = card;
            return;
        }

        if (_secondCard != null) return;

        _secondCard = card;
        if (_firstCard.cardPairValue == _secondCard.cardPairValue)
        {
            SettingsManager.Instance.settings.cardData[_firstCard.cardIndex].matched = true;
            Debug.Log("First Card Index: " + _firstCard.cardIndex);
            SettingsManager.Instance.settings.cardData[_secondCard.cardIndex].matched = true;
            Debug.Log("Second Card Index: " + _secondCard.cardIndex);
            SettingsManager.Instance.SaveSettingsFile();
            _firstCard.MatchedCard();
            _secondCard.MatchedCard();
            SettingsManager.Instance.settings.noOfMatches++;
            UpdateNoOfMatches();

            CheckWin();
        }
        else
        {
            AudioManager.Instance.PlaySfx(GameSfx.Wrong);
            _firstCard.DOKill();
            _secondCard.DOKill();
            _firstCard.UnFlipCard();
            _secondCard.UnFlipCard();
        }

        _firstCard = null;
        _secondCard = null;
    }

    private void CheckWin()
    {
        if (SettingsManager.Instance.settings.noOfMatches ==
            _selectedGridVariation.gridSize.y * _selectedGridVariation.gridSize.x / 2)
        {
            IncreaseDifficulty();
            GameManager.Instance.SetGameIsStarted(false);
            restartButton.SetActive(false);
            AudioManager.Instance.PlaySfx(GameSfx.Win);
            DOVirtual.DelayedCall(3f, () => GameManager.Instance.Restart());
        }
        else
        {
            AudioManager.Instance.PlaySfx(GameSfx.Match);
        }
    }

    private void IncreaseDifficulty()
    {
        int index = Math.Min((int)SettingsManager.Instance.settings.gridPattern + 1, gridVariations.Count - 1);
        SilentSelectGridPattern(index);
    }

    private void CheckIfAlreadyWon()
    {
        if (SettingsManager.Instance.settings.noOfMatches ==
            _selectedGridVariation.gridSize.y * _selectedGridVariation.gridSize.x / 2)
        {
            GameManager.Instance.Restart();
            Debug.Log("Already Won! Restarting...");
        }
    }

    private void UpdateNoOfMatches()
    {
        matchesTMP.text = SettingsManager.Instance.settings.noOfMatches.ToString();
        SettingsManager.Instance.SaveSettingsFile();
    }

    private void UpdateNoOfTurns()
    {
        turnsTMP.text = SettingsManager.Instance.settings.noOfTurns.ToString();
        SettingsManager.Instance.SaveSettingsFile();
    }
}