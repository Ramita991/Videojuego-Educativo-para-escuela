using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    [Header("UI Control - Cierre de Sesión")]
    [SerializeField] private GameObject syncOverlay;

    [Header("Indicadores Visuales (Checks)")]
    [SerializeField] private List<GameObject> checksGris = new List<GameObject>();
    [SerializeField] private List<GameObject> checksVerde = new List<GameObject>();

    [Header("Escena de Carga")]
    [SerializeField] private string loginSceneName = "LoginScene";

    [Header("Navegación de Sidebar")]
    [SerializeField] private Button btnModuloUsuarios;
    [SerializeField] private GameObject panelGestionUsuarios;

    private void Start()
    {
        if (btnModuloUsuarios != null)
        {
            btnModuloUsuarios.onClick.AddListener(AbrirModuloUsuarios);
        }
    }

    public void AbrirModuloUsuarios()
    {
        if (panelGestionUsuarios != null)
        {
            panelGestionUsuarios.SetActive(true);
        }
    }

    public void OnLogoutButtonClicked()
    {
        StartCoroutine(SyncBeforeLogoutCoroutine());
    }

    private IEnumerator SyncBeforeLogoutCoroutine()
    {
        if (syncOverlay != null) syncOverlay.SetActive(true);

        Debug.Log("Cerrar Sesión: Iniciando sincronización en la nube...");

        yield return new WaitForSeconds(0.8f);
        if (checksVerde.Count > 0 && checksVerde[0] != null) checksVerde[0].SetActive(true);
        if (checksGris.Count > 0 && checksGris[0] != null) checksGris[0].SetActive(false);
        Debug.Log("Cerrar Sesión: Estadísticas guardadas.");

        yield return new WaitForSeconds(0.8f);
        if (checksVerde.Count > 1 && checksVerde[1] != null) checksVerde[1].SetActive(true);
        if (checksGris.Count > 1 && checksGris[1] != null) checksGris[1].SetActive(false);
        Debug.Log("Cerrar Sesión: Medallas guardadas.");

        yield return new WaitForSeconds(0.8f);
        if (checksVerde.Count > 2 && checksVerde[2] != null) checksVerde[2].SetActive(true);
        if (checksGris.Count > 2 && checksGris[2] != null) checksGris[2].SetActive(false);
        Debug.Log("Cerrar Sesión: Configuración guardada.");

        yield return new WaitForSeconds(0.5f);
        Debug.Log($"Guardado completo. Redirigiendo automáticamente a: {loginSceneName}");
        SceneManager.LoadScene(loginSceneName);
    }
}