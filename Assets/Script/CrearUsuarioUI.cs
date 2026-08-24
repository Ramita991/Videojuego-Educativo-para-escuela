using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Controla la pantalla "Crear nuevo usuario" que usa el admin/docente
/// para dar de alta alumnos, profesores, etc.
/// </summary>

public class CrearUsuarioUI : MonoBehaviour
{
    [Header("Panel principal")]
    [SerializeField] private GameObject panelFormulario;
    [SerializeField] private GameObject panelUsuarioCreado;

    [Header("Campos del formulario")]
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private TMP_InputField inputApellido;
    [SerializeField] private TMP_InputField inputDni;
    [SerializeField] private TMP_InputField inputMail;
    [SerializeField] private TMP_Dropdown dropdownRol;
    [SerializeField] private TMP_InputField inputPasswordPreview; // solo muestra, no editable

    [Header("Botones")]
    [SerializeField] private Button botonCancelar;
    [SerializeField] private Button botonCrear;

    [Header("Feedback de error (opcional)")]
    [SerializeField] private TMP_Text textoError;

    private System.Collections.Generic.List<Rol> _rolesCache;

    private void OnEnable()
    {
        botonCrear.onClick.AddListener(OnCrearUsuario);
        botonCancelar.onClick.AddListener(OnCancelar);
        inputDni.onValueChanged.AddListener(OnDniChanged);

        LimpiarFormulario();

        // La base de datos puede tardar un frame (o más, en Android) en
        // estar lista, así que esperamos el evento en vez de asumir que
        // ya está disponible.
        if (DatabaseManager.Instance != null && DatabaseManager.Instance.EstaLista)
        {
            CargarRoles();
        }
        else if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.OnBaseDeDatosLista += CargarRoles;
        }
    }



    private void OnDisable()
    {
        botonCrear.onClick.RemoveListener(OnCrearUsuario);
        botonCancelar.onClick.RemoveListener(OnCancelar);
        inputDni.onValueChanged.RemoveListener(OnDniChanged);

        if (DatabaseManager.Instance != null)
            DatabaseManager.Instance.OnBaseDeDatosLista -= CargarRoles;
    }

    /// <summary>Carga los roles disponibles (Rol table) en el dropdown.</summary>
    private void CargarRoles()
    {
        dropdownRol.ClearOptions();
        var roles = DatabaseManager.Instance.GetTodosLosRoles();

        var opciones = new System.Collections.Generic.List<string>();
        foreach (var rol in roles)
            opciones.Add(rol.Nombre);

        dropdownRol.AddOptions(opciones);
        _rolesCache = roles;
    }

    /// <summary>Muestra en el campo de contraseña una vista previa (el DNI, tal como indica el diseño).</summary>
    private void OnDniChanged(string nuevoDni)
    {
        inputPasswordPreview.text = nuevoDni;
    }

    private void LimpiarFormulario()
    {
        inputNombre.text = "";
        inputApellido.text = "";
        inputDni.text = "";
        inputMail.text = "";
        inputPasswordPreview.text = "";
        if (textoError != null) textoError.text = "";

        panelFormulario.SetActive(true);
        panelUsuarioCreado.SetActive(false);
    }

    private void OnCancelar()
    {
        LimpiarFormulario();
        // Acá podrías además cerrar la pantalla o volver al listado de usuarios.
    }

    private void OnCrearUsuario()
    {
        if (_rolesCache == null || _rolesCache.Count == 0)
        {
            MostrarError("Los roles todavía no cargaron, esperá un segundo.");
            return;
        }

        string nombre = inputNombre.text.Trim();
        string apellido = inputApellido.text.Trim();
        string dni = inputDni.text.Trim();
        string mail = inputMail.text.Trim();

        // ---------- Validaciones básicas ----------
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) ||
            string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(mail))
        {
            MostrarError("Completá todos los campos.");
            return;
        }

        if (!dni.All(char.IsDigit))
        {
            MostrarError("El DNI solo puede contener números.");
            return;
        }

        if (!mail.Contains("@"))
        {
            MostrarError("El correo electrónico no es válido.");
            return;
        }

        if (DatabaseManager.Instance.ExisteMail(mail))
        {
            MostrarError("Ya existe un usuario con ese correo.");
            return;
        }

        if (DatabaseManager.Instance.ExisteDni(dni))
        {
            MostrarError("Ya existe un usuario con ese DNI.");
            return;
        }

        // ---------- Rol seleccionado ----------
        Rol rolSeleccionado = _rolesCache[dropdownRol.value];

        // ---------- Crear el usuario ----------
        // Nota de seguridad: guardamos un hash de la contraseña, no el DNI
        // en texto plano, aunque la contraseña INICIAL sea igual al DNI.
        // Así, si alguien accede a la base de datos, no ve las contraseñas reales.
        var nuevoUsuario = new Usuario
        {
            Nombre = nombre,
            Apellido = apellido,
            Dni = dni,
            Mail = mail,
            Password = HashPassword(dni),
            Activo = true,
            RolId = rolSeleccionado.Id
        };

        DatabaseManager.Instance.CrearUsuario(nuevoUsuario);

        // NOTA: acá todavía NO se crea la fila en Alumno/Profesor/Preceptor/
        // Director. Eso se hace en la pantalla aparte donde se carguen los
        // datos propios de cada rol (grado/curso, turno, etc). Los métodos
        // CrearAlumno/CrearProfesor/CrearPreceptor/CrearDirector ya están
        // listos en DatabaseManager para cuando llegue ese momento.

        panelFormulario.SetActive(false);
        panelUsuarioCreado.SetActive(true);
    }

    private void MostrarError(string mensaje)
    {
        if (textoError != null)
            textoError.text = mensaje;
        else
            Debug.LogWarning(mensaje);
    }

    /// <summary>
    /// Genera un hash SHA-256 de la contraseña. No es el método más robusto
    /// que existe (lo ideal a futuro es agregar "salt"), pero es muchísimo
    /// mejor que guardar la contraseña en texto plano.
    /// </summary>
    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}