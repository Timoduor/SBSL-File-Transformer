-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               10.5.8-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Version:             11.2.0.6253
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for fileetl
DROP DATABASE IF EXISTS `fileetl`;
CREATE DATABASE IF NOT EXISTS `fileetl` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;
USE `fileetl`;

-- Dumping structure for table fileetl.accounts
DROP TABLE IF EXISTS `accounts`;
CREATE TABLE IF NOT EXISTS `accounts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Entity` longtext DEFAULT NULL,
  `Name` longtext DEFAULT NULL,
  `Number` longtext DEFAULT NULL,
  `Account` longtext DEFAULT NULL,
  `Currency` longtext DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=174 DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.accounts: ~125 rows (approximately)
/*!40000 ALTER TABLE `accounts` DISABLE KEYS */;
REPLACE INTO `accounts` (`Id`, `Entity`, `Name`, `Number`, `Account`, `Currency`) VALUES
	(1, 'IMKE', 'Kisumu', '10070110050001', '', 'KES'),
	(2, 'IMKE', 'Panari', '10090110050001', '', 'KES'),
	(3, 'IMKE', 'Eldoret ', '10170110050001', '', 'KES'),
	(4, 'IMKE', 'Kenyatta', '10010110050001', '', 'KES'),
	(5, 'IMKE', 'Kitale', '10280110050001', '', 'KES'),
	(6, 'IMKE', 'Nakuru ', '10180110050001', '', 'KES'),
	(7, 'IMKE', 'Sarit', '10030110050001', '', 'KES'),
	(8, 'IMKE', 'Parklands', '10100110050001', '', 'KES'),
	(9, 'IMKE', 'Kisii', '10200110050001', '', 'KES'),
	(10, 'IMKE', 'Hailesellasie', '10430110050001', '', 'KES'),
	(11, 'IMKE', 'Mpesa B2C Elma KES', '19990126505010', '', 'KES'),
	(12, 'IMKE', 'AIRTELMONEY KES', '19990126507008', '', 'KES'),
	(13, 'IMKE', 'Mpesa C2B KES', '19990126507009', '', 'KES'),
	(14, 'IMKE', 'CHANGO C2B ULTILITY ACCOUNT - KES', '19990126512001', '', 'KES'),
	(15, 'IMKE', 'CHANGO B2C ULTILITY ACCOUNT - KES', '19990126512002', '', 'KES'),
	(16, 'IMKE', 'BANK OF TOKYO - JPY', '19990711504011', '653-0425699', 'JPY'),
	(17, 'IMKE', 'STANDARD CHARTERED BANK - TOKYO - JPY', '19990711504010', '23414841110', 'JPY'),
	(18, 'IMKE', 'CO-OPERATIVE BANK LTD - KES', '19990111501001', '1175000074200', 'KES'),
	(19, 'IMKE', 'BANK ONE LTD - MUR', '19991724051004', '9199065685', 'MUR'),
	(20, 'IMKE', 'I&M BANK (RWANDA) LIMITED', '19990224051001', '25042907001', 'RWF'),
	(21, 'IMKE', 'I&M BANK (T) LTD - TZS', '19990324051003', '30003547001', 'TZS'),
	(22, 'IMKE', 'DFCU BANK UGANDA', '19991611504014', '1013501211250', 'UGX'),
	(23, 'IMKE', 'BANK ONE LTD - USD', '19990424051004', '794260539', 'USD'),
	(24, 'IMKE', 'CITIBANK NEW YORK - USD', '19990411504018', '36126297', 'USD'),
	(25, 'IMKE', 'I&M BANK (T) LTD - USD', '19990424051002', '10035471111', 'USD'),
	(26, 'IMKE', 'JP MORGAN NEW YORK - USD', '19990411504003', '922195750', 'USD'),
	(27, 'IMKE', 'STANDARD CHARTERED BANK - NY- USD', '19990411504001', '3582023234001', 'USD'),
	(28, 'IMKE', 'ICICI BANK HONG KONG (USD)', '19990411504002', '852032336', 'USD'),
	(29, 'IMKE', 'STANDARD BANK OF S.A - ZAR', '19991411504013', '7222524', 'ZAR'),
	(30, 'IMKE', 'CURRENT ACCOUNT WITH CBK - KES', '19990110501001', '1000004479', 'KES'),
	(31, 'IMKE', 'CURRENT ACCOUNT WITH CBK - GBP', '19990510505002', '1000006088', 'GBP'),
	(32, 'IMKE', 'CURRENT ACCOUNT WITH CBK - EUR', '19990610505001', '1000006471', 'EUR'),
	(33, 'IMKE', 'CURRENT ACCOUNT WITH CBK - USD', '19990410505006', '1000005308', 'USD'),
	(34, 'IMKE', 'CURRENT ACCOUNT WITH CENTRAL BANK-FCY-TZS', '19990310505004', '1000137193', 'TZS'),
	(35, 'IMKE', 'CURRENT ACCOUNT WITH CENTRAL BANK-FCY-UGX', '19991610505005', '1000137223', 'UGX'),
	(36, 'IMKE', 'CURRENT ACCOUNT WITH CENTRAL BANK-FCY-RWF', '19990210505003', '1000206233', 'RWF'),
	(37, 'IMKE', 'STANDARD CHARTERED BANK -AUD', '19990911504013', '01270481192', 'AUD'),
	(38, 'IMKE', 'CITIBANK LONDON - CAD', '19990811504012', '18404186', 'CAD'),
	(39, 'IMKE', 'STANDARD CHARTERED BANK - FFT - EUR', '19990611504007', '018110110', 'EUR'),
	(40, 'IMKE', 'CITIBANK LONDON - GBP', '19990511504004', '8316260', 'GBP'),
	(41, 'IMKE', 'JP MORGAN LONDON - GBP', '19990511504006', '0067101063', 'GBP'),
	(42, 'IMKE', 'STANDARD CHARTERED BANK - LONDON- GBP', '19990511504005', '01269734301', 'GBP'),
	(43, 'IMKE', 'HDFC BANK - INR', '19991311504016', '00600390000077', 'INR'),
	(44, 'IMKE', 'ICICI BANK LTD - INR', '19991311504017', '000405075152', 'INR'),
	(45, 'IMKE', 'YES BANK INDIA-INR', '19991311504012', '000186800000205', 'INR'),
	(46, 'IMKE', 'CO-OPERATIVE BANK LTD - KES', '19990111501001', '01175000074200', 'KES'),
	(47, 'IMKE', 'DFCU BANK UGANDA', '19991611504014', '01013501211250', 'UGX'),
	(48, 'IMKE', 'BANK ONE LTD - USD', '19990424051004', '0794260539', 'USD'),
	(49, 'IMKE', 'CITIBANK NEW YORK - USD', '19990411504018', '36126297/ATT:MR. RAJANI', 'USD'),
	(50, 'IMRW', 'BNR UGX', '20991610505005', '1000025301', 'UGX'),
	(51, 'IMRW', '1ST BANK RAND ZAR', '20991411504017', '9023356', 'ZAR'),
	(52, 'IMRW', 'BANK ONE  EUR', '20990624008002', '0794257911', 'EUR'),
	(53, 'IMRW', 'BANK ONE  GBP', '20990524008002', '0794257912', 'GBP'),
	(54, 'IMRW', 'BANK ONE  USD', '20990424008004', '0794257910', 'USD'),
	(55, 'IMRW', 'BHF BANK EUR', '20990611504021', '0000744581', 'EUR'),
	(56, 'IMRW', 'BHF BANK JPY', '20990711504028', '0300744581', 'JPY'),
	(57, 'IMRW', 'BHF BANK USD', '20990411504035', '0100744581', 'USD'),
	(58, 'IMRW', 'BNR KES', '20990110505004', '1000025123', 'KES'),
	(59, 'IMRW', 'BNR TZS', '20990310505003', '1000025147', 'TZS'),
	(60, 'IMRW', 'I & M BANK KENYA GBP', '20990524008001', '99900618205912', 'GBP'),
	(61, 'IMRW', 'CITIBANK NEW YORK USD', '20990411504034', '36205171', 'USD'),
	(62, 'IMRW', 'CITILONDON EUR', '20990611504022', '13453235', 'EUR'),
	(63, 'IMRW', 'CITILONDON USD', '20990411504036', '11618733', 'USD'),
	(64, 'IMRW', 'COMMERZ BANK CAD', '20990811504038', '400877481200 CAD', 'CAD'),
	(65, 'IMRW', 'COMMERZ BANK CHF', '20991111504039', '400877481200 CHF', 'CHF'),
	(66, 'IMRW', 'COMMERZ BANK EUR', '20990611504020', '400877481200 EUR', 'EUR'),
	(67, 'IMRW', 'COMMERZ BANK GBP', '20990511504037', '400877481200 GBP', 'GBP'),
	(68, 'IMRW', 'COMMERZ BANK USD', '20990411504029', '400877481200 USD', 'USD'),
	(69, 'IMRW', 'I & M BANK KENYA EUR', '20990624008001', '99900618205913', 'EUR'),
	(70, 'IMRW', 'BNR RWF', '20990210501001', '1240000', 'RWF'),
	(71, 'IMRW', 'I & M BANK KENYA KES', '20990124008004', '99900618205914', 'KES'),
	(72, 'IMRW', 'I & M BANK KENYA USD', '20990424008002', '99900618205911', 'USD'),
	(73, 'IMRW', 'I & M BANK TANZANIA USD', '20990424008003', '30005770002', 'USD'),
	(74, 'IMRW', 'I&M BANK TANZANIA TZS', '20990324008001', '30005770001', 'TZS'),
	(75, 'IMRW', 'KCB BANK KENYA  USD', '20990411504033', '1101658959', 'USD'),
	(76, 'IMRW', 'YES BANK INR', '20991311504008', '041986800001040', 'INR'),
	(77, 'IMRW', 'BHF BANK EUR', '20990611504021', '0000744581 I+M BANK (RWANDA)LTD,KIG', 'EUR'),
	(78, 'IMRW', 'BHF BANK USD', '20990411504035', '0100744581 I+M BANK (RWANDA)LTD,KIG', 'USD'),
	(79, 'IMRW', 'BNR EUR', '20990610505001', '1000026561', 'EUR'),
	(80, 'IMRW', 'BNR USD', '20990410505006', '3208000', 'USD'),
	(81, 'IMTZ', 'BHF - EURO', '30990611504001', '0000670018', 'EURO'),
	(82, 'IMTZ', 'SCB BANK - USD', '30990411504006', '3582020982001', 'USD'),
	(83, 'IMTZ', 'ICICI - INR', '30991311504004', '000405075764', 'INR'),
	(84, 'IMTZ', 'CITI- GBP', '30990511504002', '13734366', 'GBP'),
	(85, 'IMTZ', 'CITI- USD', '30990411504005', '36297969', 'USD'),
	(86, 'IMTZ', 'I&M - USD', '30990424051001', '99900463005911', 'USD'),
	(87, 'IMTZ', 'I&M - EUR', '30990624051001', '99900463005913', 'EURO'),
	(88, 'IMTZ', 'I&M  - KES', '30990124051001', '99900463005910', 'KES'),
	(89, 'IMTZ', 'I&M - GBP', '30990524051001', '99900463005912', 'GBP'),
	(90, 'IMTZ', 'I&M - ZAR', '30991424051001', '99900463005914', 'ZAR'),
	(91, 'IMTZ', 'CRDB - TZS', '30990311501001', '01J1027133400', 'TZS'),
	(92, 'IMTZ', 'CRDB - USD', '30990411502001', '0250027133400', 'USD'),
	(93, 'IMTZ', 'DTB - TZS', '30990311501002', '0402498005', 'TZS'),
	(94, 'IMTZ', 'CRDB - SPENN - TZS', '30990311501004', '0150027133400', 'TZS'),
	(95, 'IMTZ', 'I&M JPY', '30990724051001', '99900463005916', 'JPY'),
	(96, 'IMTZ', 'I&M - CAD', '30990824051001', '99900463005915', 'CAD'),
	(97, 'IMTZ', 'I&M - CHF', '30991124051001', '99900463005918', 'CHF'),
	(98, 'IMTZ', 'AXIS BANK - INR', '30991311504003', '916020061637887', 'INR'),
	(99, 'IMTZ', 'BOT - TZS', '30990310501001', '9922181711', 'TZS'),
	(100, 'IMTZ', 'BOT - USD', '30990410505004', '9931203021', 'USD'),
	(101, 'IMTZ', 'BOT - UGX', '30991610505003', '9931209021', 'UGX'),
	(102, 'IMTZ', 'BOT - KES', '30990110505002', '9931208681', 'KES'),
	(103, 'IMTZ', 'BOT - RWF', '30990210505001', '9931218671', 'RWF'),
	(151, 'IMRW', 'AIRTEL B2W', '20100243506065', '', 'RWF'),
	(152, 'IMRW', 'MTN Airtime', '20100243506075', '', 'RWF'),
	(153, 'IMRW', 'MTN Momo', '20100243510014', '', 'RWF'),
	(154, 'IMRW', 'SPENN Cash INOUT', '25049787002', '', 'RWF'),
	(155, 'IMRW', 'FDI', '20100243506073', '', 'RWF'),
	(156, 'IMRW', 'HQ CDM RWANDA', '20100210050001', '', 'RWF'),
	(157, 'IMRW', 'CHIC CDM RWANDA', '20210210050001', '', 'RWF'),
	(158, 'IMRW', 'DSTV', '20013486001', '', 'RWF'),
	(159, 'IMRW', 'MTN PushPull', '20100243506064', '', 'RWF'),
	(161, 'IMRW', 'BNR EUR SETTLEMENT SUSPENSE ACCOUNT ', '20990643511012', '', 'EUR'),
	(162, 'IMRW', 'BNR EUR INWARD ACCOUNT', '20990626501001', '', 'EUR'),
	(163, 'IMRW', 'BNR EUR OUTWARD ACCOUNT', '20990643506023', '', 'EUR'),
	(164, 'IMRW', 'BNR USD SETTLEMENT SUSPENSE ACCOUNT', '20990443511012', '', 'USD'),
	(165, 'IMRW', 'BNR USD INWARD ACCOUNT', '20990426501003', '', 'USD'),
	(166, 'IMRW', 'BNR USD OUTWARD ACCOUNT', '20990443506023', '', 'USD'),
	(167, 'IMRW', 'BNR RWF SETTLEMENT SUSPENSE ACCOUNT LCY', '20990243510012', '', 'RWF'),
	(168, 'IMRW', 'BNR RWF INWARD ACCOUNT', '20990226501020', '', 'RWF'),
	(169, 'IMRW', 'BNR RWF OUTWARD ACCOUNT ', '20990243506026', '', 'RWF'),
	(170, 'IMRW', 'BNR RWF CLEARING ACCOUNT-LCY', '20100211001001', '', 'RWF'),
	(171, 'IMRW', 'BNR RWF', '20990210501001', '1240000', 'RWF'),
	(172, 'IMRW', 'BNR EUR', '20990610505001', '1000026561', 'EUR'),
	(173, 'IMRW', 'BNR USD', '20990410505006', '3208000', 'USD');
