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

## Nuove Funzionalità 1.0.11 rispetto la 1.0.10
- Il pulsante "?" visualizza le release notes

## Bug Risolti
- Aggiunti controlli di Sensori non validi nelle missioni di ripristino
- La baia esterna non si blocca se deve fare la calibrazione
- Ridotti tempi di cambio passo missione

## Compatibile con adapter 0.4.15 e 0.4.16
***

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

