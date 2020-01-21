# Scaffolding How-To

> Il sistema di _scaffolding_ prevede la creazione automatica di una struttura editabile dato un oggetto (in generale complesso)
> da modificare.

La struttura editabile viene generata a partire da **metadati** associati al `Type` dell'oggetto dato.

Attualmente i metadati vengono estratti da **meta-attributi** che decorano il `Type`.  
I meta-attributi utilizzabili sono elencati di seguito:

#### `MetadataTypeAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
Se presente sulla classe, devìa sul `Type` ad esso **associato** per la navigazione e - tramite i decoratori presenti in
quest'ultimo - la creazione della struttura.

> NB: Non viene tenuta corrispondenza con il tipo originale, ovvero: se una proprietà non viene
> definita nel tipo **associato** questa viene saltata come se non fosse presente.

#### `ScaffoldColumnAttribute`:
(_namespace_: `System.ComponentModel.DataAnnotations`)
Se associata ad una _property_, e con `Scaffold` settato a `false`, suggesisce di non includere
la _property_ stessa nella struttura.

#### `PullToRootAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)
Se associato ad una _property_ la "trasporta" nella _root_ della struttura.

#### ``
