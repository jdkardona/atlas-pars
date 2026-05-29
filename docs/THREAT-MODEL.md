# Modelo de Amenazas: Atlas PARS (Metodología STRIDE)

Este documento detalla el análisis de riesgos proactivo para el motor de autorización Atlas PARS, utilizando la metodología STRIDE para identificar vulnerabilidades arquitectónicas y documentar sus mitigaciones. Se incluye además una sección de riesgos aceptados por limitaciones de alcance (PoC).

## 1. Resumen Ejecutivo
Atlas PARS opera como un servicio centralizado de validación. Dado que emite contratos criptográficos inmutables (JWS con ECDSA) y audita operaciones críticas, sus principales vectores de ataque se centran en la falsificación de identidad entre *tenants* y la manipulación del motor de reglas.

## 2. Análisis STRIDE Mitigado

### Spoofing (Suplantación de Identidad)
* **Amenaza:** Un *Squad* (Tenant A) malintencionado o comprometido intenta enviar solicitudes de autorización haciéndose pasar por el Tenant B, modificando el header `X-Tenant-Id`.
* **Mitigación Integrada:** El motor de políticas aísla físicamente los archivos de evaluación (ej. `squad-a.json`). En producción (vía API Gateway/Ingress), la identidad del tenant se valida contra un token JWT firmado por el Identity Provider corporativo, asegurando que el claim del *Squad* coincida matemáticamente con el tenant solicitado antes de que la petición toque el `PolicyEvaluator`.

### Tampering (Manipulación de Datos)
* **Amenaza:** Un atacante interno logra acceso al disco del contenedor y modifica los umbrales de seguridad en el archivo `squad-a.json` para autorizar transferencias fraudulentas.
* **Mitigación Integrada:** 1. **Inmutabilidad de salida:** Cada decisión se firma con ECDSA (RFC 7515). Si la regla fue manipulada, el token resultante dejará rastro pericial innegable (Ver ADR 002).
  2. **GitOps:** Las políticas se despliegan como de solo lectura mediante volúmenes en Docker/Kubernetes. El contenedor no tiene permisos de escritura sobre su propia carpeta `/policies`.

### Repudiation (Repudio)
* **Amenaza:** Un usuario o un sistema externo realiza una transferencia crítica y posteriormente niega haber recibido la autorización de Atlas PARS.
* **Mitigación Integrada:** Repositorio de auditoría *Append-Only* en PostgreSQL. Cada solicitud y su respectiva decisión (PERMIT, CHALLENGE, DENY) se guardan en una columna `JSONB` junto con la firma criptográfica exacta. La interfaz de base de datos (`IAuditRepo`) carece por diseño de métodos de borrado o actualización (`DELETE`/`UPDATE`).

### Information Disclosure (Divulgación de Información)
* **Amenaza:** Los logs de auditoría en PostgreSQL filtran Información Personal Identificable (PII) o tokens de sesión activos a personal de TI no autorizado.
* **Mitigación Integrada:** El contrato de la API (`AuthorizationRequest`) utiliza un diccionario de `Context` genérico. Por política de gobierno documentada, los Squads solo deben enviar variables matemáticas o roles (ej. `amount: 500000`), delegando la ofuscación de PII a sus propios microservicios antes de llamar a Atlas.

### Denial of Service (Denegación de Servicio)
* **Amenaza:** Un pico de tráfico malicioso inunda el endpoint `POST /authorize` agotando los recursos de CPU durante el cálculo criptográfico.
* **Mitigación Integrada:** La arquitectura del `CryptoSigner` utiliza ECDSA, que es computacionalmente más ligero en la generación de firmas que RSA. Al ser un servicio *stateless*, la API está diseñada para escalar horizontalmente detrás de un Azure Container Apps.

### Elevation of Privilege (Elevación de Privilegios)
* **Amenaza:** Un atacante utiliza inyección SQL en el campo `resource` o `action` para ejecutar comandos arbitrarios en la base de datos PostgreSQL.
* **Mitigación Integrada:** Uso estricto del micro-ORM **Dapper** con consultas parametrizadas, neutralizando cualquier intento de inyección SQL. El rol de conexión (`atlas_api_user`) tiene permisos revocados explícitamente para todo excepto la instrucción `INSERT`.

---

## 3. Riesgos Aceptados y Deuda Técnica (Scope Limitations)

Debido a la restricción de tiempo de ejecución (15 horas) y el enfoque de priorización de la Prueba de Concepto (PoC), los siguientes riesgos se asumen como aceptados en la versión actual, con estrategias de mitigación definidas para la siguiente iteración (Fase 2):

### Replay Attacks (Reutilización de Tokens Validables)
* **Riesgo:** Los tokens JWS emitidos carecen de tiempo de expiración corto (`exp`) y de un identificador único de un solo uso (`jti`). Un atacante podría interceptar un token `PERMIT` válido y reenviarlo múltiples veces al recurso final.
* **Plan de Mitigación (Fase 2):** Incorporar claims `exp` y `jti` en el Payload del `CryptoSigner`. Requerir a los *Squads* receptores la implementación de una caché distribuida (ej. Redis) para registrar y rechazar los `jti` previamente procesados.

