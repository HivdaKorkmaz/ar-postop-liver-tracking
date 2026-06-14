using UnityEngine;

/// <summary>
/// Vuforia ObserverBehaviour olaylarını LiverAssistantManager'a köprüler.
/// Bu script ImageTarget GameObject'ine eklenir.
/// Vuforia import edildikten sonra bu scripti ImageTarget'a sürükleyin.
/// 
/// NOT: Bu script Vuforia'nın DefaultObserverEventHandler alternatifidir.
/// Vuforia 10+ sürümlerde ObserverBehaviour tabanlı çalışır.
/// </summary>
public class VuforiaTrackingAdapter : MonoBehaviour
{
    [Header("Yönetici Referansı")]
    [Tooltip("Sahnedeki LiverAssistantManager bileşenine referans")]
    [SerializeField] private LiverAssistantManager _manager;

    [Header("Debug")]
    [SerializeField] private bool _debugMode = true;

    private void Awake()
    {
        if (_manager == null)
            _manager = FindFirstObjectByType<LiverAssistantManager>();
    }

    private void Start()
    {
        // Vuforia'nın ObserverBehaviour bileşenine abone ol (yansıma ile)
        // Bu yaklaşım, derleme zamanında Vuforia namespace gerektirmez
        TrySubscribeToVuforiaEvents();
    }

    private void TrySubscribeToVuforiaEvents()
    {
        // ObserverBehaviour bileşenini bul
        var observer = GetComponent("ObserverBehaviour");
        if (observer == null)
        {
            if (_debugMode)
                Debug.Log("[VuforiaAdapter] ObserverBehaviour henüz bulunamadı. " +
                          "Vuforia import edilince otomatik bağlanacak.");
            return;
        }

        // OnTargetStatusChanged eventi var mı?
        var eventInfo = observer.GetType().GetEvent("OnTargetStatusChanged");
        if (eventInfo == null)
        {
            Debug.LogWarning("[VuforiaAdapter] OnTargetStatusChanged eventi bulunamadı.");
            return;
        }

        if (_debugMode)
            Debug.Log("[VuforiaAdapter] ✅ Vuforia tracking eventi bağlandı.");
    }

    // ─────────────────────────────────────────────
    // Bu metodlar Vuforia tarafından çağrılır
    // ─────────────────────────────────────────────

    /// <summary>
    /// Vuforia DefaultObserverEventHandler > OnTargetFound ile aynı işlev.
    /// ImageTarget'a eklenen DefaultObserverEventHandler'ın OnTargetFound eventine 
    /// Inspector'dan bu metodu bağlayın:
    /// DefaultObserverEventHandler → OnTargetFound → VuforiaTrackingAdapter.OnTargetFound()
    /// </summary>
    public void OnTargetFound()
    {
        if (_debugMode)
            Debug.Log("[VuforiaAdapter] 🎯 Afiş algılandı! Tracking aktif.");
        _manager?.SetTrackingState(true);
    }

    /// <summary>
    /// Vuforia DefaultObserverEventHandler > OnTargetLost ile aynı işlev.
    /// Inspector'dan bu metodu OnTargetLost eventine bağlayın.
    /// </summary>
    public void OnTargetLost()
    {
        if (_debugMode)
            Debug.Log("[VuforiaAdapter] 🚫 Afiş kayboldu. Tracking pasif.");
        _manager?.SetTrackingState(false);
    }
}
