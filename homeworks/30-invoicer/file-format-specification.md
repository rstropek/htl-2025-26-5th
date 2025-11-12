# Time Tracking Import File Format - Technical Specification

## 1. Overview

This document defines the technical specification for the time tracking import file format. The format is a semi-structured text-based format that combines key-value pairs for metadata with semicolon-delimited data rows for timesheet entries.

## 2. File Structure

### 2.1 General Structure

The file consists of three logical sections in the following order:

1. **Employee Identification Section** - Required metadata identifying the employee
2. **Timesheet Section(s)** - One or more dated timesheet blocks containing time entries

### 2.2 Character Encoding

Line endings can be LF (Unix) or CRLF (Windows)

## 3. Employee Identification Section

### 3.1 Required Fields

The following fields MUST appear at the beginning of the file, in any order:

#### 3.1.1 EMP-ID

**Syntax:**
```
EMP-ID: <employee-id>
```

**Requirements:**
- MUST be present exactly once
- `<employee-id>` is a numeric identifier, but must be stored as a string because of possible leading zeros
- Maximum length: 5 characters
- Examples: `4711`, `0815`

#### 3.1.2 EMP-NAME

**Syntax:**
```
EMP-NAME: <employee-name>
```

**Requirements:**
- MUST be present exactly once
- `<employee-name>` is a free-text string representing the employee's full name
- Maximum length: 100 characters
- Examples: `Rainer Stropek`, `Max Mustermann`

### 3.2 Field Format

- Format: `KEY: VALUE`
- The key and value MUST be separated by a colon followed by a single space (`: `)
- Each field MUST appear on its own line
- No leading or trailing whitespace is permitted on the line

### 3.3 Validation Rules

- Both `EMP-ID` and `EMP-NAME` MUST be present before any `TIMESHEETS` section
- Files missing either field are invalid
- Unknown keys (e.g., `DEPARTMENT`) MUST be rejected as invalid

## 4. Timesheet Section

### 4.1 Section Header

**Syntax:**
```
TIMESHEETS: <date>
```

**Requirements:**
- The keyword MUST be exactly `TIMESHEETS` (note: plural form)
- `<date>` MUST be in ISO 8601 format: `YYYY-MM-DD`
- Date validation:
  - Year MUST be a valid 4-digit year
  - Month MUST be in range 01-12
  - Day MUST be valid for the given month and year
  - Invalid examples: `2025-13-01` (invalid month), `2025-10-AB` (non-numeric day)

### 4.2 Time Entry Records

Each time entry record follows the timesheet header and has the following structure:

**Syntax:**
```
<start-time>;<end-time>;<description>;<category>
```

#### 4.2.1 Field Definitions

1. **start-time** (Required)
   - Format: `HH:MM` (24-hour format)
   - Valid hours: 00-23
   - Valid minutes: 00-59
   - Examples: `08:00`, `13:15`, `23:59`

2. **end-time** (Required)
   - Format: `HH:MM` (24-hour format)
   - Same validation rules as start-time
   - MUST be chronologically after start-time (not validated in this spec, but logical requirement)
   - Invalid example: `25:00` (hour out of range)

3. **description** (Required)
   - Free-text description of the work performed
   - MUST be enclosed in double quotes (`"`)
   - Can contain any characters except double quotes
   - Maximum length: 200 characters
   - Examples: `"Daily Standup Meeting"`, `"Importer Implementation"`

4. **project** (Required)
   - Project code
   - NOT quoted
   - Maximum length: 20 characters
   - Examples: `ADMIN`, `ACCOUNTING`, `CRM`

#### 4.2.2 Field Delimiter

- Fields MUST be separated by semicolons (`;`)
- No spaces before or after semicolons
- All four fields MUST be present and non-empty

### 4.3 Multiple Timesheet Sections

- A file MAY contain multiple `TIMESHEETS` sections
- Each section represents a different date
- At least one `TIMESHEETS` section with at least one time entry MUST be present

## 5. Validation Rules

### 5.1 Mandatory Requirements

A valid file MUST satisfy ALL of the following:

1. **Employee Metadata Present**
   - Contains exactly one `EMP-ID` line
   - Contains exactly one `EMP-NAME` line
   - Both appear before any `TIMESHEETS` section

2. **Timesheet Data Present**
   - Contains at least one `TIMESHEETS` section
   - Each `TIMESHEETS` section contains at least one time entry record

3. **Proper Formatting**
   - All metadata lines follow `KEY: VALUE` format
   - All time entry records have exactly 4 semicolon-delimited fields
   - Description field is properly quoted

4. **Valid Data Types**
   - Dates are valid ISO 8601 format (YYYY-MM-DD)
   - Times are valid 24-hour format (HH:MM)
   - Hours are in range 00-23
   - Minutes are in range 00-59

5. **No Unknown Keys**
   - Only recognized keys (`EMP-ID`, `EMP-NAME`, `TIMESHEETS`) are allowed
   - Unknown keys like `DEPARTMENT`, `TIMESHEET` (singular), etc. are invalid

## 8. Grammar (ISO 14977 EBNF)

![File Format Grammar](./file-format-specification.png)

```ebnf
file = employee_section, { timesheet_section }, newline ;

employee_section = ( emp_id_line, emp_name_line ) 
                 | ( emp_name_line, emp_id_line ) ;

emp_id_line = "EMP-ID: ", numeric_id, newline ;

emp_name_line = "EMP-NAME: ", text, newline ;

timesheet_section = timesheet_header, time_entry, { time_entry } ;

timesheet_header = "TIMESHEETS: ", iso_date, newline ;

time_entry = time, ";", time, ";", quoted_text, ";", project, newline ;

time = hour, ":", minute ;

iso_date = year, "-", month, "-", day ;

year = digit, digit, digit, digit ;

quoted_text = '"', { text_char - '"' }, '"' ;

text = text_char, { text_char } ;

text_char = letter | digit | " " | special_char ;

project_char = text_char, newline, ";" ;

project = project_char, { project_char } ;

numeric_id = digit, { digit } ;

newline = LF | CRLF ;
```
