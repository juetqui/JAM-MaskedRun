using UnityEngine;
using System.Collections;

public class ParabolicMotionWithDelay : MonoBehaviour
{
    public float launchAngle = 45f;   // Ángulo de lanzamiento en grados
    public float initialSpeed = 10f; // Velocidad inicial
    public float maxDistance = 20f;  // Distancia máxima antes de regenerarse

    private Vector3 startPoint;      // Punto de inicio del objeto
    private float timeElapsed = 0f; // Tiempo transcurrido desde el lanzamiento
    private bool isWaitingToReset = false; // Flag para evitar múltiples reseteos

    void Start()
    {
        // Guardar el punto de inicio
        startPoint = transform.position;
    }

    void Update()
    {
        if (isWaitingToReset) return; // No hacer nada mientras espera la regeneración

        // Calcular la posición parabólica en función del tiempo
        float angleInRadians = launchAngle * Mathf.Deg2Rad;
        float g = Mathf.Abs(Physics.gravity.y); // Usar gravedad positiva

        // Posición en el eje X y Z
        float x = initialSpeed * Mathf.Cos(angleInRadians) * timeElapsed;
        float z = 0f; // Ajusta este valor si deseas movimiento en el plano XZ

        // Posición en el eje Y
        float y = (initialSpeed * Mathf.Sin(angleInRadians) * timeElapsed) - (0.5f * g * timeElapsed * timeElapsed);

        // Actualizar la posición del objeto
        transform.position = startPoint + new Vector3(x, y, z);

        // Incrementar el tiempo transcurrido
        timeElapsed += Time.deltaTime;

        // Calcular la distancia recorrida en el plano horizontal
        float distanceTraveled = Mathf.Abs(x);

        // Regenerar el objeto si supera la distancia máxima o cae por debajo del punto de inicio
        if (distanceTraveled >= maxDistance || transform.position.y < startPoint.y)
        {
            StartCoroutine(ResetWithDelay());
        }
    }

    private IEnumerator ResetWithDelay()
    {
        isWaitingToReset = true;

        // Generar un tiempo de espera aleatorio entre 0 y 2 segundos
        float waitTime = Random.Range(0f, 2f);
        yield return new WaitForSeconds(waitTime);

        // Reiniciar la posición y el tiempo
        transform.position = startPoint;
        timeElapsed = 0f;

        isWaitingToReset = false;
    }
}
