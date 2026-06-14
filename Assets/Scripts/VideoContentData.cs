using UnityEngine;
using System;

/// <summary>
/// Her içerik türü için veri modeli.
/// Inspector'dan veya kod aracılığıyla doldurulur.
/// </summary>
[Serializable]
public class ZoneContent
{
    [Header("Kimlik")]
    public string ZoneName;          // "Beslenme", "Egzersiz", "Uyarilar"

    [Header("Panel Metinleri")]
    public string PanelTitle;        // Panel başlığı (TR)
    [TextArea(3, 8)]
    public string PanelDescription;  // Panel açıklaması (TR)

    [Header("İkon Rengi")]
    public Color ThemeColor = Color.white;

    [Header("Video (Opsiyonel)")]
    [Tooltip("Resources/Videos/ altındaki video dosyasının adı (.mp4 uzantısız)")]
    public string VideoResourcePath; // Örn: "Videos/beslenme_video"
    
    [Tooltip("Video yoksa alternatif önizleme görseli")]
    public Sprite PreviewImage;
}
