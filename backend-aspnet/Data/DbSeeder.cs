using GarageApi.Models;

namespace GarageApi.Data;

public static class DbSeeder
{
    public static void Seed(GarageDbContext db)
    {
        if (db.Users.Any()) return;

        var villes = new[]
        {
            new Ville { Nom = "Casablanca" },
            new Ville { Nom = "Boucraa" },
            new Ville { Nom = "Laayoune" },
            new Ville { Nom = "Laayoune Port" },
            new Ville { Nom = "Khouribga" },
            new Ville { Nom = "Benguerir" },
            new Ville { Nom = "Youssoufia" },
            new Ville { Nom = "Safi" },
            new Ville { Nom = "Jorf Lasfar" },
        };
        db.Villes.AddRange(villes);

        var users = new[]
        {
            new User { Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "admin", NomComplet = "Rachid Ouazzani", Service = "Direction", Actif = 1, DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new User { Username = "user", Password = BCrypt.Net.BCrypt.HashPassword("user123"), Role = "user", NomComplet = "Ahmed El Mansouri", Service = "Exploitation", Actif = 1, DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new User { Username = "chauffeur", Password = BCrypt.Net.BCrypt.HashPassword("chauffeur123"), Role = "chauffeur", NomComplet = "Youssef Bensalem", Service = "Transport", Actif = 1, DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new User { Username = "visiteur", Password = BCrypt.Net.BCrypt.HashPassword("visiteur123"), Role = "", NomComplet = "Ali Benali", Service = "Visiteurs", Actif = 1, DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
        };
        db.Users.AddRange(users);

        var vehicules = new[]
        {
            new Vehicule { Immatriculation = "12345-A-28", Marque = "Toyota", Modele = "Hilux", TypeVehicule = "Utilitaire", DateAcquisition = "2022-03-14", Kilometrage = 48200, ServiceAffecte = "Exploitation", Statut = "Disponible", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Vehicule { Immatriculation = "67890-A-28", Marque = "Toyota", Modele = "Land Cruiser", TypeVehicule = "Léger", DateAcquisition = "2021-06-01", Kilometrage = 71300, ServiceAffecte = "Direction", Statut = "En mission", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Vehicule { Immatriculation = "11223-A-28", Marque = "Mercedes", Modele = "Actros", TypeVehicule = "Poids lourd", DateAcquisition = "2019-11-20", Kilometrage = 152400, ServiceAffecte = "Logistique", Statut = "Disponible", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Vehicule { Immatriculation = "44556-A-28", Marque = "Renault", Modele = "Kangoo", TypeVehicule = "Utilitaire", DateAcquisition = "2023-01-09", Kilometrage = 21800, ServiceAffecte = "Maintenance", Statut = "En réparation", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Vehicule { Immatriculation = "77889-A-28", Marque = "Caterpillar", Modele = "950 GC", TypeVehicule = "Engin", DateAcquisition = "2018-05-15", Kilometrage = 9800, ServiceAffecte = "Extraction", Statut = "Disponible", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Vehicule { Immatriculation = "99001-A-28", Marque = "Dacia", Modele = "Duster", TypeVehicule = "Léger", DateAcquisition = "2022-09-30", Kilometrage = 33400, ServiceAffecte = "Sécurité", Statut = "Hors service", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Vehicule { Immatriculation = "55677-A-28", Marque = "Volvo", Modele = "FH16", TypeVehicule = "Poids lourd", DateAcquisition = "2020-02-18", Kilometrage = 98700, ServiceAffecte = "Logistique", Statut = "Disponible", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
        };
        db.Vehicules.AddRange(vehicules);
        db.SaveChanges();

        var missions = new[]
        {
            new Mission { VehiculeId = vehicules[0].Id, Chauffeur = "Karim Ait Ali", Destination = "Laâyoune", Motif = "Transport de pièces détachées", DateDepart = "2026-07-02", DateRetour = "2026-07-02", KmDepart = 48000, KmRetour = 48180, Statut = "Terminée", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mission { VehiculeId = vehicules[1].Id, Chauffeur = "Youssef Bensalem", Destination = "Dakhla - Port", Motif = "Réunion de direction", DateDepart = "2026-07-16", DateRetour = null, KmDepart = 71300, KmRetour = null, Statut = "En cours", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mission { VehiculeId = vehicules[2].Id, Chauffeur = "Hassan Idrissi", Destination = "Boujdour", Motif = "Livraison de matériel", DateDepart = "2026-07-10", DateRetour = "2026-07-11", KmDepart = 151900, KmRetour = 152400, Statut = "Terminée", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mission { VehiculeId = vehicules[4].Id, Chauffeur = "Omar Chafik", Destination = "Site d'extraction Nord", Motif = "Terrassement", DateDepart = "2026-07-15", DateRetour = null, KmDepart = 9750, KmRetour = null, Statut = "En cours", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mission { VehiculeId = vehicules[6].Id, Chauffeur = "Rachid Ouazzani", Destination = "Tan-Tan", Motif = "Transport de marchandises", DateDepart = "2026-06-28", DateRetour = "2026-06-30", KmDepart = 98200, KmRetour = 98700, Statut = "Terminée", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Mission { VehiculeId = vehicules[0].Id, Chauffeur = "Karim Ait Ali", Destination = "Laâyoune", Motif = "Approvisionnement atelier", DateDepart = "2026-07-20", DateRetour = null, KmDepart = 48200, KmRetour = null, Statut = "Planifiée", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
        };
        db.Missions.AddRange(missions);

        var consommations = new[]
        {
            new Consommation { VehiculeId = vehicules[0].Id, TypeConso = "Carburant", DateConso = "2026-07-01", Quantite = 60, Unite = "L", CoutUnitaire = 11.50, Kilometrage = 48000, Fournisseur = "Station Afriquia Dakhla", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Consommation { VehiculeId = vehicules[1].Id, TypeConso = "Carburant", DateConso = "2026-07-15", Quantite = 80, Unite = "L", CoutUnitaire = 11.50, Kilometrage = 71250, Fournisseur = "Station Vivo Energy", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Consommation { VehiculeId = vehicules[2].Id, TypeConso = "Carburant", DateConso = "2026-07-09", Quantite = 220, Unite = "L", CoutUnitaire = 10.90, Kilometrage = 151800, Fournisseur = "Station Afriquia Dakhla", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Consommation { VehiculeId = vehicules[2].Id, TypeConso = "Lubrifiant", DateConso = "2026-07-09", Quantite = 15, Unite = "L", CoutUnitaire = 65.00, Kilometrage = 151800, Fournisseur = "Garage OCP - Magasin", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Consommation { VehiculeId = vehicules[4].Id, TypeConso = "Carburant", DateConso = "2026-07-14", Quantite = 150, Unite = "L", CoutUnitaire = 10.90, Kilometrage = 9700, Fournisseur = "Citerne mobile chantier", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Consommation { VehiculeId = vehicules[6].Id, TypeConso = "Carburant", DateConso = "2026-06-27", Quantite = 240, Unite = "L", CoutUnitaire = 10.90, Kilometrage = 98100, Fournisseur = "Station Afriquia Dakhla", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Consommation { VehiculeId = vehicules[3].Id, TypeConso = "Lubrifiant", DateConso = "2026-06-20", Quantite = 8, Unite = "L", CoutUnitaire = 65.00, Kilometrage = 21700, Fournisseur = "Garage OCP - Magasin", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Consommation { VehiculeId = vehicules[1].Id, TypeConso = "Lubrifiant", DateConso = "2026-06-05", Quantite = 6, Unite = "L", CoutUnitaire = 70.00, Kilometrage = 70500, Fournisseur = "Garage OCP - Magasin", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
        };
        db.Consommations.AddRange(consommations);

        var interventions = new[]
        {
            new Intervention { VehiculeId = vehicules[3].Id, TypeIntervention = "Réparation", DateIntervention = "2026-07-12", Description = "Remplacement du système de freinage avant", Prestataire = "Garage OCP - Atelier central", Cout = 1850.00, Statut = "En cours", DateProchaineEcheance = null, DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Intervention { VehiculeId = vehicules[5].Id, TypeIntervention = "Réparation", DateIntervention = "2026-06-18", Description = "Panne moteur - diagnostic électronique", Prestataire = "Prestataire externe AutoTech", Cout = 4200.00, Statut = "Terminée", DateProchaineEcheance = null, DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Intervention { VehiculeId = vehicules[2].Id, TypeIntervention = "Visite technique", DateIntervention = "2026-05-02", Description = "Contrôle technique annuel", Prestataire = "Centre Dekra Laâyoune", Cout = 450.00, Statut = "Terminée", DateProchaineEcheance = "2027-05-02", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Intervention { VehiculeId = vehicules[0].Id, TypeIntervention = "Vidange", DateIntervention = "2026-06-15", Description = "Vidange moteur + filtres", Prestataire = "Garage OCP - Atelier central", Cout = 380.00, Statut = "Terminée", DateProchaineEcheance = "2026-09-15", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Intervention { VehiculeId = vehicules[4].Id, TypeIntervention = "Entretien préventif", DateIntervention = "2026-07-01", Description = "Entretien 500h - engin de chantier", Prestataire = "Garage OCP - Atelier central", Cout = 2100.00, Statut = "Terminée", DateProchaineEcheance = "2026-10-01", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Intervention { VehiculeId = vehicules[6].Id, TypeIntervention = "Visite technique", DateIntervention = "2026-04-20", Description = "Contrôle technique annuel", Prestataire = "Centre Dekra Dakhla", Cout = 500.00, Statut = "Terminée", DateProchaineEcheance = "2027-04-20", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new Intervention { VehiculeId = vehicules[1].Id, TypeIntervention = "Pneumatiques", DateIntervention = "2026-06-01", Description = "Remplacement 4 pneus", Prestataire = "Garage OCP - Atelier central", Cout = 6400.00, Statut = "Terminée", DateProchaineEcheance = null, DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
        };
        db.Interventions.AddRange(interventions);

        var demandes = new[]
        {
            new DemandeVehicule { UserId = users[2].Id, EmployeNom = "Ahmed El Mansouri", Service = "Exploitation", VehiculeId = vehicules[0].Id, Destination = "Mine de Bou Craa", Motif = "Transport de pièces de rechange", DateDemande = "2026-07-14", DateDepart = "2026-07-17", DateRetourPrevu = "2026-07-17", Statut = "Approuvée", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new DemandeVehicule { UserId = null, EmployeNom = "Fatima Zahra El Idrissi", Service = "Logistique", VehiculeId = null, Destination = "Laâyoune", Motif = "Livraison de matériel administratif", DateDemande = "2026-07-15", DateDepart = "2026-07-18", DateRetourPrevu = "2026-07-18", Statut = "En attente", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new DemandeVehicule { UserId = null, EmployeNom = "Mohamed Oubarka", Service = "Maintenance", VehiculeId = vehicules[3].Id, Destination = "Atelier central", Motif = "Réparation urgente sur site", DateDemande = "2026-07-16", DateDepart = "2026-07-19", DateRetourPrevu = "2026-07-20", Statut = "En attente", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new DemandeVehicule { UserId = null, EmployeNom = "Salma Bennis", Service = "Direction", VehiculeId = vehicules[1].Id, Destination = "Dakhla - Port", Motif = "Inspection des installations portuaires", DateDemande = "2026-07-13", DateDepart = "2026-07-21", DateRetourPrevu = "2026-07-22", Statut = "En attente", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            new DemandeVehicule { UserId = null, EmployeNom = "Hicham Lahlou", Service = "Sécurité", VehiculeId = vehicules[5].Id, Destination = "Périmètre Nord", Motif = "Patrouille de contrôle", DateDemande = "2026-07-10", DateDepart = "2026-07-12", DateRetourPrevu = "2026-07-12", Statut = "Refusée", DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
        };
        db.DemandesVehicule.AddRange(demandes);

        db.SaveChanges();
    }
}
