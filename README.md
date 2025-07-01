# RAG PROSJEKT!


## Intro

I dette prosjektet ser vi på hvilke forskjellige verktøy vi har tilgjengelige for å utvikle RAG appikasjoner, en applikasjonstype som blir mer og mer populær i arbeidsmarkedet. Vi skal bygge opp et skall av en RAG applikasjon, slik at vi får se på så mye som mulig av den underliggende teknologien som blir brukt. Det vil si, vi skal lære oss grovt hvordan LLMer fungerer, og hva vi får tilbake som resultat når vi konsumerer de i en applikasjon. I tillegg skal vi se på hvordan vi kan kombinere det beste fra C# (multithreading og asynkrone operasjoner), og Python(numbercrunshing, soft-typing) for å lage en robust applikasjon. Håpet er at dere sitter igjen med litt erfaring fra flere moderne verktøy dere selv kan bruke for å lage nye, komplekse og artige applikasjoner som tar ibruk LLMer eller andre AI modeller på en artig og ny måte.
*  Vi skal kikke på Python, og enkelt hvordan syntaksen der fungerer, siden det er blitt det defacto AI språket.
* Vi kommer til å kikke på en ny pakke til C# som heter CSnakes, som lar oss konsumere Python sin C API for å kjøre Python kode i C#, men som også Source genererer interfaces for oss basert på filene vi setter opp. 
* Vi skal se på Ollama rammeverket og hvordan dette fungerer, og hvordan vi kan dockerise en instans av en egen lokal LLM.
* Vi skal kikke på forskjellige vektordatabaseløsninger:
    - ChromaDb, en dokumentdatabase med vector nearness søk. Veldig lett å bruke, og veldig vanlig brukt i prototype / startup fasen av et prosjekt, spesielt hvis mengden data ikke blir så stor. ChromaDb har også bare tooling tilgjengelig i Python og Javascript.
    - Bruke pgVector extension til PostgreSQL for å lage vår egen SQL database med vektorsøkmuligheter. Det betyr vi kan ta i bruk Entity Framework Core verktøyet for asynkrone databaseoperasjoner veldig lett.
* Vi skal se på embedded AI løsninger, som SentenceTransformers, for å lettere kunne vektorisere og gjøre vector nearness søk med en brukerprompt. Vi skal se hvordan vi kan lagre resultatet fra disse i en av løsningene ovenfor til senere bruk. 
* Vi skal så bruke disse verktøyene for å lage en faktisk RAG applikasjon, hvor brukere kan søke etter dokumentinnhold med menneskelige prompts.

## Hva er RAG?

RAG står for Retrieval Augmentation Generation, og er sakte men sikkert blitt en av standard designprinsippene rundt å bruke LLMer for databehandling. Enkelt forklart, bruker vi LLMer for å vektorisere chunks med tekst, og lagre disse. En bruker skriver så en standardisert promt til programmet vårt, som også blir vektorisert. Vi bruker så vektorene fra denne prompten for å finne dokumenter via vector nearness (distance). Disse dokumentene blir så tatt med i orginal prompten for å gi LLMen brukeren prompter mer context enn den vanligvis har.
Håpet er da at en LLM vil gi tilbake et godt svar, selv om den ikke nødvendigvis er trent på dokumentene på forhånd.
Hvor langt er vi kommet?

## hvor langt er vi kommet?

### Bli litt kjent med ollama rammeverket:
- Vi passet på at alle har Docker installert på maskinen sin, samt WSL (Windows Subsystem for Linux).
- Vi gikk inn i Docker bildet sin terminal, og lastet ned Deepseek-r1:1.5b modellen siden den er lettvektig nok til å kjøre på de fleste maskiner. Vi brukte også litt tid på å kikke på de forskjellige andre modellene som er tilgjengelige på Ollama sit model repository. 
- Vi gikk inn i docker bildet sin terminal, og lastet ned deepseek-r1:1.5b modellen siden den er lettvektig nok til å kjøre på de fleste maskiner. Vi brukte også litt tid på å kikke på de forskjellige andre modellene som er tilgjengelige på ollama sit model repository. 
- Vi kikket på responsen vi får tilbake fra LLMen vi kjørte lokalt, både i streaming og non-streaming format. 
- Vi brukte så curl for å interacte med rest-APIET Ollama også eksponerer til oss på localhost:11434
- Vi utvidet så Docker compose filen vår, slik at vi kunne ta i bruk Docker sin cache for å sette opp en setup instans, som passer på at en ønsket modell er lasted ned. Det steget lar oss slippe å installere en ønsket modell via Ollama sin terminal. 

### Kjapp intro til python og CSnakes:
#### example.py
- Vi så hvordan Python ikke trenger noen nøkkelord for variabeldeklarering. Vi skriver et variabelnavn, og assigner en verdi.
- Vi så hvordan vi kan assigne flere forskjellige datatyper til hver variabel.
- Vi så at Python har indentbasert block-scope. I .NET bruker vi {} for å makerer en blokk med isolert kode, i Python bruker vi : etterfulgt av en indent (tab).
- Hvis vi har Python language support fra Microsoft installert, kan vi kjøre Pythonkode linje for linje (default hotkey shift+enter).
Vi kan også markere blokker med Python kode, og kjøre den blokken isolert. Vi så at når vi gjorde dette, har Python koden kun context fra blokken markert, variabelnavn deklarert utenfor blokken med kode vi markerer, vil ikke kunne brukes i den markerte blokken.

