using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FilaUsuarioView : MonoBehaviour
{
    [Header("Referencias TextMeshPro")]
    [SerializeField] private TMP_Text txtNombre;
    [SerializeField] private TMP_Text txtApellido;
    [SerializeField] private TMP_Text txtDNI;
    [SerializeField] private TMP_Text txtEmail;
    [SerializeField] private TMP_Text txtRol;

    [Header("Badge de Estado")]
    [SerializeField] private Image imgBadgeFondo;
    [SerializeField] private TMP_Text txtBadgeEstado;

    [Header("Botones de Accion")]
    [SerializeField] private Button btnEditar;
    [SerializeField] private Button btnDeshabilitar;

    // Colores visuales del diseño QuestLearn
    private readonly Color colorActivoTexto = new Color(0.18f, 0.80f, 0.44f, 1f);      // Verde #2ECC71
    private readonly Color colorActivoFondo = new Color(0.08f, 0.27f, 0.16f, 0.45f);  // Verde traslúcido
    private readonly Color colorInactivoTexto = new Color(0.90f, 0.29f, 0.23f, 1f);    // Rojo #E74C3C
    private readonly Color colorInactivoFondo = new Color(0.35f, 0.10f, 0.10f, 0.45f); // Rojo traslúcido

    private UsuarioModel usuarioActual;
    private Action<UsuarioModel> onDesactivarClickCallback;
    private Action<UsuarioModel> onEditarClickCallback;

    public void ConfigurarFila(UsuarioModel usuario, Action<UsuarioModel> onDesactivar, Action<UsuarioModel> onEditar)
    {
        usuarioActual = usuario;
        onDesactivarClickCallback = onDesactivar;
        onEditarClickCallback = onEditar;

        // Asignación de datos
        txtNombre.text = usuario.nombre;
        txtApellido.text = usuario.apellido;
        txtDNI.text = usuario.dni;
        txtEmail.text = usuario.email;
        txtRol.text = usuario.rol.ToString();

        // Control visual del badge Activo / Inactivo
        ActualizarVisualEstado(usuario.activo);

        // Limpieza y asignación de eventos OnClick
        btnEditar.onClick.RemoveAllListeners();
        btnEditar.onClick.AddListener(() => onEditarClickCallback?.Invoke(usuarioActual));

        btnDeshabilitar.onClick.RemoveAllListeners();
        btnDeshabilitar.onClick.AddListener(() => onDesactivarClickCallback?.Invoke(usuarioActual));
    }

    private void ActualizarVisualEstado(bool estaActivo)
    {
        if (estaActivo)
        {
            txtBadgeEstado.text = "Activo";
            txtBadgeEstado.color = colorActivoTexto;
            if (imgBadgeFondo != null) imgBadgeFondo.color = colorActivoFondo;
        }
        else
        {
            txtBadgeEstado.text = "Inactivo";
            txtBadgeEstado.color = colorInactivoTexto;
            if (imgBadgeFondo != null) imgBadgeFondo.color = colorInactivoFondo;
        }
    }
}