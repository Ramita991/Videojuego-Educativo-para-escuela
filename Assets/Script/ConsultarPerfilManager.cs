using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

// Cambiamos el nombre de la clase de datos para evitar duplicados
[Serializable]
public class ConsultarPerfilData
{
    public int id;
    public string nombre;
    public string apellido;
    public string dni;
    public string email;
    public string rol;
    public bool is_active;
}

// Cambiamos el nombre de la clase principal para que coincida con el archivo ConsultarPerfilManager.cs
public class ConsultarPerfilManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool usarApiRemota = false;
    [SerializeField] private string apiBaseUrl = "http://localhost:8000/api/v1/users/me";

    [Header("UI Elements - Datos Formulario")]
    [SerializeField] private TMP_Text txtNombre;
    [SerializeField] private TMP_Text txtApellido;
    [SerializeField] private TMP_Text txtDni;
    [SerializeField] private TMP_Text txtEmail;
    [SerializeField] private TMP_Text txtRol;

    [Header("UI Elements - Tarjeta Lateral")]
    [SerializeField] private TMP_Text txtNombreCompleto;
    [SerializeField] private TMP_Text txtEstadoCuenta;
    [SerializeField] private Image imgBadgeEstado;

    [Header("UI States (Opcionales)")]
    [SerializeField] private GameObject panelCargando;
    [SerializeField] private GameObject panelError;
    [SerializeField] private GameObject panelPerfil;

    private void Start()
    {
        CargarPerfil();
    }

    public void CargarPerfil()
    {
        if (usarApiRemota)
        {
            StartCoroutine(GetProfileApiRoutine());
        }
        else
        {
            CargarDesdeDatabaseManager();
        }
    }

    private void CargarDesdeDatabaseManager()
    {
        SetStatePanels(loading: false, error: false, content: true);

        // Intenta leer de la base de datos local SQLite de tus compañeros
        if (DatabaseManager.Instance != null)
        {
            string emailActual = PlayerPrefs.GetString("userEmail", "juan.perez@colegio.edu.ar");
            Usuario usuarioLocal = DatabaseManager.Instance.GetUsuarioPorMail(emailActual);

            if (usuarioLocal != null)
            {
                ConsultarPerfilData user = new ConsultarPerfilData
                {
                    id = usuarioLocal.Id,
                    nombre = usuarioLocal.Nombre,
                    apellido = usuarioLocal.Apellido,
                    dni = usuarioLocal.Dni,
                    email = usuarioLocal.Mail,
                    rol = "Estudiante", // Se puede adaptar al RolId
                    is_active = usuarioLocal.Activo
                };
                ActualizarInterfaz(user);
                return;
            }
        }

        // Datos de prueba por defecto
        ConsultarPerfilData mockUser = new ConsultarPerfilData
        {
            nombre = "Juan",
            apellido = "Pérez",
            dni = "44556677",
            email = "juan.perez@colegio.edu.ar",
            rol = "Estudiante",
            is_active = true
        };
        ActualizarInterfaz(mockUser);
    }

    private IEnumerator GetProfileApiRoutine()
    {
        SetStatePanels(loading: true, error: false, content: false);
        string token = PlayerPrefs.GetString("authToken", "DEV_MOCK_TOKEN");

        using (UnityWebRequest request = UnityWebRequest.Get(apiBaseUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                ConsultarPerfilData user = JsonUtility.FromJson<ConsultarPerfilData>(json);
                ActualizarInterfaz(user);
                SetStatePanels(loading: false, error: false, content: true);
            }
            else
            {
                SetStatePanels(loading: false, error: true, content: false);
                Debug.LogError($"[HU-011] Error API: {request.error}");
            }
        }
    }

    private void ActualizarInterfaz(ConsultarPerfilData user)
    {
        if (txtNombre != null) txtNombre.text = user.nombre;
        if (txtApellido != null) txtApellido.text = user.apellido;
        if (txtDni != null) txtDni.text = user.dni;
        if (txtEmail != null) txtEmail.text = user.email;
        if (txtRol != null) txtRol.text = user.rol;

        if (txtNombreCompleto != null) txtNombreCompleto.text = $"{user.nombre} {user.apellido}";

        if (txtEstadoCuenta != null)
        {
            if (user.is_active)
            {
                txtEstadoCuenta.text = "Cuenta Activa";
                txtEstadoCuenta.color = new Color(0.2f, 0.8f, 0.4f);
                if (imgBadgeEstado != null) imgBadgeEstado.color = new Color(0.02f, 0.37f, 0.27f);
            }
            else
            {
                txtEstadoCuenta.text = "Cuenta Inactiva";
                txtEstadoCuenta.color = new Color(0.9f, 0.3f, 0.3f);
                if (imgBadgeEstado != null) imgBadgeEstado.color = new Color(0.6f, 0.1f, 0.1f);
            }
        }
    }

    private void SetStatePanels(bool loading, bool error, bool content)
    {
        if (panelCargando != null) panelCargando.SetActive(loading);
        if (panelError != null) panelError.SetActive(error);
        if (panelPerfil != null) panelPerfil.SetActive(content);
    }

    public void OnClickVolverInicio()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void OnClickEditarPerfil()
    {
        SceneManager.LoadScene("EditarPerfil");
    }
}