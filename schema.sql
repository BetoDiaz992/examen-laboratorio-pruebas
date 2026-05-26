-- ============================================================================
-- Nombre del Script: schema.sql
-- Motor de Base de Datos: Microsoft SQL Server (2019 / 2022)
-- Compatibilidad .NET: .NET 10.0 (Entity Framework Core 10)
-- 
-- Misión: Traducir la especificación 'spec-core.md' en un esquema relacional
--         físico seguro, de alto rendimiento y preparado para producción.
--
-- Reglas de Negocio Implementadas de spec-core.md:
-- 1. Actor Único de Acceso: Tabla Administrators para credenciales únicas.
-- 2. Trazabilidad de Auditoría Pasiva (REQ-SEG-03): Shadow Properties físicas
--    (CreatedAt, CreatedBy, UpdatedAt) con valores predeterminados seguros.
-- 3. Prevención de Cruce de Horarios (REQ-CIT-02): Índice Único Filtrado en Appointments
--    para evitar colisiones de turnos a nivel físico.
-- 4. Selección Obligatoria (REQ-CIT-01): Claves foráneas (FK) contra tablas
--    Pets y Veterinarians con restricción de borrado.
-- ============================================================================

-- 1. Inicialización de la Base de Datos
USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'PetClinicDb')
BEGIN
    -- Terminar conexiones existentes de forma segura para permitir el borrado
    ALTER DATABASE PetClinicDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PetClinicDb;
END
GO

CREATE DATABASE PetClinicDb;
GO

USE PetClinicDb;
GO

-- ============================================================================
-- 2. CREACIÓN DE TABLAS (DDL)
-- ============================================================================

-- Tabla: Administrators (REQ-SEG-01 / REQ-SEG-02 / Administrador Único)
-- Almacena las credenciales de acceso restringidas del Administrador Único de PetClinic.
CREATE TABLE Administrators (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL, -- Almacena hash de seguridad (Bcrypt / Argon2)
    
    -- Shadow Properties de Auditoría Física Obligatoria (REQ-SEG-03)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'SystemAdmin', -- Inyección por defecto segura
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT PK_Administrators PRIMARY KEY (Id),
    CONSTRAINT UQ_Administrators_Email UNIQUE (Email)
);
GO

-- Tabla: Veterinarians (REQ-CIT-01 / Catálogo de Médicos de la Clínica)
-- Registra los datos relacionales de los veterinarios contratados en la clínica.
CREATE TABLE Veterinarians (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Specialty NVARCHAR(100) NOT NULL,
    MedicalLicense NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    
    -- Shadow Properties de Auditoría Física Obligatoria (REQ-SEG-03)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'SystemAdmin',
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT PK_Veterinarians PRIMARY KEY (Id),
    CONSTRAINT UQ_Veterinarians_MedicalLicense UNIQUE (MedicalLicense),
    CONSTRAINT UQ_Veterinarians_Email UNIQUE (Email)
);
GO

-- Tabla: Pets (REQ-CIT-01 / Catálogo de Pacientes Mascotas)
-- Almacena las fichas y datos de las mascotas de la clínica.
CREATE TABLE Pets (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    OwnerId UNIQUEIDENTIFIER NOT NULL, -- Identificador del propietario (relación conceptual)
    Name NVARCHAR(100) NOT NULL,
    Species NVARCHAR(50) NOT NULL,     -- Especie (Perro, Gato, Loro, etc.)
    Breed NVARCHAR(100) NULL,          -- Raza (Nulo si no se conoce o es mestizo)
    BirthDate DATE NOT NULL,
    
    -- Shadow Properties de Auditoría Física Obligatoria (REQ-SEG-03)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'SystemAdmin',
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT PK_Pets PRIMARY KEY (Id)
);
GO

