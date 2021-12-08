

Create or  alter Procedure [dbo].[search]
(
@flag char = 's',
@startdate varchar(50) = null,
@enddate varchar(50) =null,
@sSalary varchar(50) =null,
@eSalary varchar(50) =null,
@searchString varchar(50) = NULL,
@DesignationString varchar(50) = NULL,
@genderstring varchar(50) = NULL
)
As
Begin
 
if @flag = 's'
BEGIN




if @startdate != '' and @enddate != ''
	select * from ImportRecord where  DateOfBirth between @startdate AND @enddate

	if @sSalary != '' and @eSalary != ''
	select * from ImportRecord where  Salary between @sSalary AND @eSalary

	if @searchString != '' 
	select * from ImportRecord where (@searchString IS NULL OR FullName LIKE '%' + @searchString + '%') 

	
	
--And (@searchString IS NULL OR FullName LIKE '%' + @searchString + '%')
--AND (@DesignationString IS NULL OR Designation LIKE '%' + @DesignationString + '%')
--AND (@genderstring IS NULL OR Gender LIKE '%' + @genderstring + '%')

print(@sSalary)
print(@startdate)
	
END
End

[search] @flag   = 's', 
@startdate = '1995-01-01',
@enddate = '1997-01-01'
[search] @flag   = 's', 
@searchString = 'b'

[search] @flag   = 's', 
@sSalary = '5000',
@eSalary = '14000',
@startdate = '1995-01-01',
@enddate = '1997-01-01',
@searchString = 'b'
