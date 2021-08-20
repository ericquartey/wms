<style>
    a:link {
      color: red;
      background-color: transparent;
      text-decoration: none;
    }
    a:visited {
      color: coral;
      background-color: transparent;
      text-decoration: none;
    }
    a:hover, a:active {
      color: red;
      background-color: transparent;
      text-decoration: underline;
    }
    body{
      background-color: #282828;
      color: #C5C7C4;
    }
</style>

# Note di versione

## Nuove Funzionalità 1.0.23 rispetto la 1.0.22
- Aggiunti dati di manutenzione alla telemetria
- Aggiunta segnalazione di database secondario non allineato nella pagina di configurazione del backup
- Aggiunti alla vista delle celle: il tipo di supporto (Inserito o Sopra) e il cassetto

## Bug Risolti
- Corretta lettura dei dati dall'inverter
- E' obbligatorio selezionare il lotto per le richieste di deposito di articoli con lotto
- Se il database secondario non esiste viene duplicato il primario
- Nascosti alcuni dettagli per l'utente Operator nella pagina di manutenzione
- La calibrazione verticale non parte più al rientro del cassetto da baia
- Aumentato lo spostamento del comando "Ricerca di Zero"

## Compatibile con adapter 0.4.23
***

[Versione 1.0.22](#id1022)

[Versione 1.0.21](#id1021)

[Versione 1.0.20](#id1020)

[Versione 1.0.19](#id1019)

[Versione 1.0.18](#id1018)

[Versione 1.0.17](#id1017)

[Versione 1.0.16](#id1016)

[Versione 1.0.15](#id1015)

[Versione 1.0.14](#id1014)

[Versione 1.0.13](#id1013)

[Versione 1.0.12](#id1012)

[Versione 1.0.11](#id1011)

[Versione 1.0.10](#id1010)

[Versione 1.0.9](#id109)

[Versione 1.0.8](#id108)

[Versione 1.0.7](#id107)

[Versione 1.0.6](#id106)

[Versione 1.0.5](#id105)

[Versione 1.0.4](#id104)

[Versione 1.0.3](#id103)

[Versione 1.0.2](#id102)

[Versione 1.0.1](#id101)

[Versione 1.0.0](#id100)
***

<a id="id1022"></a>
## Nuove Funzionalità 1.0.22 rispetto la 1.0.21
- Aggiunta possibilità di inserire le Note nella causale della Ricerca Articolo, sia da tastiera che da barcode

## Bug Risolti
- Corretta gestione del database di backup

## Compatibile con adapter 0.4.23
***

<a id="id1021"></a>
## Nuove Funzionalità 1.0.21 rispetto la 1.0.20
- Aggiunto sensore di barriera di sicurezza intermedia nella BED

## Bug Risolti
- La pagina di Ricerca Articolo azzera i dati al cambio della selezione
- I movimenti verticali dell'elevatore sono bloccati se manca la calibrazione
- Aggiunto allarme di posizione verticale in confronto con il sensore di zero elevatore
- Correzioni alla gestione delle posizioni di baia bloccate
- I movimenti manuali della baia esterna controllano le quote minime e massime

## Compatibile con adapter 0.4.23
***

<a id="id1020"></a>
## Nuove Funzionalità 1.0.20 rispetto la 1.0.19
- Aggiunta procedura di test per la BES
- Aggiunto messaggio "Maintenance" sulla telemetria quando scade una manutenzione
- Aggiunti parametri per l'avvio ritardato della compensazione verticale
- Aggiunte traduzioni in lingua spagnola

## Bug Risolti
- Corretta l'attivazione degli avvisi di manutenzioni scadute
- Eliminato un caso di blocco delle missioni senza allarme con baie contrapposte
- Il ripristino delle missioni nella giostra riporta la serranda in posizione intermedia
- La compattazione non muove i cassetti bloccati
- Eliminato un caso di mancata calibrazione verticale al riavvio della macchina
- Corretto Test Celle Limite nella BED

## Compatibile con adapter 0.4.21
***

<a id="id1019"></a>
## Nuove Funzionalità 1.0.19 rispetto la 1.0.18
- Dopo il deposito in baia esterna doppia, se c'è anche un altro cassetto in baia si porta nell'altra posizione
- Aggiunto parametro di lunghezza messaggio nella Barra alfanumerica
- Aggiunto parametro per abilitare la conferma di rientro cassetto
- Aggiunto controllo del sensore di zero in deposito su baia esterna
- Aggiunto allarme di deposito in baia senza sensore di presenza

## Bug Risolti
- Tendaggi paradiso: corretta gestione di inserimento nuove pezze
- Corretto numero di scomparto in Ricerca articolo/Cassetti
- Dopo ogni errore inverter dell'elevatore si fa la calibrazione completa
- Nella BES l'Inserimento cassetti mostra i comandi di Muovi verso Operatore / Elevatore

## Compatibile con adapter 0.4.21
***


<a id="id1018"></a>
## Nuove Funzionalità 1.0.18 rispetto la 1.0.17
- Aggiunto parametro di baia per generare automaticamente una lista di deposito con il barcode
- Aggiunto allarme di tara cassetto a zero nella procedura di test celle limite
- Aggiunta lingua polacca
- Aggiunto Step ElevatorBayUp: dopo il deposito in giostra, posizione bassa, si porta in posizione alta

## Bug Risolti
-  Corretto errore 82 - Timeout StartPositioningBlocked
-  Corretta quota verticale nel ripristino del prelievo da cella con macchine da 1000kg
-  Aggiunto controllo del sensore di zero catena nella BES nel movimento verso operatore
-  Aggiunto controllo del peso aggiornato dalle operazioni sul cassetto in baia
-  Nella pagina di versamento si può attivare cassetto "pieno" senza il "chiudi riga" 

## Compatibile con adapter 0.4.20
***

<a id="id1017"></a>
## Nuove Funzionalità 1.0.17 rispetto la 1.0.16
- Aggiunto parametro per visualizzare "Aggiungi" nella vista del cassetto in baia

## Bug Risolti
-  Corretto uso della tastiera touch sovrapposta alle griglie dati
-  Tolta la pagina finale nella chiusura della procedura di calibrazione giostra
-  Eliminato allarme di "baia occupata in altra operazione" nei ripristini con macchine da 990kg
-  Corretto errore di file bloccato durante il backup
-  La pagina delle operazioni sul cassetto in baia si apre col tasto Indietro della ricerca articolo

## Compatibile con adapter 0.4.20
***

<a id="id1016"></a>
## Nuove Funzionalità 1.0.16 rispetto la 1.0.15
- Aggiunto Allarme per baia telescopica

## Bug Risolti
- Il cassetto sconosciuto a bordo elevatore viene depositato in baia
- Eliminata la calibrazione di mezzanotte 

## Compatibile con adapter 0.4.20
***

<a id="id1015"></a>
## Nuove Funzionalità 1.0.15 rispetto la 1.0.14
- Aggiunto tempo automatico nelle statistiche
- Aggiunta calibrazione verticale dopo cicli configurabili

## Bug Risolti
- Corretti vari errori per Tendaggi Paradiso
- I comandi per la barra alfanumerica non convertono più il carattere '*'
- Il laser non memorizza l'ultimo punto
- Corretta vista errori inverter nel menu Installazione
- La baia esterna chiude la serranda dopo l'homing
- Reso non bloccante l'errore di SQLite 'readonly database' 

## Compatibile con adapter 0.4.19
***

<a id="id1014"></a>
# Note di versione

## Nuove Funzionalità 1.0.14 rispetto la 1.0.13
- Aggiunto comando "Ricerca di Zero" nell'allarme 15 "Manca il sensore di zero con elevatore vuoto"
- Aggiunti comandi Ping e Riavvia servizio nella vista delle schede di rete
- Aggiunta lingua Russa

## Bug Risolti
- Corretti vari errori sulla BED
- Corretta segnalazione di barriera attiva all'accensione della macchina
- Corretto ripristino delle missioni con doppia baia
- Corretto timeout in accensione inverter (SwitchOnStart)
- Corretto errore in salvataggio database
- Tolto Reset Macchina dal livello Operatore

## Compatibile con adapter 0.4.18
***

<a id="id1013"></a>
## Nuove Funzionalità 1.0.13 rispetto la 1.0.12
- In fase di refilling si può dare il chiudi riga e cambiare le quantità
- Le richieste di prelievo, refilling e modifica giacenza chedono a EjLog di controllare i diritti utente
- Si può attivare una lista in attesa con il barcode del numero lista
- Dalla ricerca articolo si possono creare liste di visione selezionando i cassetti
- Le operazioni di prelievo e refilling aggiornano il peso del cassetto
- Aggiunta gestione tendaggi
- Aggiunta procedura di test baia esterna

## Bug Risolti
- Riportato Devexpress a versione 19.2.4

## Compatibile con adapter 0.4.17
***

<a id="id1012"></a>

### Nuove Funzionalità 1.0.12 rispetto la 1.0.11
- Aggiunta gestione della Baia Esterna Doppia
- Aggiunta lingua greca
- Aggiunti browser negli accessori con web server

### Bug Risolti
- La vista di chiamata cassetto non perde la selezione
- Il controllo delle luci di baia funziona anche durante i movimenti

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id1011"></a>

### Nuove Funzionalità 1.0.11 rispetto la 1.0.10
- Il pulsante "?" visualizza le release notes

### Bug Risolti
- Aggiunti controlli di Sensori non validi nelle missioni di ripristino
- La baia esterna non si blocca se deve fare la calibrazione
- Ridotti tempi di cambio passo missione

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id1010"></a>

### Nuove Funzionalità 1.0.10 rispetto la 1.0.9
- nessuna

### Bug Risolti
- I messaggi della barra alfanumerica scorrono se sono più lunghi dello spazio disponibile
- La procedura di calibrazione della giostra non si blocca in caso di errore
- Corretto aggiornamento orario da WMS

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id109"></a>

### Nuove Funzionalità 1.0.9 rispetto la 1.0.8
  - nessuna

### Bug Risolti
  - Limitati messaggi spediti alla barra alfanumerica
  - Deposito in baia a bassa velocità
  - Missioni di ripristino in fase di recupero catena

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id108"></a>

### Nuove Funzionalità 1.0.8 rispetto la 1.0.7
- Aggiunto parametro OffsetLaser per cassetto: quando esiste sostituisce l'altezza articolo

### Bug Risolti
- Migliorati tempi di cambio passo missione
- Ripristino missione con BES da posizione intermedia
- Ripristino missione con portata 990kg (fuga verticale con cassetto sporgente) 
- Mancata pulizia dati giornalieri
- Eliminati alcuni casi di "Nuove operazioni disponibili"

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id107"></a>

### Nuove Funzionalità 1.0.7 rispetto la 1.0.6
- Aggiunta visualizzazione delle schede di rete nel menu Informazioni

### Bug Risolti
- La compattazione sfrutta lo spazio delle celle più in alto
- Il lettore barcode gestisce anche il carattere "_" (underscore)
- Il testo Note delle causali si può leggere con il barcode
- La procedura di calibrazione del peso accetta solo 3 pesate differenti
- Nelle macchine da 1000kg i movimenti guidati non perdono più la posizione logica 

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id106"></a>

### Nuove Funzionalità 1.0.6 rispetto la 1.0.5
- nessuna

### Bug Risolti
- La vista Aggiornamento software - Installazione aggiorna i pacchetti
- Le impostazioni di database backup sono editabili anche a macchina spenta
- La vista di Chiamata cassetto non ha più la ricerca cassetto ma ha gli ordinamenti
- Correzione alle missioni di ripristino automatico
- Miglioramento allo scambio dati con laser e barra alfanumerica
- Eliminato menù contestuale dalle liste a scorrimento

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id105"></a>

### Nuove Funzionalità 1.0.5 rispetto la 1.0.4
- nessuna

### Bug Risolti
- Corretti movimenti guidati da cella 14 a baia bassa
- Installer: corretto aggiornamento della versione
- Aggiunto nuovo salvataggio dell'ultima posizione orizzontale
- Laser e barra non bloccano la pagina di login e non si spengono con il socketLink
- Aggiunta segnalazione di macchina in movimento nelle pagine di Celle e Cassetti

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id104"></a>

### Nuove Funzionalità 1.0.4 rispetto la 1.0.3
- Aggiunti i sensori della Baia Esterna Doppia


### Bug Risolti
- Aggiunta compattazione del database locale della telemetria
- Il laser e la barra alfanumerica non chiudono sempre la connessione
- Corretto un errore di aggiornamento dei parametri dei profili orizzontali

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id103"></a>

### Nuove Funzionalità 1.0.3 rispetto la 1.0.2
- Spostato database della telemetria sul disco E:
- La bilancia contapezzi visualizza anche i pezzi e può essere condivisa fra più baie
- Il test completo può occupare celle casuali del magazzino
- L'app del Panel PC attiva il servizio del MAS


### Bug Risolti
- Nella ricerca articolo la quantità da prelevare è limitata alla quantità presente
- La procedura di controllo baia non muove l'elevatore se la serranda è aperta
- Correzioni varie nelle procedure di ripristino
- La calibrazione completa dell'elevatore muove anche la catena
- La compattazione non può partire con un cassetto a bordo elevatore

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id102"></a>

### Nuove Funzionalità 1.0.2 rispetto la 1.0.1
- Aggiunto salvataggio del database sul pc di Ejlog
- Aggiunto parametro di accelerazione homing Asse verticale
- Spostati parametri di velocità Serranda

### Bug Risolti
- Se la missione associata al cassetto in baia è abortita il cassetto rientra automaticamente
- Completato il controllo del tipo di serranda in caso di serranda non esistente
- Ripristini automatici: corretto salvataggio dell'ultima posizione corretta
- Nei movimenti combinati si aggiornano contemporaneamente sia la posizione verticale che quella orizzontale
- La procedura di calibrazione orizzontale catena fa anche la taratura finale
- Aggiunte varie segnalazioni di errori più dettagliati al posto di "Errore inverter 1000"
- Corrette traduzioni nel menu Accessori

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id101"></a>

### Nuove Funzionalità 1.0.1 rispetto la 1.0.0
- Nessuna 

### Bug Risolti
- Menu movimenti: corretta visualizzazione del pulsante di taratura baia esterna
- Non è necessario riavviare il ppc per avere la nuova tastiera al cambio di lingua
- Menu operatore - Operazioni su cassetto - Corrette traduzioni
- Ripristini automatici: se la posizione della catena orizzontale non è valida si chiude la missione

### Compatibile con adapter 0.4.15 e 0.4.16
***

<a id="id100"></a>

### Nuove Funzionalità 1.0.0 rispetto la 0.28.39
- Gestione parametri inverter: completata lettura e scrittura per l'inverter principale 

### Bug Risolti
- Corretta gestione delle barriere di sicurezza nella Baia 2
- Corretto trasferimento baia-baia
- Corretti vari errori nelle procedure di installazione

### Compatibile con adapter 0.4.15 e 0.4.16
***

