using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.TelemetryService.Models;
using Realms;

namespace Ferretto.VW.TelemetryService.Provider
{
    internal sealed class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly Realm realm;

        private Machine? machine;

        #endregion

        #region Constructors

        public MachineProvider(Realm realm)
        {
            this.realm = realm;
        }

        #endregion

        #region Methods

        public async Task<Machine> GetAsync()
        {
            if (this.machine is null)
            {
                this.machine = this.realm.All<Machine>().SingleOrDefault();
            }
            return this.machine;
        }

        public async Task SaveAsync(Machine machine)
        {
            await this.realm.WriteAsync((r) => r.Add(machine));
        }

        #endregion
    }
}
