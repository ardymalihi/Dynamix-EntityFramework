# Entity Framework Dynamic Model Builder

## Easily creates Entity Data Model at runtime.

By creating DbContext and Poco classes at runtime using C# Compiler, this library helps those developers who want to create complex and dynamic type of applications like ERP/CRM or create dynamic Micro Services whith CRUD operation without writing code.

### How to Use:

#### Creating & Loading:

db = new DbAdapter(ConnectionString);
db.Load();

#### Query:

ie: find a User inside your User Table

var user = (from u in (IEnumerable<dynamic>)db.Instance.User
where u.UserName == "User1"
select u).FirstOrDefault();

#### Creating new object:

##### ie: adding na new user and add to the DbSet

var obj = db.New("User");
obj.UserName = "User2";
obj.Password = "123";

db.Add(obj);

Save:

db.Save();

#### Property Names Convention:

db.Instance property has the whole Database Context

Table: dbSet -> TableName (ie: db.Instance.User)
Table With Schema -> SchemaName_TableName (ie:db.Instance.tmp_User )

#### Foriegn Key Object access:
Imagine Product and User Table
Product ( ID, Name, Code, CreatedBy(FK), UpdatedBy(FK)) 
Under Product should be two Virtual properties named CreatedByObject and UpdatedByObject
Also, there should ba two Collection under User named ProductListFromCreatedBy and ProductListFromUpdatedBy

### Note:
IpropertyNotifyChanged has been implemented
It supports entity relationship (navigation properties)
supports multiple primary keys
supports multiple foreign keys

### Limitation:
Entity Framework doeas not allow entity property names to begin with underscore or other illegal characters.
Required Primary key for all the tables.
Columns cannot have the same name as their own tables.
