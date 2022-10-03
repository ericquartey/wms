mark<style>
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

    body {
        background-color: #282828;
        color: #C5C7C4;
    }
</style>

# Note di versione

## Nuove Funzionalità 1.0.52 rispetto la 1.0.51
- Test completo: con un solo cassetto l'opzione Random cambia anche la posizione in baia

## Bug Risolti
- Viste di prelievo e versamento: corretto aggiornamento della barra led
- Aggiungi articolo: corretta mancanza di tastiera a video
- Liste in attesa: corretta dimensione della descrizione
- Eliminati rallentamenti evidenziati nella versione precedente
- Le celle bloccate sono evidenziate in rosso

## Compatibile con adapter 0.4.41
***

[Versione 1.0.51](#id1051)

[Versione 1.0.50](#id1050)

[Versione 1.0.49](#id1049)

[Versione 1.0.48](#id1048)

[Versione 1.0.47](#id1047)

[Versione 1.0.46](#id1046)

[Versione 1.0.45](#id1045)

[Versione 1.0.44](#id1044)

[Versione 1.0.43](#id1043)

[Versione 1.0.42](#id1042)

[Versione 1.0.41](#id1041)

[Versione 1.0.40](#id1040)

[Versione 1.0.39](#id1039)

[Versione 1.0.38](#id1038)

[Versione 1.0.37](#id1037)

[Versione 1.0.36](#id1036)

[Versione 1.0.35](#id1035)

[Versione 1.0.34](#id1034)

[Versione 1.0.33](#id1033)

[Versione 1.0.32](#id1032)

[Versione 1.0.31](#id1031)

[Versione 1.0.30](#id1030)

[Versione 1.0.29](#id1029)

[Versione 1.0.28](#id1028)

[Versione 1.0.27](#id1027)

[Versione 1.0.26](#id1026)

[Versione 1.0.25](#id1025)

[Versione 1.0.24](#id1024)

[Versione 1.0.23](#id1023)

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

<a id="id1051"></a>
## Nuove Funzionalità 1.0.51 rispetto la 1.0.50
- Il servizio MAS usa ora il netcore 3.1
- Barra Alfanumerica - aggiunto Messaggio GET e messaggi configurabili

## Bug Risolti
- Gli utenti di WMS possono avere password vuote
- Pagina di prelievo - corretto aggiornamento della quantità da prelevare
- Card dei sensori - non serve cambiare pagina per cambiare tema chiaro/scuro
- Gli accessori si possono modificare prima di accendere la macchina
- Serranda UpperHalf - corretto ciclo di sollevamento giostra
- Chiamata cassetto - non perde più la selezione
- BID - corretta gestione cassetti alti

## Compatibile con adapter 0.4.40

<a id="id1050"></a>
## Nuove Funzionalità 1.0.50 rispetto la 1.0.49
- Aggiunta classe di rotazione ABC
- Aggiunta gestione lettore badge senza WMS
- Aggiunto utente Guest
- Ricerca articoli: aggiunta visualizzazione della data di scadenza

## Bug Risolti
- Ricerca articoli: corretta la quantità nelle altre macchine
- Gestione operatori: corretto il cambio della password di Operator
- Gestione errori altezza e peso: eliminata la conferma dei numeri di cassetto
- Login: corretto livello degli utenti di wms con nomi uguali ai locali
- Gestione celle: le celle bloccate non risultano libere
- Controllo quote celle: aggiunto Correggi sostegno per cambiare coppie di quote

## Compatibile con adapter 0.4.40

<a id="id1049"></a>
## Nuove Funzionalità 1.0.49 rispetto la 1.0.48
- Aggiunta dimensione XXS per la barra alfanumerica

## Bug Risolti
- Telemetria: corretto invio dei dati memorizzati al ripristino del collegamento con il cloud
- Spostamento da cella a cella: aggiunto controllo di serranda chiusa
- L'Installer può cambiare i parametri dei cicli di calibrazione e della tastiera touch
- Errore di centraggio: corretta compensazione in caso di effetto elastico della catena
- Macchine con più Baie: i cassetti in attesa su una Baia non bloccano le missioni sull'altra Baia

## Compatibile con adapter 0.4.38


<a id="id1048"></a>
## Nuove Funzionalità 1.0.48 rispetto la 1.0.47
- Ferretto Nuget Package 0.16.46
- Operazioni di visione e versamento: aggiunto comando (icona carrello) per aggiungere un articolo scegliendo la lista di versamento
- Accessori - Stampante: aggiunto comando di Prova Stampante

## Bug Risolti
- Corretto falso allarme 32 - Destinazione maggiore del limite superiore - con macchine da 1000kg
- BIG: il movimento si ferma se si perde il sensore di zero
- Correzione a Mostra l'immagine del barcode: si aggiorna ad ogni cambio di articolo
- Ricerca articolo - corretta selezione con il lettore barcode in caso di prelievi ripetuti
- Conferma ultima operazione: invia la conferma a cassetto fermo
- BES - Corretto falso allarme 14 (sensori di presenza) nei ripristini

## Compatibile con adapter 0.4.38


<a id="id1047"></a>
## Nuove Funzionalità 1.0.47 rispetto la 1.0.46
- Lista articoli: aggiunto ordinamento delle colonne
- 

## Bug Risolti
- L'utente Installer può modificare Offset catena e Numero macchina
- La descrizione dei cassetti da WMS si aggiorna nella Chiamata cassetto
- Corretto un caso di missione duplicata di rientro cassetto
- Movimenti: corretto errore in assenza della serranda
- Barra alfanumerica: corretto mancato spegnimento a fine operazione
- Errore ricerca di zero: la finestra non è più bloccante

## Compatibile con adapter 0.4.35 e 0.4.36

<a id="id1046"></a>

## Nuove Funzionalità 1.0.46 rispetto la 1.0.45
- Aggiunta gestione Put and Go
- Ferretto NuGet package 0.16.43
- Aggiunta serranda UpperHalf per fiera
- Barra alfanumerica: aggiunto parametro per cancellare alla chiusura della vista di prelievo o versamento
- I cassetti sono salvati sull'adapter
- Menu Info - Generale: aggiunto cambio Stato Wms per Operator

## Bug Risolti
- BIG: corretto ripristino in caso di perdita del sensore di zero
- Parametri asse orizzontale: la velocità a vuoto può essere minore di quelle dei profili
- Versamenti: Salda riga è falso di default
- Conferma collaudo: corretta baia esterna

## Compatibile con adapter 0.4.35 e 0.4.36

<a id="id1045"></a>

## Nuove Funzionalità 1.0.45 rispetto la 1.0.44
- Aggiunto parametro "Quantità limitata in prelievo e deposito"

## Bug Risolti
- Aggiornate traduzioni polacche
- Corretto ripristino in BED se si sposta la slitta in manuale
- Corretto stato di manutenzione se scadono i cicli
- Barra alfanumerica: corretto cambio scomparto con stesso articolo
- Correzioni in caso di liste chiuse da EjLog
- Correzioni per Tendaggi Paradiso
- Aggiunto ritardo per allarme 70 "Il sensore di zero della baia non risulta attivo alla fine del posizionamento"
- Correzione ai campi numerici con comandi incrementali

## Compatibile con adapter 0.4.33 e 0.4.34
***

<a id="id1044"></a>
## Nuove Funzionalità 1.0.44 rispetto la 1.0.43
- Ferretto NuGet package 0.16.42.
- Aggiunto userName in GetItemLists
- Aggiunta descrizione cassetti da EjLog

## Bug Risolti
- Manutenzioni con istruzioni duplicate
- Notifica di manutenzioni in scadenza dopo una conferma servizio

## Compatibile con adapter 0.4.33 e 0.4.34
***

<a id="id1043"></a>
## Nuove Funzionalità 1.0.43 rispetto la 1.0.42
- Aggiunto parametro "Controlla se la lista continua in un'altra macchina"
- Aggiunta funzione di compattazione automatica
- Aggiunta funzione di correzione quote posteriori

## Bug Risolti
- Aggiunta data di scadenza nell'inventario
- Correzione ai ripristini automatici
- Correzioni per Carrefour

## Compatibile con adapter 0.4.31 e 0.4.32

<a id="id1042"></a>
## Nuove Funzionalità 1.0.42 rispetto la 1.0.41
- Aggiunto utente locale Movement: è come Operator con in più i movimenti guidati e manuali
- Aggiunto parametro wms "Attesa dopo versamento"

## Bug Risolti
- Corretta procedura di calibrazione barriera di misura con BID e BED
- Se si mettono in attesa le liste da EjLog i cassetti rientrano dalla baia
- Il cassetto non rientra se ci sono molti versamenti su un unico scomparto
- Aggiunto allarme di presenza in baia dopo il sollevamento della giostra
- Aggiunto controllo per rispettare l'ordine di chiamata dei cassetti
- Aggiornate traduzioni polacche
- Correzione alla selezione della Ricerca Articolo
- Correzione alla Chiamata Cassetto: offset laser

## Compatibile con adapter 0.4.31 e 0.4.32

<a id="id1041"></a>
## Nuove Funzionalità 1.0.41 rispetto la 1.0.40
- Aggiunto parametro per stampare il barcode articolo nelle pagine di prelievo e deposito
- Procedura di risoluzione orizzontale catena: aggiunto pulsante di ricerca di zero

## Bug Risolti
- Corretta gestione della bilancia Dini Argeo
- Corretta gestione della sincronizzazione oraria con il socket link
- BED: eliminata calibrazione verticale con serranda aperta
- Dettagli manutenzione: corretta chiusura del service
- Correzioni alla Chiamata Cassetto
- Socket Link: corretto errore di cassetto sconosciuto
- Test celle limite: corretto problema con tiranti
- Ricerca articolo: aggiunta conferma sul prelievo di articoli con lotto
- Gli utenti locali sono in coda a quelli di EjLog

## Compatibile con adapter 0.4.31

<a id="id1040"></a>
## Nuove Funzionalità 1.0.40 rispetto la 1.0.39
- La configurazione del lettore barcode ha il generatore di codici a barre
- Aggiunti parametri di baia per la lettura della sagoma (barriera di misura)
- Aggiunti parametri di collegamento proxy per la telemetria
- Aggiunte segnalazioni di diagnostica per la scheda I/O
- Aggiunta procedura di risoluzione barriera
- Aggiunto comando per sospendere l'operazione di prelievo/versamento

## Bug Risolti
- I ripristini della giostra si fermano sul sensore
- Correzione alla ricerca di zero giostra
- I cassetti bloccati non si perdono al riavvio
- Aumentata la velocità della procedura di risoluzione orizzontale
- Modifiche agli errori della scheda I/O (34, 84, 94)
- Inviato messaggio di cassetto rimosso alla telemetria
- Modifiche al calcolo della sagoma per inverter AGL
- Eliminato allarme "movimento serranda troppo breve"
- BED: corretto segnale di barriera anti-intrusione interna
- Correzioni alle Barcode Actions
- SocketLink: il rientro cassetto è accettato solo se il cassetto è in attesa di conferma
- Eliminato spegnimento dai panel pc secondari

## Compatibile con adapter 0.4.31

<a id="id1039"></a>
## Nuove Funzionalità 1.0.39 rispetto la 1.0.38
- Ferretto NuGet package 0.16.41.
- Custom Carrefour

## Bug Risolti
- Parametri: Eliminata Baia Culla
- Bilancia Minebea-Sandri: correzioni varie
- Aggiunto allarme di sensori non validi durante la calibrazione verticale
- Corretta inizializzazione dopo errore di WMS
- Baia telescopica: nei movimenti si vede il sensore di baia in sede
- Posizionamento serranda: aggiunto allarme in caso di spostamento troppo veloce
- Carico/Scarico cassetto: aggiunto allarme di elevatore non carico/scarico durante il movimento
- Test primo cassetto: gestisce le celle bloccate in qualsiasi posizione
- Aumentato timeout di comunicazione con la scheda di I/O (errore 24)
- Il numero di cassetto è sempre limitato a 3 cifre
- Le commesse sono ordinate e filtrate
- Ricerca articoli: corretto funzionamento del tasto "-" per decrementare la quantità

## Compatibile con adapter 0.4.31

<a id="id1038"></a>
## Nuove Funzionalità 1.0.38 rispetto la 1.0.37
- Aggiunto comando per inserire il barcode da tastiera
- Introdotta pagina di manutenzione semplificata

## Bug Risolti
- La doppia conferma con il barcode si disabilita se il barcode è errato
- Aggiunto controllo dei sensori sulla calibrazione orizzontale dell'elevatore
- Corretta visualizzazione del cassetto a bordo nel menu movimenti
- Corretta perdita di posizione logica del cassetto a fronte di ripristini automatici su macchine da 1000kg
- Aggiunto allarme in caso di errore inverter senza segnale di fault
- Corretto comando "Nuove operazioni disponibili" 
- Corretta gestione valori decimali nella ricerca articolo

## Compatibile con adapter 0.4.30

<a id="id1037"></a>
## Nuove Funzionalità 1.0.37 rispetto la 1.0.36
- Nei movimenti manuali il riquadro con i sensori dell' Orizzontale cambia colore quando si attiva il bypass
- Aggiunto allarme 93: sensore di zero non attivo durante la calibrazione orizzontale
- Aggiunte foto su alcuni allarmi
- Aggiunti parametri per doppia conferma con barcode in prelievo e versamento
- Aggiunta multiselezione su liste in attesa

## Bug Risolti
- Il parametro aggiungi articolo non è legato alla gestione dei contenitori
- Corretta gestione dello scomparto pieno nella operazione di versamento
- Corretta ricerca articoli nella tabella di scambio per Idroinox
- Eliminato errore 3 in fase di spegnimento
- Corretto invio di login e logout alla telemetria
- Cambiate foto nella procedura guidata di ricerca di zero
- SocketLink: aggiunto parametro per cambiare il terminatore dei messaggi

## Compatibile con adapter 0.4.30

<a id="id1036"></a>
## Nuove Funzionalità 1.0.36 rispetto la 1.0.35
- Aggiunta bilancia Minebea-Intec
- Aggiunta altezza media e passo verticale medio
- Aggiunta procedura guidata al posto del comando di ricerca di zero nella pagina di allarme
- Aggiunto un parametro per specificare l'aggiornamento della giacenza di un articolo 
  tramite un valore di differenza (Idroinox)
- Aggiunto un parametro per eseguire le operazioni di prelievo e versamento per un articolo
  nella vista di cassetto in baia (Idroinox)
- Ferretto NuGet package 0.16.38.

## Bug Risolti
- Corretta abilitazione del lettore di token
- Nelle BIS senza serranda la compattazione bypassa il controllo intrusione
- Aggiunto timeout di connessione con WMS

## Compatibile con adapter 0.4.30

<a id="id1035"></a>
## Nuove Funzionalità 1.0.35 rispetto la 1.0.34
- Aggiunta lingua francese
- Aggiunta lingua ebraica
- Gestione celle: attivazione della multi-selezione
- Aggiunta configurazione del backup su server del cliente

## Bug Risolti
- Gestione cassetti: correzione al salvataggio dei dati quando si cambia la cella
- Procedura di controllo quote: Applica correzione è limitato a 5mm rispetto alle quote iniziali
- Movimenti guidati: correzione dei pulsanti con l'icona della tastiera
- Il lettore di badge funziona anche senza \r nel token
- Nelle viste del cassetto in baia ci sono meno messaggi di movimentazione
- Il timeout con WMS è portato a 5 secondi

## Compatibile con adapter 0.4.29

<a id="id1034"></a>
## Nuove Funzionalità 1.0.34 rispetto la 1.0.33
- Aggiunto parametro per mostrare il pulsante con la tastiera
- Aggiunta Gestione Utenti: si possono cambiare le password di Operator e Installer e disabilitare Operator

## Bug Risolti
- Aggiunto avviso di spazio insufficiente in magazzino
- Il lettore di badge non si blocca con tessere sconosciute
- Anche i movimenti orizzontali impostano la velocità, per la fermata di emergenza

## Compatibile con adapter 0.4.29

<a id="id1033"></a>
## Nuove Funzionalità 1.0.33 rispetto la 1.0.32
- Nella BIG l'allarme 22 (stato sensori baia) permette la ricerca dello zero
- Aggiunto parametro "Cerca gli articoli in questa macchina"
- Aggiunto parametro "Elenco commesse" per la visualizzazione delle commesse
- Selezione della commessa nelle operazioni di pick e put
- Ferretto NuGet package 0.16.35.

## Bug Risolti
- I ripristini della BES portano sempre il cassetto verso l'operatore
- I file di log hanno un limite di 100MB
- Il lettore badge non si blocca più se si seleziona utente o password
- Ingranditi i pulsanti della pagina di login
- Aggiungi prodotto non è disponibile se è abilitato il contenitore
- Ripristino baia esterna con errore di presenza
- Il test completo riporta in magazzino i cassetti prima di partire
- Il lato della baia si può modificare con l'utente Admin

## Compatibile con adapter 0.4.29

<a id="id1032"></a>
## Nuove Funzionalità 1.0.32 rispetto la 1.0.31
- Aggiunto campo Commessa nelle rettifiche (se sono definite su WMS)
- Aggiunto logout a tempo per utente Operator
- Aggiunto allarme antincendio

## Bug Risolti
- Aggiunto allarme per l' intrusione con la barriera di controllo altezza
- Correzioni alle liste in attesa
- Sostituiti i testi "vero" - "falso" con un segno di spunta
- Aggiunti allarmi di pannelli aperti per baie 2 e 3
- Corretto blocco delle missioni con errore di magazzino pieno
- La perdita di connessione con WMS non provoca il logout ma mostra un allarme

## Compatibile con adapter 0.4.27 e 0.4.28

<a id="id1031"></a>
## Nuove Funzionalità 1.0.31 rispetto la 1.0.30
- Le conferme di prelievo e versamento inviano l'utente al WMS
- Aggiunti gli identificativi di macchina e baia ai messaggi del Put To Light e alla gestione delle liste

## Bug Risolti
- Aggiunti timeout di ricezione con Inverter e Scheda di I/O
- La procedura di controllo celle non ritorna al pannello 1 in caso di allarme
- Aggiunto backup del database secondario - corregge il database incompleto in telemetria
- Abilitato il pulsante per uscire dalla procedura di risoluzione orizzontale
- La distanza necessaria per la pesata è ridotta a 80mm
- Aggiunto allarme di cassetto inesistente richiesto da WMS
- BID: la luce di baia si spegne subito al cambio cassetto
- Bilancia contapezzi: usa un valore decimale, non più intero

## Compatibile con adapter 0.4.27

<a id="id1030"></a>
## Nuove Funzionalità 1.0.30 rispetto la 1.0.29
- Aggiunta immediata articolo presente a magazzino su un cassetto
- Gestione liste di prelievo in attesa non evadibili
- Aggiunta della procedura di installazione "Risoluzione asse orizzontale"
- Aggiunta vista delle operazioni in baia con Socket Link

## Bug Risolti
- La procedura di controllo peso è obbligatoria
- Eliminato caso di mancata pesata nei movimenti guidati
- Corretto ripristino del collegamento con il WMS
- Inviamo il comando HOME al laser quando lo stato diventa Automatico
- Corretto caso di errore di "sensore di zero non attivo dopo un deposito"

## Compatibile con adapter 0.4.26

<a id="id1029"></a>
## Nuove Funzionalità 1.0.29 rispetto la 1.0.28
- Il comando di puntamento laser su scomparto è disponibile anche per l'operatore

## Bug Risolti
- Corretto errore nella creazione del database
- Corretto errore di giacenza in caso di prelievo di articoli con lotto
- Aggiunti peso e altezza nelle movimentazioni della telemetria 
- Invio di un comando di HOME al laser all'accensione della macchina
- Se la serranda è disabilitata non interroghiamo più l'inverter della serranda

## Compatibile con adapter 0.4.24 e 0.4.25
***

<a id="id1028"></a>
## Nuove Funzionalità 1.0.28 rispetto la 1.0.27
- Aggiunta lingua ungherese
- Aggiunta la possibilità di disabilitare la modifica della quantità disponibile di un articolo in prelievo

## Bug Risolti
- Telemetria: corretti vari errori (il database locale è ora in formato SqlLite)
- Compattazione: corretto un caso di celle Solo Spazio a metà scaffale
- BIG: aggiunto controllo del sensore di presenza sui ripristini
- Correzione bugs per gestione contenitore esterno (Kohler)
- BED: corretta gestione delle posizioni bloccate in baia
- Gli utenti si ricaricano alla riconnessione con WMS
- Correzione bug minore per conferma prelievo pezze (Tendaggi Paradiso)
- BED: correzioni alle missioni di ripristino

## Compatibile con adapter 0.4.24 e 0.4.25
***

<a id="id1027"></a>
## Nuove Funzionalità 1.0.27 rispetto la 1.0.26
- Nessuna

## Bug Risolti
- L'errore di cassetto sconosciuto appare anche se lo stato è già automatico
- Corretto funzionamento della BED in caso di perdita del sensore di presenza
- Corretto invio delle manutenzioni alla telemetria
- Correzione bugs per Tendaggi Paradiso

## Compatibile con adapter 0.4.24 e 0.4.25

<a id="id1026"></a>
## Nuove Funzionalità 1.0.26 rispetto la 1.0.25
- Aggiunta gestione del contenitore esterno in prelievo (Kohler)

## Bug Risolti
- Avviso in caso di perdita del sensore di presenza del cassetto in baia

## Compatibile con adapter 0.4.23 e 0.4.24
***

<a id="id1025"></a>
## Nuove Funzionalità 1.0.25 rispetto la 1.0.24
- Aggiunto comando per puntare il laser sullo scomparto selezionato

## Bug Risolti
- Il fine lista di EjLog non cancella le missioni in esecuzione
- Corretto errore in caso di mancanza di entrambi i database

## Compatibile con adapter 0.4.23
***

<a id="id1024"></a>
## Nuove Funzionalità 1.0.24 rispetto la 1.0.23
- Aggiunto allarme di altezza troppo bassa
- Dopo un errore di magazzino pieno è possibile chiamare un cassetto nelle baie doppie
- Aggiunto comando di spegnimento del pc

## Bug Risolti
- Corretto allarme di extracorsa
- Corretti allarmi di cassetto troppo pesante e troppo alto

## Compatibile con adapter 0.4.23
***

<a id="id1023"></a>
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
