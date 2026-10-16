using UnityEngine;

public class RotateUI : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [SerializeField] private float rotationSpeed = -200f; // Velocidad. El valor negativo hace que gire en sentido horario.

    void Update()
    {
        // Hace que el objeto rote continuamente en el eje Z de forma fluida y pareja
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}