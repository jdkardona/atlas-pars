# ADR 001: Motor de Evaluación de Políticas Declarativas


## Estado
Aceptado.


## Contexto
El sistema Atlas (PARS) debe evaluar solicitudes de autorización complejas (actor, recurso, acción, contexto) contra reglas de negocio. Existe un NFR crítico:latencia P95 inferior a 150 ms. Además, se cuenta con una restricción estricta de tiempo de desarrollo (15 horas).

Opciones evaluadas:
1. **Open Policy Agent (OPA):** Estándar de la industria mediante Rego. Requiere despliegue como contenedor *sidecar* y llamadas HTTP/gRPC a través de la red.
2. **Motor Nativo en C# (.NET 8):** Implementación de un evaluador en memoria dentro de la misma API, utilizando políticas estructuradas en JSON.

## Decisión
Se descarta OPA para este PoC y **se adopta un motor nativo en memoria desarrollado en C#** que evaluará políticas en formato JSON.


## Consecuencias
* **Positivas:** Al ejecutar la evaluación dentro del mismo proceso de la API, eliminamos el salto de red (*network hop*) y la latencia de serialización hacia un *sidecar*. Esto garantiza que la evaluación tome unos pocos milisegundos, priorizando el presupuesto de latencia para las operaciones más pesadas (firma criptográfica y persistencia). Además, reduce drásticamente la carga de configuración de infraestructura, alineándose con la restricción de 15 horas.
* **Negativas:** Renunciamos al ecosistema de herramientas de validación externas de OPA y a la expresividad del lenguaje Rego.