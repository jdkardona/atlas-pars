# Atlas - Política de Acceso a Recursos Sensibles (PARS)


## 1. Contexto 
Múltiples squads estan interactuando con operaciones críticas. Permitir que cada squad diseñe e implemente su propia lógica de autorización duplica los esfuerzos, genera riesgos de seguridad y reduce la mantenibilidad.
Para resolver esto es necesario crear un servicio "centralizado" de "Política de Acceso a Recursos Sensibles" (PARS). 
Importante: trazabilidad ,latencia P95 y que cada squad pueda desplegar su propia instancia (multi-tenant) 

Cuando alguien intenta realizar una acción crítica, debe solicitar autorizacion el sistema Atlas:
1. recibe la solicitud, parametros (actor,recurso, accion, contexto)
1. Revisa el manual de políticas de acceso.
2. Toma una decisión estricta (PERMIT, DENY, o CHALLENGE).
3. Firma criptografica de cada decision.
4. Archiva una copia de las deciciones para auditoria.



## 2. Clarify

### Requerimientos Funcionales
* **Input:** Recibir solicitudes estructuradas evaluando actor, recurso, acción y contexto.
* **Proceso:** Evaluación de políticas declarativas versionadas.
* Toda decisión debe tener firma criptográfica y persistencia para auditoría.

### Requerimientos No Funcionales (NFRs)
* **Rendimiento:** Latencia de percentil 95 (P95) inferior a 150 ms bajo alta concurrencia.
* **Topología:** PARS Centralizado pero multi-tenant para despliegue autónomo.
Atlas provee el motor de evaluación y firma criptográfica como un servicio centralizado, Todos los squad deben enviar sus peticiones en el mismo formato estructurado, la base de datos de auditoria es unica, pero particionada con un tenatID.  Esto permite que el despliegue autónomo de los squads no sea de infraestructura, sino de Políticas (archivos JSON).


### Restricciones Técnicas y de Negocio
* **Tiempo:** Ejecución en 15 horas procurando simplicidad, priorizar alcance.
* **Stack Obligatorio:** C# / .NET 8 para el núcleo.
* **Operatividad:** Infraestructura como Código (IaC) obligatoria. Terraform.
* **Despliegue** local con un solo comando (docker)



## 3. Alcance y Exclusiones Estratégicas

* **SÍ INCLUYE:** 
-API REST (.NET 8)
-Motor nativo  para evaluar politicas con mayor velocidad. (ver ADR 01)
-firma ECDSA
-IaC (Docker Compose local / Azure Container Apps Cloud)
-pipeline CI/CD con SAST/SCA.

* **FUERA DE ALCANCE:** 
-Interfaz grafica (Frontend)
-Login (autenticación de usuario asumida), Atlas se enfocará exclusivamente en decidir qué puede hacer ese usuario (autorización).
-clústeres locales de Kubernetes (se utilizara docker-compose).



## 4. Plan de Ejecución: Modularización y Priorización


1. **Fase 1: Arquitectura**
   * Modelado C4  y redacción de ADRs (Motor de políticas, firma criptográfica, base de datos, despliegue). 

2. **Fase 2: Infraestructura**
   * Desarrollo de IaC (Terraform), PostgresSQL, preparamos el entorno local con docker-compose

3. **Fase 3: Núcleo .NET 8**
   * Minimal API, recepcion de solicitudes, implementación del motor de evaluación, Firma y persistencia de datos. Pruebas unitarias.

4. **Fase 4: Seguridad Integrada y CI/CD**
   * Pipeline declarativo con SAST, escaneo de dependencias, Threat Model (STRIDE) y Runbook de operacion del sistema en produccion y contingencias .