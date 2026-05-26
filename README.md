# Proyecto PetClinic
# Especificación de Requisitos de Software (ERS)
## Proyecto: PetClinic Management System
### Única Fuente de la Verdad (SSoT) - Versión 1.0.0

---

## 1. Introducción y Modelo de Dominio

Este documento define la especificación técnica, las reglas de negocio y los criterios de aceptación bajo la metodología de Desarrollo Guiado por Especificación (SDD) y Desarrollo Guiado por Comportamiento (BDD) para el sistema **PetClinic Management System**. Su propósito es servir como el contrato lógico de comportamiento y diseño de la aplicación.

### 1.1 Metadatos y Actor Único de Acceso
El sistema está diseñado bajo una premisa estricta de entorno cerrado y controlado para la gestión interna de la clínica veterinaria.

*   **Actor Único:** **Administrador**.
*   **Restricciones de Acceso:** El sistema **NO DEBE** permitir el registro público de ningún tipo de usuario. El sistema **NO DEBE** proveer interfaces ni credenciales de acceso directo a veterinarios, propietarios de mascotas, pacientes ni terceros. Toda la información de estas entidades se gestiona exclusivamente a través de la interfaz del Administrador.

---

## 2. Codificación Jerárquica de Requisitos

Para asegurar la trazabilidad lógica de la implementación, se establece la siguiente nomenclatura jerárquica de requisitos:

| Código | Módulo | Categoría |
| :--- | :--- | :--- |
| **REQ-SEG-XX** | Módulo 1: Seguridad y Control de Acceso | Seguridad, Autenticación y Auditoría |
| **REQ-NAV-XX** | Módulo 2: Gestión de Citas e Interfaz | Navegación e Interfaz Principal (UI) |
| **REQ-CIT-XX** | Módulo 2: Gestión de Citas e Interfaz | Citas, Agenda y Lógica de Negocio |

---

## 3. Especificaciones por Módulo

### Módulo 1: Seguridad y Control de Acceso

#### REQ-SEG-01: Autenticación de Actor Único y Barrera de Acceso (Login)
*   **Descripción:** El sistema **DEBE** exigir la autenticación obligatoria mediante credenciales únicas de Administrador (Usuario y Contraseña) para el acceso a cualquier funcionalidad o vista de la aplicación.
*   **Comportamiento:** Cualquier intento de acceso a rutas internas o protegidas por parte de un agente no autenticado **DEBE** ser rechazado y redirigido obligatoriamente a la pantalla de Inicio de Sesión (`/Login`).
*   **Restricciones:** No existe opción de auto-registro ni de recuperación de contraseña pública.

#### REQ-SEG-02: Gestión de Sesión y Cierre de Sesión (Logout)
*   **Descripción:** El sistema **DEBE** proveer un control interactivo seguro para el cierre de la sesión activa en el menú de navegación superior.
*   **Comportamiento:** Al ejecutarse el cierre de sesión, el sistema **DEBE** invalidar inmediatamente el token de sesión o cookie de autenticación del navegador y redirigir al usuario a la pantalla de Inicio de Sesión (`/Login`).

#### REQ-SEG-03: Auditoría Transaccional Pasiva (Shadow Properties)
*   **Descripción:** El sistema **DEBE** persistir de manera invisible para el usuario una huella de auditoría técnica en la base de datos para cada inserción o modificación de datos en cualquier tabla o entidad del sistema.
*   **Comportamiento:** El motor de persistencia del sistema **DEBE** interceptar los eventos de guardado (`SaveChanges`) para escribir de forma transparente en las propiedades de sombra (Shadow Properties) las siguientes propiedades del registro:
    *   `CreatedBy`: Identificador o nombre de usuario del administrador autenticado que ejecutó la inserción.
    *   `CreatedAt`: Fecha y hora en la que se creó el registro, expresada estrictamente en formato UTC.
    *   `UpdatedAt`: Fecha y hora de la última modificación del registro, expresada en formato UTC.

---

### Módulo 2: Gestión de Citas e Interfaz de Usuario

