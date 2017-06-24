using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Reta.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using System.Security.Claims;

namespace Reta.App_Start
{
    public class SqlQuery
    {
        public static void Initialize()
        {
            using (var db = new MySqlIdentityDbContext())
            {                              
                // add "Ressenti" field in Application
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'Ressenti') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[Ressenti] NVARCHAR(Max)
                                                        END");
                // add table template email
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='EmailTemplates' and xtype='U')
                                                CREATE TABLE EmailTemplates
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        Name NVARCHAR(max) null,
		                                                [Type] NVARCHAR(max) null,
                                                        [Subject] NVARCHAR(max) null,
		                                                Body NVARCHAR(max) null,
		                                                DynamicField NVARCHAR(max) null,
		                                                CONSTRAINT [PK_dbo.Emails] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");
                // Add table Contact Linkedin
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='ContactLinkedins' and xtype='U')
                                                CREATE TABLE ContactLinkedins
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        Title NVARCHAR(max) null,
		                                                Firstname NVARCHAR(max) null,
                                                        Middlename NVARCHAR(max) null,
                                                        Lastname NVARCHAR(max) null,
                                                        Suffix NVARCHAR(max) null,
                                                        Email NVARCHAR(max) null,
		                                                Company NVARCHAR(max) null,
                                                        Position NVARCHAR(max) null,
		                                                DynamicField NVARCHAR(max) null,
		                                                CONSTRAINT [PK_dbo.ContactLinkedins] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");

                // Add table Customers
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Customers' and xtype='U')
                                                CREATE TABLE Customers
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        Title NVARCHAR(max) null,
		                                                Firstname NVARCHAR(max) null,
                                                        Middlename NVARCHAR(max) null,
                                                        Lastname NVARCHAR(max) null,
                                                        Suffix NVARCHAR(max) null,
                                                        Email NVARCHAR(max) null,
		                                                Company NVARCHAR(max) null,
                                                        Position NVARCHAR(max) null,
                                                        MobilePhone NVARCHAR(max) null,
		                                                Workphone NVARCHAR(max) null,
                                                        BusinessAddress NVARCHAR(max) null,
		                                                DynamicField NVARCHAR(max) null,
		                                                CONSTRAINT [PK_dbo.Customers] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");

                // Add table Documents
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Documents' and xtype='U')
                                                CREATE TABLE Documents
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        Filename NVARCHAR(max) null,
		                                                AbsolutPath NVARCHAR(max) null,
                                                        RelativePath NVARCHAR(max) null,
                                                        Categories NVARCHAR(max) null,
                                                        FolderName NVARCHAR(max) null,                                                       
		                                                CONSTRAINT [PK_dbo.documents] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");

                // add "Version" field in Application
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Documents', 'Version') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Documents
                                                                ADD[Version] int
                                                        END");



