using client_contact_management.Entities;
using Microsoft.EntityFrameworkCore;

namespace client_contact_management.Data
{
    public class ClientContactManagementDbContext : DbContext
    {
        public ClientContactManagementDbContext(DbContextOptions<ClientContactManagementDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ClientContact> ClientContacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().HasKey(c => c.Id);
            modelBuilder.Entity<Contact>().HasKey(c => c.Id);

            modelBuilder.Entity<ClientContact>()
                .HasKey(cc => new { cc.ClientId, cc.ContactId });

            modelBuilder.Entity<ClientContact>()
                .HasOne(cc => cc.Client)
                .WithMany(c => c.ClientContacts)
                .HasForeignKey(cc => cc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClientContact>()
                .HasOne(cc => cc.Contact)
                .WithMany(c => c.ClientContacts)
                .HasForeignKey(cc => cc.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.ClientCode)
                .IsUnique();

            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.ClientCode)
                .IsUnique()
                .HasDatabaseName("IX_Clients_ClientCode");

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Name)
                .HasDatabaseName("IX_Clients_Name");

            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Contacts_Email");

            modelBuilder.Entity<Contact>()
                .HasIndex(c => new { c.Surname, c.Name })
                .HasDatabaseName("IX_Contacts_Surname_Name");

            modelBuilder.Entity<ClientContact>()
                .HasIndex(cc => new { cc.ContactId, cc.ClientId })
                .HasDatabaseName("IX_ClientContacts_ContactId_ClientId");
        }
    }
}
