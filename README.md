# NRL PROSJEKT – GRUPPE 7

## Installasjon og kjøring

### 1. Last ned prosjektet
- Last ned `.zip` **eller**  
- Kjør kommandoen `git clone https://github.com/haldvar/G7_NRL_DEV.git`

### 2. Naviger i prosjektet
Naviger (`cd`) til rotmappen `G7_NRL_DEV`

### 3. Bygg prosjektet
- Dobbeltsjekk at `docker-compose.yml` filen ligger i directory ved å skrive `ls`
- Kjør kommandoen `docker compose up --build`

### 4. Start applikasjonen
Åpne Docker Desktop og trykk på:
`localhost:5001`

## Feilsøking
Hvis løsningen ikke vil kjøre:
- Stopp NRL_PROJECT-containeren i Docker
- Kjør docker compose på nytt fra IDE
- Prøv å kjøre localhost igjen.

--- 

## Om prosjektet

Målet med prosjektet er å utvikle en web-løsning for **NRL** som gjør det mulig å rapportere og registrere nye luftfartshindre over hele Norge.  

Brukere kan opprette egne kontoer, som deretter godkjennes av en administrator som tildeler riktig rolle og organisasjonstilhørighet:

### Brukerroller
**Pilot**  
- Lager og sender rapporter om nye luftfartshindre  
- Kan se hinderlogg  
- Løsningen støtter bruk på nettbrett  

**Registerfører**  
- Kan se rapporter  
- Endre status  
- Delegere rapporter  
- Endre/legge til opplysninger (hindertype, høyde osv.)

**Ekstern organisasjon**  
- Kan se rapporter som er sendt inn av flybesetninger i egen organisasjon

**Administrator**  
- Full tilgang  
- Brukerhåndtering (roller, organisasjoner, godkjenning)

---

## Dokumentasjon

<div align="left">

<a href="https://github.com/haldvar/G7_NRL_DEV/blob/main/Documentation/Documentation.md"><img src="https://img.shields.io/badge/documentation-blue"></a>
<a href="https://github.com/haldvar/G7_NRL_DEV/blob/main/Documentation/IconLicenses.md"><img src="https://img.shields.io/badge/licenses-purple"></a>



</div>

### Verktøy brukt
- **Docker** (Docker Desktop)  
- **MariaDB**  
- **Entity Framework** (migrations)  
- **ASP.NET MVC** (på .NET 9.0)  
- **Tailwind CSS**  

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

## Testing
Testing-scenarier og unit-testing er gjennomført (flere detaljer kan legges til senere).

## Forbedringspotensial
- Administrator-godkjenning for nyregistrerte brukere
- Tilbakemeldingssystem fra registerfører til innmelder
- Varsling ved statusendring
- Offline-modus (per nå kun draft ved manglende dekning)
- Mulighet for bruker å endre/slette innmeldinger på kartet

## Videreutvikling
Prosjektet er åpent for videre utvidelser og forbedringer.

---

## Gruppemedlemmer
- Karoline  
- Milana  
- Jan Kåre  
- Emma  
- Elise  
- Halvard  
