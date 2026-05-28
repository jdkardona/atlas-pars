# ADR 003: Persistencia de Auditoría y Modelo Multi-Tenant

## Estado
Aceptado.

## Contexto
Las decisiones firmadas deben archivarse para auditoría. La arquitectura dictamina un modelo centralizado de infraestructura pero particionado lógicamente por *squad* (`TenantId`).

Opciones evaluadas:
1. **EventStoreDB:** Nativo para inmutabilidad, pero añade alta complejidad de infraestructura.
3. **PostgreSQL (JSONB):** Base de datos relacional robusta con soporte nativo e indexado para documentos JSON.

## Decisión
**Utilizaremos PostgreSQL empleando el tipo de dato `JSONB` ,inmutabilidad aplicando un diseño "Append-Only" (solo inserciones) y un particionamiento lógico obligatorio mediante una columna indexada `TenantId`.**


## Consecuencias
* **Positivas:** Garantizamos el requerimiento Multi-Tenant lógico: un solo clúster de base de datos reduce los costos operativos y simplifica el despliegue local con `docker-compose`. El índice por `TenantId` permite búsquedas ultrarrápidas, y el uso de `JSONB` otorga la flexibilidad documental requerida por los *squads* .
* **Negativas:** La inmutabilidad no se delega al motor de almacenamiento (como en EventStore), sino que se debe garantizar mediante reglas de base de datos (revocando permisos de `UPDATE`/`DELETE` a nivel de RDBMS).