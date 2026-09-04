using System.IO;
using SQLite;
using UnityEditor;
using UnityEngine;

public class SQLiteViewerWindow : EditorWindow  
{
    private const string DB_NAME = "schema_base_de_datos_sqlite.db";
    private SQLiteConnection _connection;
    private Vector2 _scroll;
    private int _tabIndex;
    private readonly string[] _tabs = { "Usuarios", "Roles" }; // agregá más pestañas cuando sumes tablas

    [MenuItem("Tools/Ver Base de Datos")]
    public static void ShowWindow() => GetWindow<SQLiteViewerWindow>("DB Viewer");

    private void OnEnable() => Conectar();
    private void OnDisable() { _connection?.Close(); _connection = null; }

    private void Conectar()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, DB_NAME);
        if (!File.Exists(dbPath))
        {
            Debug.LogWarning($"No se encontró la base en: {dbPath}. Corré el juego al menos una vez.");
            return;
        }
        // ReadOnly para no interferir con la conexión del juego si está corriendo
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Recargar")) { _connection?.Close(); Conectar(); }

        if (_connection == null)
        {
            EditorGUILayout.HelpBox("No hay conexión. Corré el juego una vez para generar la base.", MessageType.Warning);
            return;
        }

        _tabIndex = GUILayout.Toolbar(_tabIndex, _tabs);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_tabIndex == 0)
        {
            var usuarios = _connection.Table<Usuario>().ToList();
            EditorGUILayout.LabelField($"Total: {usuarios.Count}", EditorStyles.boldLabel);
            foreach (var u in usuarios)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"#{u.Id} - {u.Nombre} {u.Apellido}");
                EditorGUILayout.LabelField($"DNI: {u.Dni}  |  Mail: {u.Mail}  |  RolId: {u.RolId}");
                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            var roles = _connection.Table<Rol>().ToList();
            EditorGUILayout.LabelField($"Total: {roles.Count}", EditorStyles.boldLabel);
            foreach (var r in roles)
                EditorGUILayout.LabelField($"#{r.Id} - {r.Nombre}");
        }

        EditorGUILayout.EndScrollView();
    }
}
