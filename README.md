# NRL PROSJEKT – GRUPPE 7

## Gruppemedlemmer
- Karoline  
- Milana  
- Jan Kåre  
- Emma  
- Elise  
- Halvard  

---

## Om prosjektet

Målet med prosjektet er å utvikle en web-løsning for **NRL** som gjør det mulig å rapportere og registrere nye luftfartshindre over hele Norge.  

Brukere kan opprette egne kontoer, som deretter godkjennes av en administrator som tildeler riktig rolle:

### Brukerroller
**Flybesetning**  
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

### Verktøy brukt
- **Docker** (Docker Desktop)  
- **MariaDB**  
- **Entity Framework** (migrations)  
- **ASP.NET MVC** (på .NET 9.0)  
- **Tailwind CSS**  

---

## Installasjon og kjøring

### 1. Last ned prosjektet
- Last ned `.zip` **eller**  
- `git clone <repo-url>`

### 2. Åpne prosjektet
Åpne `.sln` i **Visual Studio** eller **Rider**.

### 3. Klargjør database via Docker
Åpne Docker-terminal:

```bash
docker exec -it mariadbcontainer mariadb -u root -p
```
Passord:
```bash
Begripeligvis1214
```
Slett eksisterende database:
```bash
drop database nrl_project_db;
```
### 4. Kjør migrations
Naviger til:
~/G7_NRL_DEV/NRL_PROJECT
Kjør:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
### 5. Start applikasjonen
Kjør med docker-compose, åpne Docker Desktop og trykk på:
localhost:5001
Nettleseren åpnes automatisk.

## Feilsøking
Hvis løsningen ikke vil kjøre:
- Stopp NRL_PROJECT-containeren i Docker
- Kjør docker compose på nytt fra IDE
- Start containere igjen i Docker Desktop

## Trenger du fortsatt hjelp?
- Kontakt et av gruppemeldemmer (gruppe 7)
- Spør ChatGPT

--- 

### Login-informasjon (for testing)
#### Administrator
- E-post: admin@nrl.no
- Passord: Admin@123
#### Registerfører
- E-post: reg4@test.no
- Passord: Heisann1!
#### Flybesetning / Piloter
- pilot7@test.no (Forsvaret) – Heisann1!
- pilot6@test.no (Politiet) – Heisann1!
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
