Uso Estratégico 1: Prevención de Fugas de Seguridad (Shift-Left Security)

Prompt / Tarea delegada: "Genera un archivo .gitignore estricto y blindado para un stack de .NET 8, Terraform y Docker. Asegúrate de bloquear explícitamente cualquier archivo de variables de entorno (.env) y los estados locales de infraestructura (.tfstate) para evitar fugas de credenciales en el primer push. debes guiarte en los ADR que te voy a suministrar."

Por qué delegué esto: Escribir un .gitignore desde cero es propenso a errores humanos, dado que ya habia redactado los ADR y se habia definido el stack tecnologico, he delegado esta tarea a la IA para mitigar riesgos de seguridad.



Uso Estratégico 2: Modelado de Arquitectura (C4 y Mermaid)

Prompt utilizado: >   *"Actúa como un Arquitecto de Software Senior. Basándote en los ADRs suministrados y el archivo README.md, genera el código Mermaid para diagramas C4 de Nivel 1 (Contexto) y Nivel 2 (Contenedores).  incluye tambien un diagrama de secuencias.

Restricciones:

No incluyas Nivel 3 (Componentes) para evitar sobreingeniería, se definira mas adelante.

Usa sintaxis estricta de Mermaid compatible con GitHub.

Asegúrate de incluir (Nombre, Tecnología, Descripción) en cada caja según el estándar C4"


Por qué delegué esto: Al delegar la sintaxis de Mermaid a la IA, aseguré que la documentación cumpliera con el estándar técnico C4 sin perder tiempo en ajustes de diseño gráfico manual, permitiéndome validar la consistencia lógica de las relaciones entre sistemas antes de escribir código.


Uso Estratégico 3: Despliegue local e IaC

Prompt utilizado:
*"Actúa como un Ingeniero DevOps Senior especializado en Azure. Necesito configurar la infraestructura base para un sistema multi-tenant.

Redacta un docker-compose.yml para el entorno local que incluya: PostgreSQL 16 (con persistencia de volumen), un servicio de API (mapeado a un Dockerfile en src/Atlas.Api) y un volumen para un 'Policy Store' basado en archivos JSON locales.

Crea un archivo main.tf (Terraform) para desplegar en Azure Container Apps y PostgreSQL Flexible Server.

Redacta un RUNBOOK.md básico que explique cómo levantar el entorno local, realizar troubleshooting de la base de datos.
Restricción: Mantén la estructura de directorios suministrada.