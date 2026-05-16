-- ============================================================
--  Industrial Equipment Rental & Service Yard
--  MS SQL Server Script
--  CREATE DATABASE + CREATE TABLES + INSERT DATA
-- ============================================================

-- 1. CREATE DATABASE
CREATE DATABASE EquipmentRentalDB;
GO

USE EquipmentRentalDB;


-- ============================================================
-- 2. CREATE TABLES
-- ============================================================

-- ServiceYard
CREATE TABLE ServiceYard (
    YardID        INT           PRIMARY KEY IDENTITY(1,1),
    Location      VARCHAR(200)  NOT NULL,
    Capacity      INT           NOT NULL,
    ContactNumber VARCHAR(20)
);


-- Technician
CREATE TABLE Technician (
    TechnicianID   INT           PRIMARY KEY IDENTITY(1,1),
    YardID         INT           NOT NULL,
    Name           VARCHAR(100)  NOT NULL,
    Specialization VARCHAR(100),
    Phone          VARCHAR(20),
    CONSTRAINT FK_Technician_Yard FOREIGN KEY (YardID)
        REFERENCES ServiceYard(YardID)
);


-- Equipment
CREATE TABLE Equipment (
    EquipmentID  INT            PRIMARY KEY IDENTITY(1,1),
    YardID       INT            NOT NULL,
    Model        VARCHAR(100)   NOT NULL,
    EnginePower  DECIMAL(10,2),
    HourlyRate   DECIMAL(10,2)  NOT NULL,
    Status       VARCHAR(50)    NOT NULL
        CONSTRAINT CK_Equipment_Status
        CHECK (Status IN ('Available','Rented','Under Maintenance')),
    CONSTRAINT FK_Equipment_Yard FOREIGN KEY (YardID)
        REFERENCES ServiceYard(YardID)
);


-- Contractor
CREATE TABLE Contractor (
    ContractorID  INT            PRIMARY KEY IDENTITY(1,1),
    CompanyName   VARCHAR(150)   NOT NULL,
    CreditLimit   DECIMAL(12,2)  NOT NULL,
    ContactPerson VARCHAR(100),
    Phone         VARCHAR(20),
    Address       VARCHAR(200)
);


-- RentalAgreement
CREATE TABLE RentalAgreement (
    AgreementID  INT            PRIMARY KEY IDENTITY(1,1),
    EquipmentID  INT            NOT NULL,
    ContractorID INT            NOT NULL,
    StartDate    DATE           NOT NULL,
    EndDate      DATE           NOT NULL,
    TotalCost    DECIMAL(12,2),
    Status       VARCHAR(50)    NOT NULL
        CONSTRAINT CK_Agreement_Status
        CHECK (Status IN ('Active','Completed','Cancelled')),
    CONSTRAINT FK_Agreement_Equipment  FOREIGN KEY (EquipmentID)
        REFERENCES Equipment(EquipmentID),
    CONSTRAINT FK_Agreement_Contractor FOREIGN KEY (ContractorID)
        REFERENCES Contractor(ContractorID)
);


-- SafetyInspection
CREATE TABLE SafetyInspection (
    InspectionID     INT           PRIMARY KEY IDENTITY(1,1),
    AgreementID      INT           NOT NULL,
    TechnicianID     INT           NOT NULL,
    EquipmentID      INT           NOT NULL,
    InspectionDate   DATE          NOT NULL,
    InspectionType   VARCHAR(100)  NOT NULL
        CONSTRAINT CK_Inspection_Type
        CHECK (InspectionType IN ('Pre-Release','Post-Return')),
    ConditionStatus  VARCHAR(50)   NOT NULL
        CONSTRAINT CK_Inspection_Condition
        CHECK (ConditionStatus IN ('Good','Fair','Needs Repair')),
    ChecklistNotes   TEXT,
    MaintenanceNotes TEXT,
    CONSTRAINT FK_Inspection_Agreement  FOREIGN KEY (AgreementID)
        REFERENCES RentalAgreement(AgreementID),
    CONSTRAINT FK_Inspection_Technician FOREIGN KEY (TechnicianID)
        REFERENCES Technician(TechnicianID),
    CONSTRAINT FK_Inspection_Equipment  FOREIGN KEY (EquipmentID)
        REFERENCES Equipment(EquipmentID)
);


-- ============================================================
-- 3. INSERT DATA
-- ============================================================

-- ServiceYard (5 yards)
INSERT INTO ServiceYard (Location, Capacity, ContactNumber) VALUES
('Cairo - Industrial Zone A',    20, '02-24501100'),
('Giza - 6th October City',      15, '02-38201200'),
('Alexandria - Western District', 12, '03-54301300'),
('Suez - Port Area',             10, '062-3221400'),
('Mansoura - North Delta',        8, '050-2341500');


-- Technician (8 technicians)
INSERT INTO Technician (YardID, Name, Specialization, Phone) VALUES
(1, 'Ahmed Hassan',    'Crane Inspection',     '0101-1234567'),
(1, 'Mohamed Ali',     'Generator Maintenance','0102-2345678'),
(2, 'Khaled Mahmoud',  'Excavator Specialist', '0103-3456789'),
(2, 'Tamer Ibrahim',   'General Machinery',    '0111-4567890'),
(3, 'Samir Farouk',    'Hydraulic Systems',    '0112-5678901'),
(3, 'Yasser Nabil',    'Crane Inspection',     '0114-6789012'),
(4, 'Hassan Salah',    'Generator Maintenance','0115-7890123'),
(5, 'Omar Adel',       'General Machinery',    '0100-8901234');


