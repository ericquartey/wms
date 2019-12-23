using System;
using System.IO;
using System.Linq;
using Ferretto.VW.Devices.BarcodeReader;
using Newtonsoft.Json;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal class BarcodeReaderService : IBarcodeReaderService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IBarcodeConfigurationOptions options;

        private readonly IBarcodeReader reader;

        private readonly BarcodeRuleSet ruleSet;

        #endregion

        #region Constructors

        public BarcodeReaderService(
            IEventAggregator eventAggregator,
            IBarcodeReader reader,
            IBarcodeConfigurationOptions options)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.options = options;
            this.reader = reader;

            this.reader.BarcodeReceived += this.OnBarcodeReceived;

            var json = File.ReadAllText("barcode-ruleset.json");
            this.ruleSet = JsonConvert.DeserializeObject<BarcodeRuleSet>(json);
        }

        #endregion

        #region Methods

        public void Disable()
        {
            this.reader.Disconnect();
        }

        public void Enable()
        {
            this.reader.Connect(this.options);
        }

        private void OnBarcodeReceived(object sender, BarcodeEventArgs e)
        {
            // 1. select current context
            var activeContext = this.ruleSet.Contexts.First(); // TODO

            // 2. stop on first rule match
            var rule = activeContext.Match(e.Barcode);

            if (rule is null)
            {
                System.Diagnostics.Debug.WriteLine($"Barcode {e.Barcode} does not match any rule.'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Barcode {e.Barcode} matched rule: '{rule}'");

                // 3. publish event
                this.eventAggregator
                    .GetEvent<PubSubEvent<BarcodeEventArgs>>()
                    .Publish(e);
            }
        }

        #endregion
    }
}
