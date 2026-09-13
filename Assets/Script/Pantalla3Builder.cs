using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class Pantalla3Builder
{
    [MenuItem("Tools/HU-008/Crear Panel_NuevaContrasena (pantalla 3)")]
    public static void Crear()
    {
        GameObject origen = GameObject.Find("Canvas/RecuperarContrasenaUI/Panel_PedirMail");
        GameObject contenedor = GameObject.Find("Canvas/RecuperarContrasenaUI");
        if (origen == null || contenedor == null)
        {
            Debug.LogError("No encontré Panel_PedirMail o RecuperarContrasenaUI.");
            return;
        }

        Transform existente = contenedor.transform.Find("Panel_NuevaContrasena");
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject nuevo = Object.Instantiate(origen, contenedor.transform);
        nuevo.name = "Panel_NuevaContrasena";
        nuevo.SetActive(false);
        Transform t = nuevo.transform;

        // Título y subtítulo
        Transform titulo = t.Find("TextArribaG (TMP)");
        TMP_FontAsset fuente = null;
        if (titulo != null)
        {
            var tmp = titulo.GetComponent<TMP_Text>();
            fuente = tmp.font;
            tmp.text = "Creá tu nueva contraseña";
            titulo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 260);
        }
        Transform subtitulo = t.Find("Subtitulo");
        if (subtitulo != null)
        {
            subtitulo.GetComponent<TMP_Text>().text =
                "Definí una clave segura. Esto reemplaza tu DNI inicial o tu contraseña anterior.";
            subtitulo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 180);
        }

        // Campo 1: Nueva contraseña (reusamos Panel_Mail como molde)
        Transform panelMailOriginal = t.Find("Panel_Mail");
        Transform panelNuevaPass = panelMailOriginal;
        if (panelNuevaPass != null)
        {
            panelNuevaPass.gameObject.name = "Panel_NuevaPass";
            panelNuevaPass.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 90);

            var label = panelNuevaPass.Find("Mail (TMP)");
            if (label != null) { label.gameObject.name = "Label_NuevaPass"; label.GetComponent<TMP_Text>().text = "NUEVA CONTRASEÑA"; }

            var input = panelNuevaPass.Find("InputMail (TMP)");
            if (input != null)
            {
                input.gameObject.name = "InputNuevaPass";
                var campo = input.GetComponent<TMP_InputField>();
                campo.contentType = TMP_InputField.ContentType.Password;
                campo.text = "";
                var placeholder = input.GetComponentInChildren<TMP_Text>();
                // el placeholder es un TMP_Text hijo de "Text Area/Placeholder"
                var placeholderT = input.Find("Text Area/Placeholder")?.GetComponent<TMP_Text>();
                if (placeholderT != null) placeholderT.text = "Mínimo 8 caracteres";
            }
        }

        // Campo 2: Confirmar contraseña (duplicamos el campo 1 ya armado)
        GameObject panelConfirmarGO = Object.Instantiate(panelNuevaPass.gameObject, t);
        panelConfirmarGO.name = "Panel_ConfirmarPass";
        panelConfirmarGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
        var labelConfirmar = panelConfirmarGO.transform.Find("Label_NuevaPass");
        if (labelConfirmar != null) { labelConfirmar.gameObject.name = "Label_ConfirmarPass"; labelConfirmar.GetComponent<TMP_Text>().text = "CONFIRMAR NUEVA CONTRASEÑA"; }
        var inputConfirmar = panelConfirmarGO.transform.Find("InputNuevaPass");
        if (inputConfirmar != null)
        {
            inputConfirmar.gameObject.name = "InputConfirmarPass";
            var placeholderT = inputConfirmar.Find("Text Area/Placeholder")?.GetComponent<TMP_Text>();
            if (placeholderT != null) placeholderT.text = "Repetí la contraseña";
        }

        // Texto que confirma si coinciden (lo actualiza el controlador en runtime)
        GameObject coincideGO = new GameObject("TextoCoincidencia", typeof(RectTransform));
        coincideGO.transform.SetParent(t, false);
        var coincideTmp = coincideGO.AddComponent<TextMeshProUGUI>();
        coincideTmp.text = "";
        if (fuente != null) coincideTmp.font = fuente;
        coincideTmp.fontSize = 18;
        coincideTmp.alignment = TextAlignmentOptions.Center;
        var coincideRt = coincideGO.GetComponent<RectTransform>();
        coincideRt.anchorMin = coincideRt.anchorMax = new Vector2(0.5f, 0.5f);
        coincideRt.sizeDelta = new Vector2(700, 40);
        coincideRt.anchoredPosition = new Vector2(0, -105);

        // Botón: Guardar y acceder a la plataforma (reemplaza BotonEnviarEnlace)
        Transform botonGuardar = t.Find("BotonEnviarEnlace");
        if (botonGuardar != null)
        {
            botonGuardar.gameObject.name = "BotonGuardar";
            var tmp = botonGuardar.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = "Guardar y acceder a la plataforma";
            botonGuardar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -180);
        }

        // En esta pantalla no hay botón "Volver" (según el diseño de referencia)
        Transform botonVolver = t.Find("BotonVolver");
        if (botonVolver != null) Object.DestroyImmediate(botonVolver.gameObject);

        // Texto de error (opcional, lo usa el controlador)
        GameObject errorGO = new GameObject("TextoErrorPass", typeof(RectTransform));
        errorGO.transform.SetParent(t, false);
        var errorTmp = errorGO.AddComponent<TextMeshProUGUI>();
        errorTmp.text = "";
        if (fuente != null) errorTmp.font = fuente;
        errorTmp.fontSize = 18;
        errorTmp.color = new Color32(0xF8, 0x71, 0x71, 255); // rojo suave
        errorTmp.alignment = TextAlignmentOptions.Center;
        var errorRt = errorGO.GetComponent<RectTransform>();
        errorRt.anchorMin = errorRt.anchorMax = new Vector2(0.5f, 0.5f);
        errorRt.sizeDelta = new Vector2(700, 60);
        errorRt.anchoredPosition = new Vector2(0, -250);

        // Indicador: pasos 1 y 2 completados, paso 3 activo
        Transform indicador = t.Find("IndicadorPasos");
        if (indicador != null)
        {
            var paso1 = indicador.Find("Paso1")?.GetComponent<Image>();
            var paso2 = indicador.Find("Paso2")?.GetComponent<Image>();
            var paso3 = indicador.Find("Paso3")?.GetComponent<Image>();
            Color completado = new Color32(0x2D, 0xD4, 0xBF, 255);
            Color activo = new Color32(0xFF, 0xA7, 0x26, 255);
            if (paso1 != null) paso1.color = completado;
            if (paso2 != null) paso2.color = completado;
            if (paso3 != null) paso3.color = activo;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(nuevo.scene);
        Debug.Log("✅ Panel_NuevaContrasena creado. Prendelo para verlo y guardá con Ctrl+S.");
    }
}
