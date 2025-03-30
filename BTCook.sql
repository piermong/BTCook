DROP DATABASE IF EXISTS BTCook;
CREATE DATABASE IF NOT EXISTS BTCook;
USE BTCook;

-- TABLES UTILISATEURS
CREATE TABLE UTILISATEUR(
   matriculeUtilisateur BIGINT AUTO_INCREMENT PRIMARY KEY,
   nom VARCHAR(100) NOT NULL,
   prénom VARCHAR(100) NOT NULL,
   notation TINYINT NOT NULL DEFAULT 0,
   adresse TEXT NOT NULL,
   email VARCHAR(100) NOT NULL UNIQUE,
   téléphone VARCHAR(15) NOT NULL,
   régime VARCHAR(100),
   réseauxSociaux TEXT,
   password VARCHAR(255) NOT NULL
);

CREATE TABLE CUISINIER(
   matriculeCuisinier BIGINT AUTO_INCREMENT PRIMARY KEY,
   matriculeUtilisateur BIGINT NOT NULL,
   FOREIGN KEY (matriculeUtilisateur) REFERENCES UTILISATEUR(matriculeUtilisateur) ON DELETE CASCADE,
   UNIQUE(matriculeUtilisateur)
);

CREATE TABLE CLIENT(
   matriculeClient BIGINT AUTO_INCREMENT PRIMARY KEY,
   matriculeUtilisateur BIGINT NOT NULL,
   FOREIGN KEY (matriculeUtilisateur) REFERENCES UTILISATEUR(matriculeUtilisateur) ON DELETE CASCADE,
   UNIQUE(matriculeUtilisateur)
);

-- TABLES CUISINE
CREATE TABLE INGRÉDIENT(
   matriculeIngrédient BIGINT AUTO_INCREMENT PRIMARY KEY,
   nom VARCHAR(100) NOT NULL
);

CREATE TABLE PLAT(
   matriculePlat BIGINT AUTO_INCREMENT PRIMARY KEY,
   ingrédients TEXT NOT NULL,
   prix DECIMAL(10, 2) NOT NULL,
   photo VARCHAR(100),
   requêteClient TEXT,
   note TINYINT CHECK (note BETWEEN 0 AND 5),
   quantité INT NOT NULL,
   origine VARCHAR(50) NOT NULL,
   nom VARCHAR(100) NOT NULL,
   type ENUM('entrée', 'plat','dessert') NOT NULL,
   metro TEXT NOT NULL,
   matriculeCuisinier BIGINT,
   FOREIGN KEY (matriculeCuisinier) REFERENCES CUISINIER(matriculeCuisinier) ON DELETE CASCADE
);

CREATE TABLE MENU(
   matriculeMenu BIGINT AUTO_INCREMENT PRIMARY KEY,
   nom VARCHAR(100) NOT NULL,
   disponible ENUM('Disponible', 'Indisponible') NOT NULL
);


-- TABLES ÉVALUATIONS & COMMUNICATIONS
CREATE TABLE AVIS(
   matriculeAvis BIGINT AUTO_INCREMENT PRIMARY KEY,
   note TINYINT NOT NULL CHECK (note BETWEEN 0 AND 5),
   commentaire TEXT NOT NULL,
   matriculeClient BIGINT,
   matriculeCuisinier BIGINT,
   FOREIGN KEY (matriculeClient) REFERENCES CLIENT(matriculeClient) ON DELETE CASCADE,
   FOREIGN KEY (matriculeCuisinier) REFERENCES CUISINIER(matriculeCuisinier) ON DELETE CASCADE
);

CREATE TABLE MESSAGERIE(
   matriculeMessagerie BIGINT AUTO_INCREMENT PRIMARY KEY,
   message TEXT NOT NULL,
   dateEmission DATETIME NOT NULL,
   nomClient VARCHAR(100),
   nomCuisinier VARCHAR(100),
   matriculeCuisinier BIGINT NOT NULL,
   matriculeClient BIGINT NOT NULL,
   FOREIGN KEY (matriculeCuisinier) REFERENCES CUISINIER(matriculeCuisinier) ON DELETE CASCADE,
   FOREIGN KEY (matriculeClient) REFERENCES CLIENT(matriculeClient) ON DELETE CASCADE
);

