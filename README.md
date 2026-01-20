# Stanislava-Angelova-employees
This ASP.NET Core MVC application calculates which two employees have worked together on common projects for the longest total time.
It includes CSV upload, validation, clean error reporting, and a user-friendly interface.

# Features
- Upload a CSV file containing employee project assignments
- Validation of:
	- Missing columns
	- Invalid dates
	- Overlapping project periods
	- Duplicate records
- Automatic calculation of the longest-working employee pair
- Clean UI built with Bootstrap
- Clear error messages for invalid input

# CSV Format
Your CSV file must follow this structure:
|EmpID | ProjectID | DateFrom | DateTo|
|-|-|-|-|
|143| 12| 2013-11-01| 2014-01-05|
|218| 10| 2012-05-16| NULL|
|143| 10| 2009-01-01| 2011-04-27|

**Rules:**
- DateFrom and DateTo must be valid dates
- Use NULL in DateTo for employees still working on a project

**Uploading a CSV File**
Use the upload form on the home page to select a CSV file.
The application will validate the file and display the results.

# Technologies Used
- Visual Studio
- ASP.NET Core MVC
- C#
- Bootstrap 5
- LINQ
- Git & GitHub

**Repository URL** <br> ```https://github.com/StanislavaAngelova/Stanislava-Angelova-employees.git ```

# Author
Stanislava Angelova

# License
This project is open-source and available under the MIT License.




