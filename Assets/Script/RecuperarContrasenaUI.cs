using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el flujo completo de HU-008 (Recuperar contraseña):
///   Pantalla 1: pedir mail -> valida contra la base y manda un código real por mail.
///   Pantalla 2: ingresar el código de 6 dígitos (con opción de reenviar) -> valida
///               que coincida y no haya vencido (30 min).
///   Pantalla 3: cargar nueva contraseña -> valida y actualiza la base.
/// </summary>
public class RecuperarContrasenaUI : MonoBehaviour
{
    [Header("Pantalla 1 - Pedir mail")]
    [SerializeField] private GameObject panelPedirMail;
    [SerializeField] private TMP_InputField inputMail;
    [SerializeField] private Button botonEnviarEnlace;
    [SerializeField] private Button botonVolver1;
    [SerializeField] private TMP_Text textoError;

    [Header("Pantalla 2 - Código")]
    [SerializeField] private GameObject panelEnlaceEnviado;
    [SerializeField] private TMP_Text textoMailEnviado;
    [SerializeField] private TMP_InputField inputCodigo;
    [SerializeField] private Button botonVerificarCodigo;
    [SerializeField] private Button botonReenviarCodigo;
    [SerializeField] private Button botonVolver2;
    [SerializeField] private TMP_Text textoErrorCodigo;

    [Header("Pantalla 3 - Nueva contraseña")]
    [SerializeField] private GameObject panelNuevaContrasena;
    [SerializeField] private TMP_InputField inputNuevaPass;
    [SerializeField] private TMP_InputField inputConfirmarPass;
    [SerializeField] private TMP_Text textoCoincidencia;
    [SerializeField] private Button botonGuardar;
    [SerializeField] private TMP_Text textoErrorPass;

    private const int COOLDOWN_REENVIO_SEGUNDOS = 30;

    private string _mailActual;
    private TMP_Text _textoBotonReenviar;
    private Coroutine _cooldownCoroutine;

    private void OnEnable()
    {
        if (botonEnviarEnlace != null) botonEnviarEnlace.onClick.AddListener(OnEnviarEnlace);
        if (botonVolver1 != null) botonVolver1.onClick.AddListener(OnVolver);
        if (botonVerificarCodigo != null) botonVerificarCodigo.onClick.AddListener(OnVerificarCodigo);
        if (botonReenviarCodigo != null)
        {
            botonReenviarCodigo.onClick.AddListener(OnReenviarCodigo);
            _textoBotonReenviar = botonReenviarCodigo.GetComponentInChildren<TMP_Text>();
        }
        if (botonVolver2 != null) botonVolver2.onClick.AddListener(OnVolver);
        if (botonGuardar != null) botonGuardar.onClick.AddListener(OnGuardarNuevaContrasena);

        if (inputNuevaPass != null) inputNuevaPass.onValueChanged.AddListener(_ => ActualizarCoincidencia());
        if (inputConfirmarPass != null) inputConfirmarPass.onValueChanged.AddListener(_ => ActualizarCoincidencia());

        LimpiarErrores();
    }