-- TABLES COMMANDES
CREATE TABLE COMMANDE(
   matriculeCommande BIGINT AUTO_INCREMENT PRIMARY KEY,
   dateDeCommande DATETIME NOT NULL,
   duréeEstimée TIME,
   heureDépart DATETIME NOT NULL,
   notationDeCommande TEXT,
   matriculeAvis BIGINT,
   matriculeClient BIGINT NOT NULL,
   matriculesCuisiniers TEXT,
   resumer TEXT,
   prixtotal INT,
   FOREIGN KEY (matriculeAvis) REFERENCES AVIS(matriculeAvis) ON DELETE SET NULL
   
);

-- TABLES RELATIONNELLES
CREATE TABLE CONTIENT2(
   matriculeCommande BIGINT,
   matriculePlat BIGINT,
   PRIMARY KEY(matriculeCommande, matriculePlat),
   FOREIGN KEY (matriculeCommande) REFERENCES COMMANDE(matriculeCommande) ON DELETE CASCADE,
   FOREIGN KEY (matriculePlat) REFERENCES PLAT(matriculePlat) ON DELETE CASCADE
);

CREATE TABLE COMPOSE(
   matriculePlat BIGINT,
   matriculeIngrédient BIGINT,
   PRIMARY KEY(matriculePlat, matriculeIngrédient),
   FOREIGN KEY (matriculePlat) REFERENCES PLAT(matriculePlat) ON DELETE CASCADE,
   FOREIGN KEY (matriculeIngrédient) REFERENCES INGRÉDIENT(matriculeIngrédient) ON DELETE CASCADE
);

-- INSERTIONS UTILISATEURS
INSERT INTO UTILISATEUR (nom, prénom, notation, adresse, email, téléphone, régime, réseauxSociaux, password) 
VALUES 
('Dupond', 'Marie', 0, '30 Rue de la République, 75011 Paris', 'Mdupond@gmail.com', '1234567890', '', '','mdp'),
('Graveline', 'Léo', 5, '12 Avenue Pierre Grenier, 78220 Viroflay', '123', '0683590692', '', '','123'),
('Durand', 'Medhy', 0, '15 Rue Cardinet, 75017 Paris', 'Mdurand@gmail.com', '1234567890', '', '','mdp'),
('Lemoine', 'Julien', 0, '10 Rue des Lilas, 75012 Paris', 'Jlemoine@gmail.com', '0987654321', '', '','mdp'),
('Martin', 'Sophie', 0, '5 Rue de la Paix, 75008 Paris', 'Smartin@gmail.com', '0678901234', '', '','mdp');

-- INSERTIONS CUISINIERS ET CLIENTS
INSERT INTO CUISINIER (matriculeUtilisateur) VALUES (1), (4),(5);
INSERT INTO CLIENT (matriculeUtilisateur) VALUES (3), (2);

-- INSERTIONS DE PLATS
INSERT INTO PLAT (ingrédients, prix, photo, requêteClient, note, quantité, origine, nom, type, metro, matriculeCuisinier)
VALUES 

('crevettes, avocat, laitue, sauce cocktail', 8, 'salade_crevettes.jpg', NULL, 0, 6, 'Française', 'Salade de crevettes','entrée','Châtelet', 1),
('bacon, laitue, tomate, mayo, pain de mie', 7, 'club_sandwich.jpg', NULL, 0, 6, 'Américaine', 'Club Sandwich','entrée','Duroc', 2),
('tartare de saumon, avocat, oignon', 9, 'tartare_saumon.jpg', NULL, 0, 6, 'Française', 'Tartare de saumon','entrée','Invalides', 3),

