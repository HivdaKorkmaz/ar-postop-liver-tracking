using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HologramUyariManager : MonoBehaviour
{
    private struct MitGercek
    {
        public string Soru;
        public bool Cevap;
        public string Uyari;
    }

    private Canvas _anaCanvas;
    private List<MitGercek> _tumVeriler = new List<MitGercek>();
    private List<MitGercek> _aktifVeriler = new List<MitGercek>();
    private List<GameObject> _hotspotlar = new List<GameObject>();
    private List<Image> _hotspotImgList = new List<Image>();
    private GameObject _soruPaneli;
    private GameObject _arkaPlanPaneli;
    private bool _animasyonAktif = false;
    private int _tamamlananKutu = 0;

    private void Start()
    {
        VerileriYukle();
        AktifSorulariSec();
    }

    // Otomatik acilma yok! Sadece disaridan cagirilinca acilir.
    public void PaneliGoster()
    {
        if (_tumVeriler.Count == 0) { VerileriYukle(); AktifSorulariSec(); }
        _anaCanvas = FindFirstObjectByType<Canvas>();
        if (_anaCanvas != null)
        {
            HotspotlariOlustur();
            _animasyonAktif = true;
        }
    }

    public void PaneliKapat()
    {
        _animasyonAktif = false;
        if (_arkaPlanPaneli != null) { Destroy(_arkaPlanPaneli); _arkaPlanPaneli = null; }
        if (_soruPaneli != null) { Destroy(_soruPaneli); _soruPaneli = null; }
        foreach (GameObject h in _hotspotlar) { if (h != null) Destroy(h); }
        _hotspotlar.Clear();
        _hotspotImgList.Clear();
    }

    private void Update()
    {
        if (!_animasyonAktif) return;
        float alpha = Mathf.PingPong(Time.time * 1.8f, 0.5f) + 0.4f;
        for (int i = 0; i < _hotspotImgList.Count; i++)
        {
            if (_hotspotImgList[i] == null) continue;
            Color c = _hotspotImgList[i].color;
            if (c.g > 0.7f && c.r < 0.4f) continue;
            _hotspotImgList[i].color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    private void VerileriYukle()
    {
        _tumVeriler.Clear();
        _tumVeriler.Add(new MitGercek { Soru = "Karacigeri temizlemek icin detoks sulari sarttir.", Cevap = false, Uyari = "Karaciger kendini yeniler. Bilincsiz detoks sulari toksik hepatite yol acabilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Sadece alkol alanlarin karacigeri yaglanir.", Cevap = false, Uyari = "Fruktoz surubu ve paketli gidalar alkole bagli olmayan karaciger yaglanmasinin bir numarali sebebidir." });
        _tumVeriler.Add(new MitGercek { Soru = "Agri kesiciler karacigeri etkilemez.", Cevap = false, Uyari = "Parasetamol gibi maddeler karacigerde metabolize olur. Asiri doz karacigeri ciddi sekilde yorar." });
        _tumVeriler.Add(new MitGercek { Soru = "Karaciger nakli sonrasi normal hayata donulemez.", Cevap = false, Uyari = "Basarili bir nakil sonrasi hastalar normal ve aktif bir yasam surebilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Hepatit B asisi yetiskinlere de yapilabilir.", Cevap = true, Uyari = "Hepatit B asisi her yasta yapilabilir ve karaciger kanserini onlemede kritik oneme sahiptir." });
        _tumVeriler.Add(new MitGercek { Soru = "Karaciger yaglanmasi sadece kilolu kisilerde gorulur.", Cevap = false, Uyari = "Zayif bireylerde de genetik ve beslenme hatalarindan kaynakli yaglanma gorulebilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Kahve tuketimi karaciger icin zararlidir.", Cevap = false, Uyari = "Bilimsel calismalar filtreyle demlenen kahvenin karaciger fibrozunu azaltabilecegini gostermektedir." });
        _tumVeriler.Add(new MitGercek { Soru = "Sari kantaron cayi karacigere iyi gelir.", Cevap = false, Uyari = "Sari kantaron bircok ilacla etkilesime girer ve karacigerde toksik etki yapabilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Karaciger hastaliginin belirtileri gec ortaya cikar.", Cevap = true, Uyari = "Karaciger sessiz bir organdir; hasar %70-80 oranina ulasana kadar belirti vermeyebilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Karaciger vucuttaki en buyuk ic organdir.", Cevap = true, Uyari = "Evet, deri haric vucudun en buyuk ic organi karacigerdir." });
        _tumVeriler.Add(new MitGercek { Soru = "Siroz sadece alkoliklerde gorulur.", Cevap = false, Uyari = "Viral hepatitler ve asiri yaglanma da siroza neden olabilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Karaciger kendini tamamen yenileyebilen tek organdir.", Cevap = true, Uyari = "%75'i alinmis bir karaciger bile birkac hafta icinde eski boyutuna ulasabilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Limonlu su icmek karacigerdeki yaglari eritir.", Cevap = false, Uyari = "Limonlu su saglikli olsa da dogrudan karaciger yaglanmasini eritmez. Diyet ve egzersiz sarttir." });
        _tumVeriler.Add(new MitGercek { Soru = "Hepatit C'nin asisi vardir.", Cevap = false, Uyari = "Hepatit C'nin asisi yoktur ancak ilaclarla tamamen tedavi edilebilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Bitkisel takviyeler her zaman karaciger icin guvenlidir.", Cevap = false, Uyari = "Bazi bitkisel takviyeler karaciger yetmezligine bile yol acabilecek kadar toksik olabilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Obezite karaciger sagligini tehdit eder.", Cevap = true, Uyari = "Obezite, non-alkolik karaciger yaglanmasinin en buyuk risk faktorlerinden biridir." });
        _tumVeriler.Add(new MitGercek { Soru = "Karaciger hasari geri dondurulemez.", Cevap = false, Uyari = "Erken evre hasarlar (yaglanma vb.) yasam tarzi degisikligi ile tamamen iyilesebilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Seker tuketimi karacigeri yorar.", Cevap = true, Uyari = "Asiri seker (ozellikle fruktoz) karacigerde yaga donuserek depolanir ve organda hasara yol acar." });
        _tumVeriler.Add(new MitGercek { Soru = "Gunde bir kadeh alkol karaciger icin faydalidir.", Cevap = false, Uyari = "Alkolun karacigere hicbir faydasi yoktur, her miktari karaciger tarafindan metabolize edilmelidir." });
        _tumVeriler.Add(new MitGercek { Soru = "Karaciger kanin pihtilasmasini saglayan proteinler uretir.", Cevap = true, Uyari = "Karaciger yetmezliginde pihtilasma sorunlari yasanmasinin sebebi budur." });
        _tumVeriler.Add(new MitGercek { Soru = "Kolesterolun cogu karacigerde uretilir.", Cevap = true, Uyari = "Kolesterolun sadece kucuk bir kismi diyetle alinir, asil uretim yeri karacigerdir." });
        _tumVeriler.Add(new MitGercek { Soru = "Ates dusurucu suruplar cocuklarda karacigeri etkilemez.", Cevap = false, Uyari = "Cocuklarda bilincsiz ve yuksek doz parasetamol kullanimi ciddi karaciger hasarina yol acabilir." });
        _tumVeriler.Add(new MitGercek { Soru = "Ciger yemek karaciger hastaliklarini iyilestirir.", Cevap = false, Uyari = "Ciger yemek vitamin acisindan zengindir ancak karaciger hastaliklarini tedavi eden sihirli bir cozum degildir." });
        _tumVeriler.Add(new MitGercek { Soru = "Yorgunluk karaciger hastaliginin bir belirtisi olabilir.", Cevap = true, Uyari = "Karaciger hastaliklarinin en yaygin ve ilk gorulen belirtilerinden biri halsizlik ve kronik yorgunluktur." });
    }

    private void AktifSorulariSec()
    {
        // 4 saatlik dilime gore soru seti degistir
        int saatDilimi = (int)(System.DateTime.Now.TimeOfDay.TotalHours / 4);
        Random.InitState(saatDilimi + System.DateTime.Now.DayOfYear * 100);

        List<MitGercek> karisik = new List<MitGercek>(_tumVeriler);
        // Fisher-Yates karistirma
        for (int i = karisik.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            MitGercek temp = karisik[i];
            karisik[i] = karisik[j];
            karisik[j] = temp;
        }

        _aktifVeriler.Clear();
        // Toplam 6 soru seciyoruz (Her kutuya 2 soru dusecek)
        for (int i = 0; i < Mathf.Min(6, karisik.Count); i++)
            _aktifVeriler.Add(karisik[i]);
    }

    private void HotspotlariOlustur()
    {
        foreach (GameObject h in _hotspotlar) { if (h != null) Destroy(h); }
        _hotspotlar.Clear();
        _hotspotImgList.Clear();
        if (_arkaPlanPaneli != null) Destroy(_arkaPlanPaneli);

        Sprite bgSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        Sprite knobSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");

        // === ANA PANEL ===
        _arkaPlanPaneli = new GameObject("HologramArkaPlan");
        _arkaPlanPaneli.transform.SetParent(_anaCanvas.transform, false);
        RectTransform panelRT = _arkaPlanPaneli.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(800f, 750f);

        Image panelImg = _arkaPlanPaneli.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.06f, 0.1f, 0.95f);
        if (bgSprite != null) { panelImg.sprite = bgSprite; panelImg.type = Image.Type.Sliced; }

        // === KAPATMA BUTONU (X) ===
        GameObject kapatBtnGO = new GameObject("KapatBtn");
        kapatBtnGO.transform.SetParent(_arkaPlanPaneli.transform, false);
        RectTransform kapatRT = kapatBtnGO.AddComponent<RectTransform>();
        kapatRT.anchorMin = new Vector2(1f, 1f);
        kapatRT.anchorMax = new Vector2(1f, 1f);
        kapatRT.pivot = new Vector2(1f, 1f);
        kapatRT.anchoredPosition = new Vector2(-15f, -15f);
        kapatRT.sizeDelta = new Vector2(55f, 55f);

        Image kapatImg = kapatBtnGO.AddComponent<Image>();
        kapatImg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        if (knobSprite != null) kapatImg.sprite = knobSprite;

        Button kapatBtn = kapatBtnGO.AddComponent<Button>();
        kapatBtn.onClick.AddListener(() => { PaneliKapat(); });

        GameObject kapatTxtGO = new GameObject("X");
        kapatTxtGO.transform.SetParent(kapatBtnGO.transform, false);
        RectTransform kapatTxtRT = kapatTxtGO.AddComponent<RectTransform>();
        kapatTxtRT.anchorMin = Vector2.zero; kapatTxtRT.anchorMax = Vector2.one;
        kapatTxtRT.offsetMin = Vector2.zero; kapatTxtRT.offsetMax = Vector2.zero;
        TextMeshProUGUI kapatTMP = kapatTxtGO.AddComponent<TextMeshProUGUI>();
        kapatTMP.text = "X"; kapatTMP.fontSize = 28f; kapatTMP.fontStyle = FontStyles.Bold;
        kapatTMP.color = Color.white;
        kapatTMP.alignment = TextAlignmentOptions.Center; kapatTMP.raycastTarget = false;
        Material kapatMat = new Material(kapatTMP.font.material);
        kapatMat.SetColor("_FaceColor", Color.white);
        kapatTMP.fontMaterial = kapatMat;

        // === KARACIGER GORSELI (Resources/karaciger) ===
        Texture2D kcTex = Resources.Load<Texture2D>("karaciger");
        if (kcTex != null)
        {
            GameObject gorselGO = new GameObject("KaracigerGorsel");
            gorselGO.transform.SetParent(_arkaPlanPaneli.transform, false);
            RectTransform gorselRT = gorselGO.AddComponent<RectTransform>();
            gorselRT.anchorMin = new Vector2(0.5f, 0.5f);
            gorselRT.anchorMax = new Vector2(0.5f, 0.5f);
            gorselRT.anchoredPosition = new Vector2(0f, -30f);
            gorselRT.sizeDelta = new Vector2(420f, 350f);

            RawImage rawImg = gorselGO.AddComponent<RawImage>();
            rawImg.texture = kcTex;
            rawImg.color = new Color(1f, 1f, 1f, 0.35f); // Hafif saydam
            rawImg.raycastTarget = false;
        }

        // === BASLIK ===
        TextMeshProUGUI baslikTMP = MetinOlustur(_arkaPlanPaneli.transform, "Baslik",
            "KARACIGER UYARI SISTEMI", 38f, Color.white,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -10f), new Vector2(0f, 70f));
        baslikTMP.fontStyle = FontStyles.Bold;

        // === ALT BASLIK ===
        MetinOlustur(_arkaPlanPaneli.transform, "AltBaslik",
            "Dogru mu, Yanlis mi? Noktalara dokun!", 24f, Color.white,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -75f), new Vector2(0f, 40f));

        // === AYIRICI CIZGI ===
        GameObject cizgiGO = new GameObject("Cizgi");
        cizgiGO.transform.SetParent(_arkaPlanPaneli.transform, false);
        RectTransform cizgiRT = cizgiGO.AddComponent<RectTransform>();
        cizgiRT.anchorMin = new Vector2(0.1f, 1f); cizgiRT.anchorMax = new Vector2(0.9f, 1f);
        cizgiRT.pivot = new Vector2(0.5f, 1f);
        cizgiRT.anchoredPosition = new Vector2(0f, -115f); cizgiRT.sizeDelta = new Vector2(0f, 2f);
        Image cizgiImg = cizgiGO.AddComponent<Image>();
        cizgiImg.color = new Color(1f, 0.3f, 0.2f, 0.4f); cizgiImg.raycastTarget = false;

        // === ANA EKRAN KAPAT BUTONU ===
        GameObject altKapatBtnGO = ButonOlustur(_arkaPlanPaneli.transform, "KAPAT", new Vector2(0f, -320f), new Color(0.8f, 0.2f, 0.2f));
        altKapatBtnGO.GetComponent<Button>().onClick.AddListener(() => { PaneliKapat(); });


        // === HOTSPOTLAR (etiket yok, kucuk kutular) ===
        Vector2[] pozisyonlar = { new Vector2(-130f, 60f), new Vector2(100f, -20f), new Vector2(-20f, -150f) };
        Color hotspotRenk = new Color(1f, 0.25f, 0.2f, 0.7f);

        _tamamlananKutu = 0;

        for (int i = 0; i < 3; i++) // 3 kutu var
        {
            int kutuIndex = i;

            GameObject hotGO = new GameObject("Hotspot_" + i);
            hotGO.transform.SetParent(_arkaPlanPaneli.transform, false);
            RectTransform hotRT = hotGO.AddComponent<RectTransform>();
            hotRT.anchorMin = new Vector2(0.5f, 0.5f); hotRT.anchorMax = new Vector2(0.5f, 0.5f);
            hotRT.anchoredPosition = pozisyonlar[i]; hotRT.sizeDelta = new Vector2(55f, 55f);

            Image hotImg = hotGO.AddComponent<Image>();
            hotImg.color = hotspotRenk;
            if (knobSprite != null) hotImg.sprite = knobSprite;

            // Glow halkasi
            GameObject glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(hotGO.transform, false);
            RectTransform glowRT = glowGO.AddComponent<RectTransform>();
            glowRT.anchorMin = Vector2.zero; glowRT.anchorMax = Vector2.one;
            glowRT.offsetMin = new Vector2(-10f, -10f); glowRT.offsetMax = new Vector2(10f, 10f);
            Image glowImg = glowGO.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.3f, 0.2f, 0.15f);
            if (knobSprite != null) glowImg.sprite = knobSprite;
            glowImg.raycastTarget = false;

            // Soru isareti
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(hotGO.transform, false);
            RectTransform labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero; labelRT.offsetMax = Vector2.zero;
            TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "?"; labelTMP.fontSize = 30f; labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = Color.white;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.raycastTarget = false;
            // Font materyalini klonla ve yuz rengini beyaz yap
            Material labelMat = new Material(labelTMP.font.material);
            labelMat.SetColor("_FaceColor", Color.white);
            labelTMP.fontMaterial = labelMat;

            Button btn = hotGO.AddComponent<Button>();
            btn.onClick.AddListener(() => { SoruPaneliAc(kutuIndex, 0, hotImg, labelTMP); });

            _hotspotlar.Add(hotGO);
            _hotspotImgList.Add(hotImg);
        }
    }

    // kutuIndex (0, 1, 2) ve kacinciSoru (0 veya 1)
    private void SoruPaneliAc(int kutuIndex, int kacinciSoru, Image hotImg, TextMeshProUGUI hotLabel)
    {
        if (_soruPaneli != null) Destroy(_soruPaneli);
        
        // Kutularin soru indeksleri: Kutu 0 (0,1), Kutu 1 (2,3), Kutu 2 (4,5)
        int genelSoruIndex = (kutuIndex * 2) + kacinciSoru;
        if (genelSoruIndex >= _aktifVeriler.Count) return;
        
        MitGercek veri = _aktifVeriler[genelSoruIndex];

        _soruPaneli = new GameObject("SoruPaneli");
        _soruPaneli.transform.SetParent(_anaCanvas.transform, false);
        RectTransform pRT = _soruPaneli.AddComponent<RectTransform>();
        pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
        pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;
        Image pBG = _soruPaneli.AddComponent<Image>();
        pBG.color = new Color(0f, 0f, 0f, 0.8f);

        // Kart
        GameObject kartGO = new GameObject("Kart");
        kartGO.transform.SetParent(_soruPaneli.transform, false);
        RectTransform kRT = kartGO.AddComponent<RectTransform>();
        kRT.anchorMin = new Vector2(0.5f, 0.5f); kRT.anchorMax = new Vector2(0.5f, 0.5f);
        kRT.anchoredPosition = Vector2.zero; kRT.sizeDelta = new Vector2(720f, 480f);
        Image kImg = kartGO.AddComponent<Image>();
        kImg.color = new Color(0.1f, 0.1f, 0.14f, 0.97f);
        Sprite bg = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bg != null) { kImg.sprite = bg; kImg.type = Image.Type.Sliced; }

        string soruBaslik = kacinciSoru == 0 ? "1. SORU" : "2. SORU";
        
        TextMeshProUGUI bTMP = MetinOlustur(kartGO.transform, "B", soruBaslik + " - DOGRU MU, YANLIS MI?", 36f,
            new Color(1f, 0.8f, 0.15f), new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(0f, 65f));
        bTMP.fontStyle = FontStyles.Bold;

        MetinOlustur(kartGO.transform, "S", "\"" + veri.Soru + "\"", 30f,
            Color.white, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.75f),
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        GameObject dBtnGO = ButonOlustur(kartGO.transform, "DOGRU", new Vector2(-140f, -180f), new Color(0.15f, 0.65f, 0.3f));
        GameObject yBtnGO = ButonOlustur(kartGO.transform, "YANLIS", new Vector2(140f, -180f), new Color(0.85f, 0.2f, 0.2f));

        dBtnGO.GetComponent<Button>().onClick.AddListener(() => SonucGoster(kartGO, veri.Cevap == true, veri.Uyari, kutuIndex, kacinciSoru, hotImg, hotLabel));
        yBtnGO.GetComponent<Button>().onClick.AddListener(() => SonucGoster(kartGO, veri.Cevap == false, veri.Uyari, kutuIndex, kacinciSoru, hotImg, hotLabel));
    }

    private void SonucGoster(GameObject kartGO, bool dogruMu, string uyari, int kutuIndex, int kacinciSoru, Image hotImg, TextMeshProUGUI hotLabel)
    {
        for (int i = kartGO.transform.childCount - 1; i >= 0; i--)
            Destroy(kartGO.transform.GetChild(i).gameObject);

        TextMeshProUGUI sTMP = MetinOlustur(kartGO.transform, "Sonuc",
            dogruMu ? "DOGRU CEVAP!" : "YANLIS CEVAP!", 42f,
            dogruMu ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.35f, 0.25f),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -25f), new Vector2(0f, 65f));
        sTMP.fontStyle = FontStyles.Bold;

        MetinOlustur(kartGO.transform, "Uyari", uyari, 26f,
            new Color(0.9f, 0.9f, 0.92f),
            new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.78f),
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        // Eger 1. soruyu (kacinciSoru == 0) cevapladiysa "SIRADAKI SORU" butonu ciksin
        // Eger 2. soruyu (kacinciSoru == 1) cevapladiysa "KAPAT" butonu ciksin
        string btnMetin = kacinciSoru == 0 ? "SIRADAKI SORU" : "KUTUYU KAPAT";
        Color btnRenk = kacinciSoru == 0 ? new Color(0.2f, 0.6f, 0.8f) : new Color(0.35f, 0.35f, 0.45f);

        GameObject kBtnGO = ButonOlustur(kartGO.transform, btnMetin, new Vector2(0f, -180f), btnRenk);
        kBtnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 75f); // Butonu biraz genislet

        kBtnGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (kacinciSoru == 0)
            {
                // 1. soruydu, simdi 2. soruya gec
                SoruPaneliAc(kutuIndex, 1, hotImg, hotLabel);
            }
            else
            {
                // 2. soruyu da bitirdi, kutuyu kapat
                if (hotImg != null) hotImg.color = new Color(0.2f, 0.85f, 0.3f, 1f);
                if (hotLabel != null) { hotLabel.text = "OK"; hotLabel.fontSize = 22f; }
                if (_soruPaneli != null) Destroy(_soruPaneli);

                _tamamlananKutu++;
                if (_tamamlananKutu >= 3)
                {
                    StartCoroutine(TamamlamaBilgisi());
                }
            }
        });
    }

    private IEnumerator TamamlamaBilgisi()
    {
        yield return new WaitForSeconds(0.3f);

        if (_soruPaneli != null) Destroy(_soruPaneli);
        _soruPaneli = new GameObject("BilgiPaneli");
        _soruPaneli.transform.SetParent(_anaCanvas.transform, false);
        RectTransform pRT = _soruPaneli.AddComponent<RectTransform>();
        pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
        pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;
        Image pBG = _soruPaneli.AddComponent<Image>();
        pBG.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject kartGO = new GameObject("BilgiKart");
        kartGO.transform.SetParent(_soruPaneli.transform, false);
        RectTransform kRT = kartGO.AddComponent<RectTransform>();
        kRT.anchorMin = new Vector2(0.5f, 0.5f); kRT.anchorMax = new Vector2(0.5f, 0.5f);
        kRT.anchoredPosition = Vector2.zero; kRT.sizeDelta = new Vector2(720f, 520f);
        Image kImg = kartGO.AddComponent<Image>();
        kImg.color = new Color(0.08f, 0.12f, 0.08f, 0.97f);
        Sprite bg = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bg != null) { kImg.sprite = bg; kImg.type = Image.Type.Sliced; }

        TextMeshProUGUI bTMP = MetinOlustur(kartGO.transform, "Baslik",
            "TEBRIKLER!", 44f, new Color(0.3f, 1f, 0.4f),
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -20f), new Vector2(0f, 65f));
        bTMP.fontStyle = FontStyles.Bold;

        MetinOlustur(kartGO.transform, "Bilgi",
            "Tum sorulari tamamladiniz!\n\n" +
            "Biliyor muydunuz?\n" +
            "Karaciger, insan vucudundaki en buyuk ic organdir " +
            "ve 500'den fazla hayati gorevi yerine getirir. " +
            "Saglikli beslenme, duzenli hareket ve bilincsiz ilac kullanimindan " +
            "kacinmak karaciger sagliginizi korumanin en etkili yoludur.",
            24f, Color.white,
            new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.8f),
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        GameObject kBtnGO = ButonOlustur(kartGO.transform, "TAMAM", new Vector2(0f, -210f), new Color(0.2f, 0.6f, 0.3f));
        kBtnGO.GetComponent<Button>().onClick.AddListener(() =>
        {
            PaneliKapat();
        });
    }

    private TextMeshProUGUI MetinOlustur(Transform parent, string ad, string metin, float boyut, Color renk,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pozisyon, Vector2 size)
    {
        GameObject go = new GameObject(ad);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = pozisyon; rt.sizeDelta = size;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = metin; tmp.fontSize = boyut;
        tmp.color = renk;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
        // Font materyalini klonla ve yuz rengini dogrudan ata
        Material mat = new Material(tmp.font.material);
        mat.SetColor("_FaceColor", renk);
        tmp.fontMaterial = mat;
        return tmp;
    }

    private GameObject ButonOlustur(Transform parent, string yazi, Vector2 poz, Color renk)
    {
        GameObject go = new GameObject("Btn_" + yazi);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = poz; rt.sizeDelta = new Vector2(240f, 75f);
        Image img = go.AddComponent<Image>(); img.color = renk;
        Sprite bg = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bg != null) { img.sprite = bg; img.type = Image.Type.Sliced; }
        go.AddComponent<Button>();

        GameObject tGO = new GameObject("T");
        tGO.transform.SetParent(go.transform, false);
        RectTransform tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text = yazi; tmp.fontSize = 32f; tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
        // Font materyalini klonla ve yuz rengini beyaz yap
        Material btnMat = new Material(tmp.font.material);
        btnMat.SetColor("_FaceColor", Color.white);
        tmp.fontMaterial = btnMat;
        return go;
    }
}
