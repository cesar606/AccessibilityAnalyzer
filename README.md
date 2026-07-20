# Avaluador estàtic d'accessibilitat per a interfícies WPF/XAML

Treball de Fi de Grau — Grau en Enginyeria Informàtica  
Escola Politècnica Superior, Universitat de Lleida — Campus Igualada-UdL

**Autor:** Cesar Gallardo Rodriguez

---

## Descripció

Eina d'escriptori que analitza **estàticament** fitxers XAML d'aplicacions WPF per detectar incompliments d'accessibilitat, sense necessitat de compilar ni executar l'aplicació analitzada.

A diferència de les solucions existents en l'àmbit de l'escriptori Windows —que inspeccionen l'aplicació en execució a través de l'API *UI Automation*—, aquesta eina desplaça la detecció al **codi font** i al **moment del desenvolupament**, seguint el principi *shift-left*.

Les incidències detectades es classifiquen segons el grau de confiança de la detecció i són traçables al marc normatiu europeu: **WCAG 2.2**, **WCAG2ICT** i **EN 301 549**.

---

## Catàleg de regles

| ID | Regla | Criteri WCAG | Severitat |
|----|-------|--------------|-----------|
| R1 | Nom accessible absent | 4.1.2 (A) | Greu |
| R2 | Alternativa textual absent | 1.1.1 (A) | Greu |
| R3 | Nom buit o duplicat | 4.1.2 (A) | Greu / Moderada |
| R4 | Contrast insuficient | 1.4.3 (AA) | Greu |
| R5 | Mida de lletra petita | 1.4.4 (AA) | Moderada |
| R6 | Operabilitat per teclat | 2.1.1 / 2.4.3 (A) | Greu / Moderada |
| R7 | Mida de l'objectiu insuficient | 2.5.8 (AA) | Moderada |
| R8 | Colors indistingibles per daltonisme | 1.4.1 (A) | Moderada |

La regla **R8** simula els principals tipus de daltonisme (protanopia, deuteranopia i tritanopia) mitjançant matrius de transformació de color i detecta els parells de colors que es tornen indistingibles. Es classifica sempre com a *revisió manual*, ja que l'eina pot determinar que dos colors es confonen, però no si s'utilitzen per transmetre informació.

---

## Les tres categories d'incidència

L'anàlisi estàtica no pot comprovar tots els criteris d'accessibilitat. En comptes d'ignorar-ho o de reportar falsos positius, l'eina distingeix tres categories segons el **grau de confiança** de la detecció:

| Categoria | Significat | Exemple |
|-----------|------------|---------|
| **Error** | La comprovació és determinista: l'incompliment és cert. | `Foreground="#AAAAAA"` sobre fons blanc → contrast 2.32:1 |
| **Advertiment** | Molt probablement és un problema, però convé revisar-ho. | Dos controls amb el mateix nom accessible |
| **Revisió manual** | El valor o el seu significat es resolen fora de l'anàlisi estàtica. | `Foreground="{StaticResource ColorText}"` · colors confusos per daltonisme |

Aquesta distinció és deliberada: una eina que no reconeix els seus propis límits transmet una **falsa sensació de compliment**.

---

## Càlcul de la puntuació

La puntuació va de 0 a 100 i es calcula **només sobre allò que s'ha pogut verificar estàticament**.

**Ponderació per severitat.** Cada incidència penalitza segons el seu impacte real sobre l'usuari:

| Severitat | Penalització | Raonament |
|-----------|--------------|-----------|
| Greu | 10 | Pot **impedir** l'accés a la funcionalitat. |
| Moderada | 4 | **Dificulta** l'ús sense impedir-lo. |
| Lleu | 1 | Molèstia menor. |

**Penalització relativa a la mida del fitxer.** El divisor representa el pitjor escenari possible: que **tots** els controls tinguessin un error greu.

```
puntuació = 100 × (1 − penalització_total / (nombre_de_controls × 10))
```

**Les incidències de revisió manual NO penalitzen.** Es mostren sempre al costat de la puntuació, amb un avís explícit.

> La puntuació mesura *el que s'ha pogut verificar*; la revisió manual mesura *el que queda per verificar*.

---

## Arquitectura

```
AccessibilityAnalyzer/
├── AccessibilityAnalyzer.sln
├── dataset/                            Fitxers de validació amb ground truth
│
├── AccessibilityAnalyzer.Core/         Motor d'anàlisi (biblioteca de classes)
│   ├── Models/                         Model de domini i configuració
│   ├── Parsing/                        Lectura i recorregut del XAML
│   ├── Rules/                          Implementació de les regles R1–R8
│   ├── Analysis/                       Contrast, puntuació i simulació de daltonisme
│   └── Reporting/                      Generació de l'informe HTML
│
├── AccessibilityAnalyzer.App/          Interfície d'usuari (aplicació WPF)
│   ├── Controls/                       Indicador circular de puntuació
│   └── TestData/                       Fitxer XAML de prova ràpida
│
└── AccessibilityAnalyzer.Tests/        Proves unitàries (xUnit)
```

