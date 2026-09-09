using System;

public enum RolUsuario
{
    Todos,
    Directivo,
    Profesor,
    Estudiante
}

[Serializable]
public class UsuarioModel
{
    public string id;
    public string nombre;
    public string apellido;
    public string dni;
    public string email;
    public RolUsuario rol;
    public bool activo;

    public UsuarioModel(string id, string nombre, string apellido, string dni, string email, RolUsuario rol, bool activo)
    {
        this.id = id;
        this.nombre = nombre;
        this.apellido = apellido;
        this.dni = dni;
        this.email = email;
        this.rol = rol;
        this.activo = activo;
    }
}