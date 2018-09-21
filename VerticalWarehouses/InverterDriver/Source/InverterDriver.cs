using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Threading;


using Ferretto.VW.Utils;


namespace Ferretto.VW.InverterDriver
{
    public class CInverterDriver : IDriver
    {

        private readonly Socket m_sck;


        // Definisco il Thread
        private Thread m_scheduler;

        // Definisco gli Handle
        private AutoResetEvent m_hevComand;
        private AutoResetEvent m_hevComand2;
        private AutoResetEvent m_hevTerminate;

        private CMyClass m_myClass;

        public CInverterDriver()
        {
            // TODO
            // Inizializzo gli Handle
            this.m_hevComand = new AutoResetEvent(false);
            this.m_hevComand2 = new AutoResetEvent(false);
            this.m_hevTerminate = new AutoResetEvent(false);

            // Inizializzo ed avvio il Socket
            this.m_scheduler = new Thread(this.mainThreadFunc);
            this.m_scheduler.Priority = ThreadPriority.Normal;
            this.m_scheduler.Start();


            // altro codice ...
        }


        public void EseguiComandoMovimentoElevatore()
        {
            if (this.m_hevComand != null)
            {
                this.m_hevComand.Set();
            }
        }


        // Quando termino devo chiudere gli eventi ed eliminare il Thread
        public void destroy()
        {
            this.m_hevTerminate.Set();

            if (null != this.m_hevComand)
            {
                this.m_hevComand.Close();
            }
            this.m_hevComand = null;

            if (null != this.m_hevComand2)
            {
                this.m_hevComand2.Close();
            }
            this.m_hevComand2 = null;

            if (null != this.m_hevTerminate)
            {
                this.m_hevTerminate.Close();
            }
            this.m_hevTerminate = null;

            if (this.m_scheduler != null)
            {
                this.m_scheduler.Abort();
            }

            this.m_scheduler = null;
        }


        private void mainThreadFunc()
        {
            // Definosco l'array di handle e li assegno agli eventi.
            WaitHandle[] handles = new WaitHandle[3];
            handles[0] = this.m_hevComand;
            handles[1] = this.m_hevComand2;
            handles[2] = this.m_hevTerminate;

            // Creo delle costanti da usare nello switch
            const int EV_RECOGNIZED = 0;
            const int EV_RECOGNIZED2 = 1;
            const int TERMINATED = 2;

            bool bExit = false;
            while(!bExit)
            {
                // Attendo che vi sia una modifica da parte di un Handle, si esce dal WaitAny ogni 25 msec, oppure se un Handle passo nello stato di Segnalato
                int result = WaitHandle.WaitAny(handles, 25);
                switch (result)
                {
                    case EV_RECOGNIZED:
                    {
                        // eseguo il codice perchè l'evento m_hevComand è stato segnalato
                        // manda il comnando all'inverter via socket
                        break;
                    }
                    case EV_RECOGNIZED2:
                    {
                        // eseguo il codice perchè l'evento m_hevComand2 è stato segnalato
                        break;
                    }
                    case TERMINATED:
                    {
                        bExit = true;
                        break;
                    }
                    // Quando sono passati i 25 msec, e nessun evento è stato sollevato.
                    case WaitHandle.WaitTimeout:
                    {
                        // esegue il codice
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="szInitialMsg"></param>
        /// <returns></returns>
        public int Initialize(string szInitialMsg)
        {
            this.m_myClass = new CMyClass();

            // TODO Add your implementation code here

            this.m_myClass = null;

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MyFunc()
        {
          return true;
        }

    }
}
