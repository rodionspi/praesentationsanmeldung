USE Praesentationsanmeldung;

DELETE FROM Eintragungen;
DELETE FROM Praesentationen;
DELETE FROM G3Sus;
DELETE FROM Raeume;

INSERT INTO Raeume (Bezeichnung, Kapazitaet) VALUES
    ('A101', 28),
    ('A102', 28),
    ('B201', 24),
    ('Aula', 120);

INSERT INTO Praesentationen (Titel, Beschreibung, Beginn, RaumId) VALUES
    (N'Mikroplastik in Schweizer Gewässern', N'Probenahme und Auswertung an drei Flüssen', '2026-11-12T08:00:00', (SELECT Id FROM Raeume WHERE Bezeichnung = 'A101')),
    (N'Künstliche Intelligenz im Schulalltag', N'Umfrage unter Lehrpersonen und Lernenden', '2026-11-12T08:00:00', (SELECT Id FROM Raeume WHERE Bezeichnung = 'A102')),
    (N'Die Geschichte des Frauenstimmrechts', NULL, '2026-11-12T09:00:00', (SELECT Id FROM Raeume WHERE Bezeichnung = 'A101')),
    (N'Bau eines Wetterballons', N'Konstruktion, Start und Auswertung der Messdaten', '2026-11-12T09:00:00', (SELECT Id FROM Raeume WHERE Bezeichnung = 'B201')),
    (N'Musik und Konzentration', N'Experiment zur Wirkung von Hintergrundmusik beim Lernen', '2026-11-12T10:00:00', (SELECT Id FROM Raeume WHERE Bezeichnung = 'Aula'));
