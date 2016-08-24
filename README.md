Easily creates Entity Data Model at runtime.

If you are tired of creating,changing and synchronizing your data model for yours projects all the time.
If you are a big fan of dynamic programming and don't really care about intellisense, Dynamic Data Model Builder is for you.
With the help of this library, you will be able to get the whole database context as well as POCO classes with their navigation properties just by introducing the connection string to the Database class and everything will be taken care of.

*How to Use:

*Limitation:
Entity Framework doeas not allow entity property names to begin with underscore or other illegal characters.
Required Primary key for all the tables.
Columns cannot have the same name as their own tables.


*Creating & Loading:

Database db = new Database(ConnectionString);
db.Load();

Query:

ie: find a User inside your User Table

var user = (from u in (IEnumerable<dynamic>)db.Instance.User
where u.UserName == "User1"
select u).FirstOrDefault();

Creating new object:

ie: adding na new user and add to the DbSet

var obj = db.New("User");
obj.UserName = "User2";
obj.Password = " *";

db.Add(obj);

Save:

db.Save();

Property Names Convention:

db.Instance property has the whole Database Context

Table: dbSet -> TableName (ie: db.Instance.User)
Table With Schema -> SchemaName_TableName (ie:db.Instance.tmp_User )

Foriegn Key Object access:
Imagine Product and User Table
Product ( ID, Name, Code, CreatedBy(FK), UpdatedBy(FK)) 
Under Product should be two Virtual properties named CreatedByObject and UpdatedByObject
Also, there should ba two Collection under User named ProductListFromCreatedBy and ProductListFromUpdatedBy

Note:
IpropertyNotifyChanged has been implemented
It supports entity relationship (navigation properties)
supports multiple primary keys
supports multiple foreign keys