('raclette fromage, pommes de terre, jambon, cornichon', 10, 'raclette.jpg', NULL, 0, 6, 'Française', 'Raclette','plat','Châtelet', 1),
('poulet, curry, riz basmati, légumes', 12, 'poulet_curry.jpg', NULL, 0, 6, 'Indienne', 'Poulet au curry','plat','Châtelet', 1),
('steak haché, pommes de terre, sauce bordelaise', 14, 'steak_frites.jpg', NULL, 0, 6, 'Française', 'Steak frites','plat','Duroc', 2),
('lasagnes, viande hachée, sauce tomate, fromage', 12, 'lasagnes_italiennes.jpg', NULL, 0, 6, 'Italienne', 'Lasagnes traditionnelles','plat','Invalides', 3),

('chocolat, farine, œufs, sucre', 4, 'fondant_chocolat.jpg', NULL, 0, 6, 'Française', 'Fondant au chocolat','dessert','Châtelet', 1),
('pommes, sucre, cannelle', 5, 'tarte_pommes.jpg', NULL, 0, 6, 'Française', 'Tarte aux pommes','dessert','Châtelet', 1),
('pâte sablée, framboises, crème pâtissière', 6, 'tarte_framboises.jpg', NULL, 0, 6, 'Française', 'Tarte aux framboises','dessert','Duroc', 2),
('crème, sucre, vanille, œufs', 4, 'crème_brulee.jpg', NULL, 0, 6, 'Française', 'Crème brûlée','dessert','Invalides', 3);

-- Insertion d'un nouvel ingrédient
INSERT INTO INGRÉDIENT (nom) 
VALUES 
('Tomates'),
('Câpres'),
('Champignons'),
('Oignons');

-- Insertion des commandes passées par Medhy et préparées par Marie
INSERT INTO COMMANDE (dateDeCommande, duréeEstimée, heureDépart, notationDeCommande, matriculeClient, matriculesCuisiniers) 
VALUES
('2025-01-10 12:00:00', '00:30:00', '2025-01-10 12:30:00', 5, 1, 1),
('2025-01-10 12:15:00', '00:20:00', '2025-01-10 12:35:00', 4, 1, 1);

-- Insertion d'un nouveau menu
INSERT INTO MENU (nom, disponible) 
VALUES ('entrée,plat,dessert', 'Disponible'),
('entrée,plat','Disponible'),
('plat,dessert','Disponible'),
('Entrée','Disponible'),
('Plat','Disponible'),
('Dessert','Disponible');

-- Associer l'ingrédient au plat
INSERT INTO COMPOSE (matriculePlat, matriculeIngrédient) 
VALUES 
-- Ingrédient 'Tomates' pour 'Poulet au curry'
((SELECT matriculePlat FROM PLAT WHERE nom = 'Poulet au curry'), 
(SELECT matriculeIngrédient FROM INGRÉDIENT WHERE nom = 'Tomates')),

-- Ingrédient 'Câpres' pour 'Lasagnes traditionnelles'
((SELECT matriculePlat FROM PLAT WHERE nom = 'Lasagnes traditionnelles'), 
(SELECT matriculeIngrédient FROM INGRÉDIENT WHERE nom = 'Câpres')),

-- Ingrédient 'Champignons' pour 'Raclette'
((SELECT matriculePlat FROM PLAT WHERE nom = 'Raclette'), 
(SELECT matriculeIngrédient FROM INGRÉDIENT WHERE nom = 'Champignons')),

-- Ingrédient 'Oignons' pour 'Tartare de saumon'
((SELECT matriculePlat FROM PLAT WHERE nom = 'Tartare de saumon'), 
(SELECT matriculeIngrédient FROM INGRÉDIENT WHERE nom = 'Oignons'));

-- Insertion d'un avis pour la commande, une fois que la commande est livrée
INSERT INTO AVIS (note, commentaire, matriculeClient, matriculeCuisinier) 
VALUES (4, 'Bon repas, mais un peu épicé pour moi', 
(SELECT matriculeClient FROM CLIENT WHERE matriculeUtilisateur = (SELECT matriculeUtilisateur FROM UTILISATEUR WHERE nom = 'Martin' AND prénom = 'Sophie')),
(SELECT matriculeCuisinier FROM CUISINIER WHERE matriculeUtilisateur = (SELECT matriculeUtilisateur FROM UTILISATEUR WHERE nom = 'Lemoine' AND prénom = 'Julien')));