/*!40000 ALTER TABLE `accounts` ENABLE KEYS */;

-- Dumping structure for table fileetl.aspnetroleclaims
DROP TABLE IF EXISTS `aspnetroleclaims`;
CREATE TABLE IF NOT EXISTS `aspnetroleclaims` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `RoleId` varchar(255) NOT NULL,
  `ClaimType` longtext DEFAULT NULL,
  `ClaimValue` longtext DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `aspnetroles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.aspnetroleclaims: ~0 rows (approximately)
/*!40000 ALTER TABLE `aspnetroleclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetroleclaims` ENABLE KEYS */;

-- Dumping structure for table fileetl.aspnetroles
DROP TABLE IF EXISTS `aspnetroles`;
CREATE TABLE IF NOT EXISTS `aspnetroles` (
  `Id` varchar(255) NOT NULL,
  `Name` varchar(256) DEFAULT NULL,
  `NormalizedName` varchar(256) DEFAULT NULL,
  `ConcurrencyStamp` longtext DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.aspnetroles: ~0 rows (approximately)
/*!40000 ALTER TABLE `aspnetroles` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetroles` ENABLE KEYS */;

-- Dumping structure for table fileetl.aspnetuserclaims
DROP TABLE IF EXISTS `aspnetuserclaims`;
CREATE TABLE IF NOT EXISTS `aspnetuserclaims` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` varchar(255) NOT NULL,
  `ClaimType` longtext DEFAULT NULL,
  `ClaimValue` longtext DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetUserClaims_UserId` (`UserId`),
  CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `aspnetusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.aspnetuserclaims: ~0 rows (approximately)
/*!40000 ALTER TABLE `aspnetuserclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserclaims` ENABLE KEYS */;

-- Dumping structure for table fileetl.aspnetuserlogins
DROP TABLE IF EXISTS `aspnetuserlogins`;
CREATE TABLE IF NOT EXISTS `aspnetuserlogins` (
  `LoginProvider` varchar(255) NOT NULL,
  `ProviderKey` varchar(255) NOT NULL,
  `ProviderDisplayName` longtext DEFAULT NULL,
  `UserId` varchar(255) NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  KEY `IX_AspNetUserLogins_UserId` (`UserId`),
  CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `aspnetusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.aspnetuserlogins: ~0 rows (approximately)
