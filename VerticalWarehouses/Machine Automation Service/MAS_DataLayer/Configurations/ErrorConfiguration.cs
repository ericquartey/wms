using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS_DataLayer.Configurations
{
    public class ErrorConfiguration : IEntityTypeConfiguration<Error>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Error> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Code);

            builder.Property(m => m.Description)
                .IsRequired();

            builder.HasData(new Error { Code = 1001, Description = "Errore database", Issue = 5 },
                            new Error { Code = 1002, Description = "Errore caricamento configurazione", Issue = 5 },
                            new Error { Code = 1003, Description = "Errore inizializzazione dati", Issue = 5 },
                            new Error { Code = 1004, Description = "Errore salvataggio dati", Issue = 5 },
                            new Error { Code = 1005, Description = "Errore rientro cassetto", Issue = 5 },
                            new Error { Code = 1006, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1007, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1008, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1009, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1010, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1011, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1012, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1013, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1014, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1015, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1016, Description = "Errore rientro baia", Issue = 5 },
                            new Error { Code = 1017, Description = "Errore posizionamento", Issue = 5 });
        }

        #endregion
    }
}
