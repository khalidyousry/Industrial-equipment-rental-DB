-- ============================================================
-- Project 19: Industrial Equipment Rental & Service Yard
-- Required SQL operations used by the C# GUI application
-- ============================================================

USE EquipmentRentalDB;
GO

-- ============================================================
-- 1. INSERT statements on 2 different tables
-- ============================================================

INSERT INTO Contractor (CompanyName, CreditLimit, ContactPerson, Phone, Address)
VALUES ('Future Build Demo', 100000.00, 'Demo Contact', '0100-0000000', 'Cairo');

INSERT INTO Equipment (YardID, Model, EnginePower, HourlyRate, Status)
VALUES (1, 'Demo Excavator', 250.00, 700.00, 'Available');


-- ============================================================
-- 2. UPDATE statements on 2 different tables with conditions
-- ============================================================

UPDATE Equipment
SET Status = 'Under Maintenance'
WHERE EquipmentID = 2;

UPDATE Contractor
SET CreditLimit = 550000.00
WHERE ContractorID = 1;


-- ============================================================
-- 3. DELETE statements on 2 different tables with conditions
-- ============================================================

DELETE FROM Contractor
WHERE CompanyName = 'Future Build Demo'
  AND NOT EXISTS (
      SELECT 1
      FROM RentalAgreement
      WHERE RentalAgreement.ContractorID = Contractor.ContractorID
  );

DELETE FROM Equipment
WHERE Model = 'Demo Excavator'
  AND Status = 'Available'
  AND NOT EXISTS (
      SELECT 1
      FROM RentalAgreement
      WHERE RentalAgreement.EquipmentID = Equipment.EquipmentID
  )
  AND NOT EXISTS (
      SELECT 1
      FROM SafetyInspection
      WHERE SafetyInspection.EquipmentID = Equipment.EquipmentID
  );


-- ============================================================
-- 4. SELECT data from one table
-- ============================================================

SELECT ContractorID, CompanyName, CreditLimit, ContactPerson, Phone, Address
FROM Contractor
ORDER BY ContractorID;


-- ============================================================
-- 5. SELECT data using joins
-- ============================================================

SELECT ra.AgreementID,
       c.CompanyName,
       e.Model AS EquipmentModel,
       y.Location AS YardLocation,
       ra.StartDate,
       ra.EndDate,
       ra.TotalCost,
       ra.Status
FROM RentalAgreement AS ra
INNER JOIN Contractor AS c ON c.ContractorID = ra.ContractorID
INNER JOIN Equipment AS e ON e.EquipmentID = ra.EquipmentID
INNER JOIN ServiceYard AS y ON y.YardID = e.YardID
ORDER BY ra.AgreementID;


-- ============================================================
-- 6. Project 19 inquiry queries
-- ============================================================

-- Inquiry 1:
-- Which equipment model was the most rented?
SELECT TOP 1 WITH TIES
       e.Model,
       COUNT(*) AS AgreementCount
FROM Equipment AS e
INNER JOIN RentalAgreement AS ra ON ra.EquipmentID = e.EquipmentID
GROUP BY e.Model
ORDER BY COUNT(*) DESC;


-- Inquiry 2:
-- Which service yard had no equipment rentals originating from it last month?
DECLARE @StartLastMonth date =
    DATEFROMPARTS(YEAR(DATEADD(MONTH, -1, GETDATE())),
                  MONTH(DATEADD(MONTH, -1, GETDATE())), 1);
DECLARE @StartThisMonth date =
    DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

SELECT y.YardID, y.Location, y.Capacity, y.ContactNumber
FROM ServiceYard AS y
WHERE NOT EXISTS (
    SELECT 1
    FROM Equipment AS e
    INNER JOIN RentalAgreement AS ra ON ra.EquipmentID = e.EquipmentID
    WHERE e.YardID = y.YardID
      AND ra.StartDate >= @StartLastMonth
      AND ra.StartDate < @StartThisMonth
)
ORDER BY y.YardID;


-- Inquiry 3:
-- Who was the technician who performed the maximum number of safety inspections last month?
SELECT TOP 1 WITH TIES
       t.TechnicianID,
       t.Name,
       t.Specialization,
       COUNT(*) AS InspectionCount
FROM Technician AS t
INNER JOIN SafetyInspection AS si ON si.TechnicianID = t.TechnicianID
WHERE si.InspectionDate >= @StartLastMonth
  AND si.InspectionDate < @StartThisMonth
GROUP BY t.TechnicianID, t.Name, t.Specialization
ORDER BY COUNT(*) DESC;


-- Inquiry 4:
-- Identify contractors who did not form any new rental agreements last month.
SELECT c.ContractorID, c.CompanyName, c.CreditLimit, c.ContactPerson, c.Phone
FROM Contractor AS c
WHERE NOT EXISTS (
    SELECT 1
    FROM RentalAgreement AS ra
    WHERE ra.ContractorID = c.ContractorID
      AND ra.StartDate >= @StartLastMonth
      AND ra.StartDate < @StartThisMonth
)
ORDER BY c.CompanyName;


-- Inquiry 5:
-- What were the available machines at each service yard last month?
SELECT y.YardID,
       y.Location,
       e.EquipmentID,
       e.Model,
       e.EnginePower,
       e.HourlyRate,
       e.Status
FROM ServiceYard AS y
INNER JOIN Equipment AS e ON e.YardID = y.YardID
WHERE e.Status = 'Available'
  AND NOT EXISTS (
      SELECT 1
      FROM RentalAgreement AS ra
      WHERE ra.EquipmentID = e.EquipmentID
        AND ra.Status <> 'Cancelled'
        AND ra.StartDate < @StartThisMonth
        AND ra.EndDate >= @StartLastMonth
  )
ORDER BY y.YardID, e.EquipmentID;


-- Inquiry 6:
-- For each contractor, retrieve company name and total hours rented last month.
SELECT c.ContractorID,
       c.CompanyName,
       COALESCE(SUM(DATEDIFF(HOUR,
           CAST(ra.StartDate AS datetime),
           CAST(ra.EndDate AS datetime))), 0) AS TotalRentalHoursLastMonth
FROM Contractor AS c
LEFT JOIN RentalAgreement AS ra
       ON ra.ContractorID = c.ContractorID
      AND ra.StartDate >= @StartLastMonth
      AND ra.StartDate < @StartThisMonth
GROUP BY c.ContractorID, c.CompanyName
ORDER BY TotalRentalHoursLastMonth DESC, c.CompanyName;
