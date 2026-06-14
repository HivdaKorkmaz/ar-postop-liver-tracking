using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

/// <summary>
/// Bilgi Paneli UI Kontrolcusu — Mevcut hiyerarsiyi bozmaz,
/// sadece gorsel ozellikleri runtime'da modernize eder + Pop animasyonu ekler.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class InfoPanelController : MonoBehaviour
{
    // ─────────────── Inspector Referanslari ───────────────

    [Header("UI Bileşenleri")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image _iconCircle;
    [SerializeField] private RawImage _videoFrame;
    [SerializeField] private TextMeshProUGUI _noVideoText;
    [SerializeField] private Button _closeButton;
    [SerializeField] private RectTransform _panelRect;

    [Header("Video Oynatıcı")]
    [SerializeField] private VideoPlayer _videoPlayer;

    [Header("Animasyon Ayarları")]
    [Tooltip("Panel slide-in animasyon süresi (saniye)")]
    [SerializeField] private float _animDuration = 0.35f;
    [Tooltip("Panel başlangıç pozisyonu (ekran altından kayar)")]
    [SerializeField] private float _slideOffset = 600f;
    [Tooltip("Pop animasyon süresi (saniye)")]
    [SerializeField] private float _popDuration = 0.25f;

    // ─────────────── Ozel Alanlar ───────────────

    private CanvasGroup _canvasGroup;
    private Vector2 _openPosition;
    private LiverAssistantManager _manager;
    private RenderTexture _videoRenderTexture;


    // Tema renkleri — Acik, modern, ferah palet
    private static readonly Color GlassBg = new Color(0.96f, 0.97f, 0.98f, 0.92f);     // #F5F8FA buzlu beyaz
    private static readonly Color TitleColor = new Color(0.15f, 0.20f, 0.32f, 1f);     // #263352 koyu lacivert
    private static readonly Color SubText = new Color(0.28f, 0.32f, 0.40f, 1f);        // #475266 koyu gri

    // ─────────────── Unity Lifecycle ───────────────

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _manager = FindFirstObjectByType<LiverAssistantManager>();

        if (_panelRect != null)
            _openPosition = _panelRect.anchoredPosition;

        if (_closeButton != null)
            _closeButton.onClick.AddListener(() => _manager?.ClosePanel());

        SetupVideoPlayer();
        HideImmediate();
    }

    // ─────────────── Gorsel Modernizasyon ───────────────

    /// <summary>
    /// Paneldeki TUM Image ve TMP bilesenlerini bulur ve koyu temaya cevirir.
    /// Hiyerarsi isimlerine bagimli degildir — tum child'lari tarar.
    /// </summary>
    private void ApplyModernTheme()
    {
        // ── 1. ARKA PLAN: Paneldeki tum Image bilesenlerini koyu yap ──
        // Oncelikle panelin kendi Image'i
        Image selfImg = GetComponent<Image>();
        if (selfImg != null)
            selfImg.color = GlassBg;

        // panelRect uzerindeki Image
        if (_panelRect != null)
        {
            Image panelImg = _panelRect.GetComponent<Image>();
            if (panelImg != null)
                panelImg.color = GlassBg;
        }

        // PanelBG adli child'i bul (tum derinlikte ara)
        Transform bgChild = FindDeep(transform, "PanelBG");
        if (bgChild != null)
        {
            Image bgImg = bgChild.GetComponent<Image>();
            if (bgImg != null)
                bgImg.color = GlassBg;
        }

        // ── 2. METINLER: Tum TMP bilesenlerini acik renge cevir + golge ekle ──
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in allTexts)
        {
            // Baslik mi icerik mi ayirt et
            bool isTitle = (tmp == _titleText) ||
                           tmp.gameObject.name.ToLower().Contains("title") ||
                           tmp.gameObject.name.ToLower().Contains("baslik");

            tmp.color = isTitle ? TitleColor : SubText;
            ApplyUnderlay(tmp);
        }

        // ── 3. CLOSE BUTTON arka plan (varsa) ──
        if (_closeButton != null)
        {
            Image btnImg = _closeButton.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = new Color(0.90f, 0.92f, 0.96f, 0.95f); // acik lavanta
        }

        Debug.Log("[KaracigerAR] Modern tema uygulandi.");
    }

    /// <summary>
    /// TMP metnine runtime'da Underlay (golge) ekler.
    /// </summary>
    private void ApplyUnderlay(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        Material mat = tmp.fontMaterial;
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor("_UnderlayColor", new Color(0.35f, 0.45f, 0.62f, 0.18f)); // hafif mavi golge
        mat.SetFloat("_UnderlayOffsetX", 0.5f);
        mat.SetFloat("_UnderlayOffsetY", -0.5f);
        mat.SetFloat("_UnderlayDilate", 0.1f);
        mat.SetFloat("_UnderlaySoftness", 0.5f);
        tmp.fontMaterial = mat;
        tmp.UpdateMeshPadding();
    }

    /// <summary>
    /// Transform hiyerarsisinde recursive olarak isimle child arar.
    /// </summary>
    private Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    // ─────────────── Pop Animasyonu (Ease-Out Bounce) ───────────────

    /// <summary>
    /// Scale 0 -> overshoot(1.08) -> settle(1.0) seklinde tatli bir Pop efekti.
    /// </summary>
    private IEnumerator PopAnimation()
    {
        if (_panelRect == null) yield break;

        Transform t = _panelRect.transform;
        float half = _popDuration * 0.55f;
        float settle = _popDuration * 0.45f;

        // Faz 1: 0 -> 1.08 (hizli buyume)
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / half);
            float s = Mathf.Lerp(0f, 1.08f, p);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        // Faz 2: 1.08 -> 1.0 (yumusak oturma)
        elapsed = 0f;
        while (elapsed < settle)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / settle);
            float s = Mathf.Lerp(1.08f, 1.0f, p);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    // ─────────────── Public API ───────────────

    public void Show(ZoneContent content)
    {
        if (content == null) return;

        gameObject.SetActive(true);

        // Gorsel temayi uygula (sadece ilk seferde)
        ApplyModernTheme();

        // Icerigi doldur
        PopulateContent(content);

        // Baslangic durumu
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;
        }

        if (_panelRect != null)
        {
            _panelRect.anchoredPosition = _openPosition + new Vector2(0, -_slideOffset);
            _panelRect.transform.localScale = Vector3.zero; // Pop icin sifirdan baslat
        }
    }

    public IEnumerator ShowAnimated()
    {
        if (_canvasGroup == null) yield break;

        // Slide-in + Fade-in
        float elapsed = 0f;
        Vector2 startPos = _openPosition + new Vector2(0, -_slideOffset);

        while (elapsed < _animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / _animDuration);

            if (_panelRect != null)
                _panelRect.anchoredPosition = Vector2.Lerp(startPos, _openPosition, t);
            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        if (_panelRect != null)
            _panelRect.anchoredPosition = _openPosition;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        // Pop animasyonu (slide bittikten sonra)
        yield return PopAnimation();
    }

    public IEnumerator HideAnimated()
    {
        if (_canvasGroup == null)
        {
            HideImmediate();
            yield break;
        }

        _canvasGroup.interactable = false;

        float elapsed = 0f;
        Vector2 endPos = _openPosition + new Vector2(0, -_slideOffset);
        Vector2 startPos = _panelRect != null ? _panelRect.anchoredPosition : _openPosition;

        while (elapsed < _animDuration * 0.7f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / (_animDuration * 0.7f));

            if (_panelRect != null)
                _panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        StopVideoIfPlaying();
        gameObject.SetActive(false);
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public void HideImmediate()
    {
        StopVideoIfPlaying();
        gameObject.SetActive(false);
        if (_panelRect != null)
            _panelRect.transform.localScale = Vector3.one;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public Rect GetPanelScreenRect()
    {
        if (_panelRect == null) return Rect.zero;
        Vector3[] corners = new Vector3[4];
        _panelRect.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y,
                        corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    // ─────────────── Icerik Doldurma ───────────────

    private void PopulateContent(ZoneContent content)
    {
        if (_titleText != null)
            _titleText.text = content.PanelTitle;

        if (_descriptionText != null)
            _descriptionText.text = content.PanelDescription;

        if (_iconCircle != null)
            _iconCircle.color = content.ThemeColor;

        TryPlayVideo(content.VideoResourcePath);
    }

    // ─────────────── Video ───────────────

    private void SetupVideoPlayer()
    {
        if (_videoPlayer == null)
            _videoPlayer = GetComponentInChildren<VideoPlayer>();
        if (_videoPlayer == null) return;

        _videoRenderTexture = new RenderTexture(1280, 720, 0);
        _videoPlayer.targetTexture = _videoRenderTexture;

        if (_videoFrame != null)
            _videoFrame.texture = _videoRenderTexture;

        _videoPlayer.isLooping = false;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
    }

    private void TryPlayVideo(string resourcePath)
    {
        bool hasVideo = !string.IsNullOrEmpty(resourcePath);

        if (_videoFrame != null)
            _videoFrame.gameObject.SetActive(hasVideo);
        if (_noVideoText != null)
            _noVideoText.gameObject.SetActive(!hasVideo);

        if (!hasVideo || _videoPlayer == null) return;

        var clip = Resources.Load<VideoClip>(resourcePath);
        if (clip != null)
        {
            _videoPlayer.clip = clip;
            _videoPlayer.Play();
        }
        else
        {
            string streamingPath = System.IO.Path.Combine(
                Application.streamingAssetsPath,
                resourcePath.Replace("Videos/", "") + ".mp4");

            if (System.IO.File.Exists(streamingPath))
            {
                _videoPlayer.url = "file://" + streamingPath;
                _videoPlayer.Play();
            }
            else
            {
                Debug.Log($"[KaracigerAR] Video bulunamadi: {resourcePath}");
                if (_videoFrame != null) _videoFrame.gameObject.SetActive(false);
                if (_noVideoText != null)
                {
                    _noVideoText.gameObject.SetActive(true);
                    _noVideoText.text = "Video hazirlaniyor...\n\nYukaridaki bilgileri okuyabilirsiniz.";
                }
            }
        }
    }

    private void StopVideoIfPlaying()
    {
        if (_videoPlayer != null && _videoPlayer.isPlaying)
            _videoPlayer.Stop();
    }

    private void OnDestroy()
    {
        if (_videoRenderTexture != null)
            _videoRenderTexture.Release();
    }
}
