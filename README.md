# G7_NRL_DEV
Utvikling av NRL system

Dette repositoryet følger utviklingen av et verktøy for registrering av luftfartshindre for vår klient Kartverket(NRL) og Norsk Luftambulanse. Utviklerne i Gruppe 7 er:_ 

**Karoline, Milana, Jan Kåre, Halvard, Elise and Emma.**


# Dokumentasjon
## Gruppe 7 – ASP.NET MVC Webapplikasjon

Dette prosjektet er utviklet som en del av 3. semester, bachelorstudiet i IT og informasjonssystemer. Løsningen demonstrerer en moderne webapplikasjon bygget med **ASP.NET MVC**, **Tailwind CSS**, **Leaflet.js** og **MariaDB**.  
Applikasjonen er containerisert med **Docker** for enkel drift og distribusjon, og har **Docker Orchestration Support** aktivert i Visual Studio. For håndtering av geografiske data benytter systemet GeoJSON som standardformat.

### Drift og kjøring

#### Krav
-   Docker og Docker Compose installert

#### Starte systemet
-   Åpne solution i Visual Studio
-   Start prosjektet ved å trykke **Run** (F5)
-   Containere blir bygget automatisk 
-   Et internt nettverk kalt **appnet** starter og knytter tjenestene sammen
-   Applikasjonen startes

#### Funksjonalitet
MVC-struktur
-    Prosjektet benytter *Models*, *Views* og *Controllers* for å separere data, presentasjon og logikk 
-    *ViewModels* skal benyttes for å kombinere flere datamodeller i ett view. (Per 01.10.2025 er det ikke laget sammenslått ViewModel, men det kommer.)
Responsivt design
-   Sidene i applikasjonen er designet ved hjelp av Tailwind CSS og ved hjelp av generativ KI.
-   Applikasjonen skal til slutt fungere på mobil, nettbrett og desktop. Her gjenstår mye arbeid.
HTTP-håndtering
-   Applikasjonen støtter både **GET**- og **POST**-forespørsler
-   GET brukes til å hente data fra serveren
-   POST brukes til å sende data via skjema
Skjema
-   Brukeren kan fylle inn informasjon via skjema
-   Innsendt data lagres lokalt og vises på en annen side
-   Data skal på sikt lagres i en database
Kartintegrasjon
-   Applikasjonen inkluderer et kart gjennom tjenesten Leaflet.js
-   Brukeren kan markere posisjon(er) i kartet
-   Kartdata med markør i kartet og nedtegnede koordinater lagres via GeoJSON og vises på ny side etter registrering

### System arkitektur
-   Komponenter involvert i prosjektet:
    - **ASP.NET MVC** – backend og logikk
    - **Tailwind CSS** – responsivt UI
    - **Leaflet.js** – kartvisning
    - **GeoJson** - standardformat for lagring og utveksling av geografiske data. 
    - **MariaDB** – database
    - **Docker** – drift, kjører applikasjon og database i containere koblet via nettwerket **appnet**

### Testscenarier og resultater
#### Foreløpig teststrategi:

Manuell testing

 - Gå gjennom løsningen i nettleseren og bekreft at:

 - GET-forespørsler returnerer riktig data

 - POST via skjema fungerer og viser data på ny side

 - Kartet laster og koordinater hentes korrekt

#### Integrasjonstesting

Test at databasen og applikasjonen kommuniserer korrekt. Her gjenstår arbeid, siden vi ikke har hatt særlig fokus på databasen enda.

#### Fremtidige forbedringer

Legge til enhetstesting 

Vurdere bruk av automatiserte testverktøy (f.eks. Playwright).

Pull request i test!!!