/*!40000 ALTER TABLE `aspnetuserlogins` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserlogins` ENABLE KEYS */;

-- Dumping structure for table fileetl.aspnetuserroles
DROP TABLE IF EXISTS `aspnetuserroles`;
CREATE TABLE IF NOT EXISTS `aspnetuserroles` (
  `UserId` varchar(255) NOT NULL,
  `RoleId` varchar(255) NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IX_AspNetUserRoles_RoleId` (`RoleId`),
  CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `aspnetroles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `aspnetusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.aspnetuserroles: ~0 rows (approximately)
/*!40000 ALTER TABLE `aspnetuserroles` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserroles` ENABLE KEYS */;

-- Dumping structure for table fileetl.aspnetusers
DROP TABLE IF EXISTS `aspnetusers`;
CREATE TABLE IF NOT EXISTS `aspnetusers` (
  `Id` varchar(255) NOT NULL,
  `UserName` varchar(256) DEFAULT NULL,
  `NormalizedUserName` varchar(256) DEFAULT NULL,
  `Email` varchar(256) DEFAULT NULL,
  `NormalizedEmail` varchar(256) DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` longtext DEFAULT NULL,
  `SecurityStamp` longtext DEFAULT NULL,
  `ConcurrencyStamp` longtext DEFAULT NULL,
  `PhoneNumber` longtext DEFAULT NULL,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
  KEY `EmailIndex` (`NormalizedEmail`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.aspnetusers: ~0 rows (approximately)
/*!40000 ALTER TABLE `aspnetusers` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetusers` ENABLE KEYS */;

-- Dumping structure for table fileetl.aspnetusertokens
DROP TABLE IF EXISTS `aspnetusertokens`;
CREATE TABLE IF NOT EXISTS `aspnetusertokens` (
  `UserId` varchar(255) NOT NULL,
  `LoginProvider` varchar(255) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Value` longtext DEFAULT NULL,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
  CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `aspnetusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.aspnetusertokens: ~0 rows (approximately)
/*!40000 ALTER TABLE `aspnetusertokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetusertokens` ENABLE KEYS */;

-- Dumping structure for table fileetl.configurations
DROP TABLE IF EXISTS `configurations`;
CREATE TABLE IF NOT EXISTS `configurations` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ConfigType` int(11) NOT NULL,
  `Key` longtext NOT NULL,
  `Value` longtext NOT NULL,
  `Updated` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.configurations: ~38 rows (approximately)
/*!40000 ALTER TABLE `configurations` DISABLE KEYS */;
REPLACE INTO `configurations` (`Id`, `ConfigType`, `Key`, `Value`, `Updated`) VALUES
	(1, 0, 'Host', '192.168.56.1', '2021-02-08 14:41:49.401544'),
	(2, 0, 'UserName', 'tester', '2021-02-08 14:41:49.572194'),
	(3, 0, 'Password', 'password', '2021-02-08 14:41:49.591734'),
	(4, 0, 'Port', '22', '2021-02-08 14:41:49.606792'),
	(5, 0, 'ProductionFolder', 'C:\\Users\\Yida\\Downloads\\jobrunner\\940', '2021-02-08 14:41:49.622513'),
	(6, 0, 'IncludeProduction', 'True', '2021-02-08 14:41:49.635587'),
	(7, 0, 'SandboxFolder', 'C:\\Users\\Yida\\Downloads\\jobrunner\\950', '2021-02-08 14:41:49.646879'),
	(8, 0, 'IncludeSandbox', 'False', '2021-02-08 14:41:49.656925'),
	(9, 0, 'ProductionTimeSpanCheck', '3', '2021-04-28 07:17:57.699520'),
	(10, 0, 'SandboxTimeSpanCheck', '5', '2020-12-06 22:56:06.439877'),
	(11, 1, 'ServiceName', 'SBSL ETL Service', '2020-12-09 22:21:42.850588'),
	(12, 2, 'Name', 'SBSL Support Team', '2021-04-07 12:03:48.987063'),
	(15, 2, 'Port', '2500', '2021-04-07 12:03:48.972431'),
	(16, 2, 'SmtpServer', 'localhost', '2021-04-07 12:03:49.000835'),
	(18, 2, 'Recipients', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', '2021-04-07 12:03:49.026060'),
	(19, 4, 'Entity', 'IMTZ', '2020-12-22 21:21:44.552901'),
	(20, 2, 'UseSsl', 'False', '2021-04-07 12:03:48.946868'),
	(21, 2, 'UserName', 'support@sbsl.co.ke', '2021-04-07 12:03:48.782250'),
	(22, 2, 'Password', 'CfDJ8PNFZTHWEe1ClYjRbfPzggcxY3TwnWFGyV6F_-wnwQTZceSyjXjtqPzK2m7A2TueR_Wt3C0iuHJ1VODJO8m3PKqdLt33G4hgFa83GWvo0KCNGvM1-wGRGcAWbL5rTevgZQ', '2021-04-07 12:03:48.925568'),
	(23, 2, 'EmailAddress', 'support@sbsl.co.ke', '2021-04-07 12:03:49.012622'),
	(24, 3, 'EmailHeader', 'Report on Outstanding Reconciliations', '2021-01-02 12:02:11.715439'),
	(25, 5, 'SBSL_00', 'f40624e13d3b230847de22036faf918b--', '2020-12-09 23:09:40.449049'),
	(27, 3, 'ClientId', 'imbank', '2020-12-09 23:09:40.449049'),
	(28, 3, 'ClientSecret', 'NNlkX>;<%q[\\', '2020-12-09 23:09:40.449049'),
	(29, 3, 'Scope', 'ReportsAPI instance_0C9B27C5-0EA5-43CF-BFA3-C9AAD16C2BCF', '2020-12-09 23:09:40.449049'),
	(30, 3, 'ExportType', 'Excel', '2020-12-09 23:09:40.449049'),
	(31, 3, 'TokenUrl', 'https://eu1.api.blackline.com/authorize/connect/token', '2020-12-09 23:09:40.449049'),
	(32, 3, 'BaseUrl', 'api.blackline.com/api', '2020-12-09 23:09:40.449049'),
	(33, 3, 'EnvironmentUrl', 'eu1', '2020-12-09 23:09:40.449049'),
	(34, 0, 'BackUpFolder', 'C:\\Users\\Yida\\Downloads\\jobrunner\\BackUp', '2020-12-09 23:09:40.622601'),
	(35, 4, 'BackUpAllFilesPeriod', '9', '2020-12-09 23:09:40.449049'),
	(36, 2, 'UseDefaultCredentials', 'True', '2021-04-07 12:03:48.960086'),
	(37, 5, 'Jamlick.maina', '7cff7572fcc196134f896b45c5961c2e', '2021-02-01 10:22:32.000000'),
	(38, 5, 'EstherNW', 'ca57333763d57e7a12ca3f59c0c30ab3', '2021-02-01 10:22:56.000000'),
	(39, 4, 'PdfPassword', '001402498', '2021-02-08 14:18:36.000000'),
	(40, 5, 'GenevieveNyirahabim0789', '3f140c4f6ee3fb0b1d7f5d9facfe689f', '2021-02-21 12:24:04.000000'),
	(41, 5, 'System.Scheduler', '01467571fb472f33f635acd46377e0ce', '2021-04-06 15:17:52.000000'),
	(47, 0, 'Key2FilesPath', 'E:\\imbank\\KeyTesting', '2021-05-07 17:19:25.000000'),
	(49, 7, 'GLExemptAccounts', '25049787002,25049787004,20100243506064', '2021-05-14 10:47:18.752932'),
	(50, 0, 'UseUnicode', 'False', '2021-05-18 15:55:43.000000');
/*!40000 ALTER TABLE `configurations` ENABLE KEYS */;

-- Dumping structure for table fileetl.emailgroups
DROP TABLE IF EXISTS `emailgroups`;
CREATE TABLE IF NOT EXISTS `emailgroups` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `GroupName` longtext DEFAULT NULL,
  `Emails` longtext DEFAULT NULL,
  `AgeAlertDuration` int(11) NOT NULL,
  `Account` longtext DEFAULT NULL,
  `Description` longtext DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `Country` int(11) NOT NULL DEFAULT 0,
  `Sprint` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.emailgroups: ~17 rows (approximately)
/*!40000 ALTER TABLE `emailgroups` DISABLE KEYS */;
REPLACE INTO `emailgroups` (`Id`, `GroupName`, `Emails`, `AgeAlertDuration`, `Account`, `Description`, `IsActive`, `Country`, `Sprint`) VALUES
	(1, 'Test Group', 'bryson@sbsl.co.ke,bryson@sbsl.co.ke,bryson@sbsl.co.ke,bryson@sbsl.co.ke,bryson@sbsl.co.ke,bryson@sbsl.co.ke,', 7, NULL, 'This is for non ageing reports', 0, 0, 0),
	(2, 'Nostro_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 3, 'N/A', 'Nostro_Kenya', 1, 0, 0),
	(3, 'Nostro_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 5, 'N/A', 'Nostro_Kenya', 1, 0, 0),
	(4, 'Nostro_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 7, 'N/A', 'Nostro_Kenya', 1, 0, 0),
	(5, 'Nostro_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 30, 'N/A', 'Nostro_Kenya', 1, 0, 0),
	(7, 'MB_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 4, 'N/A', 'MB_Kenya', 1, 0, 1),
	(8, 'MB_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 7, 'N/A', 'MB_Kenya', 1, 0, 1),
	(9, 'MB_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 15, 'N/A', 'MB_Kenya', 1, 0, 1),
	(10, 'MB_Kenya', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 30, 'N/A', 'MB_Kenya', 1, 0, 1),
	(11, 'MB_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 2, 'N/A', 'MB_Rwanda', 1, 1, 1),
	(12, 'MB_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 5, 'N/A', 'MB_Rwanda', 1, 1, 1),
	(13, 'MB_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 10, 'N/A', 'MB_Rwanda', 1, 1, 1),
	(14, 'MB_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 30, 'N/A', 'MB_Rwanda', 1, 1, 1),
	(15, 'Nostro_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 3, 'N/A', 'Nostro_Rwanda', 1, 2, 0),
	(16, 'Nostro_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 5, 'N/A', 'Nostro_Rwanda', 1, 2, 0),
	(17, 'Nostro_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 7, 'N/A', 'Nostro_Rwanda', 1, 2, 0),
	(18, 'Nostro_Rwanda', 'bryson@sbsl.co.ke, kellen@sbsl.co.ke', 30, 'N/A', 'Nostro_Rwanda', 1, 2, 0);
/*!40000 ALTER TABLE `emailgroups` ENABLE KEYS */;

-- Dumping structure for table fileetl.plugins
DROP TABLE IF EXISTS `plugins`;
CREATE TABLE IF NOT EXISTS `plugins` (
  `Id` char(36) NOT NULL,
  `Name` longtext DEFAULT NULL,
  `Description` longtext DEFAULT NULL,
  `InputFolder` longtext DEFAULT NULL,
  `OutputFolder` longtext DEFAULT NULL,
  `StartDelay` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.plugins: ~2 rows (approximately)
/*!40000 ALTER TABLE `plugins` DISABLE KEYS */;
REPLACE INTO `plugins` (`Id`, `Name`, `Description`, `InputFolder`, `OutputFolder`, `StartDelay`) VALUES
	('ab25e115-1be3-48a3-923b-30ddb2b5c366', 'MT 940/950 Converter', 'This plugin converts the Tz MT files into Standard Ke MT files', 'C:\\Users\\Yida\\Downloads\\jobrunner', NULL, 1),
	('abbae997-0ae8-4ce7-9a14-a7b2d84b21db', 'Cdm Converter', 'This plugin loads cdm excel files and converts them to a csv format that blackline can process', 'C:\\Users\\Yida\\Downloads\\jobrunner\\output', NULL, 1);
/*!40000 ALTER TABLE `plugins` ENABLE KEYS */;

-- Dumping structure for table fileetl.processedreports
DROP TABLE IF EXISTS `processedreports`;
CREATE TABLE IF NOT EXISTS `processedreports` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `ReportId` bigint(20) NOT NULL,
  `ProcessedDate` datetime(6) NOT NULL,
  `Name` longtext DEFAULT NULL,
  `Format` longtext DEFAULT NULL,
  `Creator` longtext DEFAULT NULL,
  `EndTime` longtext DEFAULT NULL,
  `Message` longtext DEFAULT NULL,
  `Notes` longtext DEFAULT NULL,
  `StartTime` longtext DEFAULT NULL,
  `Status` longtext DEFAULT NULL,
  `UserToken` longtext DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=787 DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.processedreports: ~48 rows (approximately)
/*!40000 ALTER TABLE `processedreports` DISABLE KEYS */;
/*!40000 ALTER TABLE `processedreports` ENABLE KEYS */;

-- Dumping structure for table fileetl.uploadedfiles
DROP TABLE IF EXISTS `uploadedfiles`;
CREATE TABLE IF NOT EXISTS `uploadedfiles` (
  `Id` char(36) NOT NULL,
  `Name` longtext DEFAULT NULL,
  `Md5` longtext DEFAULT NULL,
  `UploadedDate` datetime(6) NOT NULL,
  `Size` bigint(20) NOT NULL,
  `IsProduction` tinyint(1) NOT NULL,
  `FilePath` longtext DEFAULT NULL,
  `MtAccountNo` longtext DEFAULT NULL,
  `MtStatementNo` longtext DEFAULT NULL,
  `MtSequenceNo` longtext DEFAULT NULL,
  `ProcessFor62F` tinyint(1) NOT NULL,
  `Converted` tinyint(1) NOT NULL,
  `ConvertedBy` longtext DEFAULT NULL,
  `Failed` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.uploadedfiles: ~20 rows (approximately)
/*!40000 ALTER TABLE `uploadedfiles` DISABLE KEYS */;
REPLACE INTO `uploadedfiles` (`Id`, `Name`, `Md5`, `UploadedDate`, `Size`, `IsProduction`, `FilePath`, `MtAccountNo`, `MtStatementNo`, `MtSequenceNo`, `ProcessFor62F`, `Converted`, `ConvertedBy`, `Failed`) VALUES
	('08d91dda-978a-4426-8b48-ff39cedba68f', 'BR_SUS_BALANCE_14May2021.CSV', '98fd43ca20c95b0087f6771c9241558c', '2021-05-23 14:04:53.180702', 4394, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\BR_SUS_BALANCE_14May2021.CSV', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97ab-46c6-8261-a8ce70c8152d', 'FCO_SUS_BALANCE_14May2021.CSV', 'e21d94d497186c92b075205e3ee8238f', '2021-05-23 14:04:53.429984', 5693, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\FCO_SUS_BALANCE_14May2021.CSV', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97af-4128-84fb-c5f1231e0d91', 'GLAccounts_20210513000000_SUS_IMTZ.txt', '45a818af55fbf460e77bbabfbaeec8f7', '2021-05-23 14:04:53.454339', 9424, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\GLAccounts_20210513000000_SUS_IMTZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97b2-4aed-8c87-f2addcec7a91', 'GLAccounts_20210513_174_SUS_IMTZ.txt', '0cfd9d658b1fc78b13a0221a943fd9e5', '2021-05-23 14:04:53.478080', 11714, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\GLAccounts_20210513_174_SUS_IMTZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97ba-453e-8f7e-ceac53d80c93', 'MultiCurr_2021_05_19_ance_20210422_SPEN_CTRL_TZ.txt', '6a02e9aa37bf711329afb67e1b446b60', '2021-05-23 14:04:53.528182', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210422_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97d4-47e1-8926-1f807470923c', 'MultiCurr_2021_05_19_ance_20210423_SPEN_CTRL_TZ.txt', '2b416bdd48c99a5ff89cdc87c904ebb5', '2021-05-23 14:04:53.699644', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210423_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97d9-4a42-8cf0-dd9bb2796d83', 'MultiCurr_2021_05_19_ance_20210427_SPEN_CTRL_TZ.txt', '03be30709516f0cdd87177586f906739', '2021-05-23 14:04:53.733407', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210427_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97dd-4af3-8781-81310ac0bd94', 'MultiCurr_2021_05_19_ance_20210428_SPEN_CTRL_TZ.txt', 'e5be14bb2b567f17b55306e6abe872cf', '2021-05-23 14:04:53.759889', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210428_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97e1-47b2-814f-9b1bef35c001', 'MultiCurr_2021_05_19_ance_20210429_SPEN_CTRL_TZ.txt', 'c415ea7b0d739a185ad02ccdf7a3300c', '2021-05-23 14:04:53.784794', 90, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210429_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97e5-4203-82f3-92c81697778f', 'MultiCurr_2021_05_19_ance_20210502_SPEN_CTRL_TZ.txt', '03a21c69bfe5ee7760a39535e911480b', '2021-05-23 14:04:53.808658', 90, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210502_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97e9-4479-832b-d5df022d66b5', 'MultiCurr_2021_05_19_ance_20210506_SPEN_CTRL_TZ.txt', '815d4e668da1a8862a4d78a67f793a8d', '2021-05-23 14:04:53.835845', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210506_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97ec-4f01-88a5-346fc1957c73', 'MultiCurr_2021_05_19_ance_20210507_SPEN_CTRL_TZ.txt', '9810929f59e2f8ac3c57c675ab49ffbf', '2021-05-23 14:04:53.859865', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210507_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97f2-465a-8c69-c8f66dc65462', 'MultiCurr_2021_05_19_ance_20210510_SPEN_CTRL_TZ.txt', 'c056c240f4cc7d5af2395bfabe6839b1', '2021-05-23 14:04:53.894930', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210510_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97f8-4efb-8257-5524483ea269', 'MultiCurr_2021_05_19_ance_20210511_SPEN_CTRL_TZ.txt', 'cc279b9a73cc47392dfbcb9a4d515cb7', '2021-05-23 14:04:53.938274', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210511_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-97ff-42c3-8916-99585116d6a2', 'MultiCurr_2021_05_19_ance_20210512_SPEN_CTRL_TZ.txt', '821c14a8e252a479e5cd1c8673fe4543', '2021-05-23 14:04:53.979371', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210512_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-9803-4b1a-86a3-16fc4cd86667', 'MultiCurr_2021_05_19_ance_20210513_SPEN_CTRL_TZ.txt', 'c82ae22dfc8c5eb71f6e2967ad0123a1', '2021-05-23 14:04:54.008993', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_ance_20210513_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-9808-4d2b-85a5-00e4c16e3f7b', 'MultiCurr_2021_05_19_MTN_07MAY2021_MTN_IMTZ.txt', 'd103f116c4ead34f80c2de09e383f12a', '2021-05-23 14:04:54.042616', 82, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_19_MTN_07MAY2021_MTN_IMTZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d91dda-9839-4f83-848a-1abe8167c515', '2021_05_19_21_17_Weekly_20213rdParty.csv', '5395bbf640d24f8ae4c9387c86bd1e3d', '2021-05-23 14:04:54.364681', 2055607, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imke\\Utilities\\OMNI_SETT\\Conv\\2021_05_19_21_17_Weekly_20213rdParty.csv', '', '', '', 0, 0, NULL, 0),
	('08d91dda-987f-4de9-8ff1-f03117081760', 'EndOfDayBalance_20210512.txt', '92ac9438f97adb5e52a79d67843d5920', '2021-05-23 14:04:54.822391', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210512.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d91dda-988a-4390-8f1b-24fb5ded8dcb', 'EndOfDayBalance_20210513.txt', '57d43d959e5258fdd9da54d7a1a13f2e', '2021-05-23 14:04:54.890541', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210513.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0);
/*!40000 ALTER TABLE `uploadedfiles` ENABLE KEYS */;

-- Dumping structure for table fileetl.__efmigrationshistory
DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE IF NOT EXISTS `__efmigrationshistory` (
  `MigrationId` varchar(95) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.__efmigrationshistory: ~3 rows (approximately)
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
REPLACE INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
	('20210118180133_Initialize Mysql', '3.1.10'),
	('20210403173623_Reporting', '3.1.11'),
	('20210403221733_Reporting', '3.1.11'),
	('20210404115736_Report2', '3.1.11'),
	('20210429102640_ConvertedBy', '3.1.11');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
