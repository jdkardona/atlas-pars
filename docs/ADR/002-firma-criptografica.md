# ADR 002: Mecanismo de Firma Criptográfica para Trazabilidad

## Estado
Aceptado.

## Contexto
La especificación exige que toda decisión estricta (`PERMIT`, `DENY`, `CHALLENGE`) sea firmada criptográficamente para auditoría. Esta operación entra en la "ruta crítica" (*hot path*) de cada petición, por lo que debe ser computacionalmente eficiente para no violar el NFR de $<150ms$. Además, debe permitir que los diferentes *squads* (multi-tenant) verifiquen las firmas de forma autónoma.

Opciones evaluadas:
1. **RSA (RS256):** Ampliamente soportado, pero la generación de firmas es lenta y consume alta CPU.
2. **HMAC (HS256):** Ultrarrápido, pero al ser simétrico obliga a compartir la llave privada con los *squads* para la verificación, lo cual es un riesgo de seguridad.
3. **ECDSA (ES256):** Generación de firmas asimétricas de tamaño reducido y ejecución ultrarrápida.

## Decisión
**Utilizaremos JWS (JSON Web Signature) con el algoritmo ECDSA**

## Consecuencias
* **Positivas:** ECDSA es más rápido que RSA para la generación de firmas, lo que asegura el cumplimiento de la latencia requerida. Al ser criptografía asimétrica, Atlas conservará la llave privada y podrá distribuir libremente la llave pública a los auditores y *squads* para que validen la trazabilidad de sus decisiones de forma descentralizada.
* **Negativas:** La manipulación de llaves de curva elíptica en .NET (formatos PEM o JWK) tiene una curva de implementación más compleja que los algoritmos tradicionales.