-- Equipment (12 machines)
INSERT INTO Equipment (YardID, Model, EnginePower, HourlyRate, Status) VALUES
(1, 'Caterpillar 320 Excavator',  320.00, 850.00,  'Rented'),
(1, 'Liebherr LTM 1100 Crane',    500.00, 1200.00, 'Available'),
(1, 'Cummins C550 Generator',     550.00, 400.00,  'Available'),
(2, 'Komatsu PC210 Excavator',    210.00, 780.00,  'Rented'),
(2, 'Tadano GT-550E Crane',       450.00, 1100.00, 'Rented'),
(2, 'Caterpillar 320 Excavator',  320.00, 850.00,  'Under Maintenance'),
(3, 'Liebherr LTM 1100 Crane',    500.00, 1200.00, 'Available'),
(3, 'Volvo EC300 Excavator',      300.00, 820.00,  'Rented'),
(3, 'Perkins 1106A Generator',    165.00, 350.00,  'Available'),
(4, 'Cummins C550 Generator',     550.00, 400.00,  'Rented'),
(4, 'Komatsu PC210 Excavator',    210.00, 780.00,  'Available'),
(5, 'Tadano GT-550E Crane',       450.00, 1100.00, 'Available');


-- Contractor (6 contractors)
INSERT INTO Contractor (CompanyName, CreditLimit, ContactPerson, Phone, Address) VALUES
('Delta Construction Co.',         500000.00, 'Eng. Ramadan Fouad',  '0100-1112222', 'Cairo - Nasr City, Abbas El-Akkad St.'),
('Pyramid Builders Ltd.',          750000.00, 'Eng. Hossam Galal',   '0101-2223333', 'Giza - Dokki, Tahrir Square Area'),
('NileTech Heavy Works',           300000.00, 'Eng. Sherif Mostafa', '0112-3334444', 'Alexandria - Smouha District'),
('Suez Canal Contractors',         900000.00, 'Eng. Amr Zaki',       '0111-4445555', 'Suez - Port Said Road'),
('North Delta Engineering',        250000.00, 'Eng. Walid Ragab',    '0102-5556666', 'Mansoura - El Gomhoria St.'),
('Egyptian Infrastructure Group',  600000.00, 'Eng. Karim Mansour',  '0114-6667777', 'Cairo - Heliopolis, Merghany St.');


-- RentalAgreement (10 agreements)
INSERT INTO RentalAgreement (EquipmentID, ContractorID, StartDate, EndDate, TotalCost, Status) VALUES
(1,  1, '2026-04-01', '2026-04-15', 255000.00, 'Completed'),
(4,  2, '2026-04-05', '2026-04-20', 234000.00, 'Completed'),
(5,  3, '2026-04-10', '2026-04-30', 528000.00, 'Completed'),
(8,  4, '2026-04-12', '2026-05-12', 591840.00, 'Completed'),
(10, 5, '2026-04-15', '2026-04-25', 96000.00,  'Completed'),
(1,  2, '2026-05-01', '2026-05-20', 306000.00, 'Active'),
(4,  6, '2026-05-02', '2026-05-15', 218400.00, 'Active'),
(5,  1, '2026-05-03', '2026-05-25', 580800.00, 'Active'),
(10, 3, '2026-05-04', '2026-05-14', 96000.00,  'Active'),
(8,  5, '2026-05-05', '2026-06-05', 614880.00, 'Active');


-- SafetyInspection (14 inspections)
INSERT INTO SafetyInspection
    (AgreementID, TechnicianID, EquipmentID, InspectionDate, InspectionType, ConditionStatus, ChecklistNotes, MaintenanceNotes)
VALUES
(1,  1, 1,  '2026-03-31', 'Pre-Release', 'Good',         'All systems operational. Hydraulics checked.', NULL),
(1,  1, 1,  '2026-04-15', 'Post-Return', 'Fair',         'Minor hydraulic leak detected.',               'Hydraulic seal replaced.'),
(2,  3, 4,  '2026-04-04', 'Pre-Release', 'Good',         'Engine and tracks in good condition.',         NULL),
(2,  3, 4,  '2026-04-20', 'Post-Return', 'Good',         'Returned in good condition.',                  NULL),
(3,  5, 5,  '2026-04-09', 'Pre-Release', 'Good',         'Crane arm and cables inspected.',              NULL),
(3,  5, 5,  '2026-04-30', 'Post-Return', 'Needs Repair', 'Cable wear detected on main hoist.',           'Main hoist cable replaced. Load test passed.'),
(4,  6, 8,  '2026-04-11', 'Pre-Release', 'Good',         'Boom and outriggers checked.',                 NULL),
(4,  6, 8,  '2026-05-12', 'Post-Return', 'Good',         'Returned clean and operational.',              NULL),
(5,  7, 10, '2026-04-14', 'Pre-Release', 'Good',         'Generator output and fuel system checked.',    NULL),
(5,  7, 10, '2026-04-25', 'Post-Return', 'Fair',         'Fuel filter needs replacement.',               'Fuel filter replaced.'),
(6,  1, 1,  '2026-04-30', 'Pre-Release', 'Good',         'Post-repair hydraulics verified OK.',          NULL),
(7,  3, 4,  '2026-05-01', 'Pre-Release', 'Good',         'Full pre-release checklist passed.',           NULL),
(8,  5, 5,  '2026-05-02', 'Pre-Release', 'Good',         'Cable replaced and tested. Ready for use.',    NULL),
(9,  7, 10, '2026-05-03', 'Pre-Release', 'Good',         'Generator serviced and fuel topped up.',       NULL);



