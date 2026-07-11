# Avaluador estàtic d'accessibilitat per a interfícies WPF/XAML

Treball de Fi de Grau — Grau en Enginyeria Informàtica  
Escola Politècnica Superior, Universitat de Lleida

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

---

## Les tres categories d'incidència

L'anàlisi estàtica no pot comprovar tots els criteris d'accessibilitat. En comptes d'ignorar-ho o de reportar falsos positius, l'eina distingeix tres categories segons el **grau de confiança** de la detecció:

| Categoria | Significat | Exemple |
|-----------|------------|---------|
| **Error** | La comprovació és determinista: l'incompliment és cert. | `Foreground="#AAAAAA"` sobre fons blanc → contrast 2.32:1 |
| **Advertiment** | Molt probablement és un problema, però convé revisar-ho. | Dos controls amb el mateix nom accessible |
| **Revisió manual** | El valor es resol en temps d'execució i l'eina no pot decidir-ho. | `Foreground="{StaticResource ColorText}"` |

Aquesta distinció és deliberada: una eina que no reconeix els seus propis límits transmet una **falsa sensació de compliment**.

---

## Càlcul de la puntuació

La puntuació va de 0 a 100 i es calcula **només sobre allò que s'ha pogut verificar estàticament**.

**Ponderació per severitat.** Cada incidència penalitza segons el seu impacte real sobre l'usuari:

| Severitat | Penalització | Raonament |
|-----------|--------------|-----------|
| Greu | 10 | Pot **impedir** l'accés a la funcionalitat (p. ex. un botó invisible per a un lector de pantalla). |
| Moderada | 4 | **Dificulta** l'ús sense impedir-lo (p. ex. lletra massa petita). |
| Lleu | 1 | Molèstia menor. |

**Penalització relativa a la mida del fitxer.** Deu errors en una finestra de 12 controls són molt més greus que deu errors en una aplicació de 500. El divisor representa el pitjor escenari possible: que **tots** els controls tinguessin un error greu.

```
puntuació = 100 × (1 − penalització_total / (nombre_de_controls × 10))
```

**Les incidències de revisió manual NO penalitzen.** Penalitzar-les seria injust (poden ser correctes) i ignorar-les seria perillós (amagaria els límits de l'anàlisi). Per això es mostren **sempre al costat de la puntuació**, amb un avís explícit.

> La puntuació mesura *el que s'ha pogut verificar*; la revisió manual mesura *el que queda per verificar*. Barrejar-les falsejaria totes dues.

---

## Arquitectura

```
AccessibilityAnalyzer/
├── AccessibilityAnalyzer.Core/     Motor d'anàlisi (biblioteca de classes)
│   ├── Models/                     Model de domini
│   ├── Parsing/                    Lectura i recorregut del XAML
│   ├── Rules/                      Implementació de les regles R1–R7
│   └── Analysis/                   Càlcul de contrast i de puntuació
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
| `IAccessibilityRule` | Contracte comú de totes les regles. Afegir una regla nova consisteix a crear una classe que l'implementi, sense tocar el motor. |
| `AccessibilityAnalyzerEngine` | Coordina l'anàlisi: invoca el parser, aplica les regles i genera l'informe. |
| `AnalysisReport` | Resultat complet: incidències, comptadors per categoria i puntuació. |
| `ScoreCalculator` | Càlcul de la puntuació ponderada. |
| `ColorUtils` | Luminància relativa i ràtio de contrast segons la fórmula de WCAG 2.2. |
| `AnalysisSettings` | Llindars configurables (mida de lletra, ràtio de contrast, mida de l'objectiu). |

---

## Decisions de disseny

**Anàlisi estàtica sobre XML.** XAML és, en essència, un document XML, de manera que es recorre amb `System.Xml.Linq` sense executar l'aplicació. L'aportació del treball són les **regles d'accessibilitat**, no el *parsing*.

**Severitat i categoria són eixos independents.** La severitat mesura l'impacte sobre l'usuari; la categoria, la confiança de la detecció. Un mateix problema greu pot ser un `Error` (si els colors són literals) o `RevisioManual` (si depenen d'un tema).

**Resolució del fons heretat.** Un control sense `Background` explícit hereta el del seu contenidor. El parser reconstrueix la jerarquia pare-fill perquè la regla de contrast pugui pujar per l'arbre fins a trobar qui declara el fons.

**Llindars configurables.** Els valors per defecte són els que exigeix la normativa (12 px de lletra, ràtio 4.5:1, objectius de 24×24 px), però es poden ajustar.

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
- [x] Model de l'informe i càlcul de puntuació
- [ ] Interfície d'auditoria
- [ ] Exportació de l'informe
- [ ] Conjunt de casos de prova i validació

---

## Llicència

Treball acadèmic. Universitat de Lleida.