                // Add table DocumentVersions
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='DocumentVersions' and xtype='U')
                                                CREATE TABLE DocumentVersions
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
		                                                AbsolutPath NVARCHAR(max) null,
                                                        RelativePath NVARCHAR(max) null,
                                                        Version INT null,        
                                                        DocumentId NVARCHAR(max) null,                                              
		                                                CONSTRAINT [PK_dbo.documentVersions] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");
                // add "Metadata" field in Application
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'Metadata') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[Metadata] NVARCHAR(Max)
                                                        END");

                // Add table Interviews
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Interviews' and xtype='U')
                                                CREATE TABLE Interviews
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        Name NVARCHAR(max) null,     
                                                        CandidatId NVARCHAR(128) NULL,
                                                        Commentaire NVARCHAR(128) NULL,
                                                        Skills NVARCHAR(128) NULL,
                                                        Configuration NVARCHAR(max) null,
                                                        Metadata NVARCHAR(max) null,                       
		                                                CONSTRAINT [PK_dbo.interviews] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");
                // Add table Spaces
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Spaces' and xtype='U')
                                                CREATE TABLE Spaces
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        Name NVARCHAR(max) null, 
                                                        Configuration NVARCHAR(max) null,
                                                        Metadata NVARCHAR(max) null,                       
		                                                CONSTRAINT [PK_dbo.spaces] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");

                // add "BlackListed" field in Application
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'BlackListed') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[BlackListed] BIT 
                                                        END");

                // add "Questions" field in interview
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Interviews', 'Questions') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Interviews
                                                                ADD[Questions] NVARCHAR(max) null 
                                                        END");

                // Alter column size of interviews table 
                db.Database.ExecuteSqlCommand(@"IF (SELECT TOP 1 CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Interviews' AND COLUMN_NAME = 'Commentaire') = 128
                                                BEGIN
		                                                ALTER TABLE Interviews ALTER COLUMN Commentaire NVARCHAR(max)
                                                END");

                db.Database.ExecuteSqlCommand(@"IF (SELECT TOP 1 CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Interviews' AND COLUMN_NAME = 'Skills') = 128
                                                BEGIN
		                                                ALTER TABLE Interviews ALTER COLUMN Skills NVARCHAR(max)
                                                END");

                // add "CustomerId" field in Application
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'CustomerId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[CustomerId] NVARCHAR(max) 
                                                        END");

                // add "SpaceId" field in Candidats table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in Candidats table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[DeletedOn] datetime
                                                        END");

                // add "SpaceId" field in CurriculumVitaes table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('CurriculumVitaes', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE CurriculumVitaes
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in CurriculumVitaes table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('CurriculumVitaes', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE CurriculumVitaes
                                                                ADD[DeletedOn] datetime
                                                        END");
                // add "SpaceId" field in ContactLinkedins table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('ContactLinkedins', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE ContactLinkedins
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in ContactLinkedins table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('ContactLinkedins', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE ContactLinkedins
                                                                ADD[DeletedOn] datetime
                                                        END");
                // add "SpaceId" field in Customers table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Customers', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Customers
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in Customers table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Customers', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Customers
                                                                ADD[DeletedOn] datetime
                                                        END");

                // add "SpaceId" field in Documents table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Documents', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Documents
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in Documents table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Documents', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Documents
                                                                ADD[DeletedOn] datetime
                                                        END");
                // add "SpaceId" field in DocumentVersions table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('DocumentVersions', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE DocumentVersions
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in DocumentVersions table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('DocumentVersions', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE DocumentVersions
                                                                ADD[DeletedOn] datetime
                                                        END");

                // add "SpaceId" field in EmailTemplates table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('EmailTemplates', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE EmailTemplates
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in EmailTemplates table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('EmailTemplates', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE EmailTemplates
                                                                ADD[DeletedOn] datetime
                                                        END");
                // add "SpaceId" field in Interviews table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Interviews', 'SpaceId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Interviews
                                                                ADD[SpaceId] NVARCHAR(Max)
                                                        END");

                // add "DeletedOn" field in Interviews table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Interviews', 'DeletedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Interviews
                                                                ADD[DeletedOn] datetime
                                                        END");

                // Add table Quizs
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Quizs' and xtype='U')
                                                CREATE TABLE Quizs
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        QuizInfo NVARCHAR(max) null,                                                 
                                                        Metadata NVARCHAR(max) null,
                                                        CreatedBy NVARCHAR(128) NULL,     
                                                        CreatedOn datetime NULL,    
                                                        SpaceId NVARCHAR(128) NULL,
                                                        DeletedBy NVARCHAR(128) NULL,
                                                        DeletedOn datetime NULL,
                                                                               
		                                                CONSTRAINT [PK_dbo.Quizs] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");
                // Add table Quizruns
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Quizruns' and xtype='U')
                                                CREATE TABLE Quizruns
                                                (
		                                                Id NVARCHAR(128) NOT NULL,	  
                                                        QuizId NVARCHAR(128) NULL,
                                                        UserId NVARCHAR(128) NULL,
                                                        QuizRunInfo NVARCHAR(max) null,   
                                                        StartedOn datetime NULL,
                                                        FinishedOn datetime NULL,
                                                        TimeSpent  BIGINT NULL,                                            
                                                        Metadata NVARCHAR(max) null,
                                                        CreatedBy NVARCHAR(128) NULL,     
                                                        CreatedOn datetime NULL,    
                                                        SpaceId NVARCHAR(128) NULL,
                                                        DeletedBy NVARCHAR(128) NULL,
                                                        DeletedOn datetime NULL,
                                                                               
		                                                CONSTRAINT [PK_dbo.Quizruns] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");

                // add "UserId" field in Candidats table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'UserId') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[UserId] NVARCHAR(128)
                                                        END");

                // Add table Missions
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Missions' and xtype='U')
                                                CREATE TABLE Missions
                                                (
		                                                Id NVARCHAR(128) NOT NULL,
                                                        Name NVARCHAR(max) null,  	  
                                                        Info NVARCHAR(max) null,                                                 
                                                        Metadata NVARCHAR(max) null,
                                                        CreatedBy NVARCHAR(128) NULL,     
                                                        CreatedOn datetime NULL,    
                                                        SpaceId NVARCHAR(128) NULL,
                                                        DeletedBy NVARCHAR(128) NULL,
                                                        DeletedOn datetime NULL,
                                                                               
		                                                CONSTRAINT [PK_dbo.Missions] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");


                // add "BodyUrl" field in EmailTemplates table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('EmailTemplates', 'BodyUrl') IS NULL
                                                        BEGIN
                                                                ALTER TABLE EmailTemplates
                                                                ADD[BodyUrl] NVARCHAR(max)
                                                        END");

                // add "Lang" field in EmailTemplates table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('EmailTemplates', 'Lang') IS NULL
                                                        BEGIN
                                                                ALTER TABLE EmailTemplates
                                                                ADD[Lang] NVARCHAR(128)
                                                        END");

                // add "CreatedBy" field in Candidats table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'CreatedBy') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[CreatedBy] NVARCHAR(128)
                                                        END");

                // add "DeletedBy" field in Candidats table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'DeletedBy') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[DeletedBy] NVARCHAR(128)
                                                        END");

                // add "CreatedOn" field in Candidats table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Candidats', 'CreatedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Candidats
                                                                ADD[CreatedOn] datetime
                                                        END");

                // Add table Questions
                db.Database.ExecuteSqlCommand(@"IF not exists (select * from sysobjects where name='Questions' and xtype='U')
                                                CREATE TABLE Questions
                                                (
		                                                Id NVARCHAR(128) NOT NULL,
                                                        Name NVARCHAR(max) null,  	  
                                                        Info NVARCHAR(max) null,                                                 
                                                        Metadata NVARCHAR(max) null,
                                                        CreatedBy NVARCHAR(128) NULL,     
                                                        CreatedOn datetime NULL,    
                                                        SpaceId NVARCHAR(128) NULL,
                                                        DeletedBy NVARCHAR(128) NULL,
                                                        DeletedOn datetime NULL,
                                                        Description NVARCHAR(max) null, 
                                                        Tags NVARCHAR(max) null, 
                                                                               
		                                                CONSTRAINT [PK_dbo.Questions] PRIMARY KEY CLUSTERED ([Id] ASC)
                                                )");

                // add "DeletedBy" field in Interviews table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Interviews', 'DeletedBy') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Interviews
                                                                ADD[DeletedBy]  NVARCHAR(128)
                                                        END");

                // add "CreatedBy" field in Interviews table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Interviews', 'CreatedBy') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Interviews
                                                                ADD[CreatedBy]  NVARCHAR(128)
                                                        END");

                // add "CreatedOn" field in Interviews table
                db.Database.ExecuteSqlCommand(@"IF COL_LENGTH('Interviews', 'CreatedOn') IS NULL
                                                        BEGIN
                                                                ALTER TABLE Interviews
                                                                ADD[CreatedOn] datetime
                                                        END");

                // Modify "Typerecherche" field in Candidats table
                db.Database.ExecuteSqlCommand(@"IF (SELECT TOP 1 DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Candidats' AND COLUMN_NAME = 'Typerecherche') = 'int'
                                                BEGIN
                                                        ALTER TABLE Candidats
                                                        ALTER COLUMN Typerecherche NVARCHAR(max) NULL
                                                END");

            }

            using (var dbuser = new ApplicationDbContext())
            {
                IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(dbuser);
                ApplicationUserManager UserManager = new ApplicationUserManager(store);
                var user = UserManager.FindByEmail("vincent.lemaitre.01@gmail.com");

                if (!UserManager.IsInRole(user.Id, "Supervisor"))
                {

                    var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbuser));

                    if (!RoleManager.RoleExists("Supervisor"))
                    {
                        IdentityRole newRole = new IdentityRole("Supervisor");
                        IdentityResult result = RoleManager.Create(newRole);
                        if (result.Succeeded)
                            UserManager.AddToRole(user.Id, "Supervisor");

                    }
                    else
                    {
                        UserManager.AddToRole(user.Id, "Supervisor");
                    }
                }

                // Create Supervision space
                var db = new MySqlIdentityDbContext();
                Space spaceSupervision = new Space();
                if (db.Spaces.Where(a => a.Name == "Root").FirstOrDefault() == null)
                {
                    spaceSupervision.Name = "Root";
                    db.Spaces.Add(spaceSupervision);
                    db.SaveChanges();
                }
                if (user != null)
                {
                    var claims = UserManager.GetClaims(user.Id);
                    var claim = claims.Where(a => a.Type == "SpaceId").FirstOrDefault();
                    if (claim == null)
                    {
                        UserManager.AddClaim(user.Id, new Claim("SpaceId", spaceSupervision.Id));
                    }
                }
            }
        }

        public static void InitializeMySqlDatabase()
        {
            // Ajout du compte superviseur et creation de la base si pas exister
            using (var context = new ApplicationDbContext())
            {
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var userToInsert = new ApplicationUser { UserName = "vincent.lemaitre.01@gmail.com", Email = "vincent.lemaitre.01@gmail.com", PhoneNumber = "0797697898" };
                var exist = userManager.FindByEmail("vincent.lemaitre.01@gmail.com");
                if (exist == null)
                {
                    userManager.Create(userToInsert, "pti.preu");

                    // ADD ROLE SUPERVISOR TO USER
                    var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                    if (!RoleManager.RoleExists("Supervisor"))
                    {
                        IdentityRole newRole = new IdentityRole("Supervisor");
                        IdentityResult result = RoleManager.CreateAsync(newRole).Result;
                    }

                    userManager.AddToRoleAsync(userToInsert.Id, "Supervisor");
                }
            }

            // passage du fichier de migration
            Space spaceSupervision = new Space();
            using (var db = new MySqlIdentityDbContext())
            {
                string filPath = AppDomain.CurrentDomain.BaseDirectory + "/Migration/initializeQuery.sql";
                string creationQuery = File.ReadAllText(filPath);
                db.Database.ExecuteSqlCommand(creationQuery);

                // Create Supervision space
                if(db.Spaces.Where(a => a.Name == "Root").FirstOrDefault() == null)
                {
                    spaceSupervision.Name = "Root";
                    db.Spaces.Add(spaceSupervision);
                    db.SaveChanges();
                }
               
            }

            // Ajout de l'espace au compte superviseur
            using (var context2 = new ApplicationDbContext())
            {
                var userStore = new UserStore<ApplicationUser>(context2);
                var userManager = new UserManager<ApplicationUser>(userStore);
               
                var exist = userManager.FindByEmail("vincent.lemaitre.01@gmail.com");
                if (exist != null)
                {
                    var claims = userManager.GetClaims(exist.Id);
                    var claim = claims.Where(a => a.Type == "SpaceId").FirstOrDefault();
                    if (claim == null)
                    {
                        userManager.AddClaim(exist.Id, new Claim("SpaceId", spaceSupervision.Id));
                    }
                }
            }
        }
    }

}