#### REQ-NAV-01: Navegación Principal (Ribbon Menu)
*   **Descripción:** Tras una autenticación exitosa, el sistema **DEBE** renderizar en la cabecera superior de la aplicación un menú de navegación tipo cinta (Ribbon Menu) de forma persistente.
*   **Comportamiento:** Esta barra de navegación es de visualización exclusiva para el Administrador autenticado y **DEBE** contener única y estrictamente los siguientes cuatro accesos estructurados:
    1.  **Inicio** (Redirección a pantalla principal/dashboard)
    2.  **Propietarios** (Redirección a gestión de clientes)
    3.  **Mascotas** (Redirección a gestión de animales)
    4.  **Citas** (Redirección a la agenda del negocio)

#### REQ-CIT-01: Dependencia de Agendamiento
*   **Descripción:** El sistema **DEBE** restringir la creación de una cita de manera que solo sea posible si el Administrador selecciona una Mascota (Paciente) y un Veterinario (Médico) previamente registrados y activos en la base de datos del sistema.
*   **Comportamiento:** El formulario de creación de citas no **DEBE** permitir el ingreso de datos manuales de texto libre para estos campos, sino la selección de registros relacionales válidos y preexistentes en la base de datos.

#### REQ-CIT-02: Prevención de Superposición de Horarios (Solapamiento)
*   **Descripción:** El sistema **NO DEBE** permitir el registro o modificación de una cita si el Veterinario seleccionado ya cuenta con otra cita activa en su agenda cuyo bloque de tiempo (definido por fecha, hora de inicio y hora de fin) se cruce de manera total o parcial con el bloque de la nueva cita propuesta.
*   **Comportamiento:** En caso de solapamiento de agenda, el sistema **DEBE** abortar la transacción de inserción en la base de datos, rechazar la operación en la interfaz de usuario y desplegar un mensaje de error claro y descriptivo al Administrador (ej. *"El veterinario seleccionado ya posee una cita programada en el bloque de tiempo seleccionado"*).

#### REQ-CIT-03: Estado Inicial de la Entidad Cita
*   **Descripción:** Toda cita registrada exitosamente en el sistema **DEBE** ser persistida con el estado por defecto de **"Programada"**.
*   **Comportamiento:** Al instanciarse un nuevo registro de cita, el sistema **DEBE** asignar automáticamente este valor literal o enumerado antes de persistirlo en la base de datos.

#### REQ-CIT-04: Acción Prominente de Registro
*   **Descripción:** La interfaz del módulo de "Citas" **DEBE** presentar un botón de acción principal (Call to Action) prominente, claramente distinguible visualmente, denominado **"Añadir Cita"**.
*   **Comportamiento:** Al hacer clic en dicho botón, el sistema **DEBE** disparar la transición visual o modal que exponga el formulario de registro de una nueva cita.

#### REQ-CIT-05: Visualización Dual Dinámica
*   **Descripción:** El panel principal del módulo de Citas **DEBE** ofrecer un mecanismo de control de interfaz de usuario para que el Administrador alterne dinámicamente y sin recarga completa de página entre dos modos de visualización de las citas de la agenda:
    1.  **Vista de Lista:** Una tabla de datos estructurada y ordenada cronológicamente por fecha y hora de la cita.
    2.  **Vista de Cuadrícula:** Un calendario interactivo de formato semanal que represente visualmente los bloques de tiempo y permita identificar huecos en la agenda del equipo veterinario.

---

## 4. Criterios de Aceptación (Escenarios BDD - Gherkin)

### Módulo 1: Seguridad y Control de Acceso

#### REQ-SEG-01 (Autenticación y Barrera de Acceso)

