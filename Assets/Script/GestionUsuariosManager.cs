using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GestionUsuariosManager : MonoBehaviour
{
    [Header("Seguridad y Control de Acceso (HU-002)")]
    [SerializeField] private RolUsuario rolUsuarioLogueado = RolUsuario.Directivo;
    [SerializeField] private GameObject panelAccesoDenegado;

    [Header("Barra de Herramientas")]
    [SerializeField] private TMP_InputField inputBuscador;
    [SerializeField] private TMP_Dropdown dropdownRol;
    [SerializeField] private TMP_Dropdown dropdownEstados;
    [SerializeField] private Button btnNuevoUsuario;

    [Header("Estructura de la Tabla")]
    [SerializeField] private Transform filaContenedor;
    [SerializeField] private GameObject filaUsuarioPrefab;
    [SerializeField] private GameObject mensajeListaVacia;

    [Header("Paginacion")]
    [SerializeField] private TMP_Text txtPaginacionInfo;
    [SerializeField] private Button btnAtras;
    [SerializeField] private Button btnAdelante;
    [SerializeField] private TMP_Text txtPaginaActual;
    [SerializeField] private int usuariosPorPagina = 4;

    [Header("Navegación entre Módulos")]
    [SerializeField] private GameObject panelGestionUsuarios;
    [SerializeField] private GameObject panelDesactivarUsuario; // HU-004
    [SerializeField] private GameObject panelPerfilUsuario;      // Módulo Perfil
    [SerializeField] private GameObject panelCrearUsuario;       // HU-001 (Formulario de alta)

    // Estado interno
    private List<UsuarioModel> baseDatosUsuarios = new List<UsuarioModel>();
    private List<UsuarioModel> usuariosFiltrados = new List<UsuarioModel>();
    private int paginaActual = 1;
    private int totalPaginas = 1;

    // Estado estático accesible para los módulos receptores
    public static UsuarioModel UsuarioSeleccionadoParaDesactivar { get; private set; }
    public static UsuarioModel UsuarioSeleccionadoParaPerfil { get; private set; }

    private void Awake()
    {
        InicializarFiltros();
        ConfigurarEventosPaginacion();

        if (btnNuevoUsuario != null)
        {
            btnNuevoUsuario.onClick.AddListener(AbrirFormularioCrearUsuario);
        }
    }

    private void OnEnable()
    {
        ValidarAccesoDirectivo();

        if (DatabaseManager.Instance != null && DatabaseManager.Instance.EstaLista)
        {
            RefrescarTablaDesdeBD();
        }
        else if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.OnBaseDeDatosLista += RefrescarTablaDesdeBD;
        }
    }

    private void OnDisable()
    {
        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.OnBaseDeDatosLista -= RefrescarTablaDesdeBD;
        }
    }

    private void ValidarAccesoDirectivo()
    {
        if (rolUsuarioLogueado != RolUsuario.Directivo)
        {
            if (panelAccesoDenegado != null) panelAccesoDenegado.SetActive(true);
            if (panelGestionUsuarios != null) panelGestionUsuarios.SetActive(false);
            Debug.LogWarning("[Seguridad] Acceso denegado: Se requieren permisos de Directivo.");
            return;
        }
    }

    private void InicializarFiltros()
    {
        if (inputBuscador != null)
        {
            inputBuscador.onValueChanged.AddListener(delegate { OnFiltrosCambiados(); });
        }

        if (dropdownRol != null)
        {
            dropdownRol.ClearOptions();
            dropdownRol.AddOptions(new List<string> { "Rol: Todos", "Directivo", "Profesor", "Estudiante" });
            dropdownRol.onValueChanged.AddListener(delegate { OnFiltrosCambiados(); });
        }

        if (dropdownEstados != null)
        {
            dropdownEstados.ClearOptions();
            dropdownEstados.AddOptions(new List<string> { "Estado: Todos", "Activo", "Inactivo" });
            dropdownEstados.onValueChanged.AddListener(delegate { OnFiltrosCambiados(); });
        }
    }

    private void ConfigurarEventosPaginacion()
    {
        if (btnAtras != null)
        {
            btnAtras.onClick.AddListener(() =>
            {
                if (paginaActual > 1)
                {
                    paginaActual--;
                    RenderizarTabla();
                }
            });
        }

        if (btnAdelante != null)
        {
            btnAdelante.onClick.AddListener(() =>
            {
                if (paginaActual < totalPaginas)
                {
                    paginaActual++;
                    RenderizarTabla();
                }
            });
        }
    }

    public void RefrescarTablaDesdeBD()
    {
        CargarUsuariosDesdeSQLite();
        paginaActual = 1;
        AplicarFiltros();
    }

    private void CargarUsuariosDesdeSQLite()
    {
        baseDatosUsuarios.Clear();

        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("[DB] DatabaseManager no encontrado en la escena.");
            return;
        }

        List<Usuario> usuariosSQL = DatabaseManager.Instance.GetTodosLosUsuarios();
        List<Rol> rolesSQL = DatabaseManager.Instance.GetTodosLosRoles();

        foreach (var u in usuariosSQL)
        {
            Rol rolObj = rolesSQL.FirstOrDefault(r => r.Id == u.RolId);
            string nombreRol = rolObj != null ? rolObj.Nombre : "Alumno";

            RolUsuario rolEnum = RolUsuario.Estudiante;
            if (nombreRol == "Director") rolEnum = RolUsuario.Directivo;
            else if (nombreRol == "Profesor") rolEnum = RolUsuario.Profesor;

            baseDatosUsuarios.Add(new UsuarioModel(
                u.Id.ToString(),
                u.Nombre,
                u.Apellido,
                u.Dni,
                u.Mail,
                rolEnum,
                u.Activo
            ));
        }

        Debug.Log($"[DB] Se cargaron {baseDatosUsuarios.Count} usuarios desde SQLite.");
    }

    private void OnFiltrosCambiados()
    {
        paginaActual = 1;
        AplicarFiltros();
    }

    public void AplicarFiltros()
    {
        string textoBusqueda = inputBuscador != null ? inputBuscador.text.Trim().ToLower() : "";
        int indexRol = dropdownRol != null ? dropdownRol.value : 0;
        int indexEstado = dropdownEstados != null ? dropdownEstados.value : 0;

        usuariosFiltrados = baseDatosUsuarios.Where(u =>
        {
            bool coincideBusqueda = string.IsNullOrEmpty(textoBusqueda) ||
                                     u.nombre.ToLower().Contains(textoBusqueda) ||
                                     u.apellido.ToLower().Contains(textoBusqueda) ||
                                     u.dni.Contains(textoBusqueda) ||
                                     u.email.ToLower().Contains(textoBusqueda);

            bool coincideRol = true;
            if (indexRol == 1) coincideRol = u.rol == RolUsuario.Directivo;
            else if (indexRol == 2) coincideRol = u.rol == RolUsuario.Profesor;
            else if (indexRol == 3) coincideRol = u.rol == RolUsuario.Estudiante;

            bool coincideEstado = true;
            if (indexEstado == 1) coincideEstado = u.activo == true;
            else if (indexEstado == 2) coincideEstado = u.activo == false;

            return coincideBusqueda && coincideRol && coincideEstado;
        }).ToList();

        totalPaginas = Mathf.Max(1, Mathf.CeilToInt((float)usuariosFiltrados.Count / usuariosPorPagina));
        if (paginaActual > totalPaginas) paginaActual = totalPaginas;

        RenderizarTabla();
    }

    private void RenderizarTabla()
    {
        foreach (Transform child in filaContenedor)
        {
            Destroy(child.gameObject);
        }

        if (usuariosFiltrados.Count == 0)
        {
            if (mensajeListaVacia != null) mensajeListaVacia.SetActive(true);
            if (txtPaginacionInfo != null) txtPaginacionInfo.text = "Mostrando 0 de 0 usuarios";
            ActualizarBotonesPaginacion();
            return;
        }

        if (mensajeListaVacia != null) mensajeListaVacia.SetActive(false);

        int indiceInicio = (paginaActual - 1) * usuariosPorPagina;
        List<UsuarioModel> paginaUsuarios = usuariosFiltrados.Skip(indiceInicio).Take(usuariosPorPagina).ToList();

        foreach (var usuario in paginaUsuarios)
        {
            GameObject filaObj = Instantiate(filaUsuarioPrefab, filaContenedor);
            FilaUsuarioView view = filaObj.GetComponent<FilaUsuarioView>();

            if (view != null)
            {
                view.ConfigurarFila(
                    usuario,
                    onDesactivar: (u) => IrPantallaDesactivarUsuario(u),
                    onEditar: (u) => IrPantallaPerfilUsuario(u)
                );
            }
        }

        int primerRegistro = indiceInicio + 1;
        int ultimoRegistro = Mathf.Min(indiceInicio + usuariosPorPagina, usuariosFiltrados.Count);
        if (txtPaginacionInfo != null)
            txtPaginacionInfo.text = $"Mostrando {primerRegistro} a {ultimoRegistro} de {usuariosFiltrados.Count} usuarios";
        if (txtPaginaActual != null)
            txtPaginaActual.text = paginaActual.ToString();

        ActualizarBotonesPaginacion();
    }

    private void ActualizarBotonesPaginacion()
    {
        if (btnAtras != null) btnAtras.interactable = (paginaActual > 1);
        if (btnAdelante != null) btnAdelante.interactable = (paginaActual < totalPaginas && usuariosFiltrados.Count > 0);
    }

    // Navegación a HU-004: Desactivar Usuario
    public void IrPantallaDesactivarUsuario(UsuarioModel usuario)
{
    UsuarioSeleccionadoParaDesactivar = usuario;
    Debug.Log($"[Navegación] Destino: HU-004 Desactivar. Usuario: {usuario.nombre} {usuario.apellido} (ID: {usuario.id})");

    // Comprobamos si la escena está cargada en los Build Settings
    if (Application.CanStreamedLevelBeLoaded("DesactivateUserScene"))
    {
        if (panelGestionUsuarios != null)
        {
            panelGestionUsuarios.SetActive(false);
        }
        SceneManager.LoadScene("DesactivateUserScene", LoadSceneMode.Additive);
    }
    else
    {
        Debug.LogWarning("[HU-004] La escena 'DesactivateUserScene' aún no existe o no fue agregada a Build Settings. Se ejecutó la acción pero se mantiene la vista actual para pruebas.");
    }
}
    // Navegación al Módulo de Perfil / Edición
    public void IrPantallaPerfilUsuario(UsuarioModel usuario)
{
    UsuarioSeleccionadoParaPerfil = usuario;
    Debug.Log($"[Navegación] Destino: Perfil. Usuario: {usuario.nombre} {usuario.apellido} (Mail: {usuario.email})");

    // Nombre de la escena asignada para el módulo de perfil
    string nombreEscenaPerfil = "ProfileUserScene"; // Modificá este string si tu compañero le puso otro nombre

    if (Application.CanStreamedLevelBeLoaded(nombreEscenaPerfil))
    {
        if (panelGestionUsuarios != null)
        {
            panelGestionUsuarios.SetActive(false);
        }
        SceneManager.LoadScene(nombreEscenaPerfil, LoadSceneMode.Additive);
    }
    else
    {
        Debug.LogWarning($"[Perfil] La escena '{nombreEscenaPerfil}' aún no existe o no fue agregada a Build Settings. Se ejecutó la acción pero se mantiene la vista actual para pruebas.");
    }
}
    // Abrir formulario de alta de nuevo usuario
    private void AbrirFormularioCrearUsuario()
    {
        if (panelCrearUsuario != null)
        {
            if (panelGestionUsuarios != null) panelGestionUsuarios.SetActive(false);
            panelCrearUsuario.SetActive(true);
        }
    }

    // Método para ser llamado desde HU-004 tras desactivar un usuario en SQLite
    public void ConfirmarDesactivacionEnBD(int usuarioId)
    {
        Usuario userSQL = DatabaseManager.Instance.GetTodosLosUsuarios().FirstOrDefault(u => u.Id == usuarioId);
        if (userSQL != null)
        {
            userSQL.Activo = false;
            DatabaseManager.Instance.ActualizarUsuario(userSQL);
            Debug.Log($"[DB] Usuario #{usuarioId} desactivado exitosamente en SQLite.");
            RefrescarTablaDesdeBD();
        }
    }
}