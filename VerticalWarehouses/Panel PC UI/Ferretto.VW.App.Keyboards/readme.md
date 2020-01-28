# Keyboards

Gli `UserControl` presenti in questo progetto, assieme ai _tools_ ad essi allegati, intendono
facilitare l'interazione utente in scenari di _data entry_ (_Input_).

#### `KeyboardButton`
`UserControl` che altro non è che l'esposizione grafica di un'istanza di **`KeyboardKeyCommand`**.

Un _KeyboardKeyCommand_ presenta, come proprietà fondamentale `Command`.
Il valore di `Command` rappresenta, sotto forma di stringa, il comando che va eseguito sull'`IInputElement` in _focus_.
La stringa può avere la forma:

- **`{cmd}`**: dove `cmd` è uno dei valori presenti in [questa lista](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.key?view=netframework-4.8#fields).
- **testo**: porzione di testo da inserire in corrispondenza della selezione.
