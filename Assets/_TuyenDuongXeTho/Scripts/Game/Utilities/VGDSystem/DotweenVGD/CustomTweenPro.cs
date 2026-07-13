using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening; // Bắt buộc phải có thư viện này
using TMPro;       // Bắt buộc phải có thư viện này
using System.Collections.Generic;

[AddComponentMenu("VGDTools/Custom Tween Pro")]
public class CustomTweenPro : MonoBehaviour
{
    // --- ENUMS CHO MANAGER ---
    public enum ManagerPreset { Custom, PoolingSystem }
    public enum EnableAction { None, Play, Restart, PlayBackwards, Complete, ResetToStart }
    public enum DisableAction { None, Pause, Rewind, Complete, Kill, ResetToStart }

    public enum TweenActionType
    {
        LocalMove, WorldMove, UIAnchorPos,
        LocalScale, UISizeDelta,
        LocalRotate,
        Fade, Color,
        Text,
        ShakePosition, ShakeRotation, ShakeScale,
        PunchPosition, PunchRotation, PunchScale,
        CameraFOV, CameraOrthoSize,
        AudioVolume, AudioPitch
    }

    public enum TweenPlayType { To, From }

    [Header("Manager Settings")]
    public ManagerPreset preset = ManagerPreset.Custom;
    public EnableAction onEnableAction = EnableAction.None;
    public DisableAction onDisableAction = DisableAction.None;

    [Header("Core Settings")]
    public string tweenId = "";
    public TweenActionType actionType = TweenActionType.LocalMove;
    public TweenPlayType playType = TweenPlayType.To;
    public UpdateType updateType = UpdateType.Normal;
    public bool ignoreTimeScale = false;

    public bool autoPlay = true;
    public bool autoKill = true;
    public bool isRelative = false;

    [Header("Tween Parameters")]
    public float duration = 1f;
    public float delay = 0f;
    public Ease easeType = Ease.OutQuad;
    public int loops = 0;
    public LoopType loopType = LoopType.Restart;

    [Header("Target Values")]
    public Vector3 targetVector3;
    public float targetFloat = 1f;
    public Color targetColor = Color.white;
    [TextArea] public string targetString = "";

    [Header("Special Effect Parameters")]
    public int vibrato = 10;
    public float elasticity = 1f;
    public float randomness = 90f;
    public bool snapping = false;
    public bool richTextEnabled = true;
    public ScrambleMode scrambleMode = ScrambleMode.None;

    [Header("Events")]
    public UnityEvent onStart;
    public UnityEvent onUpdate;
    public UnityEvent onStepComplete;
    public UnityEvent onComplete;

    // Biến quan trọng chứa Tween hiện tại
    private Tweener currentTween;

    // Global Registry
    private static Dictionary<string, List<CustomTweenPro>> allTweensByID = new Dictionary<string, List<CustomTweenPro>>();

    // --- Lazy Components ---
    private RectTransform _rect;
    private RectTransform Rect => _rect ??= GetComponent<RectTransform>();
    private Image _image;
    private Image Img => _image ??= GetComponent<Image>();
    private SpriteRenderer _sprite;
    private SpriteRenderer Sprite => _sprite ??= GetComponent<SpriteRenderer>();
    private CanvasGroup _canvasGroup;
    private CanvasGroup CanvasGrp => _canvasGroup ??= GetComponent<CanvasGroup>();
    private Text _text;
    private Text Txt => _text ??= GetComponent<Text>();
    private TMP_Text _tmpText;
    private TMP_Text TMPTxt => _tmpText ??= GetComponent<TMP_Text>();
    private Camera _cam;
    private Camera Cam => _cam ??= GetComponent<Camera>();
    private AudioSource _audio;
    private AudioSource AudioSrc => _audio ??= GetComponent<AudioSource>();

    // Trạng thái ban đầu để Reset
    private Vector3 initialPos;
    private Vector3 initialScale;
    private Quaternion initialRot;
    private float initialAlpha;