#### CSnakes.Runtime
[CSnakes](https://tonybaloney.github.io/CSnakes/getting-started/) er en nuget pakke som lar oss kjøre Python kode i vår C# applikasjon.

Den gjør dette ved først, å bruke source generation for å automatisk lage ferdig typet interfaces som representerer en python fil. Den bruker så disse interfacene for å kalle på C-APIet til Python via en Pythonruntime referanse i CLR.
Den kommer også med mangen handye tools som gjør det lett å håndtere. bl.a. er dependency injection extension fra Microsoft inkludert i pakken som standard, så vi slipper å importere det selv.

Det lar oss opprette en ApplicationBuilder instanse i vår console app (linje 7 i program.cs)

```cs
var builder = Host.CreateApplicationBuilder();
```

Så må vi definere hvor Pythonfilene våre ligger.
Det gjør vi ved å først inkludere alle Pythonfilene i vår csproj fil:

```xml
<ItemGroup>

    <AdditionalFiles Include="./PythonFiles/*.py">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AdditionalFiles>

```
Legg merke til at vi bruker AdditionalFiles taggen her, og ikke None eller Content. AdditionalFiles taggen brukes av sourcegeneration enginen i C# for å finne kildekoden den skal bruke for å generere ny C# kildekode. Vi inkluderer også en CopyToOutputDirectory tag, som forteller at contentet skal inkluderes når applikasjonen bygges.
Vi kan da referere til den koden som et pythonHome i vår kildekode:

```cs
var pythonHome = Path.Join(Environment.CurrentDirectory, "./PythonFiles");
```

Legg merke til at vi bruker Environment.CurrentDirectory her, da slipper vi å tenke på om programmet kjører i Release eller Debug mode.

Vi kan da legge til Python runtimen, og all source generert kode i dependency containeren vår, via builderen:

```cs
builder.Services.WithPython().WithHome(pythonHome).FromRedistributable();

var app = builder.Build();
```
Dette betyr at andre ting vi legger i dependency containeren vår, f.eks en LLM service eller tilsvarende, kan ta inn og konsumere en Pythonfil som en Pythonmodul via dette python environmentet. På samme måte som når vi henter inn en service i en Controller i WebApi med controllere. 

For å demonstrere, henter vi bare ut servicen igjen med en gang fra app:

```cs
var pythonEnvironment = app.Services.GetRequiredService<IPythonEnvironment>();
```
Her henter vi ut en referanse til vårt Python environment, som vi satt opp i builderen vår. Inni denne interfacen er det også referanser til hver Pythonfil, som det er blitt generert source code for.

```cs

var exampleModule = pythonEnvironment.Example();
```
Legg merke til at metodenavnet over, refererer til Python filnavnet, i dette tilfellet refererer det til example.py.
Det er blitt laget en interface tilgjengelig i IPythonEnvironmentet vi kan bruke, som er kartlagt for hver fil. 
Vi kan så kalle på funksjoner som er definert der. Der har vi allerede laget en funksjon som heter sum_two_numbers, som tar inn to tall, og returnerer et helt tall.

Vi kan så bruke denne funksjonen i C#, ved å kalle den direkte:

```cs
var result = exampleModule.SumTwoNumbers(1,2);
```
En liten særhet rundt dette er at dere vil se at result er typet som long. Det er fordi ints i Python blir konvertert til 64 bit integers (long) i C# via CSnakes. Det er fordi Python sine integers ikke egentlig har en øvre eller nedre grense, men utviklerene av CSnakes gjør noe patternmatching i bakgrunnen, som gjør at tall enten returneres som long (64 bit integers), eller som BigInt(), som er en datastruktur i C#, som likt Python, er uten øvre grense.
Legg merke til at denne interfacen faktisk kaller Python kode direkte. Den lager ikke egne C# classer som fungerer likt som Python filene vi skriver, den bruker interfacen for å kalle Python direkte i CLR til C#.
Det finnes også en kraftig .NET pakke som heter [Python.NET](https://pythonnet.github.io/), men der må du lage og definere disse interfacene, samt hvordan behandle GIL (Global Interpeter Lock) selv. Det krever mye forståelse av hvordan Python sin kildekode faktisk kjører på en maskin, men det er tilgjengelig for de som er spesielt interessert.


#### Virtual Environments og PipInstaller / package management.
Python har i dag veldig mye tooling rundt LLM. Mye bedre og mer robuste tools enn tilgjengelig for .NET. Vi kan dra nytte av disse i vår kode også. CSnakes tillater å sette opp det som heter et virtuelt Python environment, det vil si et sted som kan "holde staten" til din Python instanse. Der kan vi bl.a. laste ned ekstrapakker, tilsvarende nuget til dotnet, som gir oss ferdiglaget verktøy for forskjellige oppgaver i Python.
For å gjøre dette må vi utvide dependency injection av Python instansen vår litt. Vi må sette opp, og inkludere en referanse til et Python environment, samt referere til en package manager som Python kan bruke. Python har flere, men den vanligste er Pip.
CSnakes har gjort dette veldig lett:

```cs
builder.Services.WithPython().WithHome(pythonHome).WithPipInstaller().WithVirtualEnvironment(Path.Join(pythonHome, ".venv")).FromRedistributable();
```
Legg merke til at neste gang vi kjører programmet vårt, vil det dukke opp en .venv folder i PythonFiles mappen vår. Denne kommer til å inneholde masse kildekode som ikke trenger å være med i repoet vårt, så vi kan gjerne legge .venv mappen i GitIgnore. 

For å inkludere spesifikke pakker i Python, leter PipInstalleren etter en fil som heter requirements.txt Det er en newline separert liste av pakkenavn. Vi kan lage en requirements.txt fil i vår PythonFiles mappe, så legge til noen gøye pakker vi kan jobbe med. Som med våre Python filer, må denne også refereres til i csproj filen vår:

```xml
  <ItemGroup>

    <AdditionalFiles Include="./PythonFiles/*.py">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AdditionalFiles>

    <AdditionalFiles Include="./PythonFiles/requirements.txt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AdditionalFiles>

  </ItemGroup>
```

Legg også merke til \*.py, som er et patternmatching shortcut tilgjengelig, for å referere til samtlige filer i ./PythonFiles mappen.


Inkludert i dette prosjektet så langt, har vi sett på både en Ollama pakke, og en vektordatabase som heter ChromaDb. Disse er begge inkludert i dette repoet sin requirements.txt så langt.

#### ChromaDb og vektorer

Når vi skal tillate naturlig språk for å søke gjennom et datasett (det som ofte heter et semantisk søk), må vi ha en måte å simplifisere dette søket på. Løsningen fra AI verden, er å vektorisere både dokumentene, og språket vårt via en embedded LLM. ChromaDb har dette innebygget, når vi adder dokumenter til databasen, blir disse vektorisert, samme med hvis vi gjør et tekst-søk.


Så hva betyr egentlig dette med vektorisering?
En vektor, enkelt forklart, er bare en samling av tall: [1,2,3,4,5,6,7,9,123]. Når vi mater tekst til en LLM, blir den teksten gjort om til en samling av tall, basert på treningen LLMen. La oss si vi mater en LLM ordet Dog:
La oss si vi mater en llm ordet Dog:

```bash
    "Dog" -> [0.3, 0.7, -0.2, 0.9]
```

Vi ser vi får tilbake et sett med tall, som representerer hvordan ordet Dog er representert av LLMen sinn opplæring. Hvis LLMen er trent på språk, vil den også vektlegge ord med tilsvarende eller lik betydning "likt". La oss si vi mater den ordet Puppy:

```bash
    "Puppy" ->  [0.4, 0.6, -0.1, 0.8]
```
Legg merke til at selv om teksten Dog og Puppy er svært annerledes, er tallene veldig lik. Det er fordi i treningen til LLMen er disse ofte brukt i samme context, og i samme betydning. Det betyr disse ordene er svært "nær hverandre", tallene i samlingen er av svært lik verdi. 


Når vi gjør søk mot dokumenter i ChromaDb, vil vi også få tilbake en distanse mellom spørringen, og potensielle dokumenter i return. I chromadb_test.py har vi satt opp at vi vil ha to dokumenter tilbake fra en spørring. I denne modulen bruker vi bare en in memory utgave av ChromaDb, som betyr vi må seede den hver gang. I modulen vår chromadb_test.py ser dere at vi seeder den med fire små tekstblokker, og lager et enkelt kall mot db via en innkommende "prompt".


Vi kan så bruke den i Program.cs for å se hva vi vårtilbake fra ChromaDb.

```cs

var chromaDbModule = pythonEnvironment.ChromadbTest();

var chromaTest = chromaDbModule.CreateAndFetchFromChromadb("which document contains information about fruits?");
```

```python
{'ids': [['0', '1']], 'embeddings': None, 'documents': [['Testdocument containing information about the import of fruit into the norwegian market!', 'Testdocument containing information about creating llms and working with vectorisation across a set of documents']], 'uris': None, 'included': ['metadatas', 'documents', 'distances'], 'data': None, 'metadatas': [[None, None]], 'distances': [[0.7562352418899536, 1.4355132579803467]]}
```

Legg merke til at, i tillegg til to dokumentforslag i "documents" nøkkelen, har vi også et sett med tall som heter "distances". tallene der, representerer hvor "lang avstand" det er mellom vektorene for dokumentene våre, og vektoren til spørringen. Jo nærmere 0, jo bedre. Her ser vi, når vi spør "which document contains information about fruits?", at dokumentet som er "nærmest" i distanse er 'Testdocument containing information about the import of fruit into the norwegian market!'. Viktig å vite, at embedded LLMer er ofte trent på engelsk språklig plain text. En av problemstillingene vi får senere, vil være å lage om et datasett til plain text.  

