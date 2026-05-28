# ADR 004: Estrategia de Orquestación e Infraestructura como Código (IaC)

## Estado
Aceptado.

## Contexto
Se requiere entregar Infraestructura como Código (IaC) simulando un despliegue Cloud real que soporte el diseño multi-tenant. Simultáneamente, el proyecto debe ejecutarse localmente mediante un solo comando.

Opciones evaluadas para la nube:
1. **Azure Kubernetes Service (AKS) / EKS:** Orquestación completa, ideal para microservicios complejos, pero genera sobrecarga de gestión, no encaja en la ventana de 15 horas de desarrollo.
2. **Serverless Containers (Azure Container Apps / AWS ECS Fargate):** Contenedores gestionados sin aprovisionamiento de nodos, con soporte multi-tenant nativo.

## Decisión
**Utilizaremos `docker-compose` como único orquestador para el entorno de desarrollo local, y Terraform diseñando una arquitectura orientada a Azure Container Apps para la topología Cloud.**


## Consecuencias
* **Positivas:** Cubre el requisito de "despliegue local con 1 comando" ($docker-compose up$). En la nube, Azure Container Apps permite aislar de manera natural a los *squads* si fuese necesario, con un modelo de escalado a cero (scale-to-zero) que optimiza costos. 
* **Negativas:** Existirá una falta de paridad exacta entre el entorno local (Docker Compose) y el de producción (Container Apps), lo cual es un riesgo aceptable dado el tamaño reducido del PoC.