**Per què tres projectes?** El motor (`Core`) no depèn de la interfície (`App`), cosa que permet provar cada regla de manera aïllada, reutilitzar el motor des d'altres entorns i modificar la interfície sense afectar la lògica. Els tests (`Tests`) referencien només el motor. Les dependències són unidireccionals: `App → Core` i `Tests → Core`.

### Components principals

| Component | Responsabilitat |
|-----------|-----------------|
| `XamlParser` | Converteix el XAML en un arbre de controls, amb jerarquia i número de línia. |
| `IAccessibilityRule` | Contracte comú de totes les regles. Afegir-ne una consisteix a crear una classe. |
| `AccessibilityAnalyzerEngine` | Coordina l'anàlisi: parser, regles i generació de l'informe. Permet activar o desactivar regles. |
| `AnalysisReport` | Resultat complet: incidències, comptadors per categoria i puntuació. |
| `FolderAnalysisReport` | Resultat agregat de l'anàlisi d'un directori complet. |
| `ScoreCalculator` | Càlcul de la puntuació ponderada. |
| `ColorUtils` | Luminància relativa i ràtio de contrast segons WCAG 2.2. |
| `ColorBlindnessSimulator` | Simulació dels tipus de daltonisme mitjançant matrius de transformació. |
| `AnalysisSettings` | Llindars configurables (mida de lletra, ràtio de contrast, mida de l'objectiu). |
| `HtmlReportGenerator` | Exportació de l'informe com a document HTML accessible. |

---

## Funcionalitats

- Càrrega i anàlisi de fitxers XAML individuals sense executar l'aplicació.
- Anàlisi de directoris complets amb informe agregat i rànquing de fitxers.
- Detecció de vuit tipus d'incidència d'accessibilitat (R1–R8).
- Classificació per severitat i per grau de confiança (error / advertiment / revisió manual).
- Informe amb puntuació global, comptadors i detall per regla, amb la ubicació de cada incidència.
- Indicador circular de puntuació.
- Vista desplegable de les incidències per fitxer en l'anàlisi de directoris.
- Exportació de l'informe en format HTML autocontingut i accessible.
- Configuració dels llindars d'anàlisi amb vista prèvia en temps real.
- Activació i desactivació individual de regles.

---

## Decisions de disseny

**Anàlisi estàtica sobre XML.** XAML és, en essència, un document XML; es recorre amb `System.Xml.Linq` sense executar l'aplicació. L'aportació del treball són les **regles d'accessibilitat**, no el *parsing*.

**Severitat i categoria són eixos independents.** La severitat mesura l'impacte sobre l'usuari; la categoria, la confiança de la detecció.

**Resolució del fons heretat.** El parser reconstrueix la jerarquia pare-fill perquè la regla de contrast pugui pujar per l'arbre fins a trobar qui declara el fons.

**La pròpia eina és accessible.** La interfície compleix els criteris que la mateixa eina comprova (*dogfooding*): analitzada amb ella mateixa, obté el 100 %. Durant el desenvolupament, l'eina va detectar tres incompliments reals de contrast a la seva pròpia interfície, que van ser corregits.

---

## Validació

L'eina s'ha validat mitjançant dues estratègies complementàries:

**Dataset amb ground truth.** Tres fitxers XAML dissenyats amb incidències conegudes. L'eina els detecta tots sense cap fals positiu ni cap fals negatiu (precisió i exhaustivitat del 100 %).

**XAML de projectes reals.** Deu fitxers descarregats de repositoris de codi obert (microsoft/WPF-Samples, syncfusion/wpf-demos, PrismLibrary/Prism-Samples-Wpf). Dels deu, sis obtenen un 100 % net (zero falsos positius) i en quatre es detecten incidències reals, incloent-hi 11 errors d'accessibilitat en un exemple del directori Accessibility del propi repositori de Microsoft.

**Proves unitàries.** 27 tests automatitzats amb xUnit, cobreixen el parser, el càlcul de contrast, la puntuació, cada regla (amb casos de detecció i d'absència de falsos positius) i el motor complet (inclosa la desactivació de regles).

---

## Requisits

- Windows
- .NET 10.0 (LTS)
- Visual Studio 2026 amb la càrrega de treball *Desenvolupament d'escriptori de .NET*

---

## Execució

1. Obrir `AccessibilityAnalyzer.sln` amb Visual Studio.
2. Establir `AccessibilityAnalyzer.App` com a projecte d'inici.
3. Executar (F5).

---

## Estat del desenvolupament

- [x] Estructura del projecte (Core + App + Tests)
- [x] Parser de XAML amb jerarquia i informació de línia
- [x] Regles R1–R8 del catàleg (incloent-hi simulació de daltonisme)
- [x] Model de l'informe i càlcul de puntuació
- [x] Interfície d'auditoria amb indicador de puntuació i desplegables
- [x] Exportació de l'informe en HTML
- [x] Configuració de llindars amb vista prèvia en viu i activació de regles (RF6)
- [x] Anàlisi de directoris complets amb rànquing de fitxers
- [x] 27 proves unitàries (xUnit)
- [x] Conjunt de casos de prova i validació amb XAML reals

---

## Llicència

Treball acadèmic. Universitat de Lleida.