```gherkin
Escenario: Redirección obligatoria al intentar acceder a rutas protegidas sin autenticación
  Dado que un usuario no se encuentra autenticado en la plataforma
  Cuando intenta navegar directamente a la URL "/Citas"
  Entonces el sistema DEBE rechazar la petición de navegación
  Y el sistema DEBE redirigir obligatoriamente al usuario a la pantalla de Inicio de Sesión "/Login"

Escenario: Autenticación exitosa del Administrador
  Dado que el Administrador se encuentra en la pantalla de Inicio de Sesión "/Login"
  Cuando ingresa el usuario válido "admin"
  Y la contraseña correcta "Admin123!"
  Y hace clic en el botón "Iniciar Sesión"
  Entonces el sistema DEBE validar satisfactoriamente las credenciales
  Y el sistema DEBE inicializar una sesión segura
  Y el sistema DEBE redirigir al Administrador al módulo de "/Inicio"

Escenario: Intento de autenticación fallido por credenciales inválidas
  Dado que el Administrador se encuentra en la pantalla de Inicio de Sesión "/Login"
  Cuando ingresa un usuario no registrado o una contraseña incorrecta
  Y hace clic en el botón "Iniciar Sesión"
  Entonces el sistema DEBE rechazar la solicitud de inicio de sesión
  Y el sistema DEBE mostrar un mensaje de error visible con el texto "Credenciales incorrectas o usuario inexistente"
  Y el sistema DEBE mantener al usuario en la pantalla "/Login"
```

#### REQ-SEG-02 (Cierre de Sesión)

```gherkin
Escenario: Cierre de sesión y destrucción de credenciales
  Dado que el Administrador ha iniciado sesión y se encuentra navegando en el sistema
  Cuando hace clic en el botón "Cerrar Sesión" del Ribbon Menu
  Entonces el sistema DEBE invalidar de forma inmediata la cookie de sesión o el token de seguridad activo
  Y el sistema DEBE destruir el contexto de usuario autenticado
  Y el sistema DEBE redirigir al usuario a la pantalla de Inicio de Sesión "/Login"
```

#### REQ-SEG-03 (Auditoría Transaccional)

```gherkin
Escenario: Registro automático de metadatos de auditoría al insertar datos
  Dado que el Administrador con identificador "admin_01" está autenticado y se encuentra registrando una nueva entidad
  Cuando el sistema persiste con éxito el registro en la base de datos
  Entonces el sistema DEBE escribir de forma invisible en la entidad los siguientes campos de sombra:
    | Propiedad  | Valor Esperado |
    | CreatedBy  | "admin_01" |
    | CreatedAt  | Fecha y Hora actual del servidor en UTC |
    | UpdatedAt  | Igual a CreatedAt o Nulo |

Escenario: Registro automático de metadatos de auditoría al modificar datos
  Dado que un registro ya existe en el sistema con CreatedBy "admin_01" y CreatedAt "2026-05-25T10:00:00Z"
  Y el Administrador "admin_02" realiza una edición sobre dicho registro
  Cuando el sistema persiste los cambios en la base de datos
  Entonces el sistema DEBE escribir de forma invisible en la entidad el siguiente campo de sombra:
    | Propiedad  | Valor Esperado |
    | UpdatedAt  | Fecha y Hora actual del servidor en UTC |
  Y el sistema NO DEBE alterar bajo ninguna circunstancia el valor original de "CreatedBy" ("admin_01")
  Y el sistema NO DEBE alterar bajo ninguna circunstancia el valor original de "CreatedAt" ("2026-05-25T10:00:00Z")
```

---

### Módulo 2: Gestión de Citas e Interfaz de Usuario

#### REQ-NAV-01 (Menú Cinta)

```gherkin
Escenario: Visualización y opciones del Ribbon Menu tras inicio de sesión
  Dado que el Administrador ha iniciado sesión exitosamente
  Cuando se renderiza la cabecera de cualquier módulo del sistema
  Entonces el sistema DEBE pintar en pantalla un menú tipo cinta en la cabecera superior
  Y el menú DEBE contener estrictamente y con visibilidad completa los siguientes accesos interactivos:
    | Opción       | Destino       |
    | Inicio       | /Inicio       |
    | Propietarios | /Propietarios |
    | Mascotas     | /Mascotas     |
    | Citas        | /Citas        |
```

#### REQ-CIT-01 (Dependencia de Agendamiento)

```gherkin
Escenario: Validación de selección obligatoria de entidades preexistentes
  Dado que el Administrador se encuentra en el formulario de creación de una cita
  Cuando intenta agendar la cita ingresando datos libres de texto o dejando vacía la mascota o el veterinario
  Entonces el sistema DEBE bloquear la acción de envío
  Y el sistema DEBE exigir la selección interactiva de un registro de Mascota y un registro de Veterinario preexistentes en el almacén de datos

Escenario: Selección correcta de dependencias de agendamiento
  Dado que existen en la base de datos la mascota "Rocky" y el veterinario "Dra. Laura"
  Cuando el Administrador asocia a "Rocky" y a "Dra. Laura" en el formulario de la cita
  Y completa los datos obligatorios del bloque de tiempo
  Entonces el sistema DEBE permitir el envío del formulario para su validación de solapamiento
```

