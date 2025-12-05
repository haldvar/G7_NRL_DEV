# NRL PROSJEKT – GRUPPE 7

*README-en er skrevet på norsk, ettersom den fungerer som overordnet prosjektbeskrivelse og veiledning for sensorer og oppdragsgiver.
All teknisk dokumentasjon – inkludert DOCUMENTATION.md og dokumentasjon i selve koden – er skrevet på engelsk, i tråd med bransjestandard for utviklingsprosjekter.*

## Innhold
[Prosjektbeskrivelse](#prosjektbeskrivelse)

[Dokumentasjon](#dokumentasjon)

[Testing](#testing)

[Forbedringspotensial](#forbedringspotensial)

[Sikkerhet](#sikkerhet)

[Videreutvikling](#videreutvikling)

[Installasjon](#installasjon)

[Om oss](#om-oss)

---

## Prosjektbeskrivelse

<div align="left">

<a href="https://github.com/haldvar/G7_NRL_DEV/blob/main/Documentation/Documentation.md"><img src="https://img.shields.io/badge/documentation-blue"></a>
<a href="https://github.com/haldvar/G7_NRL_DEV/blob/main/Documentation/IconLicenses.md"><img src="https://img.shields.io/badge/licenses-purple"></a>
<a href="https://github.com/haldvar/G7_NRL_DEV/blob/main/NRL_PROJECT.Tests/README.md"><img src="https://img.shields.io/badge/testing-green"></a>
<a href="https://github.com/haldvar/G7_NRL_DEV/blob/main/Documentation/UserTesting.md"><img src="https://img.shields.io/badge/user_testing-red"></a>

</div>

Målet med prosjektet er å utvikle en web-løsning for **NRL** som gjør det mulig å rapportere og registrere nye luftfartshindre over hele Norge.  

Brukere kan opprette egne kontoer, som deretter godkjennes av en administrator som tildeler riktig rolle og organisasjonstilhørighet:

### Brukerroller
**Pilot**  
- Går rett til Hinderrapportering ved innlogging
- Lager og sender rapporter om nye luftfartshindre
- Kan se hinderlogg for sine egne innsendte rapporter
- Ser alle innsendte rapporter i Situasjonskartet, men med anonymisert innsender.
- Løsningen er tilpasset bruk på nettbrett

**Registerfører**  
- Går rett til Rapporthåndtering ved innlogging
- Kan se alle innsendte rapporter.
- Endre status
- Delegere rapporter
- Endre/legge til opplysninger (hindertype, høyde osv.)
- Ser hvem som har sendt inn rapportene som vises i Situasjonskart

**Ekstern organisasjon**  
- Går rett til rapporter fra egen organisasjon ved innlogging
- Kan se rapporter som er sendt inn av flybesetninger i egen organisasjon

**Administrator**  
- Går rett til Brukerhåndtering ved innlogging
- Full tilgang til hele løsningen
- Brukerhåndtering (roller, organisasjoner, godkjenning)

### Brukerregistrering (Innloggingssida)
 
- Personer kan opprette egen bruker
- Tilgangsrolle og organsiasjonstilhørighet må legges til av Admin.
- Uten godkjenning fra Admin kan bruker kun melde nytt hinder, se egne rapporter og se anonymisert Situasjonskart.

---

### Login-informasjon (for testing)

#### Administrator
- E-post: admin@test.no
- Passord: Heisann1!
  
#### Registerførere

- reg1@test.no – Heisann1!
- reg2@test.no – Heisann1!
  
#### Flybesetning / Piloter
- pilot3@test.no (Forsvaret) – Heisann1!
- pilot2@test.no (Politiet) – Heisann1!
  
#### Eksterne organisasjoner
- ext2_Politiet@test.no – Heisann1!
- ext1_Forsvaret@test.no – Heisann1!

--- 

## Dokumentasjon

### Verktøy brukt
- **[Docker](https://www.docker.com)** (Docker Desktop)  
- **[MariaDB](https://mariadb.org)**  
- **[Entity Framework](https://learn.microsoft.com/en-us/ef/)** (migrations)  
- **[ASP.NET MVC](https://dotnet.microsoft.com/en-us/apps/aspnet/mvc)** (på .NET 9.0)  
- **[Tailwind CSS](https://tailwindcss.com)**
- **[Leaflet](https://leafletjs.com)**
  - **[Leaflet.pm](https://github.com/themre/leaflet.pm)**
  
### Systemarkitektur

Dette prosjektet er bygd med en MVC (Model–View–Controller) struktur. Det vil si at systemets oppgaver – datastrukturering, datahåndtering og datasamling/framvisning – er delt inn i komponentene **models, controllers** og **views**. Under kan du se en modell av vår applikasjonsstruktur.

![alt text](https://github.com/haldvar/G7_NRL_DEV/blob/main/Documentation/Images/system_model.png)

Prosjektet fungerer gjennom en nettleser. MVC applikasjonen ligger i en Docker container som kommuniserer med MariaDB databasen. I MVC applikasjonen ligger det forskjellige "layers" som jobber sammen og formaterer output, mens i databasen lagres alt av nødvendig data om prosjektet.

---

## Testing

*Prosjektet har to typer tester:*

### Brukervennlighetstester
Utført som scenario‑baserte tester på EXPO‑dagen, der testpersoner skulle simulere en rolle  som vi har i løsningen vår (pilot, registerfører, administrator eller ekstern organisasjon).
Fokus: hvor intuitivt og raskt det er å registrere et hinder.
Resultat: gjennomsnittlig registreringstid ca. 37 sekunder, med konsistente tilbakemeldinger som ga grunnlag for forbedringer i brukergrensesnittet.

### Enhetstester (70 stk)
Verifiserer sikkerhet, forretningslogikk og dataflyt.
Status: 70/70 passerer
Kjøretid: under 1 sekund
Rammeverk: xUnit 2.5.3 | .NET 9.0


### Kjøre enhetstester:

**Forutsetning:** Prosjekt er kjørende etter beskrivelsen i readme

- Naviger til `NRL_PROJECT.Tests.` i terminal
- Kjør kommando: ```dotnet test```

**Forventet output:**

```
Test summary: total: 70; failed: 0; succeeded: 70; skipped: 0
Build succeeded
```

**Status:** 70/70 passerer | < 1s kjøretid

*For fullstendig testdokumentasjon, se —->* [NRL_PROJECT.Tests
/README.md](https://github.com/haldvar/G7_NRL_DEV/blob/main/NRL_PROJECT.Tests/README.md)

## Forbedringspotensial
- Tilbakemeldingssystem fra registerfører til innmelder
- Varsling ved statusendring
- Offline-modus (per nå kun draft ved manglende dekning)
- Mulighet for bruker å endre/slette innmeldinger på kartet
- 2-faktor og sending av sikkerhetskoder
- Reset/endring/glemtpassord av passord fra innloggingssiden

## Sikkerhet
### Autentisering og Autorisasjon
Applikasjonen bruker ASP.NET Core Identity for brukerautentisering med rollebasert tilgangskontroll. Systemet opererer med fire brukerroller:

- **Pilot:** Rapportere hindre, administrere egne rapporter
- **Registerfører:** Saksbehandling, statusendringer, full tilgang til rapporter
- **Administrator:** Brukeradministrasjon, rolletildeling, systemkonfigurasjon
- **Ekstern Organisasjon:** Lesetilgang til godkjente rapporter for egen organisasjon

### Dataisolasjon og Validering

Piloter har kun tilgang til egne data, eksterne organisasjoner ser kun rapporter relatert til egen organisasjon, mens registerfører og administratorer har full tilgang. All input valideres: filstørrelse (maks 5MB), tillatte formater, datatyper og koordinater. Sikkerhet er verifisert med 12 dedikerte enhetstester. 


## Videreutvikling
Prosjektet er åpent for videre utvidelser og forbedringer.

---

## Installasjon

### *Forutsetninger*
- Ha [Docker Desktop](https://www.docker.com/get-started/) installert og kjørende
  - Pass på å ikke ha andre containers kjørende
- Ha [```git```](https://git-scm.com/install/windows) installert for å kunne kjøre git-kommandoer

### 1. Last ned prosjektet
- Last ned prosjektet som [`.zip`](https://github.com/haldvar/G7_NRL_DEV/archive/refs/heads/main.zip)
  
 **eller**
  
- Åpne terminal
- Naviger (cd) til din mappe for kodeprosjekter
- Kjør kommandoen `git clone https://github.com/haldvar/G7_NRL_DEV.git`

### 2. Naviger i prosjektet
Naviger (`cd`) til rotmappen `G7_NRL_DEV`

### 3. Bygg prosjektet
- Dobbeltsjekk at `docker-compose.yml` filen ligger i directory ved å skrive `ls`
- Kjør kommandoen `docker compose up --build`

### 4. Start applikasjonen
Åpne Docker Desktop og trykk på:
`localhost:5001`

### Alternativ fremgangsmåte
- Åpne ```~/G7_NRL_DEV/```**```GROUP7.sln```** i din IDE (Rider, VSCode, Visual Studio...)
- Kjør løsningen som ```docker compose```

### Feilsøking
Hvis løsningen ikke vil kjøre:
- Stopp alle containere i Docker Desktop
- Kjør ```docker compose down``` i docker terminal
- Evt slett tidligere containers
- Kjør ```docker compose up --build``` på nytt

### Bruk av applikasjonen

*For å teste fullstendige funksjonalitet se —->* [Documentation/UserTesting.md](https://github.com/haldvar/G7_NRL_DEV/blob/main/Documentation/UserTesting.md)


---

## Om oss
### Gruppemedlemmer
- Karoline  
- Milana  
- Jan Kåre  
- Emma  
- Elise  
- Halvard
