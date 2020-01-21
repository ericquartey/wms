# Scaffolding How-To

> Il sistema di _scaffolding_ prevede la creazione automatica di una struttura editabile dato un oggetto (in generale complesso)
> da modificare.

La struttura editabile viene generata a partire da **metadati** associati al `Type` dell'oggetto dato.

Attualmente i metadati vengono estratti da **meta-attributi** che decorano il `Type`.  
I meta-attributi utilizzabili sono elencati di seguito.

## Attributi di Struttura
Sono i meta-attributi che generano la struttura navigabile.

#### `MetadataTypeAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
Se presente sulla classe, devìa sul `Type` ad esso **associato** per la navigazione e - tramite i decoratori presenti in
quest'ultimo - la creazione della struttura.

> NB: Non viene tenuta corrispondenza con il tipo originale, ovvero: se una proprietà non viene
> definita nel tipo **associato** questa viene saltata come se non fosse presente nel tipo originario.

---
#### `ScaffoldColumnAttribute`:
(_namespace_: `System.ComponentModel.DataAnnotations`)  
Se associata ad una _property_, e con `Scaffold` settato a `false`, suggesisce di non includere
la _property_ stessa nella struttura.

---
#### `PullToRootAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
Se associato ad una _property_ la "trasporta" nella _root_ della struttura.

---
#### `CategoryAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
&Egrave; un **attributo localizzabile[^1]** che, associato ad una _property_ la include nella
omonima categoria (che, se non esiste ancora, viene creata) nell'attuale livello d'annidamento.

> Per il _flattening_ connaturato a questo sistema di scaffolding ed attuato sulla struttura, 
> il `CategoryAttribute` **deve essere presente** su qualsiasi _property_ **enumerabile**. 
> Altrimenti un'eccezione di tipo `ScaffoldingException` viene lanciata.

---
#### `CategoryParameterAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
In associazione al `CategoryAttribute` sopra definito, consente di includere dei valori peculiari
dell'istanza per differenziare la stringa di **categoria** utilizzata per i raggruppamenti.
Possono essere presenti in più istanze sulla stessa _property_.

La logica è la seguente:
- Proprietà **`Enumerable`**: i valori delle proprietà suggeriti dalle `PropertyReference` vengono
cercati nel tipo dell'elemento raggruppato (_item type_).

> Il `CategoryParameterAttribute` è obbligatorio (almeno uno dev'essere presente) 
> in un contesto **enumerable**!
> Altrimenti un'eccezione di tipo `ScaffoldingException` viene lanciata.

- Proprietà **complessa** (`.IsClass`): i valori della proprietà suggeriti dalle `PropertyReference`
vengono cercati **prima** nel tipo/istanza della _property_ oggetto di decorazione, e **poi** (_fallback_)
nel tipo/istanza _owner_ (_parent_) della _property_.

---
#### `UnfoldAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
Se associato ad un oggetto di tipo complesso (`.IsClass`) ne spacchetta (_unfold_) le proprietà
in maniera **ricorsiva** appiattendole su un unico livello.

> La **ricorsione** sull'appiattimento viene interrotta qualora si incontri 
> un `PullToRootAttribute` su un "ramo a valle".

---
#### `TagAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
Se associati (anche più d'uno) ad una _property_, aiutano il motore di ricerca interno
al _control_ `Scaffolder` a reperire il campo.  
&Egrave; un attributo localizzabile[^1].

## Attributi di Valore
Sono i meta-attributi deputati ad intervenire nella definizione del singolo campo.

#### `EditableAttribute`:
(_namespace_: `System.ComponentModel.DataAnnotations`)  
Se presente con valore `Editable` uguale a `false`.

---
#### `DisplayAttribute`:
(_namespace_: `System.ComponentModel.DataAnnotations`)  
Aiuta a mostrare in maniera leggibile il nome della proprietà semplice[^2] oggetto di _editing_.

---
#### `ValidationAttribute(s)`:
(_namespace_: `System.ComponentModel.DataAnnotations`)  
Sono i ben noti `RequiredAttribute`, `RangeAttribute`, `RegularExpressionAttribute`, ...
che intervengono a validare il valore del campo prima del _commit_.

> Il `RangeAttribute`, se presente, concorre a mostrare (nella _grid_ interna al _control_ `Scaffolder`)
> i valori di massimo e minimo ammissibili.

---
#### `UnitAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
Se associato ad una _property_, concorre a mostrare (nella _grid_ interna al _control_ `Scaffolder`)
l'unità di misura del campo.  
&Egrave; un attributo localizzabile[^1].

---
#### `DefaultValueAttribute`:
(_namespace_: `Ferretto.VW.MAS.Scaffolding.DataAnnotations`)  
Se associato ad una _property_, concorre a mostrare (nella _grid_ interna al _control_ `Scaffolder`)
il valore di _default_ per il campo.

---
## Note importanti
Ad ogni _step_ iterativo nel corso del _parsing_ della struttura, se ci si trova
di fronte un'**istanza complessa** (`.IsClass`) di valore **`null`**, il "ramo viene lì reciso"
nel senso che lo _scaffolding_ non prosegue.

[^1]: implementa `ILocalizeString`.
[^2]: una proprietà è considerata **semplice** se il suo tipo è `.IsValueType`, oppure se è `string` o se è serializzabile (`.IsSerializable`).

## Esempi

Nel seguente esempio la proprietà **complessa** di tipo `Laser` appartenente al tipo baia
(`BayMetadata` richiama appunto a metadati relativi al tipo `Bay`) viene:

- **Spacchettato** (`UnfoldAttribute`): tutte le sue sottoproprietà vengono appiattite su un unico livello.
- **Tirato sulla _root_** (`PullToRootAttribute`): tutte le proprietà appaiono al livello 0 della struttura.
- **Categorizzato** (`CategoryAttribute`, `CategoryParameterAttribute`): tutte le proprietà vengono 
raggruppate in una categoria il cui nome è definito nella risorsa `Vertimag` alla chiave `BayLaser`.
Di più, tale stringa viene contestualizzata andando a pescare il valore della proprietà `Number`, **in
questo caso** presente nell'istanza `Bay` stessa (e non nell'istanza di tipo `Laser`!).
Il tutto risulterà in una categoria del tipo _"Laser baia 1"_ ed in un corrispondente livello di
navigazione nel _control_ `Scaffolder`.

> Si noti l'utilizzo, previsto per il `CategoryParameterAttribute`, di un **`ValueStringifierType`**:  
> si tratta di un tipo che **deve** implementare `IValueStringifier` e che si premura di rendere
> "leggibile" il valore recuperato che andrà a comporre la stringa name della categoria.

```c#

class BayMetadata{

// ...

[Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayLaser))]
[CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
[PullToRoot, Unfold]
public Laser Laser { get; set; }

// ...
}
```
