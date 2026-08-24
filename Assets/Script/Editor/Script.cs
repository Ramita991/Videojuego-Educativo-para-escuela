using System.IO;
using SQLite;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de Editor para crear (si hace falta) y sembrar la base de
/// datos "maestra" que se distribuye con el juego.
///
/// A diferencia de una base de runtime, esta trabaja sobre StreamingAssets
/// (la copia "de fábrica" que viaja con el build), no sobre
/// persistentDataPath (la copia de cada jugador). DatabaseManager se
/// encarga de copiar esta base maestra a persistentDataPath la primera
/// vez que corre el juego.
///
/// Uso: Unity -> menú "Tools" -> "Crear y Sembrar Base de Datos".
/// </summary>
public class SeedDatabaseWindow : EditorWindow
{
    private const string DB_NAME = "schema_base_de_datos_sqlite.db";

    [MenuItem("Tools/Crear y Sembrar Base de Datos")]
    public static void CrearYSembrarBaseDeDatos()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;

        // StreamingAssets no existe en un proyecto nuevo hasta que algo la crea.
        if (!Directory.Exists(streamingAssetsPath))
        {
            Directory.CreateDirectory(streamingAssetsPath);
            Debug.Log($"Se creó la carpeta StreamingAssets en: {streamingAssetsPath}");
        }

        string dbPath = Path.Combine(streamingAssetsPath, DB_NAME);
        bool esBaseNueva = !File.Exists(dbPath);

        // Create: si el archivo no existe, lo crea. ReadWrite: para poder insertar.
        using (var connection = new SQLiteConnection(dbPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create))
        {
            // CreateTable<T>() usa "CREATE TABLE IF NOT EXISTS" por dentro,
            // así que es seguro correrlo de nuevo: no borra datos ni pisa
            // lo que ya está cargado, solo crea lo que falte.
            connection.CreateTable<Rol>();
            connection.CreateTable<Usuario>();
            connection.CreateTable<Alumno>();
            connection.CreateTable<Profesor>();
            connection.CreateTable<Preceptor>();
            connection.CreateTable<Director>();
            connection.CreateTable<Grado>();
            connection.CreateTable<Curso>();
            connection.CreateTable<GradoCurso>();

            Debug.Log(esBaseNueva
                ? $"Base de datos nueva creada en: {dbPath}"
                : $"Tablas verificadas/actualizadas en: {dbPath}");

            // ---------- Sembrar roles ----------
            string[] rolesEsperados = { "Alumno", "Profesor", "Preceptor", "Director" };

            int agregados = 0;
            foreach (string nombreRol in rolesEsperados)
            {
                bool yaExiste = connection.Table<Rol>()
                                           .Where(r => r.Nombre == nombreRol)
                                           .Count() > 0;

                if (!yaExiste)
                {
                    connection.Insert(new Rol { Nombre = nombreRol });
                    agregados++;
                    Debug.Log($"  + rol agregado: {nombreRol}");
                }
            }

            var todos = connection.Table<Rol>().ToList();
            Debug.Log($"Listo. Se agregaron {agregados} roles nuevos. " +
                      $"La tabla 'rol' ahora tiene {todos.Count} filas en total.");
        }

        AssetDatabase.Refresh();
    }

    // ------------------------------------------------------------
    // Utilidad para desarrollo: borra la copia de runtime para
    // forzar que se vuelva a copiar la base maestra actualizada.
    // Ver nota importante más abajo sobre por qué hace falta esto.
    // ------------------------------------------------------------
    [MenuItem("Tools/Borrar Base de Datos Runtime (persistentDataPath)")]
    public static void BorrarBaseRuntime()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, DB_NAME);

        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
            Debug.Log($"Se borró la base runtime en: {dbPath}. " +
                      "La próxima vez que corras el juego se va a copiar de nuevo desde StreamingAssets.");
        }
        else
        {
            Debug.Log("No hay base runtime para borrar.");
        }
    }
}
