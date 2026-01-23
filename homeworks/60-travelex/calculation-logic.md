# Berechnungslogik

## Kilometergelder

Befindet sich das Kraftfahrzeug nicht im Betriebsvermögen (d.h. betriebliche Nutzung weniger als 50%), kann das Kilometergeld für betrieblich bedingte Fahrten geltend gemacht werden. Wird das Kilometergeld geltend gemacht, sind damit sämtliche Aufwendungen (auch Parkgebühren und Mauten) abgegolten.

Das Kilometergeld beträgt für PKW, Kombi pro km	€0,50

## Tagesgelder im Inland (Diäten)

Mehraufwendungen für Verpflegung (Tagesgelder), dürfen bei ausschließlich betrieblich veranlassten Reisen, wenn diese länger als 3 Stunden dauern, mit 2,50 EUR pro angefangener Stunde (1/12 von 30 EUR = 2,50 EUR) abgesetzt werden. Insgesamt dürfen jedoch maximal 30 EUR pro Tag (24 Stunden) berücksichtigt werden.

Dauert eine Reise daher mehr als 11 Stunden steht der volle Satz zu.

Beispiel:
* Beginn einer Geschäftsreise am ersten Tag um 7:00 Uhr
* Ende der Reise am 2. Tag um 14:30 Uhr

Berechnung:
* 1. Tag: Tagesgeld für 24 Std. = 30€
* 2. Tag: Tagesgeld für 8 Std. (8/12 von 30) = 20€

---

# Calculation Logic (English Translation)

## Mileage Allowance

If the motor vehicle is not part of business assets (i.e., business use is less than 50%), the mileage allowance can be claimed for business-related trips. If the mileage allowance is claimed, it covers all expenses (including parking fees and tolls).

The mileage allowance for passenger cars and station wagons is €0.50 per km.

## Daily Allowances in Austria (Per Diem)

Additional meal expenses (daily allowances) may be deducted for trips that are exclusively business-related, provided they last longer than 3 hours. The allowance is €2.50 per started hour (1/12 of €30 = €2.50). However, a maximum of €30 per day (24 hours) may be taken into account.

Therefore, if a trip lasts more than 11 hours, the full rate applies.

Example:
* Start of a business trip on the first day at 7:00
* End of the trip on the 2nd day at 14:30

Calculation:
* Day 1: daily allowance for 24 hours = €30
* Day 2: daily allowance for 8 hours (8/12 of 30) = €20

---

# Test Cases

## Per Diem

### Validation / Edge Cases

- **Null travel throws**
	- Given: `travel == null`
	- Expect: `ArgumentNullException`

- **End before start results in zero per diem**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 07:59:59Z`
	- Expect: `PerDiem = 0.00`

### Threshold at 3 Hours

- **Up to 3 hours is zero**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 11:00:00Z` (3:00:00)
	- Expect: `PerDiem = 0.00`

- **Exactly 3 hours is zero**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 11:00:00Z` (3:00:00)
	- Expect: `PerDiem = 0.00`

- **More than 3 hours uses “per started hour”**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 11:00:01Z` (3:00:01)
	- Expect: 4 started hours × €2.50 ⇒ `PerDiem = 10.00`

- **Just under 4 hours counts as 4 started hours**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 11:59:59Z` (3:59:59)
	- Expect: 4 started hours × €2.50 ⇒ `PerDiem = 10.00`

### Full-Rate Threshold (> 11 Hours)

- **Exactly 11 hours is not the full rate**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 19:00:00Z` (11:00:00)
	- Expect: 11 started hours × €2.50 ⇒ `PerDiem = 27.50`

- **Just under 11 hours is still not the full rate**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 18:59:59Z` (10:59:59)
	- Expect: 11 started hours × €2.50 ⇒ `PerDiem = 27.50`

- **More than 11 hours is full rate**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-20 19:00:01Z` (11:00:01)
	- Expect: `PerDiem = 30.00`

### 24-Hour Blocks (Spanning Multiple Days)

- **Exactly 24 hours is one full day**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-21 08:00:00Z` (24:00:00)
	- Expect: `PerDiem = 30.00`

- **24 hours plus 2 hours counts remainder hours**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-21 10:00:00Z` (26:00:00)
	- Expect: 1 full day (€30) + 2 started hours × €2.50 ⇒ `PerDiem = 35.00`

- **24 hours plus 3 hours counts remainder hours**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-21 11:00:00Z` (27:00:00)
	- Expect: 1 full day (€30) + 3 started hours × €2.50 ⇒ `PerDiem = 37.50`

- **24 hours plus 3 hours and 1 second adds four started hours**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-21 11:00:01Z` (27:00:01)
	- Expect: 1 full day (€30) + 4 started hours × €2.50 ⇒ `PerDiem = 40.00`

- **48 hours is two full days**
	- Given: `Start = 2026-01-20 08:00:00Z`, `End = 2026-01-22 08:00:00Z` (48:00:00)
	- Expect: 2 full days ⇒ `PerDiem = 60.00`

- **Spanning more than 24 hours uses 24-hour blocks from the start time (example case)**
	- Given: `Start = 2026-01-19 07:00:00Z`, `End = 2026-01-20 14:30:00Z`
	- Expect: 1 full 24h block (€30) + 8 started hours × €2.50 (€20) ⇒ `PerDiem = 50.00`

## Mileage Allowance and Expenses

- **Mileage is calculated from drive entries**
	- Given: two drives with private car, `75 km` each
	- Expect: `Mileage = 75.00`

- **Expenses are Summed Even When Mileage Allowance Is Claimed**
	- Given: at least one private-car drive with `km > 0` and an expense of `€100`
	- Expect: `Expenses = 100.00`

- **Expenses are not zero when drive entries have 0 km**
	- Given: private-car drive with `0 km` and an expense of `€100`
	- Expect: `Expenses = 100.00`

- **Expenses are summed**
	- Given: expenses of `€498` and `€120`
	- Expect: `Expenses = 618.00`
