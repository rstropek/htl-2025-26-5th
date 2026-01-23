# Reisekostenabrechnung

## Einleitung

Ihre Aufgabe ist die Entwicklung einer Software zum Berechnen der Reisekosten entsprechend der österreichischen Richtlinien. Folgende Vereinfachungen und Annahmen gelten:

* Die Software kann nur Reisen innerhalb Österreichs verarbeiten. Grenzübertritte und Auslandsreisen werden nicht berücksichtigt.
* Wenn Fahren mit PKWs gemeldet werden, wird davon ausgegangen, dass es sich um Privat-PKWs handelt.
* Wenn Barauslagen gemeldet werden, wird davon ausgegangen, dass diese tatsächlich angefallen sind und dienstrelevante Ausgaben darstellen.

Die Regeln für das Berechnen der Reisekosten sind in [calculation-logic.md](./calculation-logic.md) im Detail beschrieben. Sie wurden mit minimalen Vereinfachungen von den offiziellen Richtlinien in Österreich (Quelle: WKO) übernommen.

## Funktionale Anforderungen

### US1: Reisekostenabrechnung hochladen

In diesem Beispiel gehen wir davon aus, dass Reisen in einer vorgelagerten Software erfasst und in einem standardisierten Format exportiert werden können. Das Dateiformat ist in [travel-file-spec.md](./travel-file-spec.md) im Detail beschrieben. Der Ordner [data](./data/) enthält Beispieldateien (korrekte und fehlerhafte).

Als Benutzer:in möchte ich eine Reisekostenabrechnungsdatei im spezifizierten Format hochladen können, um meine Reisekosten berechnen zu lassen.

![Hochladen](./upload.png)

### US2: Übersicht über alle Reisen anzeigen

Als Benutzer:in möchte ich eine Übersicht über alle meine hochgeladenen Reisen sehen können.

![Übersicht](./list.png)

Akzeptanzkriterien:
- Die zuletzt hochgeladene Reise wird ganz oben in der Liste angezeigt, danach absteigend sortiert nach Erfassungszeitpunkt (ID kann verwendet werden).
- Die Liste muss den Reisenden und den Zweck der Reise anzeigen.
- Es muss eine Möglichkeit zur Navigation zur Detailansicht jeder Reise geben.
- Es muss eine Möglichkeit zur Navigation zum Hochladen einer neuen Reise geben.

### US3: Detaillierte Ansicht einer Reise

Als Benutzer:in möchte ich die Details einer einzelnen Reise einsehen können, einschließlich aller Reisekosten.

![Detailansicht](./details.png)

Akzeptanzkriterien:
- Die Detailansicht muss alle Informationen beinhalten, die in der oben dargestellten Abbildung gezeigt werden.
- Es muss eine Möglichkeit zur Navigation zurück zur Übersicht aller Reisen geben.
- Es muss eine Möglichkeit zur Navigation zum Hochladen einer neuen Reise geben.

## Qualitätsanforderungen

### Startercode

Es muss der Startercode aus dem Ordner [starter](./starter/) verwendet werden. Es müssen die dort enthaltenen Technologien zum Einsatz kommen.

Der Startercode enthält "TODO"-Kommentare, die anzeigen, wo Code ergänzt werden muss.

Folgende Dinge sind im Startercode fertig implementiert:

* Grundlegende Projektstruktur
* Datenbankzugriff mit Entity Framework Core
* Web-API und Frontend-Logik zum Hochladen von Dateien (**Tipp:** Machen Sie sich mit dem Code vertraut, da wir bisher diese Aspekte im Unterricht nicht behandelt haben)
* Unit- und Integrationstests für Parser und API-Endpunkte (manche Tests müssen erweitert werden, siehe unten)
* CSS und HTML-Grundgerüste für die Frontend-Komponenten (ohne Data Binding und Logik)

### Zu ergänzender Code

* [TravelFileParser.cs](./starter/AppServices/TravelFileParser.cs): Implementierung des Parsers für das Reisekostendateiformat.
* [Reimbursement.cs](./starter/AppServices/Reimbursement.cs): Implementierung der Logik zur Berechnung der Reisekosten.
* [ReiumbursementCalculatorTests.cs](./starter/AppServicesTests/ReimbursementCalculatorTests.cs): Fügen Sie mindestens fünf Unit-Tests hinzu, die die Berechnung der Reisekosten abdecken. Die Unit-Tests müssen Testfälle abdecken, die in [calculation-logic.md](./calculation-logic.md) beschrieben sind.
* [TravelEndpoints.cs](./starter/WebApi/TravelEndpoints.cs): Ergänzen Sie die API-Endpunkte, um die Anforderungen der Benutzerstories zu erfüllen.
* [travels-list](./starter/Frontend/src/app/travels-list/): Ergänzen Sie die Komponente, um die Anforderungen der Übersicht aller Reisen zu erfüllen.
* [travel-details](./starter/Frontend/src/app/travel-details/): Ergänzen Sie Sie die Komponente, um die Anforderungen der Detailansicht einer Reise zu erfüllen.
