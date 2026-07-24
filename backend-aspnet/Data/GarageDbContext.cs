using Microsoft.EntityFrameworkCore;
using GarageApi.Models;

namespace GarageApi.Data;

public class GarageDbContext : DbContext
{
    public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options) { }

    public DbSet<Vehicule> Vehicules => Set<Vehicule>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<Consommation> Consommations => Set<Consommation>();
    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<DemandeVehicule> DemandesVehicule => Set<DemandeVehicule>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ville> Villes => Set<Ville>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicule>(entity =>
        {
            entity.ToTable("vehicules");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Immatriculation).IsUnique();
            entity.Property(e => e.Immatriculation).HasColumnName("immatriculation").IsRequired();
            entity.Property(e => e.Marque).HasColumnName("marque").IsRequired();
            entity.Property(e => e.Modele).HasColumnName("modele").IsRequired();
            entity.Property(e => e.TypeVehicule).HasColumnName("type_vehicule").IsRequired();
            entity.Property(e => e.DateAcquisition).HasColumnName("date_acquisition");
            entity.Property(e => e.Kilometrage).HasColumnName("kilometrage").HasDefaultValue(0);
            entity.Property(e => e.ServiceAffecte).HasColumnName("service_affecte");
            entity.Property(e => e.Statut).HasColumnName("statut").HasDefaultValue("Disponible");
            entity.Property(e => e.DateCreation).HasColumnName("date_creation");
        });

        modelBuilder.Entity<Mission>(entity =>
        {
            entity.ToTable("missions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VehiculeId).HasColumnName("vehicule_id");
            entity.Property(e => e.Chauffeur).HasColumnName("chauffeur").IsRequired();
            entity.Property(e => e.Destination).HasColumnName("destination").IsRequired();
            entity.Property(e => e.Motif).HasColumnName("motif");
            entity.Property(e => e.DateDepart).HasColumnName("date_depart").IsRequired();
            entity.Property(e => e.DateRetour).HasColumnName("date_retour");
            entity.Property(e => e.KmDepart).HasColumnName("km_depart");
            entity.Property(e => e.KmRetour).HasColumnName("km_retour");
            entity.Property(e => e.Statut).HasColumnName("statut").HasDefaultValue("Planifiée");
            entity.Property(e => e.DateCreation).HasColumnName("date_creation");
            entity.HasOne(e => e.Vehicule).WithMany(v => v.Missions).HasForeignKey(e => e.VehiculeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.VehiculeId).HasDatabaseName("idx_missions_vehicule");
        });

        modelBuilder.Entity<Consommation>(entity =>
        {
            entity.ToTable("consommations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VehiculeId).HasColumnName("vehicule_id");
            entity.Property(e => e.TypeConso).HasColumnName("type_conso").IsRequired();
            entity.Property(e => e.DateConso).HasColumnName("date_conso").IsRequired();
            entity.Property(e => e.Quantite).HasColumnName("quantite").IsRequired();
            entity.Property(e => e.Unite).HasColumnName("unite").HasDefaultValue("L");
            entity.Property(e => e.CoutUnitaire).HasColumnName("cout_unitaire").HasDefaultValue(0.0);
            entity.Property(e => e.Kilometrage).HasColumnName("kilometrage");
            entity.Property(e => e.Fournisseur).HasColumnName("fournisseur");
            entity.Property(e => e.DateCreation).HasColumnName("date_creation");
            entity.HasOne(e => e.Vehicule).WithMany(v => v.Consommations).HasForeignKey(e => e.VehiculeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.VehiculeId).HasDatabaseName("idx_conso_vehicule");
        });

        modelBuilder.Entity<Intervention>(entity =>
        {
            entity.ToTable("interventions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VehiculeId).HasColumnName("vehicule_id");
            entity.Property(e => e.TypeIntervention).HasColumnName("type_intervention").IsRequired();
            entity.Property(e => e.DateIntervention).HasColumnName("date_intervention").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Prestataire).HasColumnName("prestataire");
            entity.Property(e => e.Cout).HasColumnName("cout").HasDefaultValue(0.0);
            entity.Property(e => e.Statut).HasColumnName("statut").HasDefaultValue("Planifiée");
            entity.Property(e => e.DateProchaineEcheance).HasColumnName("date_prochaine_echeance");
            entity.Property(e => e.DateCreation).HasColumnName("date_creation");
            entity.HasOne(e => e.Vehicule).WithMany(v => v.Interventions).HasForeignKey(e => e.VehiculeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.VehiculeId).HasDatabaseName("idx_interv_vehicule");
        });

        modelBuilder.Entity<DemandeVehicule>(entity =>
        {
            entity.ToTable("demandes_vehicule");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EmployeNom).HasColumnName("employe_nom").IsRequired();
            entity.Property(e => e.Service).HasColumnName("service").IsRequired();
            entity.Property(e => e.VehiculeId).HasColumnName("vehicule_id");
            entity.Property(e => e.Destination).HasColumnName("destination").IsRequired();
            entity.Property(e => e.Motif).HasColumnName("motif");
            entity.Property(e => e.DateDemande).HasColumnName("date_demande");
            entity.Property(e => e.DateDepart).HasColumnName("date_depart").IsRequired();
            entity.Property(e => e.DateRetourPrevu).HasColumnName("date_retour_prevu");
            entity.Property(e => e.Statut).HasColumnName("statut").HasDefaultValue("En attente");
            entity.Property(e => e.ChauffeurTraitant).HasColumnName("chauffeur_traitant");
            entity.Property(e => e.MissionId).HasColumnName("mission_id");
            entity.Property(e => e.DateTraitement).HasColumnName("date_traitement");
            entity.Property(e => e.CommentaireTraitement).HasColumnName("commentaire_traitement");
            entity.Property(e => e.DateCreation).HasColumnName("date_creation");
            entity.HasOne(e => e.Vehicule).WithMany(v => v.DemandesVehicule).HasForeignKey(e => e.VehiculeId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.VehiculeId).HasDatabaseName("idx_demandes_vehicule");
        });

        modelBuilder.Entity<Ville>(entity =>
        {
            entity.ToTable("villes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nom).HasColumnName("nom").IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").IsRequired();
            entity.Property(e => e.NomComplet).HasColumnName("nom_complet").IsRequired();
            entity.Property(e => e.Service).HasColumnName("service");
            entity.Property(e => e.Actif).HasColumnName("actif").HasDefaultValue(1);
            entity.Property(e => e.DateCreation).HasColumnName("date_creation");
        });
    }
}
