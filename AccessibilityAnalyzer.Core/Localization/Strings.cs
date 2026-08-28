// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.Core.Localization
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides all user-facing strings in the active language.
    /// </summary>
    public static class Strings
    {
        private static Language _current = Language.Catala;

        /// <summary>
        /// Gets or sets the active language.
        /// </summary>
        public static Language Current
        {
            get => _current;
            set => _current = value;
        }

        /// <summary>
        /// Returns the translation for the given key in the active language.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <returns>The translated string, or the key itself if not found.</returns>
        public static string Get(string key)
        {
            if (Translations.TryGetValue(_current, out Dictionary<string, string>? lang)
                && lang.TryGetValue(key, out string? value))
            {
                return value;
            }

            // Fallback to Catalan.
            if (Translations.TryGetValue(Language.Catala, out Dictionary<string, string>? fallback)
                && fallback.TryGetValue(key, out string? fallbackValue))
            {
                return fallbackValue;
            }

            return key;
        }

        private static readonly Dictionary<Language, Dictionary<string, string>> Translations = new()
        {
            [Language.Catala] = new Dictionary<string, string>
            {
                // UI - Main Window
                ["LoadFile"] = "Carregar fitxer",
                ["LoadFolder"] = "Analitzar carpeta",
                ["ExportHtml"] = "Exportar HTML",
                ["Settings"] = "Configuració",
                ["About"] = "Sobre l'eina",
                ["NoFileLoaded"] = "Cap fitxer carregat",
                ["IssuesDetected"] = "Incidències detectades",
                ["Summary"] = "Resum",
                ["Accessible"] = "accessible",
                ["Average"] = "mitjana",
                ["WelcomeTitle"] = "Avaluador d'accessibilitat WPF/XAML",
                ["WelcomeMessage"] = "Carrega un fitxer XAML o una carpeta per començar, o arrossega'l directament a aquesta finestra.",
                ["ControlsAnalysed"] = "{0} controls analitzats",
                ["AverageOf"] = "Puntuació mitjana de {0} fitxers",
                ["FilesAnalysed"] = "{0} fitxers analitzats",
                ["ManualWarning"] = "Les revisions manuals no es tenen en compte en la puntuació: cal comprovar-les a mà.",
                ["FileRanking"] = "Fitxers ordenats per puntuació (de menor a major)",
                ["RootFolder"] = "Arrel",
                ["NoIssues"] = "No s'ha detectat cap incidència en aquest fitxer.",
                ["NoXamlFound"] = "No s'ha trobat cap fitxer XAML en aquesta carpeta.",
                ["XamlOnly"] = "Només es poden analitzar fitxers XAML o carpetes que en continguin.",
                ["SettingsUpdated"] = "La configuració s'ha actualitzat. Torna a analitzar el fitxer per aplicar-la.",

                // UI - Counters
                ["Error_s"] = "error",
                ["Error_p"] = "errors",
                ["Warning_s"] = "advertiment",
                ["Warning_p"] = "advertiments",
                ["Manual_s"] = "revisió manual",
                ["Manual_p"] = "revisions manuals",

                // Categories
                ["CategoryError"] = "ERROR",
                ["CategoryWarning"] = "ADVERTIMENT",
                ["CategoryManual"] = "REVISIÓ MANUAL",
                ["Line"] = "línia",

                // Settings Window
                ["SettingsTitle"] = "Configuració de l'anàlisi",
                ["Thresholds"] = "Llindars d'anàlisi",
                ["ThresholdsDesc"] = "Ajusta els valors mínims que l'eina considerarà acceptables.",
                ["MinFontSize"] = "Mida mínima de lletra (px):",
                ["MinContrast"] = "Ràtio mínim de contrast:",
                ["MinTargetSize"] = "Mida mínima de l'objectiu (px):",
                ["ActiveRules"] = "Regles actives",
                ["ResetDefaults"] = "Restaurar valors per defecte",
                ["Accept"] = "Acceptar",
                ["InvalidValues"] = "Revisa els valors:\n\n· Mida de lletra: entre 1 i 72 px.\n· Ràtio de contrast: entre 1 i 21.\n· Mida de l'objectiu: entre 1 i 100 px.",
                ["InvalidValuesTitle"] = "Valors fora de rang",
                ["FontPreview"] = "Exemple a {0} px",
                ["ContrastPreview"] = "Contrast {0}:1",
                ["LanguageLabel"] = "Idioma:",

                // Export
                ["ExportSaved"] = "Informe desat correctament.\n\nVols obrir-lo ara?",
                ["ExportDone"] = "Exportació completada",
                ["ExportError"] = "No s'ha pogut desar l'informe.",
                ["ExportErrorTitle"] = "Error d'escriptura",
                ["XmlError"] = "El fitxer no és un XAML vàlid.",
                ["XmlErrorTitle"] = "Error de format",
                ["ReadError"] = "No s'ha pogut llegir el fitxer.",
                ["ReadErrorTitle"] = "Error de lectura",

                // Rules
                ["R1.Name"] = "Nom accessible absent",
                ["R1.Message"] = "El control '{0}' no exposa cap nom accessible.",
                ["R2.Name"] = "Alternativa textual absent",
                ["R2.Message"] = "L'element gràfic '{0}' no proporciona cap alternativa textual.",
                ["R3.Name"] = "Nom buit o duplicat",
                ["R3.Empty"] = "El control '{0}' declara un nom accessible buit.",
                ["R3.Duplicate"] = "El nom accessible '{0}' està duplicat en més d'un control.",
                ["R4.Name"] = "Contrast insuficient",
                ["R4.Error"] = "El contrast del control '{0}' és de {1}, inferior al mínim exigit ({2}).",
                ["R4.Manual"] = "No es pot determinar el color del text del control '{0}' de manera estàtica: cal revisió manual.",
                ["R5.Name"] = "Mida de lletra petita",
                ["R5.Message"] = "La mida de lletra ({0}) del control '{1}' és inferior al mínim recomanat ({2}).",
                ["R6.Name"] = "Operabilitat per teclat",
                ["R6.TabStop"] = "El control '{0}' està exclòs de la navegació per teclat (IsTabStop=\"False\").",
                ["R6.Focusable"] = "El control '{0}' no és enfocable per teclat (Focusable=\"False\").",
                ["R7.Name"] = "Mida de l'objectiu insuficient",
                ["R7.Width"] = "L'amplada del control '{0}' ({1}) és inferior al mínim de {2} px.",
                ["R7.Height"] = "L'alçada del control '{0}' ({1}) és inferior al mínim de {2} px.",
                ["R8.Name"] = "Colors indistingibles per daltonisme",
                ["R8.Message"] = "Els colors {0} i {1} es poden confondre sota {2}. Si transmeten informació pel color, cal revisar-ho.",
                ["R8.Protanopia"] = "protanopia",
                ["R8.Deuteranopia"] = "deuteranopia",
                ["R8.Tritanopia"] = "tritanopia",

                // HTML Report
                ["HtmlTitle"] = "Informe d'accessibilitat",
                ["HtmlFolderTitle"] = "Informe d'accessibilitat de directori",
                ["HtmlFileAnalysed"] = "Fitxer analitzat",
                ["HtmlFilesAnalysed"] = "fitxers analitzats",
                ["HtmlControls"] = "controls",
                ["HtmlAverageScore"] = "puntuació mitjana",
                ["HtmlConfirmedErrors"] = "errors confirmats",
                ["HtmlWarnings"] = "advertiments",
                ["HtmlManualReview"] = "elements per revisar manualment",
                ["HtmlManualNote"] = "Les revisions manuals no es tenen en compte en la puntuació: corresponen a valors que es resolen en temps d'execució i que l'anàlisi estàtica no pot verificar. Cal comprovar-les manualment.",
                ["HtmlNoIssues"] = "No s'ha detectat cap incidència en aquest fitxer.",
                ["HtmlCleanFolder"] = "Cap incidència detectada.",
                ["HtmlFileRanking"] = "Fitxers ordenats per puntuació",
                ["HtmlScoreLabel"] = "Puntuació d'accessibilitat: {0} per cent",
                ["HtmlAverageLabel"] = "Puntuació mitjana d'accessibilitat: {0} per cent",
                ["HtmlFooter"] = "Generat el {0} per l'Avaluador estàtic d'accessibilitat per a interfícies WPF/XAML.",
                ["HtmlNormative"] = "Criteris basats en WCAG 2.2, WCAG2ICT i EN 301 549.",
                ["HtmlIssueCount"] = "incidència/es",

                // About
                ["AboutText"] = "Avaluador estàtic d'accessibilitat per a interfícies WPF/XAML\n"
                    + "Treball de Fi de Grau — Cesar Gallardo Rodriguez\n"
                    + "Universitat de Lleida — Campus Igualada-UdL\n\n"
                    + "L'eina analitza fitxers XAML d'aplicacions WPF sense executar-les "
                    + "i hi detecta incompliments d'accessibilitat traçables a la normativa "
                    + "europea: WCAG 2.2, WCAG2ICT i EN 301 549.\n\n"
                    + "Com interpretar els resultats:\n\n"
                    + "• ERROR — Incompliment confirmat. Cal corregir-lo.\n"
                    + "• ADVERTIMENT — Molt probable, però convé revisar-lo.\n"
                    + "• REVISIÓ MANUAL — L'eina no pot decidir-ho: cal verificació humana.\n\n"
                    + "La puntuació (0–100) reflecteix només el que s'ha pogut verificar "
                    + "estàticament. Les revisions manuals NO penalitzen la puntuació, "
                    + "però no s'han d'ignorar.",

                ["DarkMode"] = "Mode fosc",
                ["LightMode"] = "Mode clar",
                ["ExportJson"] = "Exportar JSON",
                ["ImportReport"] = "Importar informe",
                ["InvalidReport"] = "El fitxer no conté un informe vàlid.",
                ["InvalidReportTitle"] = "Format no reconegut",
            },

            [Language.Castella] = new Dictionary<string, string>
            {
                ["LoadFile"] = "Cargar fichero",
                ["LoadFolder"] = "Analizar carpeta",
                ["ExportHtml"] = "Exportar HTML",
                ["Settings"] = "Configuración",
                ["About"] = "Sobre la herramienta",
                ["NoFileLoaded"] = "Ningún fichero cargado",
                ["IssuesDetected"] = "Incidencias detectadas",
                ["Summary"] = "Resumen",
                ["Accessible"] = "accesible",
                ["Average"] = "media",
                ["WelcomeTitle"] = "Evaluador de accesibilidad WPF/XAML",
                ["WelcomeMessage"] = "Carga un fichero XAML o una carpeta para comenzar, o arrástralo directamente a esta ventana.",
                ["ControlsAnalysed"] = "{0} controles analizados",
                ["AverageOf"] = "Puntuación media de {0} ficheros",
                ["FilesAnalysed"] = "{0} ficheros analizados",
                ["ManualWarning"] = "Las revisiones manuales no se tienen en cuenta en la puntuación: es necesario comprobarlas a mano.",
                ["FileRanking"] = "Ficheros ordenados por puntuación (de menor a mayor)",
                ["RootFolder"] = "Raíz",
                ["NoIssues"] = "No se ha detectado ninguna incidencia en este fichero.",
                ["NoXamlFound"] = "No se ha encontrado ningún fichero XAML en esta carpeta.",
                ["XamlOnly"] = "Solo se pueden analizar ficheros XAML o carpetas que los contengan.",
                ["Error_s"] = "error",
                ["Error_p"] = "errores",
                ["Warning_s"] = "advertencia",
                ["Warning_p"] = "advertencias",
                ["Manual_s"] = "revisión manual",
                ["Manual_p"] = "revisiones manuales",
                ["CategoryError"] = "ERROR",
                ["CategoryWarning"] = "ADVERTENCIA",
                ["CategoryManual"] = "REVISIÓN MANUAL",
                ["Line"] = "línea",
                ["SettingsTitle"] = "Configuración del análisis",
                ["Thresholds"] = "Umbrales de análisis",
                ["ThresholdsDesc"] = "Ajusta los valores mínimos que la herramienta considerará aceptables.",
                ["MinFontSize"] = "Tamaño mínimo de fuente (px):",
                ["MinContrast"] = "Ratio mínimo de contraste:",
                ["MinTargetSize"] = "Tamaño mínimo del objetivo (px):",
                ["ActiveRules"] = "Reglas activas",
                ["ResetDefaults"] = "Restaurar valores por defecto",
                ["Accept"] = "Aceptar",
                ["InvalidValues"] = "Revisa los valores:\n\n· Tamaño de fuente: entre 1 y 72 px.\n· Ratio de contraste: entre 1 y 21.\n· Tamaño del objetivo: entre 1 y 100 px.",
                ["InvalidValuesTitle"] = "Valores fuera de rango",
                ["FontPreview"] = "Ejemplo a {0} px",
                ["ContrastPreview"] = "Contraste {0}:1",
                ["LanguageLabel"] = "Idioma:",
                ["ExportSaved"] = "Informe guardado correctamente.\n\n¿Quieres abrirlo ahora?",
                ["ExportDone"] = "Exportación completada",
                ["ExportError"] = "No se ha podido guardar el informe.",
                ["ExportErrorTitle"] = "Error de escritura",
                ["XmlError"] = "El fichero no es un XAML válido.",
                ["XmlErrorTitle"] = "Error de formato",
                ["ReadError"] = "No se ha podido leer el fichero.",
                ["ReadErrorTitle"] = "Error de lectura",
                ["R1.Name"] = "Nombre accesible ausente",
                ["R1.Message"] = "El control '{0}' no expone ningún nombre accesible.",
                ["R2.Name"] = "Alternativa textual ausente",
                ["R2.Message"] = "El elemento gráfico '{0}' no proporciona ninguna alternativa textual.",
                ["R3.Name"] = "Nombre vacío o duplicado",
                ["R3.Empty"] = "El control '{0}' declara un nombre accesible vacío.",
                ["R3.Duplicate"] = "El nombre accesible '{0}' está duplicado en más de un control.",
                ["R4.Name"] = "Contraste insuficiente",
                ["R4.Error"] = "El contraste del control '{0}' es de {1}, inferior al mínimo exigido ({2}).",
                ["R4.Manual"] = "No se puede determinar el color del texto del control '{0}' de forma estática: requiere revisión manual.",
                ["R5.Name"] = "Tamaño de fuente pequeño",
                ["R5.Message"] = "El tamaño de fuente ({0}) del control '{1}' es inferior al mínimo recomendado ({2}).",
                ["R6.Name"] = "Operabilidad por teclado",
                ["R6.TabStop"] = "El control '{0}' está excluido de la navegación por teclado (IsTabStop=\"False\").",
                ["R6.Focusable"] = "El control '{0}' no es enfocable por teclado (Focusable=\"False\").",
                ["R7.Name"] = "Tamaño del objetivo insuficiente",
                ["R7.Width"] = "El ancho del control '{0}' ({1}) es inferior al mínimo de {2} px.",
                ["R7.Height"] = "La altura del control '{0}' ({1}) es inferior al mínimo de {2} px.",
                ["R8.Name"] = "Colores indistinguibles por daltonismo",
                ["R8.Message"] = "Los colores {0} y {1} pueden confundirse bajo {2}. Si transmiten información por color, debe revisarse.",
                ["R8.Protanopia"] = "protanopia",
                ["R8.Deuteranopia"] = "deuteranopia",
                ["R8.Tritanopia"] = "tritanopia",
                ["HtmlTitle"] = "Informe de accesibilidad",
                ["HtmlFolderTitle"] = "Informe de accesibilidad de directorio",
                ["HtmlFileAnalysed"] = "Fichero analizado",
                ["HtmlFilesAnalysed"] = "ficheros analizados",
                ["HtmlControls"] = "controles",
                ["HtmlAverageScore"] = "puntuación media",
                ["HtmlConfirmedErrors"] = "errores confirmados",
                ["HtmlWarnings"] = "advertencias",
                ["HtmlManualReview"] = "elementos que requieren revisión manual",
                ["HtmlManualNote"] = "Las revisiones manuales no se tienen en cuenta en la puntuación: corresponden a valores que se resuelven en tiempo de ejecución y que el análisis estático no puede verificar. Es necesario comprobarlas manualmente.",
                ["HtmlNoIssues"] = "No se ha detectado ninguna incidencia en este fichero.",
                ["HtmlCleanFolder"] = "Ninguna incidencia detectada.",
                ["HtmlFileRanking"] = "Ficheros ordenados por puntuación",
                ["HtmlScoreLabel"] = "Puntuación de accesibilidad: {0} por ciento",
                ["HtmlAverageLabel"] = "Puntuación media de accesibilidad: {0} por ciento",
                ["HtmlFooter"] = "Generado el {0} por el Evaluador estático de accesibilidad para interfaces WPF/XAML.",
                ["HtmlNormative"] = "Criterios basados en WCAG 2.2, WCAG2ICT y EN 301 549.",
                ["HtmlIssueCount"] = "incidencia/s",
                ["AboutText"] = "Evaluador estático de accesibilidad para interfaces WPF/XAML\n"
                    + "Trabajo de Fin de Grado — Cesar Gallardo Rodriguez\n"
                    + "Universitat de Lleida — Campus Igualada-UdL\n\n"
                    + "La herramienta analiza ficheros XAML de aplicaciones WPF sin ejecutarlas "
                    + "y detecta incumplimientos de accesibilidad trazables a la normativa "
                    + "europea: WCAG 2.2, WCAG2ICT y EN 301 549.\n\n"
                    + "Cómo interpretar los resultados:\n\n"
                    + "• ERROR — Incumplimiento confirmado. Debe corregirse.\n"
                    + "• ADVERTENCIA — Muy probable, pero conviene revisarlo.\n"
                    + "• REVISIÓN MANUAL — La herramienta no puede decidirlo: requiere verificación humana.\n\n"
                    + "La puntuación (0–100) refleja solo lo que se ha podido verificar "
                    + "estáticamente. Las revisiones manuales NO penalizan la puntuación, "
                    + "pero no deben ignorarse.",

                ["DarkMode"] = "Modo oscuro",
                ["LightMode"] = "Modo claro",
                ["ExportJson"] = "Exportar JSON",
                ["ImportReport"] = "Importar informe",
                ["InvalidReport"] = "El fichero no contiene un informe válido.",
                ["InvalidReportTitle"] = "Formato no reconocido",
            },

            [Language.English] = new Dictionary<string, string>
            {
                ["LoadFile"] = "Load file",
                ["LoadFolder"] = "Analyse folder",
                ["ExportHtml"] = "Export HTML",
                ["Settings"] = "Settings",
                ["About"] = "About",
                ["NoFileLoaded"] = "No file loaded",
                ["IssuesDetected"] = "Issues detected",
                ["Summary"] = "Summary",
                ["Accessible"] = "accessible",
                ["Average"] = "average",
                ["WelcomeTitle"] = "WPF/XAML Accessibility Analyser",
                ["WelcomeMessage"] = "Load a XAML file or folder to start, or drag and drop it onto this window.",
                ["ControlsAnalysed"] = "{0} controls analysed",
                ["AverageOf"] = "Average score of {0} files",
                ["FilesAnalysed"] = "{0} files analysed",
                ["ManualWarning"] = "Manual review items are not included in the score: they must be checked by hand.",
                ["FileRanking"] = "Files ranked by score (lowest first)",
                ["RootFolder"] = "Root",
                ["NoIssues"] = "No issues detected in this file.",
                ["NoXamlFound"] = "No XAML files found in this folder.",
                ["XamlOnly"] = "Only XAML files or folders containing them can be analysed.",
                ["Error_s"] = "error",
                ["Error_p"] = "errors",
                ["Warning_s"] = "warning",
                ["Warning_p"] = "warnings",
                ["Manual_s"] = "manual review",
                ["Manual_p"] = "manual reviews",
                ["CategoryError"] = "ERROR",
                ["CategoryWarning"] = "WARNING",
                ["CategoryManual"] = "MANUAL REVIEW",
                ["Line"] = "line",
                ["SettingsTitle"] = "Analysis settings",
                ["Thresholds"] = "Analysis thresholds",
                ["ThresholdsDesc"] = "Adjust the minimum values the tool will consider acceptable.",
                ["MinFontSize"] = "Minimum font size (px):",
                ["MinContrast"] = "Minimum contrast ratio:",
                ["MinTargetSize"] = "Minimum target size (px):",
                ["ActiveRules"] = "Active rules",
                ["ResetDefaults"] = "Restore defaults",
                ["Accept"] = "Accept",
                ["InvalidValues"] = "Check the values:\n\n· Font size: between 1 and 72 px.\n· Contrast ratio: between 1 and 21.\n· Target size: between 1 and 100 px.",
                ["InvalidValuesTitle"] = "Values out of range",
                ["FontPreview"] = "Example at {0} px",
                ["ContrastPreview"] = "Contrast {0}:1",
                ["LanguageLabel"] = "Language:",
                ["ExportSaved"] = "Report saved successfully.\n\nWould you like to open it now?",
                ["ExportDone"] = "Export completed",
                ["ExportError"] = "The report could not be saved.",
                ["ExportErrorTitle"] = "Write error",
                ["XmlError"] = "The file is not valid XAML.",
                ["XmlErrorTitle"] = "Format error",
                ["ReadError"] = "The file could not be read.",
                ["ReadErrorTitle"] = "Read error",
                ["R1.Name"] = "Accessible name missing",
                ["R1.Message"] = "The control '{0}' does not expose an accessible name.",
                ["R2.Name"] = "Text alternative missing",
                ["R2.Message"] = "The graphic element '{0}' does not provide a text alternative.",
                ["R3.Name"] = "Empty or duplicate name",
                ["R3.Empty"] = "The control '{0}' declares an empty accessible name.",
                ["R3.Duplicate"] = "The accessible name '{0}' is duplicated across multiple controls.",
                ["R4.Name"] = "Insufficient contrast",
                ["R4.Error"] = "The contrast of control '{0}' is {1}, below the required minimum ({2}).",
                ["R4.Manual"] = "The text colour of control '{0}' cannot be determined statically: manual review required.",
                ["R5.Name"] = "Small font size",
                ["R5.Message"] = "The font size ({0}) of control '{1}' is below the recommended minimum ({2}).",
                ["R6.Name"] = "Keyboard operability",
                ["R6.TabStop"] = "The control '{0}' is excluded from keyboard navigation (IsTabStop=\"False\").",
                ["R6.Focusable"] = "The control '{0}' is not keyboard-focusable (Focusable=\"False\").",
                ["R7.Name"] = "Insufficient target size",
                ["R7.Width"] = "The width of control '{0}' ({1}) is below the minimum of {2} px.",
                ["R7.Height"] = "The height of control '{0}' ({1}) is below the minimum of {2} px.",
                ["R8.Name"] = "Colours indistinguishable under colour blindness",
                ["R8.Message"] = "The colours {0} and {1} may be confused under {2}. If they convey information by colour, this should be reviewed.",
                ["R8.Protanopia"] = "protanopia",
                ["R8.Deuteranopia"] = "deuteranopia",
                ["R8.Tritanopia"] = "tritanopia",
                ["HtmlTitle"] = "Accessibility report",
                ["HtmlFolderTitle"] = "Directory accessibility report",
                ["HtmlFileAnalysed"] = "File analysed",
                ["HtmlFilesAnalysed"] = "files analysed",
                ["HtmlControls"] = "controls",
                ["HtmlAverageScore"] = "average score",
                ["HtmlConfirmedErrors"] = "confirmed errors",
                ["HtmlWarnings"] = "warnings",
                ["HtmlManualReview"] = "items requiring manual review",
                ["HtmlManualNote"] = "Manual review items are not included in the score: they correspond to values resolved at runtime that static analysis cannot verify. They must be checked manually.",
                ["HtmlNoIssues"] = "No issues detected in this file.",
                ["HtmlCleanFolder"] = "No issues detected.",
                ["HtmlFileRanking"] = "Files ranked by score",
                ["HtmlScoreLabel"] = "Accessibility score: {0} per cent",
                ["HtmlAverageLabel"] = "Average accessibility score: {0} per cent",
                ["HtmlFooter"] = "Generated on {0} by the Static Accessibility Analyser for WPF/XAML interfaces.",
                ["HtmlNormative"] = "Criteria based on WCAG 2.2, WCAG2ICT and EN 301 549.",
                ["HtmlIssueCount"] = "issue(s)",
                ["AboutText"] = "Static Accessibility Analyser for WPF/XAML Interfaces\n"
                    + "Bachelor's Thesis — Cesar Gallardo Rodriguez\n"
                    + "Universitat de Lleida — Campus Igualada-UdL\n\n"
                    + "The tool analyses XAML files from WPF applications without executing them "
                    + "and detects accessibility violations traceable to the European regulatory "
                    + "framework: WCAG 2.2, WCAG2ICT and EN 301 549.\n\n"
                    + "How to interpret the results:\n\n"
                    + "• ERROR — Confirmed violation. Must be corrected.\n"
                    + "• WARNING — Very likely, but should be reviewed.\n"
                    + "• MANUAL REVIEW — The tool cannot decide: human verification required.\n\n"
                    + "The score (0–100) reflects only what could be verified "
                    + "statically. Manual review items do NOT penalise the score, "
                    + "but should not be ignored.",

                ["DarkMode"] = "Dark mode",
                ["LightMode"] = "Light mode",
                ["ExportJson"] = "Export JSON",
                ["ImportReport"] = "Import report",
                ["InvalidReport"] = "The file does not contain a valid report.",
                ["InvalidReportTitle"] = "Unrecognised format",

            },
        };
    }
}