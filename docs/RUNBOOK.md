# RUNBOOK: Operación de Atlas PARS

Este documento detalla los procedimientos operativos estándar para el entorno de desarrollo y despliegue de Atlas PARS.

## 1. Arranque del Entorno Local

Para levantar el ecosistema completo (Base de datos PostgreSQL, volúmenes lógicos y la API), sigue este orden estricto:

**A. Levantar Infraestructura (Docker):**

Desde la raíz del proyecto, inicializa los contenedores:

```bash
cd deploy
docker compose up -d
```

B**. Iniciar el Motor (API):**

Desde la raíz del proyecto y arranca el servidor .NET:

```bash
cd ..
dotnet run --project src/Atlas.Api/Atlas.Api.csproj
```
El servicio quedará escuchando en http://localhost:5000 o el puerto configurado.


## 2. Aseguramiento de Calidad (QA) y Pruebas
El sistema cuenta con una suite de pruebas unitarias (xUnit) diseñadas para garantizar la inmutabilidad de las reglas de negocio (PolicyEvaluator) y el estándar criptográfico (CryptoSigner).

Ejecución Rápida:

```bash
dotnet test
```

Ejecución Detallada (Para depuración):

```bash
dotnet test --logger "console;verbosity=detailed"
```


## 3. Integración Continua (CI/CD)

Atlas PARS utiliza GitHub Actions para su pipeline de CI.

Ubicación del Pipeline: .github/workflows/ci.yml

Gatillo (Trigger): Se ejecuta automáticamente en cada push o pull_request a la rama main.

Proceso Automático:

Instala .NET 8 en un entorno Ubuntu.

Compila el código (build).

Ejecuta la suite de pruebas.

Monitoreo: Si una prueba falla en la nube, el pipeline se romperá (estado rojo) y se debe revisar la pestaña "Actions" en el repositorio de GitHub para auditar los logs del error.



## 4. Verificación de Salud (Health Checks)

Para asegurar que los servicios base están listos:

Base de datos: Ejecuta docker ps (debe mostrar el contenedor de Postgres en estado "Up" y el puerto 5432 expuesto).

Logs de BD: docker compose logs -f atlas-db



## 5. Troubleshooting (Resolución de Problemas)
Escenario A: La Base de Datos no conecta (Error 500 en la API)

Causa común: El puerto 5432 ya está siendo utilizado por otra instancia local de Postgres en la máquina host.

Solución: Detén el servicio local de Postgres o cambia el mapeo de puertos en el docker-compose.yml.

Escenario B: Corrupción de datos o fallos de migración (JSONB)

Solución: Destruir el volumen de datos persistente e iniciar en limpio. (ADVERTENCIA: Destruye los logs de auditoría locales):

```bash
cd deploy
docker compose down -v
docker compose up -d
```

Escenario C: El Evaluador devuelve siempre DENY (Fail-Safe)

Causa común: El archivo JSON de políticas para el tenant especificado no existe en la carpeta deploy/policies/, o el nombre del archivo no coincide con el Header HTTP X-Tenant-Id.