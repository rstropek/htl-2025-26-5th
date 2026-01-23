# Travel File Specification

## Background

A _Travel File_ is a structured data file used to store and exchange information related to business travels. It contains a header with general information about the travel, followed by multiple entries detailing travel segments and expenses.

## Example

Here is an example of a _Travel File_:

```txt
2026-01-19T07:30:00Z|2026-01-20T17:00:00Z|John Doe|Training at Customer XYZ
DRIVE|75|Drive to airport
EXPENSE|498|Flight to/from Bregenz
EXPENSE|25|Taxi fare
EXPENSE|120|Hotel stay
EXPENSE|25|Taxi fare
DRIVE|75|Drive from airport
```

* The first line is the header containing overall travel information.
  * Start of the entire business trip (ISO 8601 format)
  * End of the entire business trip (ISO 8601 format)
  * Traveler's name (mandatory)
  * Purpose of the trip (mandatory)
* Subsequent lines (optional) represent individual travel expenses.
  * `DRIVE`: Travel by private car, with distance in kilometers and a description.
  * `EXPENSE`: General expenses with amount in EUR and a description. The travel must be reimbursed for these expenses by the employer.
* Compared to real-world travel expenses reimbursements, we simplify the following aspects:
  * We do not distinguish between different vehicle types (e.g. car, motorcycle, bike, etc.). We assume that **all private vehicle travel is done by **car**.
  * We assume that the traveler must pay for all meals during the trip. We assume that **no meals are provided by the employer or third parties** (e.g. hotels, airlines, customers, etc.).
  * We assume that the traveler does **not transport any additional persons** during the trip.
  * We assume that all travels are done **within Austria**.

## File Validation

You must implement logic to parse and validate a _Travel File_. Distinguish between the following validation errors:

* Empty file
* Invalid header
    * Invalid number of fields
    * Invalid start date format
    * Invalid end date format
    * Start date is after end date
    * Empty traveler's name
    * Empty trip purpose
* Invalid `DRIVE` entry
    * Invalid number of fields
    * Invalid distance (must be > 0)
    * Empty description
* Invalid `EXPENSE` entry
    * Invalid number of fields
    * Invalid amount (must be > 0)
    * Empty description
* Invalid entry type (not `DRIVE` or `EXPENSE`)