-- REQUÊTES DE VÉRIFICATION
SELECT * FROM MENU;
SELECT * FROM PLAT;
SELECT * FROM UTILISATEUR;
SELECT * FROM INGRÉDIENT;
SELECT * FROM CUISINIER;
SELECT * FROM CLIENT;
SELECT * FROM AVIS;
SELECT * FROM COMMANDE;


/*CREATE USER 'administrateur'@'127.0.0.1' IDENTIFIED BY 'adminpassword';
GRANT ALL PRIVILEGES ON BTCook.* TO 'administrateur'@'localhost' WITH GRANT OPTION;

-- Création du cuisinier avec accès en lecture/écriture sur certaines tables et lecture seule sur d'autres
CREATE USER 'cuisinier'@'localhost' IDENTIFIED BY 'cuisinierpassword';
GRANT SELECT, UPDATE ON VUE_CUISINIER_PROFIL TO 'cuisinier'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON VUE_CUISINIER_COMMANDES TO 'cuisinier'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON VUE_CUISINIER_AVIS TO 'cuisinier'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON VUE_CUISINIER_MESSAGERIE TO 'cuisinier'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON VUE_CUISINIER_BONS_PROMO TO 'cuisinier'@'localhost';
-- Accès en lecture aux plats de tous les cuisiniers
GRANT SELECT ON VUE_CUISINIER_TOUTES_PLATS TO 'cuisinier'@'localhost';
-- Accès en lecture aux ingrédients de tous les cuisiniers
GRANT SELECT ON VUE_CUISINIER_TOUTES_INGREDIENTS TO 'cuisinier'@'localhost';
-- Accès en modification uniquement sur les plats créés par le cuisinier
GRANT UPDATE, DELETE ON VUE_CUISINIER_PLATS_MODIFIABLES TO 'cuisinier'@'localhost';
-- Accès en modification uniquement sur les ingrédients créés par le cuisinier
GRANT UPDATE, DELETE ON VUE_CUISINIER_INGREDIENTS_MODIFIABLES TO 'cuisinier'@'localhost';

-- Création du client avec accès en lecture/écriture limité
CREATE USER 'client'@'localhost' IDENTIFIED BY 'clientpassword';
GRANT SELECT, UPDATE ON VUE_CLIENT_PROFIL TO 'client'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON VUE_CLIENT_COMMANDES TO 'client'@'localhost';
GRANT SELECT, INSERT, UPDATE,DELETE ON VUE_CLIENT_AVIS TO 'client'@'localhost';
GRANT SELECT, INSERT, UPDATE, DELETE ON VUE_CLIENT_MESSAGERIE TO 'client'@'localhost';
GRANT SELECT ON VUE_CLIENT_BONS_PROMO TO 'client'@'localhost';
GRANT SELECT ON BTCook.INGRÉDIENT TO 'client'@'localhost';
GRANT SELECT ON BTCook.PLAT TO 'client'@'localhost';

-- 1. VUES POUR LES CLIENTS
-- Vue des avis laissés par le client connecté
CREATE OR REPLACE VIEW VUE_CLIENT_AVIS AS
SELECT a.*
FROM AVIS a
JOIN CLIENT c ON a.matriculeClient = c.matriculeClient
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue des messages envoyés/reçus par le client connecté
CREATE OR REPLACE VIEW VUE_CLIENT_MESSAGERIE AS
SELECT m.*
FROM MESSAGERIE m
JOIN CLIENT c ON m.matriculeClient = c.matriculeClient
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue des bons promotionnels du client connecté
CREATE OR REPLACE VIEW VUE_CLIENT_BONS_PROMO AS
SELECT b.*
FROM BON_PROMOTIONNEL b
JOIN CLIENT c ON b.matriculeClient = c.matriculeClient
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue du profil du client connecté
CREATE OR REPLACE VIEW VUE_CLIENT_PROFIL AS
SELECT * FROM CLIENT c
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue des commandes du client connecté
CREATE OR REPLACE VIEW VUE_CLIENT_COMMANDES AS
SELECT co.*
FROM COMMANDE co
JOIN CLIENT c ON co.matriculeClient = c.matriculeClient
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- 2. VUES POUR LES CUISINIERS
-- Vue des avis concernant le cuisinier connecté
CREATE OR REPLACE VIEW VUE_CUISINIER_AVIS AS
SELECT a.*
FROM AVIS a
JOIN CUISINIER c ON a.matriculeCuisinier = c.matriculeCuisinier
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue des messages envoyés/reçus par le cuisinier connecté
CREATE OR REPLACE VIEW VUE_CUISINIER_MESSAGERIE AS
SELECT m.*
FROM MESSAGERIE m
JOIN CUISINIER c ON m.matriculeCuisinier = c.matriculeCuisinier
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue des commandes du cuisinier connecté
CREATE OR REPLACE VIEW VUE_CUISINIER_COMMANDES AS
SELECT co.*
FROM COMMANDE co
JOIN CUISINIER c ON co.matriculeCuisinier = c.matriculeCuisinier
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue des bons promotionnels du cuisinier connecté
CREATE OR REPLACE VIEW VUE_CUISINIER_BONS_PROMO AS
SELECT b.*
FROM BON_PROMOTIONNEL b
JOIN CLIENT c ON b.matriculeCuisinier = c.matriculeCuisinier
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Vue du profil du cuisinier connecté
CREATE OR REPLACE VIEW VUE_CUISINIER_PROFIL AS
SELECT * FROM CUISINIER c
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();

-- Les cuisiniers verront tous les plats/ingrédients mais ne pourront modifier que ceux qu'ils ont créés

-- Vue des plats de tous les cuisiniers
CREATE OR REPLACE VIEW VUE_CUISINIER_TOUTES_PLATS AS
SELECT p.*
FROM PLAT p;
-- Vue des ingrédients de tous les cuisiniers
CREATE OR REPLACE VIEW VUE_CUISINIER_TOUTES_INGREDIENTS AS
SELECT i.*
FROM INGRÉDIENT i;

-- Vue des plats modifiables uniquement par le cuisinier connecté
CREATE OR REPLACE VIEW VUE_CUISINIER_PLATS_MODIFIABLES AS
SELECT p.*
FROM PLAT p
JOIN CONTIENT3 c3 ON p.matriculePlat = c3.matriculePlat
JOIN MENU m ON c3.matriculeMenu = m.matriculeMenu
JOIN CONTIENT2 c2 ON m.matriculeMenu = c2.matriculeMenu
JOIN COMMANDE co ON c2.matriculeCommande = co.matriculeCommande
JOIN CUISINIER c ON co.matriculeCuisinier = c.matriculeCuisinier
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();  -- Limiter aux plats créés par le cuisinier connecté

-- Vue des ingrédients modifiables uniquement par le cuisinier connecté
CREATE OR REPLACE VIEW VUE_CUISINIER_INGREDIENTS_MODIFIABLES AS
SELECT i.*
FROM INGRÉDIENT i
JOIN COMPOSE co ON i.matriculeIngrédient = co.matriculeIngrédient
JOIN PLAT p ON co.matriculePlat = p.matriculePlat
JOIN CONTIENT3 c3 ON p.matriculePlat = c3.matriculePlat
JOIN MENU m ON c3.matriculeMenu = m.matriculeMenu
JOIN CONTIENT2 c2 ON m.matriculeMenu = c2.matriculeMenu
JOIN COMMANDE cmd ON c2.matriculeCommande = cmd.matriculeCommande
JOIN CUISINIER c ON cmd.matriculeCuisinier = c.matriculeCuisinier
JOIN UTILISATEUR u ON c.matriculeUtilisateur = u.matriculeUtilisateur
WHERE u.email = CURRENT_USER();  -- Limiter aux ingrédients créés par le cuisinier connecté

-- Appliquer les changements
FLUSH PRIVILEGES;