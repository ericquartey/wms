using Ferretto.VW.MAS.DataModels.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    public class ErrorDefinitionConfiguration : IEntityTypeConfiguration<ErrorDefinition>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ErrorDefinition> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Code);

            builder.Property(m => m.Description)
                .IsRequired();

            builder
                .HasOne(e => e.Statistics)
                .WithOne(s => s.Error)
                .HasForeignKey<ErrorStatistic>(s => s.Code)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasData(new ErrorDefinition { Code = 1001, Description = "Errore database", Severity = 5 },
                            new ErrorDefinition { Code = 1002, Description = "Errore caricamento configurazione", Severity = 5 },
                            new ErrorDefinition { Code = 1003, Description = "Errore inizializzazione dati", Severity = 5 },
                            new ErrorDefinition { Code = 1004, Description = "Errore salvataggio dati", Severity = 5 },
                            new ErrorDefinition { Code = 1005, Description = "Errore rientro cassetto", Severity = 5 },
                            new ErrorDefinition { Code = 1006, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1007, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1008, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1009, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1010, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1011, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1012, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1013, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1014, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1015, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1016, Description = "Errore rientro baia", Severity = 5 },
                            new ErrorDefinition { Code = 1017, Description = "Errore posizionamento", Severity = 5 });
        }

        #endregion
    }
}
