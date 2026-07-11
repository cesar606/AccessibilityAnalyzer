# Avaluador estàtic d'accessibilitat per a interfícies WPF/XAML

Treball de Fi de Grau — Grau en Enginyeria Informàtica
Escola Politècnica Superior, Universitat de Lleida

**Autor:** Cesar Gallardo Rodriguez

---

## Descripció

Eina d'escriptori que analitza **estàticament** fitxers XAML d'aplicacions WPF per detectar incompliments d'accessibilitat, sense necessitat de compilar ni executar l'aplicació analitzada.

A diferència de les solucions existents en l'àmbit de l'escriptori Windows —que inspeccionen l'aplicació en execució a través de l'API *UI Automation*—, aquesta eina desplaça la detecció al **codi font** i al **moment del desenvolupament**, seguint el principi *shift-left*.

Les incidències detectades es classifiquen segons el grau de confiança de la detecció (error, advertiment o revisió manual) i són traçables al marc normatiu europeu: **WCAG 2.2**, **WCAG2ICT** i **EN 301 549**.

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

---

## Arquitectura

El projecte se separa en dos mòduls independents:

```
AccessibilityAnalyzer/
├── AccessibilityAnalyzer.Core/     Motor d'anàlisi (biblioteca de classes)
│   ├── Models/                     Model de domini
│   ├── Parsing/                    Lectura i recorregut del XAML
│   ├── Rules/                      Implementació de les regles R1–R7
│   └── Analysis/                   Càlculs auxiliars (contrast de color)
│
└── AccessibilityAnalyzer.App/      Interfície d'usuari (aplicació WPF)
    └── TestData/                   Fitxers XAML de prova
```

**Per què dos projectes?** El motor no depèn de la interfície, cosa que permet:

- provar cada regla de manera aïllada, sense obrir cap finestra;
- reutilitzar el motor des d'altres entorns (per exemple, integració contínua);
- modificar la interfície sense afectar la lògica d'anàlisi.

La dependència és unidireccional: `App → Core`. El motor no sap res de la interfície.

### Components principals

| Component | Responsabilitat |
|-----------|-----------------|
| `XamlParser` | Converteix el fitxer XAML en un arbre de controls, conservant la jerarquia i el número de línia de cada element. |
| `IAccessibilityRule` | Contracte comú de totes les regles. Afegir una regla nova consisteix a crear una classe que l'implementi. |
| `AccessibilityAnalyzerEngine` | Coordina l'anàlisi: invoca el parser i aplica totes les regles actives. |
| `AnalysisSettings` | Llindars configurables (mida mínima de lletra, ràtio de contrast, mida mínima de l'objectiu). |
| `ColorUtils` | Càlcul de luminància relativa i ràtio de contrast segons la fórmula de WCAG 2.2. |

---

## Decisions de disseny

**Anàlisi estàtica sobre XML.** XAML és, en essència, un document XML, de manera que es recorre amb `System.Xml.Linq` sense executar l'aplicació. No calia implementar cap analitzador lèxic propi.

**Tres categories d'incidència.** L'anàlisi estàtica no pot comprovar tots els criteris d'accessibilitat. En comptes d'ignorar-ho o de reportar falsos positius, l'eina distingeix:

- **Error** — la comprovació és determinista i l'incompliment és cert.
- **Advertiment** — molt probablement és un problema, però convé revisar-ho.
- **Revisió manual** — el valor es resol en temps d'execució (temes, *bindings*, recursos) i l'anàlisi estàtica no pot decidir-ho.

Aquesta distinció evita transmetre una falsa sensació de compliment, un problema habitual en les eines d'avaluació automàtica.

**Severitat i categoria són eixos independents.** La severitat mesura l'impacte sobre l'usuari; la categoria, la confiança de la detecció. Un mateix problema greu pot ser un error o requerir revisió manual segons si els valors es poden resoldre estàticament.

**Llindars configurables.** Els valors per defecte són els que exigeix la normativa (12 px de lletra, ràtio 4.5:1, objectius de 24×24 px), però l'usuari pot ajustar-los.

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

- [x] Estructura del projecte (Core + App)
- [x] Parser de XAML amb jerarquia i informació de línia
- [x] Regles R1–R7 del catàleg
- [x] Llindars configurables
- [ ] Model de l'informe i puntuació
- [ ] Interfície d'auditoria
- [ ] Exportació de l'informe
- [ ] Conjunt de casos de prova i validació

---

## Llicència

Treball acadèmic. Universitat de Lleida.