    void Awake()
    {
        // Lưu trạng thái gốc
        if (transform != null)
        {
            initialPos = transform.localPosition;
            initialScale = transform.localScale;
            initialRot = transform.localRotation;
        }

        if (CanvasGrp) initialAlpha = CanvasGrp.alpha;
        else if (Img) initialAlpha = Img.color.a;

        // Đăng ký ID
        if (!string.IsNullOrEmpty(tweenId))
        {
            if (!allTweensByID.ContainsKey(tweenId))
                allTweensByID[tweenId] = new List<CustomTweenPro>();
            allTweensByID[tweenId].Add(this);
        }
    }

    void Start()
    {
        // Logic cũ cho AutoPlay nếu không dùng Manager
        if (preset == ManagerPreset.Custom && onEnableAction == EnableAction.None && autoPlay)
        {
            Play();
        }
    }

    void OnEnable()
    {
        // Xử lý hành động khi Object được bật
        switch (onEnableAction)
        {
            case EnableAction.Play: Play(); break;
            case EnableAction.Restart: Restart(); break;
            case EnableAction.PlayBackwards: Rewind(); break;

            // SỬA LỖI Ở ĐÂY: Phải gọi currentTween.Complete()
            case EnableAction.Complete:
                if (currentTween != null) currentTween.Complete();
                break;

            case EnableAction.ResetToStart: ResetToInitialState(); break;

            case EnableAction.None:
                if (preset == ManagerPreset.Custom && autoPlay) Play();
                break;
        }
    }

    void OnDisable()
    {
        // Xử lý hành động khi Object bị tắt
        switch (onDisableAction)
        {
            case DisableAction.Pause: Pause(); break;
            case DisableAction.Rewind: Rewind(); break;

            // SỬA LỖI Ở ĐÂY: Phải gọi currentTween.Complete()
            case DisableAction.Complete:
                if (currentTween != null) currentTween.Complete();
                break;

            // SỬA LỖI Ở ĐÂY: Phải gọi currentTween.Kill()
            case DisableAction.Kill:
                if (currentTween != null) currentTween.Kill();
                break;

            case DisableAction.ResetToStart: ResetToInitialState(); break;

            case DisableAction.None:
                if (currentTween != null && currentTween.IsActive()) currentTween.Pause();
                break;
        }
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(tweenId) && allTweensByID.ContainsKey(tweenId))
        {
            allTweensByID[tweenId].Remove(this);
        }

