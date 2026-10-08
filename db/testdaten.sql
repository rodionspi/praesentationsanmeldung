USE Praesentationsanmeldung;

DELETE FROM Eintragungen;
DELETE FROM Praesentationen;
DELETE FROM G3Sus;
DELETE FROM Raeume;

INSERT INTO Raeume (Id, Bezeichnung, Kapazitaet) VALUES
    ('A101', 28),
    ('A102', 28),
    ('B201', 24),
    ('Aula', 120);

INSERT INTO Praesentationen (Id, Titel, Beschreibung, Beginn, RaumId) VALUES
    (N'Mikroplastik in Schweizer Gewässern', N'Probenahme und Auswertung an drei Flüssen', '2026-11-12T08:00:00', 1),
    (N'Künstliche Intelligenz im Schulalltag', N'Umfrage unter Lehrpersonen und Lernenden', '2026-11-12T08:00:00', 2),
    (N'Die Geschichte des Frauenstimmrechts', NULL, '2026-11-12T09:00:00', 1),
    (N'Bau eines Wetterballons', N'Konstruktion, Start und Auswertung der Messdaten', '2026-11-12T09:00:00', 3),
    (N'Musik und Konzentration', N'Experiment zur Wirkung von Hintergrundmusik beim Lernen', '2026-11-12T10:00:00', 4);

INSERT INTO G3Sus (Id, Vorname, Nachname, Klasse) VALUES
    (N'Lena', N'Müller', N'G3a'),
    (N'Noah', N'Meier', N'G3a'),
    (N'Mia', N'Schmid', N'G3a'),
    (N'Luca', N'Keller', N'G3b'),
    (N'Elena', N'Weber', N'G3b'),
    (N'Leon', N'Huber', N'G3b'),
    (N'Sara', N'Brunner', N'G3c'),
    (N'Jonas', N'Baumann', N'G3c');