-- Tabla: Appointments (REQ-CIT-01 / REQ-CIT-02 / REQ-CIT-03 / Calendario de Citas)
-- Controla la agenda médica relacional. Su estado inicial por defecto es 'SCHEDULED' (Programada).
CREATE TABLE Appointments (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PetId UNIQUEIDENTIFIER NOT NULL,
    VeterinarianId UNIQUEIDENTIFIER NOT NULL,
    ScheduledTime DATETIME2 NOT NULL, -- Fecha y bloque horario de inicio de la cita
    Reason NVARCHAR(500) NOT NULL,    -- Motivo de la consulta
    State NVARCHAR(30) NOT NULL DEFAULT 'SCHEDULED', -- Estados: SCHEDULED, COMPLETED, CANCELLED
    
    -- Shadow Properties de Auditoría Física Obligatoria (REQ-SEG-03)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT 'SystemAdmin',
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT PK_Appointments PRIMARY KEY (Id),
    
    -- Claves foráneas con borrado restringido para evitar la pérdida accidental de datos históricos
    CONSTRAINT FK_Appointments_Pets FOREIGN KEY (PetId) 
        REFERENCES Pets(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Appointments_Veterinarians FOREIGN KEY (VeterinarianId) 
        REFERENCES Veterinarians(Id) ON DELETE NO ACTION
);
GO

-- ============================================================================
-- 3. ÍNDICES DE INTEGRIDAD FÍSICA Y OPTIMIZACIÓN (DML/DDL)
-- ============================================================================

-- 3.1 PREVENCIÓN DE SOLAPAMIENTO DE CITAS A NIVEL DE MOTOR (REQ-CIT-02 / CRÍTICO)
--     Índice único filtrado que impide físicamente registrar dos citas solapadas en la misma
--     fecha y bloque de hora para un mismo veterinario. Excluye citas canceladas ('CANCELLED')
--     para permitir liberar bloques horarios en cancelaciones.
CREATE UNIQUE INDEX UX_Appointments_Veterinarian_Time
ON Appointments(VeterinarianId, ScheduledTime)
WHERE State != 'CANCELLED';
GO

-- 3.2 Índices secundarios en claves foráneas (FK) para optimizar consultas y JOINs
CREATE INDEX IX_Appointments_PetId ON Appointments(PetId);
GO

-- 3.3 Índice compuesto de calendario semanal (REQ-CIT-05 / Visualización Dual)
--     Acelera el renderizado del calendario semanal y la búsqueda de bloques horarios libres.
CREATE INDEX IX_Appointments_ScheduledTime_Vet
ON Appointments(ScheduledTime, VeterinarianId);
GO

-- ============================================================================
-- 4. INSERTs SEMILLAS DE PRUEBA (SEEDS DATA)
-- ============================================================================

PRINT 'Insertando registros semilla para pruebas...';

-- 4.1 Semilla: Administrador Único (REQ-SEG-01 / admin@petclinic.com)
INSERT INTO Administrators (Id, Name, Email, PasswordHash)
VALUES (
    'D04E76A0-534A-4A62-97B7-5A1E8A9BC6C8',
    'Administrador Clínico Principal',
    'admin@petclinic.com',
    'ClinicAdminSecurePass10!' -- Hash de prueba
);

-- 4.2 Semilla: Veterinarios Clínicos (REQ-CIT-01)
INSERT INTO Veterinarians (Id, Name, Specialty, MedicalLicense, Email)
VALUES 
(
    'A5E947A1-309A-4C28-BBBE-7CDA29DECF12',
    'Dr. Carlos Silva',
    'Consulta General y Vacunación',
    'LIC-12345',
    'carlos.silva@petclinic.com'
),
(
    'B18F62C8-7FCD-4E89-982A-E274B76A0F44',
    'Dra. Laura Martínez',
    'Cirugía Veterinaria',
    'LIC-54321',
    'laura.martinez@petclinic.com'
);

-- 4.3 Semilla: Mascotas Registradas (REQ-CIT-01)
INSERT INTO Pets (Id, OwnerId, Name, Species, Breed, BirthDate)
VALUES
(
    'C18F62C8-7FCD-4E89-982A-E274B76A0F33',
    'E9B8C7D6-E5F4-3210-FEDC-BA98765432AA',
    'Toby',
    'Perro',
    'Golden Retriever',
    '2021-06-15'
),
(
    'F5D947A1-309A-4C28-BBBE-7CDA29DECF81',
    'E9B8C7D6-E5F4-3210-FEDC-BA98765432BB',
    'Luna',
    'Gato',
    'Siamés',
    '2023-02-10'
);

-- 4.4 Semilla: Cita inicial de demostración (REQ-CIT-03)
INSERT INTO Appointments (Id, PetId, VeterinarianId, ScheduledTime, Reason, State)
VALUES
(
    '8F2D3A1C-9B8A-4D2C-8A1A-3C2B1A0F9E8D',
    'C18F62C8-7FCD-4E89-982A-E274B76A0F33', -- Toby
    'A5E947A1-309A-4C28-BBBE-7CDA29DECF12', -- Dr. Carlos Silva
    '2026-06-01 10:00:00.0000000',          -- Bloque Lunes 10:00 AM
    'Chequeo médico de rutina',
    'SCHEDULED'
);
GO

PRINT 'Esquema de base de datos y semillas creados exitosamente en PetClinicDb.';
GO
