using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class Pantalla2Builder
{
    [MenuItem("Tools/HU-008/Crear Panel_EnlaceEnviado (pantalla 2, con código)")]
    public static void Crear()
    {
        GameObject origen = GameObject.Find("Canvas/RecuperarContrasenaUI/Panel_PedirMail");
        GameObject contenedor = GameObject.Find("Canvas/RecuperarContrasenaUI");
        if (origen == null || contenedor == null)
        {
            Debug.LogError("No encontré Panel_PedirMail o RecuperarContrasenaUI.");
            return;
        }

        Transform existente = contenedor.transform.Find("Panel_EnlaceEnviado");
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject nuevo = Object.Instantiate(origen, contenedor.transform);
        nuevo.name = "Panel_EnlaceEnviado";
        nuevo.SetActive(false);
        Transform t = nuevo.transform;

        Transform titulo = t.Find("TextArribaG (TMP)");
        TMP_FontAsset fuente = null;
        if (titulo != null)
        {
            var tmp = titulo.GetComponent<TMP_Text>();
            fuente = tmp.font;
            tmp.text = "¡Enlace enviado!";
            titulo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);
        }

        Transform subtitulo = t.Find("Subtitulo");
        if (subtitulo != null)
        {
            subtitulo.gameObject.name = "TextoMailEnviado";
            subtitulo.GetComponent<TMP_Text>().text =
                "Ingrese el código enviado al email registrado.";
            var rt = subtitulo.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 225);
            rt.sizeDelta = new Vector2(750, 90);
        }

        // Campo del código (reusamos Panel_Mail como molde)
        Transform panelCodigo = t.Find("Panel_Mail");
        if (panelCodigo != null)
        {
            panelCodigo.gameObject.name = "Panel_Codigo";
            panelCodigo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 110);

            var label = panelCodigo.Find("Mail (TMP)");
            if (label != null) label.GetComponent<TMP_Text>().text = "CÓDIGO DE VERIFICACIÓN";

            var input = panelCodigo.Find("InputMail (TMP)");
            if (input != null)
            {
                input.gameObject.name = "InputCodigo";
                var campo = input.GetComponent<TMP_InputField>();
                campo.characterLimit = 6;
                campo.text = "";
                var placeholderT = input.Find("Text Area/Placeholder")?.GetComponent<TMP_Text>();
                if (placeholderT != null) placeholderT.text = "000000";
            }
        }

        // Botón: Verificar código
        Transform botonVerificar = t.Find("BotonEnviarEnlace");
        if (botonVerificar != null)
        {
            botonVerificar.gameObject.name = "BotonVerificarCodigo";
            var tmp = botonVerificar.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = "Verificar código";
            botonVerificar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
        }

        // Texto de "correo despachado" / estado del envío
        GameObject estadoGO = new GameObject("TextoEstadoEnvio", typeof(RectTransform));
        estadoGO.transform.SetParent(t, false);
        var estadoTmp = estadoGO.AddComponent<TextMeshProUGUI>();
        estadoTmp.text = "✓ Correo despachado correctamente";
        if (fuente != null) estadoTmp.font = fuente;
        estadoTmp.fontSize = 18;
        estadoTmp.color = new Color32(0x4A, 0xDE, 0x80, 255);
        estadoTmp.alignment = TextAlignmentOptions.Center;
        var estadoRt = estadoGO.GetComponent<RectTransform>();
        estadoRt.anchorMin = estadoRt.anchorMax = new Vector2(0.5f, 0.5f);
        estadoRt.sizeDelta = new Vector2(700, 40);
        estadoRt.anchoredPosition = new Vector2(0, -75);

        // NUEVO: Botón "Enviar código nuevo" (reenvío, con cooldown de 30s manejado por el controlador)
        GameObject reenviarGO = Object.Instantiate(botonVerificar.gameObject, t);
        reenviarGO.name = "BotonReenviarCodigo";
        var imgReenviar = reenviarGO.GetComponent<Image>();
        if (imgReenviar != null) imgReenviar.color = new Color32(0x1A, 0x24, 0x38, 255); // estilo "outline", no naranja
        var tmpReenviar = reenviarGO.GetComponentInChildren<TMP_Text>();
        if (tmpReenviar != null) { tmpReenviar.text = "Enviar código nuevo"; tmpReenviar.color = Color.white; }
        reenviarGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -130);

        // Botón: Volver (se mantiene, más abajo)
        Transform botonVolver = t.Find("BotonVolver");
        if (botonVolver != null)
            botonVolver.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -240);

        // Texto de error (código incorrecto / vencido)
        GameObject errorGO = new GameObject("TextoErrorCodigo", typeof(RectTransform));
        errorGO.transform.SetParent(t, false);
        var errorTmp = errorGO.AddComponent<TextMeshProUGUI>();
        errorTmp.text = "";
        if (fuente != null) errorTmp.font = fuente;
        errorTmp.fontSize = 18;
        errorTmp.color = new Color32(0xF8, 0x71, 0x71, 255);
        errorTmp.alignment = TextAlignmentOptions.Center;
        var errorRt = errorGO.GetComponent<RectTransform>();
        errorRt.anchorMin = errorRt.anchorMax = new Vector2(0.5f, 0.5f);
        errorRt.sizeDelta = new Vector2(700, 50);
        errorRt.anchoredPosition = new Vector2(0, -300);

        // Indicador: paso 1 completado, paso 2 activo
        Transform indicador = t.Find("IndicadorPasos");
        if (indicador != null)
        {
            var paso1 = indicador.Find("Paso1")?.GetComponent<Image>();
            var paso2 = indicador.Find("Paso2")?.GetComponent<Image>();
            var paso3 = indicador.Find("Paso3")?.GetComponent<Image>();
            Color completado = new Color32(0x2D, 0xD4, 0xBF, 255);
            Color activo = new Color32(0xFF, 0xA7, 0x26, 255);
            Color inactivo = new Color32(0x33, 0x3F, 0x55, 255);
            if (paso1 != null) paso1.color = completado;
            if (paso2 != null) paso2.color = activo;
            if (paso3 != null) paso3.color = inactivo;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(nuevo.scene);
        Debug.Log("✅ Panel_EnlaceEnviado (con código + reenvío) creado. Prendelo para verlo y guardá con Ctrl+S.");
    }
}
