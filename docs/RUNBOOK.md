# RUNBOOK: Operación de Atlas PARS

Este documento detalla los procedimientos operativos estándar para el entorno de desarrollo y despliegue de Atlas PARS.

## 1. Arranque del Entorno Local
Para levantar todo el ecosistema (Base de datos y mocks), ejecuta desde la raíz:

```bash
cd deploy
docker compose up -d
```

## Verificación de Salud
Para asegurar que los servicios están listos:

Base de datos: docker ps (debe mostrar el estado "Up (healthy)").

Logs: docker compose logs -f api (si la API ya está desplegada).

## 2. Troubleshooting
Escenario: La Base de Datos no conecta
Verifica que el puerto 5432 no esté siendo utilizado por otra instancia local de Postgres.

Si hubo un error de corrupción de datos en el volumen, reiniciar con:

```bash
docker compose down -v
docker compose up -d
```