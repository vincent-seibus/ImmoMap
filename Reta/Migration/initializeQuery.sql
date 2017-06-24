
--
-- Table structure for table `emailtemplates`
--

CREATE TABLE IF NOT EXISTS `emailtemplates` (
  `Id` varchar(128) NOT NULL,
  `Name` longtext,
  `Type` longtext,
  `Subject` longtext,
  `Body` longtext,
  `BodyUrl` longtext NULL,
  `Lang` VARCHAR(128) NULL,
  `DynamicField` longtext,
  `SpaceId` longtext,
  `DeletedOn` datetime,
  `DeletedBy` varchar(128),
  `CreatedBy` varchar(128),
  `CreatedOn` datetime,
  PRIMARY KEY (`ID`)
); 

--
-- Table structure for table `spaces`
--

CREATE TABLE IF NOT EXISTS `spaces` (
  `Id` varchar(128) NOT NULL,
  `Name` longtext,
  `Configuration` longtext,
  `Metadata` longtext,
  `DeletedOn` datetime,
  `DeletedBy` varchar(128),
  `CreatedBy` varchar(128),
  `CreatedOn` datetime,
  `SpaceId` longtext,
  PRIMARY KEY (`Id`)
); 

CREATE TABLE IF NOT EXISTS `buildings` (
  `Id` varchar(128) NOT NULL,
  `Name` longtext,
  `Configuration` longtext,
  `Metadata` longtext,
  `DeletedOn` datetime,
  `DeletedBy` varchar(128),
  `CreatedBy` varchar(128),
  `CreatedOn` datetime,
  `SpaceId` longtext,
  PRIMARY KEY (`Id`)
); 