#### REQ-CIT-02 (Prevención de Solapamiento de Horarios)

```gherkin
Escenario: Registro de cita fallido por solapamiento completo con agenda del veterinario
  Dado que el veterinario "Dr. Pérez" tiene una cita programada y activa en el bloque "2026-05-28 de 09:00:00 a 10:00:00"
  Cuando el Administrador intenta registrar una nueva cita para el mismo veterinario "Dr. Pérez" en el bloque "2026-05-28 de 09:00:00 a 10:00:00"
  Entonces el sistema DEBE rechazar la transacción
  Y el sistema DEBE abortar la persistencia de datos
  Y el sistema DEBE retornar un mensaje de error visible: "El veterinario seleccionado ya posee una cita activa que se cruza con el horario solicitado"

Escenario: Registro de cita fallido por solapamiento parcial con agenda del veterinario
  Dado que el veterinario "Dr. Pérez" tiene una cita programada y activa en el bloque "2026-05-28 de 14:00:00 a 15:00:00"
  Cuando el Administrador intenta registrar una nueva cita para el mismo veterinario "Dr. Pérez" en el bloque "2026-05-28 de 14:30:00 a 15:30:00"
  Entonces el sistema DEBE rechazar la transacción
  Y el sistema DEBE retornar un mensaje de error detallando la colisión horaria

Escenario: Registro de cita exitoso por no colisionar con la agenda del veterinario
  Dado que el veterinario "Dr. Pérez" tiene una cita programada y activa en el bloque "2026-05-28 de 09:00:00 a 10:00:00"
  Cuando el Administrador intenta registrar una nueva cita para el mismo veterinario "Dr. Pérez" en el bloque "2026-05-28 de 10:00:00 a 11:00:00"
  Entonces el sistema DEBE aprobar la transacción
  Y el sistema DEBE persistir la cita exitosamente
```

#### REQ-CIT-03 (Estado Inicial de la Cita)

```gherkin
Escenario: Asignación por defecto del estado de cita al crearse
  Dado que el Administrador crea exitosamente una cita para la mascota "Luna" con el veterinario "Dr. Gómez"
  Cuando el registro de la cita se inserta por primera vez en la base de datos
  Entonces el sistema DEBE forzar que el campo "Estado" de la cita persista con el valor por defecto "Programada"
```

#### REQ-CIT-04 (Acción Prominente de Registro)

```gherkin
Escenario: Acceso al formulario de citas mediante el botón principal
  Dado que el Administrador se encuentra en la pantalla principal del módulo de Citas
  Cuando se visualiza la interfaz
  Entonces el sistema DEBE renderizar en un lugar prioritario y visible del encabezado o panel principal el botón de Acción Principal "Añadir Cita"
  Y al hacer clic en este botón el sistema DEBE desplegar el formulario de agendamiento de cita
```

#### REQ-CIT-05 (Visualización Dual Dinámica)

```gherkin
Escenario: Cambio interactivo a Vista de Lista de Citas
  Dado que el Administrador visualiza la Vista de Cuadrícula (calendario interactivo) del módulo de Citas
  Cuando presiona el selector interactivo de visualización y elige "Vista de Lista"
  Entonces el sistema DEBE alternar dinámicamente el contenedor principal ocultando el calendario
  Y el sistema DEBE mostrar una tabla de datos organizada por columnas ordenadas de forma cronológica por fecha y hora

Escenario: Cambio interactivo a Vista de Cuadrícula de Citas
  Dado que el Administrador visualiza la Vista de Lista (tabla estructurada) del módulo de Citas
  Cuando presiona el selector interactivo de visualización y elige "Vista de Cuadrícula"
  Entonces el sistema DEBE alternar dinámicamente el contenedor principal ocultando la tabla de datos
  Y el sistema DEBE renderizar el calendario interactivo de formato semanal con la representación visual de bloques de tiempo y huecos disponibles
```****
