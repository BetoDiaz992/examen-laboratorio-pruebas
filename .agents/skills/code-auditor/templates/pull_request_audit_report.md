# Revisión de Pull Request: Auditoría de Calidad y Arquitectura - PetClinic

*   **Estado de la Auditoría:** [🔴 RECHAZADO / 🟢 APROBADO]
*   **Fecha de Evaluación:** [Fecha de corrida de CI]
*   **Autor de la Revisión:** Guardián de CI / Auditor de Código (Antigravity QA)
*   **Rama Destino:** `main` (Producción)

---

## 1. Veredicto y Resumen Ejecutivo

> [!NOTE]
> [Proporcionar un resumen de 2-3 párrafos detallando el estado general de la base de código, resaltando la pureza de la arquitectura y la cobertura de pruebas de negocio].

### Semáforo de Integración Continua (CI Checks)

| Check de Calidad | Estado | Puntuación | Requisito Mínimo |
| :--- | :---: | :---: | :---: |
| **Pureza Arquitectónica (DDD)** | [🟢 PASÓ / 🔴 RECHAZADO] | [100% / 0%] | 100% (Sin fugas SQL en Dominio) |
| **Cobertura de Pruebas Lógicas**| [🟢 PASÓ / 🔴 RECHAZADO] | [XX%] | Mínimo 85% de criterios |
| **Control de Cruce de Citas** | [🟢 PASÓ / 🔴 RECHAZADO] | [Sí / No] | Obligatorio |
| **Seguridad Web ([Authorize])** | [🟢 PASÓ / 🔴 RECHAZADO] | [100% / XX%] | 100% de controladores (menos Login)|
| **Uso del Servidor MCP** | [🟢 PASÓ / 🔴 RECHAZADO] | [Sí / No] | Obligatorio (Google Stitch) |

---

## 2. Checklist de Cumplimiento Técnico

### 2.1 Módulo: `PetClinic.Domain`
*   [ ] **Cero Dependencias:** El proyecto no contiene referencias a NuGet de Entity Framework ni SQL Server.
*   [ ] **Encapsulación DDD:** Las propiedades de las entidades no poseen setters públicos.
*   [ ] **Eventos de Dominio:** Los cambios críticos disparan eventos (`IDomainEvent`) inmutables.
*   [ ] **Excepciones de Negocio:** La validación de invariantes lanza excepciones de dominio semánticas.

### 2.2 Módulo: `PetClinic.Infrastructure`
*   [ ] **Desacoplamiento:** Referencia únicamente a `PetClinic.Domain`.
*   [ ] **Motor Relacional:** Mapeos configurados estrictamente para Microsoft SQL Server.
*   [ ] **Auditoría Shadow Properties:** EF Core intercepta `SaveChangesAsync` e inyecta `CreatedAt`/`CreatedBy`.

### 2.3 Módulo: `PetClinic.Web`
*   [ ] **Desacoplamiento Base Datos:** Los controladores no inyectan `DbContext` directamente.
*   [ ] **Seguridad de Acceso:** Todos los controladores poseen la etiqueta `[Authorize]` (menos Login).
*   [ ] **Layout con Cinta:** El Ribbon Menu se oculta usando Razor `@User.Identity.IsAuthenticated`.
*   [ ] **Stitch MCP UI:** Maquetación limpia recuperada dinámicamente desde el servidor MCP de Google Stitch.

---

## 3. Cobertura de Pruebas Unitarias (`PetClinic.Tests`)

*   **Escenarios de Aceptación Identificados (`spec-core.md`):** [Número]
*   **Pruebas Implementadas Reales:** [Número]
*   **Porcentaje de Cobertura Semántica:** [XX%]

### Escenarios Críticos Validados:
*   [ ] Autenticación de Administrador Único exitosa y fallo.
*   [ ] Creación exitosa de Citas Médicas.
*   [ ] **[CRÍTICO]** Detección y bloqueo de cruce de horarios para un mismo veterinario.

---

## 4. Retroalimentación Detallada y Tareas de Corrección (Si Aplica)

### Hallazgos Críticos (Bloqueantes de Producción):
1.  **[Ejemplo] Fuga de persistencia en Dominio:** Encontrada la directiva `using Microsoft.EntityFrameworkCore` en la entidad `Appointment`. Debe retirarse de inmediato.
2.  **[Ejemplo] Falta atributo [Authorize]:** El controlador `/OwnersController` no está protegido.

### Tareas Sugeridas para el Siguiente Commit:
*   [ ] Corrección de...
*   [ ] Agregar la prueba unitaria faltante para...
