# Product Import File Format - Technical Specification

## 1. Overview

This document defines the technical specification for the product import file format. The format is a structured text-based format that combines a schema definition header with comma-delimited data rows for product entries.

## 2. File Structure

### 2.1 General Structure

The file consists of two logical sections separated by a delimiter:

1. **Header Section** - Schema definition describing column names, data types, and optionality
2. **Separator** - A line containing exactly `---`
3. **Data Section** - One or more data rows containing product information

### 2.2 Character Encoding

Line endings can be LF (Unix) or CRLF (Windows)

## 3. Header Section

### 3.1 Purpose

The header section defines the schema for the data rows. Each line in the header describes one column with its name, data type, and whether it is mandatory or optional.

### 3.2 Header Line Format

**Syntax:**
```
<column-name>: <data-type>, <optionality>
```

**Requirements:**
- Each column definition MUST appear on its own line
- The column name and data type MUST be separated by a colon followed by a single space (`: `)
- The data type and optionality MUST be separated by a comma followed by a single space (`, `)
- No leading or trailing whitespace is permitted on the line

### 3.3 Column Name

**Requirements:**
- Column names are case-sensitive identifiers
- Must start with a letter
- Can contain letters, digits, and underscores
- Examples: `ProductCode`, `ProductName`, `PricePerUnit`

### 3.4 Data Types

The following data types are supported:

#### 3.4.1 STRING

**Syntax:**
```
STRING(<max-length>)
```

**Requirements:**
- `<max-length>` is a positive integer specifying the maximum character length
- Examples: `STRING(10)`, `STRING(100)`, `STRING(255)`

#### 3.4.2 DECIMAL

**Syntax:**
```
DECIMAL
```

**Requirements:**
- Represents a decimal number
- No length specification is required

### 3.5 Optionality Markers

Each column MUST specify one of the following optionality markers:

| Marker | Description |
|--------|-------------|
| `MANDATORY` | The column value MUST be present and non-empty in every data row |
| `OPTIONAL` | The column value MAY be empty in data rows |

### 3.6 Validation Rules

- At least one header line MUST be present
- All header lines MUST appear before the separator (`---`)
- Unknown data types (e.g., `VARCHAR`, `INTEGER`) MUST be rejected as invalid
- Unknown optionality markers (e.g., `NOT NULL`, `NULLABLE`) MUST be rejected as invalid
- Invalid header format (wrong separators like `-`, `->`, `=>`, `;`, `&`) MUST be rejected

## 5. Data Section

### 5.1 Data Row Format

**Syntax:**
```
<value1>,<value2>,<value3>,...,<valueN>
```

**Requirements:**
- Each data row MUST contain exactly as many values as there are columns defined in the header
- Values MUST be separated by commas (`,`)
- No spaces before or after commas (unless part of a quoted string value)

### 5.2 Value Formats

#### 5.2.1 STRING Values

**Syntax:**
```
"<text>"
```

**Requirements:**
- String values MUST be enclosed in double quotes (`"`)
- The text can contain any characters except double quotes
- The text length MUST NOT exceed the maximum length specified in the header
- For OPTIONAL columns, an empty value is represented as an empty pair of quotes or nothing between commas: `,,"next value"` or `,"","next value"`

**Examples:**
- `"Mountain Bike Alpha"`
- `"Entry-level hardtail mountain bike ideal for light trails and weekend rides."`
- `""` (empty optional string)

#### 5.2.2 DECIMAL Values

**Syntax:**
```
<number>
```

**Requirements:**
- Decimal values MUST NOT be quoted
- Must be a valid decimal number
- Can include a decimal point (`.`) for fractional values
- Can be negative (prefixed with `-`)

**Examples:**
- `699.99`
- `1199.50`
- `549.00`
- `1299`

### 5.3 Empty Optional Values

For OPTIONAL columns, empty values are permitted:

- For STRING columns: either `""` (empty quotes) or nothing between commas
- For DECIMAL columns: nothing between commas

**Example with empty optional values:**
```
"BKE0011","Replacement Screws",,,1299.00
```

In this example, the third and fourth values (both OPTIONAL) are empty.

## 7. Validation Rules

### 7.1 Mandatory Requirements

A valid file MUST satisfy ALL of the following:

1. **Header Section Present**
   - Contains at least one valid header line
   - All header lines follow the correct format: `<name>: <type>, <optionality>`

2. **Separator Present**
   - Contains exactly one separator line (`---`)
   - Separator appears after all headers and before data rows

3. **Data Section Present**
   - Contains at least one data row
   - Each data row has exactly as many values as columns defined in header

4. **Proper Formatting**
   - Header lines use `: ` (colon-space) to separate name and type
   - Header lines use `, ` (comma-space) to separate type and optionality
   - Data values use `,` (comma) as delimiter
   - String values are properly quoted with double quotes

5. **Valid Data Types**
   - Only `STRING(<n>)` and `DECIMAL` are valid data types
   - Unknown types like `VARCHAR`, `INTEGER`, `INT` are invalid

6. **Valid Optionality Markers**
   - Only `MANDATORY` and `OPTIONAL` are valid markers
   - Unknown markers like `NOT NULL`, `NULLABLE`, `REQUIRED` are invalid

7. **Data Type Matching**
   - STRING values MUST be quoted
   - DECIMAL values MUST NOT be quoted
   - DECIMAL values MUST be valid numbers

8. **Mandatory Values Present**
   - All MANDATORY columns MUST have non-empty values in every data row

### 7.2 Error Conditions

The following conditions MUST result in a validation error:

| Error Type | Description | Example |
|------------|-------------|---------|
| Missing Header | No header section before separator | File starts with data rows |
| Header Format Error | Invalid separator in header line | `ProductCode - STRING(10), MANDATORY` |
| Invalid Header | Unrecognized header line format | `This is a test` |
| Unknown Data Type | Data type not recognized | `VARCHAR(10)`, `INTEGER` |
| Invalid Optional Marker | Optionality marker not recognized | `NOT NULL`, `NULLABLE` |
| Missing Column | Data row has fewer values than headers | Row with 4 values when 5 expected |
| Missing Quotes | String value not enclosed in quotes | `BKE0001` instead of `"BKE0001"` |
| Wrong Data Type | Value format doesn't match column type | Quoted decimal: `"699.99"` |

## 8. Grammar (ISO 14977 EBNF)

```ebnf
file = header_section, separator, data_section ;

header_section = header_line, { header_line } ;

header_line = column_name, ": ", data_type, ", ", optionality, newline ;

column_name = letter, { letter | digit | "_" } ;

data_type = string_type | decimal_type ;

string_type = "STRING(", positive_integer, ")" ;

decimal_type = "DECIMAL" ;

optionality = "MANDATORY" | "OPTIONAL" ;

separator = "---", newline ;

data_section = data_row, { data_row } ;

data_row = value, { ",", value }, newline ;

value = string_value | decimal_value | empty_value ;

string_value = '"', { text_char - '"' }, '"' ;

decimal_value = [ "-" ], digit, { digit }, [ ".", digit, { digit } ] ;

empty_value = '""' | ? entirely empty ?;

text_char = ? any character except double quote ? ;

newline = LF | CRLF ;
```
