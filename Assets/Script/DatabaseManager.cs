using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using UnityEngine;

// ============================================================
// MODELOS (uno por tabla - agregar el resto siguiendo el mismo patrón)
// ============================================================

[Table("rol")]
public class Rol
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; }
}

[Table("usuario")]
public class Usuario
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; }

    [Column("apellido")]
    public string Apellido { get; set; }

    [Column("mail")]
    public string Mail { get; set; }

    [Column("password")]
    public string Password { get; set; }

    [Column("activo")]
    public bool Activo { get; set; }

    [Column("dni")]
    public string Dni { get; set; }

    [Column("rol_id")]
    public int RolId { get; set; }
}

// TODO: agregar el resto de los modelos (Alumno, Profesor, Preceptor,
// Director, Grado, Curso, Materia, Avatar, etc.) siguiendo este mismo
// patrón: una clase por tabla, con [Table("nombre_tabla")] y una
// propiedad por columna con [Column("nombre_columna")].

// ============================================================
// GESTOR DE BASE DE DATOS
// ============================================================

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private SQLiteConnection _connection;
    private const string DB_NAME = "schema_base_de_datos_sqlite.db";

    private void Awake()
    {
        // Singleton: nos aseguramos de que solo exista una instancia
        // y que persista entre escenas.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, DB_NAME);

        // Si es la primera vez que corre el juego, copiamos la base
        // "limpia" desde StreamingAssets hacia persistentDataPath,
        // que es la única ubicación con permisos de escritura en
        // todas las plataformas (incluido Android).
        if (!File.Exists(dbPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, DB_NAME);

#if UNITY_ANDROID && !UNITY_EDITOR
            // En Android, StreamingAssets vive dentro del .apk comprimido,
            // así que hay que leerlo con UnityWebRequest en vez de File.Copy.
            var www = UnityEngine.Networking.UnityWebRequest.Get(sourcePath);
            www.SendWebRequest();
            while (!www.isDone) { } // simplificado; en un caso real conviene una corutina
            File.WriteAllBytes(dbPath, www.downloadHandler.data);
#else
            File.Copy(sourcePath, dbPath);
#endif
        }

        _connection = new SQLiteConnection(dbPath);
        Debug.Log($"Base de datos inicializada en: {dbPath}");
    }

    // ---------- Ejemplos de operaciones ----------

    public List<Rol> GetTodosLosRoles()
    {
        return _connection.Table<Rol>().ToList();
    }

    public Usuario GetUsuarioPorMail(string mail)
    {
        return _connection.Table<Usuario>()
                           .Where(u => u.Mail == mail)
                           .FirstOrDefault();
    }

    public int CrearUsuario(Usuario nuevoUsuario)
    {
        return _connection.Insert(nuevoUsuario);
    }

    public void ActualizarUsuario(Usuario usuario)
    {
        _connection.Update(usuario);
    }

    public bool ExisteMail(string mail)
    {
        return _connection.Table<Usuario>().Where(u => u.Mail == mail).Count() > 0;
    }

    public bool ExisteDni(string dni)
    {
        return _connection.Table<Usuario>().Where(u => u.Dni == dni).Count() > 0;
    }

    private void OnApplicationQuit()
    {
        _connection?.Close();
    }
}
