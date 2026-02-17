using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    // Constants
    private const float CARD_ANIM_TIME = 0.1f;
    private const float CARD_RESET_TIME = 1f;

    // Sprites
    [SerializeField] private Sprite backSprite;

    // Public
    public int cardIndex;
    public int cardPairValue;
    public Sprite forwardSprite;

    // Card components
    private Transform _card;
    private Button _cardButton;
    private Image _cardImage;

    // Animation Sequences
    private Sequence _cardFlipSequence;
    private Sequence _cardUnFlipSequence;

    // Start is called before the first frame update
    private void Awake()
    {
        _card = transform;
        _cardButton = _card.GetComponent<Button>();
        _cardImage = _card.GetComponent<Image>();
    }

    public void FlipCard()
    {
        _cardFlipSequence = DOTween.Sequence();
        _cardFlipSequence.AppendCallback(() => AudioManager.Instance.PlaySfx(GameSfx.Flip));
        _cardFlipSequence.AppendCallback(() => _cardButton.interactable = false);
        _cardFlipSequence.Append(DOVirtual.DelayedCall(CARD_ANIM_TIME / 2, () => _cardImage.sprite = forwardSprite));
        _cardFlipSequence.Join(_card.DORotate(Vector3.up * 180, CARD_ANIM_TIME).SetEase(Ease.InOutSine));
    }

    public void UnFlipCard()
    {
        _cardUnFlipSequence = DOTween.Sequence();
        _cardUnFlipSequence.AppendInterval(CARD_RESET_TIME);
        // _cardUnFlipSequence.AppendCallback(() => AudioManager.Instance.PlaySfx(GameSfx.Flip));
        _cardUnFlipSequence.Append(DOVirtual.DelayedCall(CARD_ANIM_TIME / 2, () => _cardImage.sprite = backSprite));
        _cardUnFlipSequence.Join(_card.DORotate(Vector3.zero, CARD_ANIM_TIME).SetEase(Ease.InOutSine));
        _cardUnFlipSequence.AppendCallback(() => _cardButton.interactable = true);
    }

    public void MatchedCard()
    {
        _card.DOKill();
        _card.rotation = Quaternion.Euler(0, 180, 0);
        // StartingPosition();
        _card.DOShakeRotation(CARD_RESET_TIME, new Vector3(0, -1, 3), 10, 90, true, ShakeRandomnessMode.Harmonic);
    }

    public void StartingPosition()
    {
        _card.DOKill();
        _card.rotation = Quaternion.Euler(0, 180, 0);
        _cardImage.sprite = forwardSprite;
        _cardButton.interactable = false;
    }
}