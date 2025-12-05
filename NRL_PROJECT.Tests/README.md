# Dokumentasjon for tester

## Innhold

[Introduksjon](#introduksjon)

[Brukervennlig testing](#brukervennlig-testing)

[Testresultater](#testresultater)

[Utbedringer i UI/UX](#utbedringer-i-UI/UX)

[Enhetstesting](#enhetstesting)

[Hvordan gjøre testing](#hvordan-gjøre-testing)

[Struktur i testprosjektet](#struktur-i-testprosjektet)

[Teststrategi og testoversikt](#teststrategi-og-testoversiktn)

[Testmønstre](#testmønstren)

[Beste praksis](#beste-praksis)

[Kjente advarsler](#kjente-advarsler)

[Refleksjon](#refleksjon)

[Konklusjon](#konklusjon)

---

## Introduksjon
Her har vi delt opp readme’en i testing i to deler: brukervennlighetstester og enhetstesting. 

## Brukervennlig testing
For å teste brukervennlighet lagde vi ulike testscenarioer som vi ønsket at medelever (og andre) skulle gå gjennom på EXPO-dagen. Vi lagde opprinnelig fem stk, hvor brukeren skulle late som den var en gitt rolle, og utføre stegvis noen gitte oppdager. Den første handlet om brukerregistrering, et testscenario for pilotbrukeren der den registrerte et hinder, en for registerfører, en for administrator og den siste som en ekstern organisasjon. Det viste seg å være for optimistisk å gjennomføre alle fire på en hektisk EXPO-dag, og vi endte opp med å fokusere på pilotens hinderregistrering som eneste testscenario. Denne så vi på som verdifull for å få testet “pilotvennlig”-design, og generelt om det var intuitivt og raskt å melde inn et hinder. Her var det tidtaking for stegene etter innloggingen - den faktiske registreringen - som vi var nøye på å forklare testdeltakeren at ikke var en måling av deres evner, men hvor godt eller dårlig designet er.

**Testscenario:**
*Pilot*
Gruppa: Ta tid hvor lang tid det tar å lage rapport på punkt 2a-d. 
Forbered testtaker på oppgavene etter innlogging

- Logge inn med
  - Bruker: pilot2@test.no
  - Passord: Heisann1!

Husk tidtaking!

- Rapporter et hinder:
  - Marker punkt eller linje i kartet
  - Legg til kommentar
  - Legg til et bilde av lufthinderet
  - Send inn rapport
- Verifiser at rapporten din kommer med kommentar og bilde i  hinderloggen. 
- Finn rapporten din i situasjonskart.
- Logg ut
- Hva synes testtaker om brukergrensesnittet til hinderregistrering?
- Hvordan var det å bruke systemet? Tror du andre lett kan bruke systemet?
- Har du andre kommentarer?
- Testtaker velger twist fra posen!

## Testresultater

| Testperson | Tid | Grensesnitt/ Kommentarer |
| :---         |     :---      |          :--- |
| 1   | 36 sek     |  Meldte hinderet for fort - skjønte ikke funksjonen til “Meld hinder”-knappen (trodde kanskje den tok den videre til legg til bilde- og kommentar-funksjoner) - Hinderloggen kunne vært lettere å skille linjene fra hverandre. Enten med avstand/farger   |
| 2     | 45 sek       | - “Er kommentaren med eller ikke?”. ← Ønsket bekreftelse på at kommentar er lagt til      |
| 3     | 32 sek       | Fint! Ellers ingen kommentar      |
| 4     | 36 sek       | Trykket på “meld hinder” med en gang, skjønte ikke måttet trykke markere hinder i kartet først      |
| 5     | 35 sek       | Fint kart      |

```Snitt --> 36,8 sek```

Vi ser på 36,8 som en akseptabel tid for å registrere et hinder. Fire av fem brukere brukte mellom 32-36 sekunder, som er svært konsistent. Den ene brukeren brukte litt ektra tid enn de andre - 45 sekunder - og det kunne vært nyttig å få mer innsikt i “avviket”. Men gitt at testingen foregikk i hektiske omstendigheter, med høy lyd, høyt tempo og mye nerver for de fleste av studentene, så tror vi det har en innvirkning. Som forsåvidt er et godt testmiljø for piloten, som også er utsatt for stort tidspress. Kommentarene om “Meld hinder”-knappen, lesbarhet i hinderloggen og kommentar-bekreftelse er nyttige tilbakemeldinger, og vi vil vise utbedringene i neste del.

## Utbedringer i UI/UX
**“Meld hinder-knappen”:**
Vi har gjort slik at den ikke er synlig før man har markert kartet med et punkt eller linjer. Slik vil man ikke kunne trykke på knappen for raskt, og få feilmelding, og bruke unødvendig tid på det.

Bilde
*Før man har markert hinder i kart*

Bilde
*Etter man har markert hinder kart*

Bilde
Bekreftelse på at kommentaren er lagret:

Bilde
Hinderlogg fått en farge på annenhver linje:

## Enhetstesting

- Totalt antall tester: 70
- Suksessrate: 100%
- Kjøretid: < 1 sekund
- Rammeverk: xUnit 2.5.3
- Plattform: .NET 9.0


Dette prosjektet inneholder 70 enhetstester og dekker både controller-logikk og modell-logikk, med spesiell vekt på autentisering, autorisasjon og korrekt dataflyt. Alle tester kjører mot en in-memory EF Core database for rask, isolert kjøring.

## Hvordan gjøre testing
**Forutsetning:** Prosjekt er kjørende etter readme-beskrivelsen i `NRL_PROJECT`. 
Åpne terminalen. Fra prosjektets mappe, naviger til `NRL_PROJECT.Tests`.

- Naviger til `NRL_PROJECT.Tests`
- Skriv kodene inn i terminal:

### Alle testene:
`dotnet test`

Forventet output:
```
Test summary: total: 70; failed: 0; succeeded: 70; skipped: 0
Build succeeded
```

### Spesifikke testmetoder
a) Kjør én enkelt test
```
dotnet test --filter "FullyQualifiedName~MapController_AuthenticatedUser_CanSubmitReport"
```

b) Kjør alle tester med "Valid" i navnet
```
dotnet test --filter "DisplayName~Valid"
```

c) Med detaljert output
```
dotnet test --verbosity normal
```

d) Eller full logging
```
dotnet test --logger "console;verbosity=detailed"
```

## Struktur i testprosjektet

```text
NRL_PROJECT.Tests/
├─ Controllers/                    59 tester (84%)
│  ├─ AuthorizationTests           12 tester
│  ├─ HomeControllerTests          3 tester
│  ├─ MapControllerTests           22 tester
│  └─ RegistrarControllerTests     22 tester
├─ Data/                           2 tester (3%)
│  └─ DbContextTests               2 tester
└─ Models/                         9 tester (13%)
   ├─ MapDataTests                 3 tester
   ├─ ObstacleDataTests            4 tester
   └─ ValidationTests              2 tester
```

---

## Teststrategi og testoversikt

### Sikkerhet (12 tester)

**Fokusområde:** Autentisering, autorisasjon og dataisolasjon

**Hva vi tester:**

- Kun innloggede brukere får tilgang
- Rollebasert tilgang med [Authorize]-attributter
- Brukere ser kun egne rapporter

**Testmetoder:** Refleksjon verifiserer attributter, ClaimsPrincipal simulerer brukere, Mock UserManager.

#### Nøkkeltester:
- Refleksjonstest

  - ```RegistrarController_HasAuthorizeAttributeWithRegistrarRole```

- Dataisolasjon

  - ```MapController_MyReports_OnlyShowsCurrentUserReports```

- Rolleseparasjon

  - ```RegistrarController_PilotRole_CannotAccessRegistrarFunctions```



### Forretningslogikk (44 tester)

**Fokusområde:** Controller-metoder, validering og dataflyt

**MapControllerTests (22 tester)** - Pilot-arbeidsflyt

- Innsending av hindre med Point/LineString-geometri
- Filhåndtering (5MB-grense, typevalidering)
- Validering og feilhåndtering

**Nøkkeltester:**

- SubmitObstacleWithLocation_WithValidPointGeoJson_CreatesReportSuccessfully
- SubmitObstacleWithLocation_WithValidImage_SavesImageSuccessfully
- SubmitObstacleWithLocation_WithOversizedImage_ReturnsError

**RegistrarControllerTests (22 tester)** - Registrar-arbeidsflyt

- Statusoverganger (New → InProgress → Resolved)
- Dataoppdateringer med validering (høyde 0-10000m)
- Rapportoverføringer og kommentarhåndtering

**Nøkkeltester:**

- ```UpdateStatus_ToGodkjent_UpdatesStatusCorrectly```
- ```UpdateObstacleData_WithInvalidHeight_ReturnsError```
- ```TransferReport_AssignsNewRegistrar```


**HomeControllerTests (3 tester)** - Offentlige sider
- Index, About, Privacy

**Testmetoder:** In-memory database, Mocking av avhengigheter, AAA-mønster.


### Modellogikk (9 tester)

**Fokusområde:** Standardverdier, formatering og validering

**MapDataTests (3 tester)**

- Koordinatformatering for norsk visning (5 desimalers presisjon)
- Point-format: "Punkt (lat, lon)"
- LineString-format: "Linje med N punkter"

**ObstacleDataTests (4 tester)**

- Standardverdier: ```ObstacleType = "Ukjent"```
- Initialisering og null-sikkerhet

ValidationTests (2 tester)

- ```[Required], [StringLength(255)]```

**Testmetoder:** Rene enhetstester uten eksterne avhengigheter.

### Database-operasjoner (2 tester)

**DbContextTests (2 tester)**

- Verifiserer at EF Core-integrasjon fungerer
- Nyttig som smoke test for databasekonfigurasjon
- Bekrefter at modeller kan lagres og lastes

## Testmønstre
Alle tester følger AAA-mønsteret, bruker in-memory database for hastighet, og mocker eksterne avhengigheter.

### Eksempel:

```
[Fact]
public async Task SubmitObstacle_ValidData_Succeeds()
{
    // Arrange
    var controller = CreateTestController();
    var model = new ObstacleData { /* ... */ };
    
    // Act
    var result = await controller.SubmitObstacleWithLocation(model);
    
    // Assert
    Assert.IsType<RedirectToActionResult>(result);
}
```

---

## Beste praksis
1. Helper-metoder - Reduser duplisering med gjenbrukbare oppsettmetoder
2. Ett fokus per test - Test én ting om gangen
3. Meningsfulle assertions - Verifiser innhold, ikke bare typer
4. Rydd opp - Implementer ```IDisposable``` for ressursrydding

---

## Kjente advarsler

**128 compiler warnings** (104 i hovedprosjekt, 24 i tester):

- Nullable reference warnings (CS8618, CS8602, CS8625, etc.)
- Security header warnings (ASP0019)

**Status:** Trygge å ignorere - alle tester passerer, applikasjonen fungerer som den skal.



## Refleksjon

Testene ble skrevet **etter implementasjon**, ikke parallelt. Selv om dette ikke er ideelt, ga det stor verdi:

- Bekreftet at funksjonaliteten fungerer
- Gir trygghet ved refaktorering
- Dokumenterer forventet oppførsel
- Økte forståelsen av testmønstre og mocking

**Hva vi ville gjort annerledes:**

- Startet testing tidligere
- Prioritert sikkerhetstester først
- Laget en systematisk plan for mulige feilscenarier

---

## Konklusjon

Brukervennlighetstesting var nyttig, og gjorde at vi fikk gjort noen siste UX-utbedringer før innlevering av denne oppgaven. Vi ser at vi kunne ha gjort brukervennlighetstesting mer systematisk etter at vi fikk front-designet ordentlig på plass - og fått gjennomført flere av testscenarioene. 
Enhetstesting tar tid, men det er tid som er godt brukt. Neste gang starter vi fra dag én, spesielt for sikkerhet. Men selv retrospektive tester har verdi - de beviser at systemet fungerer og beskytter mot fremtidige feil.

---

*Sist oppdatert: Desember 2025
Totalt: 70 tester (59 controller + 9 modell + 2 data)
Rammeverk: xUnit 2.5.3 | .NET 9.0*
