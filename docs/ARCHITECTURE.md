# Arquitectura del Sistema: Atlas PARS

Este documento describe la arquitectura de **Atlas PARS** siguiendo el Modelo C4. 

> **Nota Estratégica:** Se ha omitido el Nivel 3 (Componentes) para priorizar la agilidad.

---

## Nivel 1: Diagrama de Contexto
Este diagrama muestra a Atlas PARS en el centro. Su responsabilidad es actuar como una capa centralizada de autorización que abstrae la complejidad de seguridad para los diferentes *Squads*.

Atlas es el "guardia de seguridad" central. Los diferentes equipos (Squads) no necesitan programar su propia seguridad; simplemente le preguntan a Atlas si una operacion está permitida, y Atlas valida la identidad del usuario con el proveedor corporativo (externo) y les responde a los squad basándose en reglas centralizadas.

flowchart TD
    Cliente(("🧑‍💻 Usuario Final<br>[Persona]<br>Cliente que intenta realizar una operacion crítica."))
    
    SquadApp["Aplicación del Squad<br>[Software System]<br>Plataformas de negocio (Pagos, transferencias).Requieren saber si el usuario tiene autorizacion para esa operacion."]
    
    AtlasPARS["🛡️ <br>ATLAS PARS</br>[Software System] Motor centralizado que evalúa las reglas de negocio y firma decisiones."]
    
    IdP["🔑 Identity Provider [Sistema Externo] Auth0, Gestiona las contraseñas e identidades."]

    Cliente -- "Inicia operacion'<br>[Navegador / App Móvil]" --> SquadApp
    SquadApp -- "Solicita Autorización<br>(Actor, Recurso, Acción, Contexto)<br>[REST / JSON]" --> AtlasPARS
    AtlasPARS -- "Verifica Autenticacion, Delega la validación del token JWT<br>[JWKS]" --> IdP
    
    style Cliente fill:#08427b,stroke:#052e56,color:#ffffff
    style SquadApp fill:#2e6295,stroke:#1e466b,color:#ffffff
    style AtlasPARS fill:#1168bd,stroke:#0b4884,color:#ffffff,stroke-width:3px
    style IdP fill:#999999,stroke:#6b6b6b,color:#ffffff


## Nivel 2: Diagrama de Contenedores

Areas funcionales: el API que procesa la lógica en tiempo real, el Policy Store que guarda las reglas como archivos de configuración (Policy-as-Code) y la Base de Datos de Auditoria que mantiene un registro histórico inmutable de cada decisión tomada.

flowchart TD
    SquadApp(("Aplicación del Squad<br>[Software System]"))

    subgraph Atlas_PARS ["Atlas PARS"]
        direction TB
        API("Atlas Evaluation API<br>[Container: .NET 8 Minimal API]<br>Recibe peticiones, inyecta TenantId<br>y ejecuta el motor nativo de políticas.")
        DB[("Audit Database<br>[Container: PostgreSQL 16]<br>Almacena logs inmutables particionados<br>por TenantId usando campos JSONB.")]
        Policies{{"Policy Store / File System<br>[Container: Local Files / Blob]<br>Archivos JSON con las reglas<br>de negocio por Squad."}}
    end

    SquadApp -- "POST /authorize<br>[HTTPS]" --> API
    API -- "Lee reglas de evaluación<br>[File I/O]" --> Policies
    API -- "Inserta log de decisión<br>con firma ECDSA<br>[TCP/IP / SQL]" --> DB

    style API fill:#438dd5,stroke:#2e6295,color:#ffffff
    style DB fill:#438dd5,stroke:#2e6295,color:#ffffff
    style Policies fill:#438dd5,stroke:#2e6295,color:#ffffff
    style SquadApp fill:#08427b,stroke:#052e56,color:#ffffff


## Flujo de Autorizacion: Diagrama de Secuencia

Cuando Atlas recibe la petición, valida al usuario, consulta las reglas, firma la decisión con un código de seguridad (firma criptográfica), guarda el resultado en el historial y finalmente le dice al Squad si puede continuar, si se deniega, o si el usuario debe pasar por un paso extra de seguridad (Challenge).

sequenceDiagram
    autonumber
    actor U as Usuario Final
    participant SQ as App del Squad
    participant API as Atlas API (.NET)
    participant IDP as Identity Provider
    participant DB as PostgreSQL (JSONB)

    U->>SQ: Intenta acción crítica
    Note over SQ: Construye el Contexto<br/>(Actor, Recurso, Acción)
    SQ->>API: POST /authorize (Contexto + JWT)
    
    rect rgb(230, 240, 255)
        Note right of API: Inicio del Hot Path (< 150ms)
        API->>IDP: Obtiene JWKS (Llave Pública)
        IDP-->>API: Retorna llaves
        API->>API: 1. Valida firma del JWT
        API->>API: 2. Motor evalúa Políticas en Memoria
        API->>API: 3. Genera Firma Criptográfica (ECDSA)
        API->>DB: 4. Guarda Audit Log (Append-Only)
    end
    
    DB-->>API: Confirmación de persistencia
    API-->>SQ: Respuesta (PERMIT / DENY / CHALLENGE) + Firma
    
    alt Es PERMIT
        SQ-->>U: Acción completada con éxito
    else Es DENY
        SQ-->>U: Acceso denegado (HTTP 403)
    else Es CHALLENGE
        SQ-->>U: Solicita factor adicional (MFA / Biometría)
    end