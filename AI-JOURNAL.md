 **Prevención de Fugas de Seguridad (Shift-Left Security)**

Prompt: "Genera un archivo .gitignore estricto y blindado para un stack de .NET 8, Terraform y Docker. Asegúrate de bloquear explícitamente cualquier archivo de variables de entorno (.env) y los estados locales de infraestructura (.tfstate) para evitar fugas de credenciales en el primer push. debes guiarte en los ADR que te voy a suministrar."

-- Escribir un .gitignore desde cero es propenso a errores humanos, dado que ya habia redactado los ADR y se habia definido el stack tecnologico, he delegado esta tarea a la IA para mitigar riesgos de seguridad.



 **Modelado de Arquitectura (C4 y Mermaid)**

Prompt: "Actúa como un Arquitecto de Software Senior. Basándote en los ADRs suministrados y el archivo README.md, genera el código Mermaid para diagramas C4 de Nivel 1 (Contexto) y Nivel 2 (Contenedores).  incluye tambien un diagrama de secuencias.

Restricciones:

No incluyas Nivel 3 (Componentes) para evitar sobreingeniería, se definira mas adelante.

Usa sintaxis estricta de Mermaid compatible con GitHub.

Asegúrate de incluir (Nombre, Tecnología, Descripción) en cada caja según el estándar C4"

-- Al delegar la sintaxis de Mermaid a la IA, aseguré que la documentación cumpliera con el estándar técnico C4 sin perder tiempo en ajustes de diseño gráfico manual, permitiéndome validar la consistencia lógica de las relaciones entre sistemas antes de escribir código.


**Despliegue local e IaC**

Prompt utilizado:
"Actúa como un Ingeniero DevOps Senior especializado en Azure. Necesito configurar la infraestructura base para un sistema multi-tenant.

Redacta un docker-compose.yml para el entorno local que incluya: PostgreSQL 16 (con persistencia de volumen), un servicio de API (mapeado a un Dockerfile en src/Atlas.Api) y un volumen para un 'Policy Store' basado en archivos JSON locales.

Crea un archivo main.tf (Terraform) para desplegar en Azure Container Apps y PostgreSQL Flexible Server.

Redacta un RUNBOOK.md básico que explique cómo levantar el entorno local, realizar troubleshooting de la base de datos.
Restricción: Mantén la estructura de directorios suministrada."



**Conytratos y dockerfile**

Promt: "Eres un desarrollador backend Senior en .NET 8. Estamos construyendo Atlas PARS, un motor centralizado de autorizaciónes. el proyecto se llama Atlas.Api.csproj, y se encuentra en la carpeta /src/Atlas.Api

Necesitoq ue me ayudes a generar los modelos de dominio (Contratos de API) para la evaluación de politicas.

Deben ser record de C# (inmutables).

AuthorizationRequest: Debe recibir el token JWT (string), la acción solicitada (string), el recurso (string) y el contexto (clave-valor).

AuthorizationResponse: Debe devolver la decisión (PERMIT, DENY, CHALLENGE) y la firma criptográfica (string).

tambien necesito que me ayudes a generar un Dockerfile optimizado para producción (multi-stage) para una Minimal API en .NET 8."



**Prompt de Especificación (Interfaces y Minimal API):**
"Actúa como un desarrollador Senior en .NET 8. ya tenemos definidos los records AuthorizationRequest y AuthorizationResponse.

Tarea 1: Define dos interfaces limpias en C#.

IPolicyEvaluator: Debe tener un método que reciba el AuthorizationRequest y devuelva un Decision.

ICryptoSigner: Debe tener un método que reciba el JSON del Request y la Decision, y devuelva un string con la firma ECDSA (ES256).

Tarea 2: Crea el orquestador con Minimal APIs. Configura la inyección de dependencias para las interfaces del paso anterior vamos a simular por ahora las clases PolicyEvaluator y CryptoSigner, finalmente  crea el endpoint POST /authorize que recibe la petición, llama al evaluador, luego al firmador, y devuelve la respuesta."


**PolicyEvaluator y CryptoSigner**
Prompt : Actúa como un desarrollador Senior en .NET 8. Debes reemplazar la clase DummyPolicyEvaluator creando una implementación real llamada LocalPolicyEvaluator que implemente IPolicyEvaluator

Reemplaza la clase DummyCryptoSigner implementando ICryptoSigner con una nueva clase CryptoSigner con ECDSA 

Asegúrate de manejar posibles errores



**Persitencia**
Prompt :Actúa como un Desarrollador Backend Senior. Necesito implementar la capa de persistencia para Atlas PARS.

Crea un script SQL para una tabla authorization_logs en PostgreSQL que incluya un ID UUID, un TenantId indexado, los datos de la decisión, la firma criptográfica y una columna RequestContext de tipo JSONB.

Crea una interfaz IAuditRepo y su implementación PgAuditRepo usando Dapper y Npgsql para C#. El repositorio solo debe tener un método de inserción.

Actualiza el Program.cs para extraer el TenantId desde un header X-Tenant-Id, pasarlo al PolicyEvaluator (para que lea el JSON correcto ej. 'squad-a.json') y guardar la transacción en la base de datos.



**Automatización de Pruebas (xUnit) PolicyEvaluator**

Prompt utilizado: Actúa como un SDET (Software Development Engineer in Test) Senior en .NET. Configura un proyecto de pruebas unitarias usando xUnit. Crea una suite de pruebas para PolicyEvaluator.
Especificaciones:

Crea pruebas para los escenarios críticos: Transferencia válida (PERMIT), transferencia que excede el límite (DENY), regla que requiere autenticación fuerte (CHALLENGE), y escenario Fail-Safe (archivo de tenant inexistente devuelve DENY).

este es un ejmplo de una policy de squad-a:

{
  "tenantId": "squad-a",
  "rules": [
    {
      "action": "transfer",
      "minAmount": 0,
      "maxAmount": 499999,
      "decision": "PERMIT"
    },
    {
      "action": "transfer",
      "minAmount": 500000,
      "maxAmount": 1000000,
      "decision": "CHALLENGE"
    },
    {
      "action": "transfer",
      "minAmount": 1000001,
      "maxAmount": 99999999999,
      "decision": "DENY"
    }
  ]
}




**Pruebas Unitarias de CryptoSigner**

Prompt utilizado: "Actúa como un Ingeniero de Seguridad AppSec. Escribe una suite de pruebas xUnit para la clase CryptoSigner, te dejo a libertar decicir los elementos de prueba."



**Pipeline CI/CD**

Prompt utilizado: Actúa como un Ingeniero DevOps . Diseña un pipeline de Integración Continua (CI) en un archivo YAML para GitHub Actions. El pipeline debe: 1. Activarse en cada push o pull request hacia la rama 'main'. 2. Levantar un entorno Ubuntu, configurar .NET 8 y restaurar la solución (Atlas.sln). 3.  Ejecutar la suite completa de pruebas unitarias de PolicyEvaluator y  CryptoSigner antes de permitir cualquier despliegue.


**Integración de DevSecOps**

Prompt : Actúa como un experto en DevSecOps. Extiende mi pipeline de GitHub Actions para incluir escaneos de seguridad automatizados. Necesito SAST (análisis estático), SCA (dependencias) y escaneo de vulnerabilidades en la imagen Docker. Usa herramientas como (CodeQL y Trivy) y asegúrate de que el pipeline falle si se detectan vulnerabilidades críticas. Este trabajo dependera primero de que se ejecuten las pruebas unitarias.