    private void OnDisable()
    {
        if (botonEnviarEnlace != null) botonEnviarEnlace.onClick.RemoveListener(OnEnviarEnlace);
        if (botonVolver1 != null) botonVolver1.onClick.RemoveListener(OnVolver);
        if (botonVerificarCodigo != null) botonVerificarCodigo.onClick.RemoveListener(OnVerificarCodigo);
        if (botonReenviarCodigo != null) botonReenviarCodigo.onClick.RemoveListener(OnReenviarCodigo);
        if (botonVolver2 != null) botonVolver2.onClick.RemoveListener(OnVolver);
        if (botonGuardar != null) botonGuardar.onClick.RemoveListener(OnGuardarNuevaContrasena);

        if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);
    }

    // ================= PANTALLA 1 =================

    private async void OnEnviarEnlace()
    {
        string mail = inputMail.text.Trim();

        if (string.IsNullOrEmpty(mail) || !mail.Contains("@"))
        {
            MostrarError("El correo electrónico no es válido.");
            return;
        }

        if (DatabaseManager.Instance == null || !DatabaseManager.Instance.EstaLista)
        {
            MostrarError("La base de datos todavía no está lista, esperá un segundo.");
            return;
        }

        if (!DatabaseManager.Instance.ExisteMail(mail))
        {
            MostrarError("El email no existe en la base de datos.");
            return;
        }

        LimpiarErrores();
        botonEnviarEnlace.interactable = false;

        bool enviado = await GenerarYEnviarCodigo(mail);

        botonEnviarEnlace.interactable = true;

        if (!enviado)
        {
            MostrarError("No se pudo enviar el mail. Revisá tu conexión e intentá de nuevo.");
            return;
        }

        _mailActual = mail;

        if (panelEnlaceEnviado != null)
        {
            panelPedirMail.SetActive(false);
            panelEnlaceEnviado.SetActive(true);
        }

        if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);
        _cooldownCoroutine = StartCoroutine(CooldownReenvio());
    }

    // ================= PANTALLA 2 =================

    private void OnVerificarCodigo()
    {
        string codigo = inputCodigo.text.Trim();

        if (string.IsNullOrEmpty(codigo))
        {
            MostrarErrorCodigo("Ingresá el código que te llegó por mail.");
            return;
        }

        if (!DatabaseManager.Instance.ValidarCodigoRecuperacion(_mailActual, codigo))
        {
            MostrarErrorCodigo("Código incorrecto o vencido. Pedí uno nuevo si hace falta.");
            return;
        }

        if (textoErrorCodigo != null) textoErrorCodigo.text = "";

        if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);

        if (panelNuevaContrasena != null)
        {
            panelEnlaceEnviado.SetActive(false);
            panelNuevaContrasena.SetActive(true);
        }
    }

    private async void OnReenviarCodigo()
    {
        if (string.IsNullOrEmpty(_mailActual)) return;

        botonReenviarCodigo.interactable = false;
        bool enviado = await GenerarYEnviarCodigo(_mailActual);

        if (!enviado)
        {
            MostrarErrorCodigo("No se pudo reenviar el mail. Probá de nuevo en un momento.");
        }
        else
        {
            if (textoErrorCodigo != null) textoErrorCodigo.text = "";
        }

        if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);
        _cooldownCoroutine = StartCoroutine(CooldownReenvio());
    }

    /// <summary>Deja el botón de reenvío deshabilitado 30 segundos, mostrando la cuenta regresiva.</summary>
    private IEnumerator CooldownReenvio()
    {
        if (botonReenviarCodigo == null) yield break;

        botonReenviarCodigo.interactable = false;
        for (int restantes = COOLDOWN_REENVIO_SEGUNDOS; restantes > 0; restantes--)
        {
            if (_textoBotonReenviar != null)
                _textoBotonReenviar.text = $"Enviar código nuevo ({restantes}s)";
            yield return new WaitForSeconds(1f);
        }

        if (_textoBotonReenviar != null) _textoBotonReenviar.text = "Enviar código nuevo";
        botonReenviarCodigo.interactable = true;
    }

    // ================= PANTALLA 3 =================

    private void OnGuardarNuevaContrasena()
    {
        string pass1 = inputNuevaPass.text;
        string pass2 = inputConfirmarPass.text;

        if (pass1.Length < 8)
        {
            MostrarErrorPass("La contraseña tiene que tener mínimo 8 caracteres.");
            return;
        }

        if (pass1 != pass2)
        {
            MostrarErrorPass("Las contraseñas no coinciden.");
            return;
        }

        var usuario = DatabaseManager.Instance.GetUsuarioPorMail(_mailActual);
        if (usuario == null)
        {
            MostrarErrorPass("No se encontró el usuario. Volvé a empezar el proceso.");
            return;
        }

        usuario.Password = pass1;
        DatabaseManager.Instance.ActualizarUsuario(usuario);

        if (textoErrorPass != null) textoErrorPass.text = "";

        Debug.Log($"[Recuperar contraseña] Contraseña actualizada correctamente para: {_mailActual}");

        // TODO: cuando conecten el Login real, acá conviene mandarlo ahí en vez de solo resetear.
        ResetearFlujo();
    }

    private void ActualizarCoincidencia()
    {
        if (textoCoincidencia == null) return;

        if (string.IsNullOrEmpty(inputConfirmarPass.text))
        {
            textoCoincidencia.text = "";
            return;
        }

        bool coincide = inputNuevaPass.text == inputConfirmarPass.text;
        textoCoincidencia.text = coincide ? "✓ Las contraseñas coinciden" : "Las contraseñas no coinciden";
        textoCoincidencia.color = coincide
            ? new Color32(0x4A, 0xDE, 0x80, 255)
            : new Color32(0xF8, 0x71, 0x71, 255);
    }

    // ================= COMUNES =================

    private async System.Threading.Tasks.Task<bool> GenerarYEnviarCodigo(string mail)
    {
        string codigo = GenerarCodigo();
        long expira = DateTime.UtcNow.AddMinutes(30).Ticks;
        DatabaseManager.Instance.GuardarCodigoRecuperacion(mail, codigo, expira);
        return await EmailSender.EnviarCodigoAsync(mail, codigo);
    }

    private void OnVolver()
    {
        // TODO: conectar con la pantalla de Login real (ver instrucciones que te pasé por chat).
        Debug.Log("[Recuperar contraseña] Volver a iniciar sesión (pendiente conectar con Login).");
    }

    private void ResetearFlujo()
    {
        _mailActual = null;
        if (inputMail != null) inputMail.text = "";
        if (inputCodigo != null) inputCodigo.text = "";
        if (inputNuevaPass != null) inputNuevaPass.text = "";
        if (inputConfirmarPass != null) inputConfirmarPass.text = "";

        if (panelNuevaContrasena != null) panelNuevaContrasena.SetActive(false);
        if (panelEnlaceEnviado != null) panelEnlaceEnviado.SetActive(false);
        if (panelPedirMail != null) panelPedirMail.SetActive(true);
    }

    private string GenerarCodigo()
    {
        var rnd = new System.Random();
        return rnd.Next(0, 1000000).ToString("D6");
    }

    private void MostrarError(string mensaje)
    {
        if (textoError != null) textoError.text = mensaje;
        else Debug.LogWarning(mensaje);
    }

    private void MostrarErrorCodigo(string mensaje)
    {
        if (textoErrorCodigo != null) textoErrorCodigo.text = mensaje;
        else Debug.LogWarning(mensaje);
    }

    private void MostrarErrorPass(string mensaje)
    {
        if (textoErrorPass != null) textoErrorPass.text = mensaje;
        else Debug.LogWarning(mensaje);
    }

    private void LimpiarErrores()
    {
        if (textoError != null) textoError.text = "";
        if (textoErrorCodigo != null) textoErrorCodigo.text = "";
        if (textoErrorPass != null) textoErrorPass.text = "";
    }
}
