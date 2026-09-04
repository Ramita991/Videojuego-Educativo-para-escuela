# 🎮 QuestLearn

Videojuego educativo de preguntas y respuestas (Quiz) desarrollado en Unity con backend en Firebase, orientado a instituciones educativas.

---

## 📋 Descripción

QuestLearn es una plataforma de gamificación educativa que permite a estudiantes y profesores participar en quizzes interactivos. Los directivos pueden gestionar usuarios, roles y contenido desde la misma aplicación.

---

## 🛠️ Stack tecnológico

| Tecnología | Uso |
|---|---|
| **Unity 6.5** | Motor del videojuego (PC y Android) |
| **C#** | Lenguaje de programación |
| **Firebase Authentication** | Login y gestión de sesiones |
| **Firebase Firestore** | Base de datos en la nube |
| **TextMeshPro** | UI y textos en Unity |
| **Git + GitHub** | Control de versiones |

---

## 👥 Roles del sistema

| Rol | Permisos |
|---|---|
| **Directivo** | Crear/editar usuarios, ver reportes, configurar el sistema |
| **Profesor** | Crear preguntas, ver resultados de sus alumnos |
| **Estudiante** | Jugar quizzes, ver su puntaje y progreso |

---

## 🗄️ Estructura de la base de datos (Firestore)

La base de datos vive en **Firebase Firestore** (nube). No es un archivo local. A continuación se detalla la estructura de colecciones:

### 📁 `usuarios`
```
usuarios/
  {uid}/
    ├── nombre        (string)     → Ej: "Juan"
    ├── apellido      (string)     → Ej: "Pérez"
    ├── dni           (string)     → Ej: "44556677"
    ├── email         (string)     → Ej: "juan@colegio.edu.ar"
    ├── rol           (string)     → "Estudiante" | "Profesor" | "Directivo"
    ├── uid           (string)     → ID generado por Firebase Auth
    └── creadoEn      (timestamp)  → Fecha de creación
```

### 📁 `preguntas` *(próximamente)*
```
preguntas/
  {id}/
    ├── enunciado     (string)
    ├── opciones      (array)
    ├── respuesta     (string)
    ├── categoria     (string)
    ├── dificultad    (string)   → "facil" | "medio" | "dificil"
    └── creadoPor     (string)   → uid del profesor
```

### 📁 `puntajes` *(próximamente)*
```
puntajes/
  {id}/
    ├── usuarioId     (string)
    ├── quizId        (string)
    ├── puntaje       (number)
    └── fecha         (timestamp)
```

---

## 🚀 Historias de usuario

| ID | Historia | Estado |
|---|---|---|
| HU-001 | Crear usuario | ✅ En desarrollo |
| HU-002 | Login | 🔲 Pendiente |
| HU-003 | Jugar quiz | 🔲 Pendiente |
| HU-004 | Ver puntajes | 🔲 Pendiente |

---

## ⚙️ Configuración del proyecto

### Requisitos previos
- Unity 6.5 (6000.5.0f1) o superior
- Cuenta de Firebase con proyecto creado
- Git instalado

### Pasos para correr el proyecto

1. Cloná el repositorio:
```bash
git clone https://github.com/tu-equipo/questlearn.git
```

2. Abrí el proyecto en Unity Hub

3. Configurá Firebase:
   - Creá tu propio proyecto en [Firebase Console](https://console.firebase.google.com)
   - Descargá el archivo `google-services.json`
   - Colocalo en `Assets/`

> ⚠️ **Importante:** El archivo `google-services.json` **no se sube al repositorio** (está en `.gitignore`). Cada integrante del equipo debe obtener el suyo desde la consola de Firebase.

4. Habilitá en Firebase:
   - Authentication → Correo y contraseña
   - Firestore Database → Modo prueba

---

## 🔐 Variables de entorno / Archivos ignorados

Los siguientes archivos **no se suben a GitHub** por seguridad:

```
google-services.json
GoogleService-Info.plist
Assets/StreamingAssets/*.db
```

---

## 📄 Licencia

Proyecto educativo — Prácticas profesionales.
