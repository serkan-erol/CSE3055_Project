
--------------------------------------------------------------------------------------------------------------------------------
---------------------------------------------------------- INSERT DATA ---------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------

-- User Entries --
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Serkan EROL', 'serkan@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ebrar ÇELİKKAYA', 'ebrar@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ali KOLDAŞ', 'ali@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Rahmet YILMAZ', 'rahmet@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Mehmet KARAKAYA', 'mehmet@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ayşe KARAKAYA', 'ayse@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Fatma ASLAN', 'fatma@test.com', '1234', 'Employee');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Zeynep HAR', 'zeynep@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Selin GÜNDÜZ', 'selin@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Sümeyye YILMAZ', 'sumeyye@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Nur ÜLKÜ', 'nur@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Mete ÜLKÜ', 'mete@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Miray YAZICI', 'miray@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Yasin ŞENSOY', 'yasin@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Yunus Emir ÖZGÜL', 'yunus@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Talat BULUT', 'talat@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ahmet BENK', 'ahmet@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Ece KARAKAYA', 'ece@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Elif ÇINAR', 'elif@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Esin KARAKAYA', 'esin@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Emrah KARAKAYA', 'emrah@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Emre KARAKAYA', 'emre@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Burcu KARASLAN', 'burcu@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Deniz KARASLAN', 'deniz@test.com', '1234', 'Customer');
INSERT INTO dbo.[User] (UserName, ContactEmail, PasswordHash, UserType)
VALUES ('Erdal ÇALIŞKAN', 'erdal@test.com', '1234', 'Customer');

-- Employee Entries --
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (3, 'sales', 3);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (4, 'local_admin', 4);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (5, 'admin', 7);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (6, 'sys_admin', 9);
INSERT INTO dbo.[Employee] (EmployeeID, EmployeeRole, AccessLevel)
VALUES (7, 'ceo', 10);

-- Customer Entries --
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (1, 'Person', 1, 'Yozgat', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (2, 'Company', 1, 'istanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (8, 'Company', 1, 'istanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (9, 'Company', 1, 'Bursa', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (10, 'Company', 1, 'Adana', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (11, 'Company', 1, 'Samsun', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (12, 'Company', 1, 'Samsun', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (13, 'Company', 1, 'İzmir', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (14, 'Company', 1, 'Antalya', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (15, 'Company', 1, 'Amsterdam', 'Netherlands');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (16, 'Company', 1, 'İstanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (17, 'Company', 1, 'Tiflis', 'Georgia');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (18, 'Company', 1, 'Berlin', 'Germany');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (19, 'Company', 1, 'Madrid', 'Spain');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (20, 'Company', 1, 'Paris', 'France');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (21, 'Company', 1, 'Rome', 'Italy');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (22, 'Company', 1, 'Istanbul', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (23, 'Company', 1, 'Ankara', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (24, 'Company', 1, 'Izmir', 'Türkiye');
INSERT INTO dbo.[Customer] (CustomerID, CustomerType, ReliabilityStatus, City, Country)
VALUES (25, 'Company', 1, 'Adana', 'Türkiye');

-- Saved Payment Method Entries --
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (1, '1111-5678-9012-3459', 'Debit', '2028-01-01', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (2, '2222-5678-9012-3458', 'Credit', '2027-06-04', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (8, '8888-5678-9012-3457', 'Debit', '2031-02-07', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (9, '9999-5678-9012-3456', 'Credit', '2030-08-10', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (10, '1010-5678-9012-3455', 'Debit', '2029-03-13', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (11, '1111-5678-9012-3454', 'Credit', '2028-09-16', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (12, '1212-5678-9012-3453', 'Debit', '2027-12-19', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (13, '1313-5678-9012-3452', 'Credit', '2027-04-22', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (14, '1414-5678-9012-3451', 'Debit', '2026-07-25', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (15, '1515-5678-9012-3450', 'Credit', '2029-09-26', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (16, '1616-5678-9012-3449', 'Debit', '2030-12-26', '2027-01-01');
INSERT INTO dbo.[SavedPaymentMethod] (CustomerID, CardNumber, CardType, CardExpirationDate, RecordExpirationDate)
VALUES (17, '1717-5678-9012-3448', 'Credit', '2032-12-26', '2027-01-01');

-- Saved Bank Information Entries --
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (1, 'Akbank', '1234567890', 'TR01 0006 2000 0000 0006 6700 0001');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (2, 'Garanti Bank', '1234567890', 'TR02 0006 2000 0000 0006 6700 0002');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (8, 'Türkiye İş Bankası', '1234567890', 'TR08 0006 2000 0000 0006 6700 0008');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (9, 'Türkiye İş Bankası', '1234567890', 'TR09 0006 2000 0000 0006 6700 0009');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (18, 'Türkiye İş Bankası', '1234567890', 'TR18 0006 2000 0000 0006 6700 0018');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (19, 'Ziraat Bankası', '1234567890', 'TR19 0006 2000 0000 0006 6700 0019');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (20, 'Ziraat Bankası', '1234567890', 'TR20 0006 2000 0000 0006 6700 0020');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (21, 'Halkbank', '1234567890', 'TR21 0006 2000 0000 0006 6700 0021');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (22, 'Halkbank', '1234567890', 'TR22 0006 2000 0000 0006 6700 0022');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (23, 'Yapı Kredi Bankası', '1234567890', 'TR23 0006 2000 0000 0006 6700 0023');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (24, 'Yapı Kredi Bankası', '1234567890', 'TR24 0006 2000 0000 0006 6700 0024');
INSERT INTO dbo.[SavedBankInformation] (CustomerID, BankName, AccountNo, IBAN)
VALUES (25, 'Yapı Kredi Bankası', '1234567890', 'TR25 0006 2000 0000 0006 6700 0025');

-- Fabric Entries --
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'White', 1.00, 100, 10.00, 'White cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Red', 1.00, 100, 10.00, 'Red cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Blue', 1.00, 100, 10.00, 'Blue cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Green', 1.00, 100, 10.00, 'Green cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Yellow', 1.00, 100, 10.00, 'Yellow cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Purple', 1.00, 100, 10.00, 'Purple cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Orange', 1.00, 100, 10.00, 'Orange cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Pink', 1.00, 100, 10.00, 'Pink cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Brown', 1.00, 100, 10.00, 'Brown cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Black', 1.00, 100, 10.00, 'Black cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Gray', 1.00, 100, 10.00, 'Gray cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Silver', 1.00, 100, 10.00, 'Silver cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Cotton', '100% Cotton', 'Gold', 1.00, 100, 10.00, 'Gold cotton fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Gold', 1.00, 100, 10.00, 'White polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Red', 1.00, 100, 10.00, 'Red polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Blue', 1.00, 100, 10.00, 'Blue polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Green', 1.00, 100, 10.00, 'Green polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Yellow', 1.00, 100, 10.00, 'Yellow polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Purple', 1.00, 100, 10.00, 'Purple polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Orange', 1.00, 100, 10.00, 'Orange polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Pink', 1.00, 100, 10.00, 'Pink polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Brown', 1.00, 100, 10.00, 'Brown polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Black', 1.00, 100, 10.00, 'Black polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Gray', 1.00, 100, 10.00, 'Gray polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Silver', 1.00, 100, 10.00, 'Silver polyester fabric');
INSERT INTO dbo.[Fabric] (FabricType, Composition, Color, WeightPerUnit, StockQuantity, UnitPrice, Description)
VALUES ('Polyester', '70% Polyester + 30% Cotton', 'Gold', 1.00, 100, 10.00, 'Gold polyester fabric');