        // SỬA LỖI: Luôn kiểm tra null trước khi gọi Kill
        if (currentTween != null && currentTween.IsActive()) currentTween.Kill();
    }

    public void Play()
    {
        // Xử lý Tween cũ trước khi chạy cái mới
        if (currentTween != null && currentTween.IsActive())
        {
            if (preset == ManagerPreset.PoolingSystem)
                currentTween.Restart(); // Pooling thì tái sử dụng
            else
                currentTween.Kill(); // Custom thì hủy cái cũ
        }

        Transform targetTr = Rect != null ? Rect : transform;

        // Chọn hiệu ứng dựa trên ActionType
        switch (actionType)
        {
            case TweenActionType.LocalMove: currentTween = targetTr.DOLocalMove(targetVector3, duration, snapping); break;
            case TweenActionType.WorldMove: currentTween = targetTr.DOMove(targetVector3, duration, snapping); break;
            case TweenActionType.UIAnchorPos: if (Rect) currentTween = Rect.DOAnchorPos(targetVector3, duration, snapping); break;
            case TweenActionType.LocalScale: currentTween = targetTr.DOScale(targetVector3, duration); break;
            case TweenActionType.UISizeDelta: if (Rect) currentTween = Rect.DOSizeDelta(targetVector3, duration, snapping); break;
            case TweenActionType.LocalRotate: currentTween = targetTr.DOLocalRotate(targetVector3, duration, RotateMode.FastBeyond360); break;

            case TweenActionType.Fade:
                if (CanvasGrp) currentTween = CanvasGrp.DOFade(targetFloat, duration);
                else if (Img) currentTween = Img.DOFade(targetFloat, duration);
                else if (Sprite) currentTween = Sprite.DOFade(targetFloat, duration);
                else if (TMPTxt) currentTween = TMPTxt.DOFade(targetFloat, duration);
                else if (Txt) currentTween = Txt.DOFade(targetFloat, duration);
                break;

            case TweenActionType.Color:
                if (Img) currentTween = Img.DOColor(targetColor, duration);
                else if (Sprite) currentTween = Sprite.DOColor(targetColor, duration);
                else if (TMPTxt) currentTween = TMPTxt.DOColor(targetColor, duration);
                else if (Txt) currentTween = Txt.DOColor(targetColor, duration);
                break;

            case TweenActionType.Text:
                if (Txt)
                {
                    currentTween = Txt.DOText(targetString, duration, richTextEnabled, scrambleMode);
                }
                else if (TMPTxt)
                {
                    // Đã sửa lỗi TMP bằng cách check define symbol
#if DOTWEEN_TMP
                    currentTween = TMPTxt.DOText(targetString, duration, richTextEnabled, scrambleMode);
#else
                    Debug.LogWarning("Vui lòng bật tính năng TextMeshPro trong DOTween Setup.");
#endif
                }
                break;

            case TweenActionType.ShakePosition: currentTween = targetTr.DOShakePosition(duration, targetVector3, vibrato, randomness, snapping, true); break;
            case TweenActionType.ShakeRotation: currentTween = targetTr.DOShakeRotation(duration, targetVector3, vibrato, randomness, true); break;
            case TweenActionType.ShakeScale: currentTween = targetTr.DOShakeScale(duration, targetVector3, vibrato, randomness, true); break;

            case TweenActionType.PunchPosition: currentTween = targetTr.DOPunchPosition(targetVector3, duration, vibrato, elasticity, snapping); break;
            case TweenActionType.PunchRotation: currentTween = targetTr.DOPunchRotation(targetVector3, duration, vibrato, elasticity); break;
            case TweenActionType.PunchScale: currentTween = targetTr.DOPunchScale(targetVector3, duration, vibrato, elasticity); break;

            case TweenActionType.CameraFOV: if (Cam) currentTween = Cam.DOFieldOfView(targetFloat, duration); break;
            case TweenActionType.CameraOrthoSize: if (Cam) currentTween = Cam.DOOrthoSize(targetFloat, duration); break;
            case TweenActionType.AudioVolume: if (AudioSrc) currentTween = AudioSrc.DOFade(targetFloat, duration); break;
            case TweenActionType.AudioPitch: if (AudioSrc) currentTween = AudioSrc.DOPitch(targetFloat, duration); break;
        }

        if (currentTween == null) return;

        // Thiết lập các thông số chung
        if (playType == TweenPlayType.From) currentTween.From();
        if (isRelative) currentTween.SetRelative(true);

        currentTween.SetEase(easeType)
                    .SetDelay(delay)
                    .SetAutoKill(autoKill)
                    .SetUpdate(updateType, ignoreTimeScale);

        // Events
        if (onStart.GetPersistentEventCount() > 0) currentTween.OnStart(onStart.Invoke);
        if (onUpdate.GetPersistentEventCount() > 0) currentTween.OnUpdate(onUpdate.Invoke);
        if (onStepComplete.GetPersistentEventCount() > 0) currentTween.OnStepComplete(onStepComplete.Invoke);
        if (onComplete.GetPersistentEventCount() > 0) currentTween.OnComplete(onComplete.Invoke);

        if (loops != 0) currentTween.SetLoops(loops, loopType);
    }

    // Các hàm Helper công khai
    public void Restart() { currentTween?.Restart(); }
    public void Pause() { currentTween?.Pause(); }
    public void Resume() { currentTween?.Play(); }
    public void Rewind() { currentTween?.PlayBackwards(); }

    private void ResetToInitialState()
    {
        if (currentTween != null) currentTween.Kill();

        transform.localPosition = initialPos;
        transform.localScale = initialScale;
        transform.localRotation = initialRot;
        if (CanvasGrp) CanvasGrp.alpha = initialAlpha;
        else if (Img)
        {
            var c = Img.color; c.a = initialAlpha; Img.color = c;
        }
    }

    // Static Function
    public static void PlayByID(string idToPlay)
    {
        if (allTweensByID.ContainsKey(idToPlay))
        {
            foreach (var tween in allTweensByID[idToPlay])
            {
                if (tween != null) tween.Play();
            }
        }
    }
}