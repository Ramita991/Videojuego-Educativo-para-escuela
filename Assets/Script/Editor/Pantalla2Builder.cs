using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class Pantalla2Builder
{
    [MenuItem("Tools/HU-008/Crear Panel_EnlaceEnviado (pantalla 2)")]
    public static void Crear()
    {
        GameObject origen = GameObject.Find("Canvas/RecuperarContrasenaUI/Panel_PedirMail");
        GameObject contenedor = GameObject.Find("Canvas/RecuperarContrasenaUI");
        if (origen == null || contenedor == null)
        {
            Debug.LogError("No encontré Panel_PedirMail o RecuperarContrasenaUI.");
            return;
        }

        // Si ya existe una copia previa, la borramos para no duplicar de más
        Transform existente = contenedor.transform.Find("Panel_EnlaceEnviado");
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        // Duplicamos toda la pantalla 1 para heredar el mismo estilo (card, fuentes, colores)
        GameObject nuevo = Object.Instantiate(origen, contenedor.transform);
        nuevo.name = "Panel_EnlaceEnviado";
        nuevo.SetActive(false); // arranca apagada, se prende cuando corresponda

        Transform t = nuevo.transform;

        // Ya no necesitamos el campo de mail en esta pantalla
        Transform panelMail = t.Find("Panel_Mail");
        if (panelMail != null) Object.DestroyImmediate(panelMail.gameObject);

        // Título
        Transform titulo = t.Find("TextArribaG (TMP)");
        if (titulo != null)
        {
            var tmp = titulo.GetComponent<TMP_Text>();
            tmp.text = "¡Enlace enviado!";
            var rt = titulo.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 150);
        }

        // Subtítulo (acá va el mail enmascarado — lo actualiza el controlador en runtime)
        Transform subtitulo = t.Find("Subtitulo");
        TMP_Text subtituloTmp = null;
        if (subtitulo != null)
        {
            subtituloTmp = subtitulo.GetComponent<TMP_Text>();
            subtituloTmp.text = "Revisá la bandeja de entrada de tu correo. El enlace expira en 30 minutos por seguridad.";
            var rt = subtitulo.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 70);
            rt.sizeDelta = new Vector2(750, 120);
        }
        // Le ponemos un nombre fijo para poder encontrarlo fácil desde el controlador
        if (subtitulo != null) subtitulo.gameObject.name = "TextoMailEnviado";

        // Botón principal: ahora es "Simular: abrir enlace recibido"
        Transform botonSimular = t.Find("BotonEnviarEnlace");
        if (botonSimular != null)
        {
            botonSimular.gameObject.name = "BotonSimularAbrirEnlace";
            var tmp = botonSimular.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = "Simular: abrir enlace recibido →";
            var rt = botonSimular.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -80);
        }

        // Botón secundario: se mantiene igual (Volver a iniciar sesión)
        Transform botonVolver = t.Find("BotonVolver");
        if (botonVolver != null)
        {
            var rt = botonVolver.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -190);
        }

        // Mensaje de estado, abajo de todo
        GameObject estadoGO = new GameObject("TextoEstado", typeof(RectTransform));
        estadoGO.transform.SetParent(t, false);
        var estadoTmp = estadoGO.AddComponent<TextMeshProUGUI>();
        estadoTmp.text = "✅ Correo despachado correctamente";
        if (subtituloTmp != null) estadoTmp.font = subtituloTmp.font;
        estadoTmp.fontSize = 20;
        estadoTmp.color = new Color32(0x4A, 0xDE, 0x80, 255); // verde
        estadoTmp.alignment = TextAlignmentOptions.Center;
        var estadoRt = estadoGO.GetComponent<RectTransform>();
        estadoRt.anchorMin = estadoRt.anchorMax = new Vector2(0.5f, 0.5f);
        estadoRt.sizeDelta = new Vector2(700, 50);
        estadoRt.anchoredPosition = new Vector2(0, -280);

        // Indicador de pasos: en esta pantalla, el paso 2 es el activo
        Transform indicador = t.Find("IndicadorPasos");
        if (indicador != null)
        {
            var paso1 = indicador.Find("Paso1")?.GetComponent<Image>();
            var paso2 = indicador.Find("Paso2")?.GetComponent<Image>();
            var paso3 = indicador.Find("Paso3")?.GetComponent<Image>();
            Color completado = new Color32(0x2D, 0xD4, 0xBF, 255); // verde azulado (listo)
            Color activo = new Color32(0xFF, 0xA7, 0x26, 255);     // naranja (actual)
            Color inactivo = new Color32(0x33, 0x3F, 0x55, 255);   // gris (falta)
            if (paso1 != null) paso1.color = completado;
            if (paso2 != null) paso2.color = activo;
            if (paso3 != null) paso3.color = inactivo;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(nuevo.scene);
        Debug.Log("✅ Panel_EnlaceEnviado creado. Revisalo en la Hierarchy (está apagado, prendelo para verlo) y guardá con Ctrl+S.");
    }
}
