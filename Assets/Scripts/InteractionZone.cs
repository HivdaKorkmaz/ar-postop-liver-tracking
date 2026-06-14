using UnityEngine;

/// <summary>
/// Her etkileşim bölgesine eklenen, bu bölgenin hangi içeriği tetikleyeceğini tanımlayan bileşen.
/// ImageTarget altındaki görünmez Quad/Plane nesnelerine bu script eklenir.
/// </summary>
public class InteractionZone : MonoBehaviour
{
    [Header("Bölge Kimliği")]
    [Tooltip("Bu bölgenin adı: 'Beslenme', 'Egzersiz', 'Uyarilar'")]
    public string ZoneName = "Beslenme";

    [Header("Görsel Geri Bildirim (Opsiyonel)")]
    [Tooltip("Dokunulduğunda kısa süre parlayan highlight efekti için renderer")]
    [SerializeField] private Renderer _highlightRenderer;

    [Header("Debug")]
    [SerializeField] private bool _showDebugGizmo = true;

    private void Awake()
    {
        // Collider yoksa ekle
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    /// <summary>
    /// LiverAssistantManager tarafından çağrılır — dokunma animasyonu tetikler
    /// </summary>
    public void OnTouched()
    {
        if (_highlightRenderer != null)
            StartCoroutine(FlashHighlight());
    }

    private System.Collections.IEnumerator FlashHighlight()
    {
        if (_highlightRenderer == null) yield break;
        _highlightRenderer.enabled = true;
        yield return new WaitForSeconds(0.15f);
        _highlightRenderer.enabled = false;
    }

    private void OnDrawGizmos()
    {
        if (!_showDebugGizmo) return;

        // Editörde bölge bounding box'ını göster
        Gizmos.color = ZoneName switch
        {
            "Beslenme" => new Color(1f, 0.3f, 0.3f, 0.25f),
            "Egzersiz" => new Color(0.3f, 1f, 0.3f, 0.25f),
            "Uyarilar" => new Color(1f, 0.8f, 0.2f, 0.25f),
            _ => new Color(0.5f, 0.5f, 1f, 0.25f)
        };

        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
            
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.8f);
            Gizmos.DrawWireCube(col.center, col.size);
        }

        // İsim etiketi (sadece Scene view'de)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.05f, $"[{ZoneName}]");
#endif
    }
}
