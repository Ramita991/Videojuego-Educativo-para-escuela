using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

[Table("alumno")]
public class Alumno
{
    [PrimaryKey,Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("username")]
    public string Username { get; set; }

    // Nullable porque el formulario de "Crear usuario" todavía no tiene
    // selector de grado/curso ni de avatar (se pueden asignar después).
    [Column("grado_curso_id")]
    public int? GradoCursoId { get; set; }

    [Column("avatar_id")]
    public int? AvatarId { get; set; }

}

[Table("profesor")]
public class Profesor
{
    [PrimaryKey, Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("avatar_id")]
    public int? AvatarId { get; set; }
}

[Table("preceptor")]
public class Preceptor
{
    [PrimaryKey, Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("turno")]
    public string Turno { get; set; }
}

[Table("director")]
public class Director
{
    [PrimaryKey, Column("usuario_id")]
    public int UsuarioId { get; set; }
}

[Table("grado")]
public class Grado
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("numero")]
    public int Numero { get; set; }
}

[Table("curso")]
public class Curso
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("division")]
    public string Division { get; set; }
}

[Table("grado_curso")]
public class GradoCurso
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("grado_id")]
    public int GradoId { get; set; }

    [Column("curso_id")]
    public int CursoId { get; set; }

    [Column("ciclo_lectivo")]
    public int CicloLectivo { get; set; }
}

/// <summary>Fila lista para mostrar en un dropdown: id + etiqueta legible (ej: "1° A").</summary>
public class GradoCursoOpcion
{
    public int GradoCursoId;
    public string Etiqueta;
}

// Agregar aquí las propiedades específicas de Alumno (ej: grado, curso, etc.)
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

    /// <summary>True cuando la conexión ya está lista para usarse.</summary>
    public bool EstaLista { get; private set; } = false;

    /// <summary>Se dispara una sola vez, cuando la base de datos termina de inicializarse.</summary>
    public event Action OnBaseDeDatosLista;

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

        StartCoroutine(InitializeDatabase());
    }

    /// <summary>
    /// Copia la base "limpia" desde StreamingAssets a persistentDataPath
    /// (si hace falta) y abre la conexión. Es una corrutina porque en
    /// Android la lectura de StreamingAssets es asincrónica (UnityWebRequest).
    /// </summary>
    private IEnumerator InitializeDatabase()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, DB_NAME);

        if (!File.Exists(dbPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, DB_NAME);

#if UNITY_ANDROID && !UNITY_EDITOR
            // En Android, StreamingAssets vive dentro del .apk comprimido,
            // así que hay que leerlo con UnityWebRequest en vez de File.Copy.
            using (var www = UnityEngine.Networking.UnityWebRequest.Get(sourcePath))
            {
                yield return www.SendWebRequest();
 
                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"No se pudo copiar la base de datos desde StreamingAssets: {www.error}");
                    yield break;
                }
 
                File.WriteAllBytes(dbPath, www.downloadHandler.data);
            }
#else
            File.Copy(sourcePath, dbPath);
            yield return null;
#endif
        }

        _connection = new SQLiteConnection(dbPath);
        Debug.Log($"Base de datos inicializada en: {dbPath}");

        EstaLista = true;
        OnBaseDeDatosLista?.Invoke();
    }

    // ---------- Roles / Usuario ----------

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

    // ---------- Grado / Curso ----------

    /// <summary>
    /// Devuelve las combinaciones de grado/curso de un ciclo lectivo (por
    /// defecto, el año actual) ya armadas con una etiqueta legible para
    /// mostrar en un dropdown (ej: "1° A").
    /// </summary>
    public List<GradoCursoOpcion> GetGradosCursosDisponibles(int? cicloLectivo = null)
    {
        int ciclo = cicloLectivo ?? DateTime.Now.Year;

        var grados = _connection.Table<Grado>().ToList();
        var cursos = _connection.Table<Curso>().ToList();
        var combinaciones = _connection.Table<GradoCurso>()
                                        .Where(gc => gc.CicloLectivo == ciclo)
                                        .ToList();

        var resultado = new List<GradoCursoOpcion>();
        foreach (var gc in combinaciones)
        {
            var grado = grados.FirstOrDefault(g => g.Id == gc.GradoId);
            var curso = cursos.FirstOrDefault(c => c.Id == gc.CursoId);

            string numero = grado != null ? grado.Numero.ToString() : "?";
            string division = curso != null ? curso.Division : "?";

            resultado.Add(new GradoCursoOpcion
            {
                GradoCursoId = gc.Id,
                Etiqueta = $"{numero}° {division}"
            });
        }

        return resultado;
    }

    // ---------- Alumno / Profesor / Preceptor / Director ----------
    // Cada uno de estos crea la fila "extra" que complementa al Usuario
    // según el rol. Por ahora solo guardan el usuario_id; a futuro se les
    // pueden sumar los campos específicos de cada rol (ver TODOs arriba).

    public int CrearAlumno(Alumno alumno)
    {
        return _connection.Insert(alumno);
    }

    public int CrearProfesor(Profesor profesor)
    {
        return _connection.Insert(profesor);
    }

    public int CrearPreceptor(Preceptor preceptor)
    {
        return _connection.Insert(preceptor);
    }

    public int CrearDirector(Director director)
    {
        return _connection.Insert(director);
    }

    private void OnApplicationQuit()
    {
        _connection?.Close();
    }
}