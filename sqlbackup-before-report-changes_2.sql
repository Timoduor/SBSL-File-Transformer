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
) ENGINE=InnoDB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8mb4;

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
	(37, 5, 'Jamlick.maina', '7cff7572fcc196134f896b45c5961c2e---', '2021-02-01 10:22:32.000000'),
	(38, 5, 'EstherNW', 'ca57333763d57e7a12ca3f59c0c30ab3---', '2021-02-01 10:22:56.000000'),
	(39, 4, 'PdfPassword', '001402498', '2021-02-08 14:18:36.000000'),
	(40, 5, 'GenevieveNyirahabim0789', '3f140c4f6ee3fb0b1d7f5d9facfe689f---', '2021-02-21 12:24:04.000000'),
	(41, 5, 'System.Scheduler', '01467571fb472f33f635acd46377e0ce', '2021-04-06 15:17:52.000000'),
	(47, 0, 'KeyFilesPath', ' ', '2021-05-07 17:19:25.000000'),
	(49, 7, 'GLExemptAccounts', '25049787002,25049787004,20100243506064', '2021-05-14 10:47:18.752932');
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
) ENGINE=InnoDB AUTO_INCREMENT=739 DEFAULT CHARSET=utf8mb4;

-- Dumping data for table fileetl.processedreports: ~55 rows (approximately)
/*!40000 ALTER TABLE `processedreports` DISABLE KEYS */;
REPLACE INTO `processedreports` (`Id`, `ReportId`, `ProcessedDate`, `Name`, `Format`, `Creator`, `EndTime`, `Message`, `Notes`, `StartTime`, `Status`, `UserToken`) VALUES
	(677, 3641685, '2021-04-07 14:20:07.497593', 'Rwanda_Nostro_BNR Open Items Daily Report', 'Excel', 'System Scheduler', '04/06/2021 07:56:44', '04/06/2021 07:56:44', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/06/2021 07:56:43', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(678, 3641684, '2021-04-07 14:21:03.783713', 'Rwanda_NOSTRO_BNR Balance Proofing', 'Excel', 'System Scheduler', '04/06/2021 07:56:39', '04/06/2021 07:56:39', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '04/06/2021 07:56:37', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(679, 3641683, '2021-04-07 14:21:57.084267', 'RWANDA_ABC_SPENN CASHINOUT GROUP  OPEN ITEMS REPORT', 'Excel', 'System Scheduler', '04/06/2021 07:56:31', '04/06/2021 07:56:31', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/06/2021 07:56:29', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(680, 3641678, '2021-04-07 14:22:50.544422', 'RWANDA_ABC_MTN PUSH PULL OPEN ITEMS REPORT', 'Excel', 'System Scheduler', '04/06/2021 07:56:25', '04/06/2021 07:56:25', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/06/2021 07:56:23', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(681, 3641673, '2021-04-07 14:23:45.823239', 'Rwanda_ABC_Mobile Banking Balance Proofing Report', 'Excel', 'System Scheduler', '04/06/2021 07:56:04', '04/06/2021 07:56:04', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/06/2021 07:56:00', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(682, 3641670, '2021-04-07 14:24:39.531529', 'RWANDA_ABC_FDI UTILITIES OPEN ITEMS REPORT', 'Excel', 'System Scheduler', '04/06/2021 07:55:56', '04/06/2021 07:55:56', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/06/2021 07:55:54', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(683, 3641667, '2021-04-07 14:25:37.407188', 'RWANDA_ABC_CDM HQ Open Item Report', 'Excel', 'System Scheduler', '04/06/2021 07:55:49', '04/06/2021 07:55:49', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/06/2021 07:55:47', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(684, 3641665, '2021-04-07 14:26:34.293303', 'RWANDA_ABC_CDM CHIC Open Item Report', 'Excel', 'System Scheduler', '04/06/2021 07:55:42', '04/06/2021 07:55:42', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/06/2021 07:55:41', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(685, 3641664, '2021-04-07 14:27:34.184982', 'Rwanda_ABC_CDM Balance Proofing Report', 'Excel', 'System Scheduler', '04/06/2021 07:55:35', '04/06/2021 07:55:35', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/06/2021 07:55:33', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(686, 3641655, '2021-04-07 14:28:32.958918', 'Rwanda Nostro Open Items Daily Report', 'Excel', 'System Scheduler', '04/06/2021 07:55:21', '04/06/2021 07:55:21', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/06/2021 07:55:18', 'Complete', '3f7c88cff8f00c03d413002798c07130'),
	(687, 3641654, '2021-04-07 14:36:29.773175', 'Rwanda Nostro Balance Proofing', 'Excel', 'System Scheduler', '04/06/2021 07:55:15', '04/06/2021 07:55:15', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '04/06/2021 07:55:13', 'Complete', 'feb17eceec8e7e3f0e039cd44ab41fbb'),
	(688, 3599319, '2021-04-07 14:36:59.407070', 'Rwanda Nostro Open Items Daily Report', 'Excel', 'System Scheduler', '03/30/2021 04:10:36', '03/30/2021 04:10:36', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/30/2021 04:10:34', 'Complete', 'feb17eceec8e7e3f0e039cd44ab41fbb'),
	(689, 3597430, '2021-04-07 14:37:28.832049', 'User Role Assignment', 'Excel', 'System Scheduler', '03/30/2021 00:56:16', '03/30/2021 00:56:16', 'Certification Status By People - Summary report showing number of accounts by Person and Role in each reconciliation status (Prepared, Approved, Reviewed, Auto-Certified, Not Prepared) along with the number of Not Assigned. This report has a filter on Assignment Roles.', '03/30/2021 00:56:15', 'Complete', 'feb17eceec8e7e3f0e039cd44ab41fbb'),
	(690, 3597414, '2021-04-07 14:37:58.629037', 'User Access', 'Excel', 'System Scheduler', '03/30/2021 00:55:16', '03/30/2021 00:55:16', 'User Access - List of users showing their current authorized Roles by module, Entity hierarchy and Team assignments with the date and user name of who made the last changes to the user\'s status.', '03/30/2021 00:55:14', 'Complete', 'feb17eceec8e7e3f0e039cd44ab41fbb'),
	(691, 3597266, '2021-04-07 14:38:27.662790', 'RW CDM CHIC Open Item Report', 'Excel', 'System Scheduler', '03/30/2021 00:36:07', '03/30/2021 00:36:07', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/30/2021 00:36:06', 'Complete', 'feb17eceec8e7e3f0e039cd44ab41fbb'),
	(692, 3583276, '2021-04-07 14:38:57.483472', 'Rwanda BNR Open Items Daily Report', 'Excel', 'System Scheduler', '03/26/2021 23:25:47', '03/26/2021 23:25:47', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/26/2021 23:25:46', 'Complete', 'feb17eceec8e7e3f0e039cd44ab41fbb'),
	(693, 3583275, '2021-04-07 14:39:26.539216', 'Rwanda BNR Balance Proofing', 'Excel', 'System Scheduler', '03/26/2021 23:25:40', '03/26/2021 23:25:40', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '03/26/2021 23:25:39', 'Complete', 'feb17eceec8e7e3f0e039cd44ab41fbb'),
	(694, 3674878, '2021-04-10 14:57:51.098222', 'Kenya Utilities Balance Proofing Report', 'Excel', 'System Scheduler', '04/09/2021 05:01:53', '04/09/2021 05:01:53', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/09/2021 05:01:51', 'Complete', '4d98f352d6c6f1be5cd18c4165ffaf50'),
	(695, 3663326, '2021-04-10 14:58:20.383359', 'Mpesa C2B Chango balance proofing report', 'Excel', 'System Scheduler', '04/08/2021 06:51:51', '04/08/2021 06:51:51', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/08/2021 06:51:48', 'Complete', '4d98f352d6c6f1be5cd18c4165ffaf50'),
	(696, 3663321, '2021-04-10 15:00:29.933437', 'Mpesa B2C Chango balance proofing report', 'Excel', 'System Scheduler', '04/08/2021 06:51:45', '04/08/2021 06:51:45', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/08/2021 06:51:43', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(697, 3663086, '2021-04-10 15:01:02.569056', 'Mpesa C2B Chango balance proofing report', 'Excel', 'System Scheduler', '04/08/2021 06:36:02', '04/08/2021 06:36:02', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/08/2021 06:36:00', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(698, 3663045, '2021-04-10 15:01:33.959263', 'Mpesa B2C Chango balance proofing report', 'Excel', 'System Scheduler', '04/08/2021 06:34:03', '04/08/2021 06:34:03', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/08/2021 06:34:01', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(699, 3663018, '2021-04-10 15:02:07.484328', 'Mpesa B2C Chango balance proofing report', 'Excel', 'System Scheduler', '04/08/2021 06:32:08', '04/08/2021 06:32:08', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/08/2021 06:32:06', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(700, 3663016, '2021-04-10 15:02:38.887752', 'Mpesa C2B Chango balance proofing report', 'Excel', 'System Scheduler', '04/08/2021 06:32:02', '04/08/2021 06:32:02', 'Summary showing the status of each reconciliation, Unidentified Difference, and individual Supporting Items and their Item Class.', '04/08/2021 06:32:00', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(701, 3663010, '2021-04-10 15:03:10.504617', 'MPESA_C2B_CHANGO Open Items Daily Report', 'Excel', 'System Scheduler', '04/08/2021 06:31:50', '04/08/2021 06:31:50', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/08/2021 06:31:48', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(702, 3571453, '2021-04-10 15:03:43.271526', 'TZ Mobile Banking Balance Proofing', 'Excel', 'System Scheduler', '03/25/2021 05:07:16', '03/25/2021 05:07:16', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '03/25/2021 05:07:14', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(703, 3565874, '2021-04-10 15:04:12.805203', 'Rwanda BNR Open Items Daily Report', 'Excel', 'System Scheduler', '03/24/2021 12:30:33', '03/24/2021 12:30:33', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/24/2021 12:30:31', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(704, 3546177, '2021-04-10 15:04:42.221946', 'Rwanda BNR Open Items Daily Report', 'Excel', 'System Scheduler', '03/22/2021 08:28:53', '03/22/2021 08:28:53', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/22/2021 08:28:51', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(705, 3494298, '2021-04-10 15:05:15.599613', 'KE Nostro Open Items Daily Report', 'Excel', 'System Scheduler', '03/17/2021 01:22:18', '03/17/2021 01:22:18', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/17/2021 01:22:15', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(706, 3494295, '2021-04-10 15:05:45.401263', 'KE Nostro Open Items Daily Report', 'Excel', 'System Scheduler', '03/17/2021 01:21:50', '03/17/2021 01:21:50', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/17/2021 01:21:48', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(707, 3342609, '2021-04-10 15:06:14.527690', 'Rwanda Nostro Balance Proofing', 'Excel', 'System Scheduler', '03/02/2021 00:51:45', '03/02/2021 00:51:45', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '03/02/2021 00:51:43', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(708, 3342607, '2021-04-10 15:06:43.970420', 'Rwanda Nostro Open Items Daily Report', 'Excel', 'System Scheduler', '03/02/2021 00:50:38', '03/02/2021 00:50:38', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/02/2021 00:50:36', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(709, 3339174, '2021-04-10 15:07:13.217914', 'KE Global Nostro Balance Proofing', 'Excel', 'System Scheduler', '03/01/2021 11:13:25', '03/01/2021 11:13:25', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '03/01/2021 11:13:23', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(710, 3337358, '2021-04-10 15:07:42.880841', 'KE Nostro Open Items Daily Report', 'Excel', 'System Scheduler', '03/01/2021 07:40:32', '03/01/2021 07:40:32', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/01/2021 07:40:30', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(711, 3337357, '2021-04-10 15:08:12.442143', 'KE Nostro Balance Proofing', 'Excel', 'System Scheduler', '03/01/2021 07:40:21', '03/01/2021 07:40:21', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '03/01/2021 07:40:19', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(712, 3337354, '2021-04-10 15:08:41.622056', 'KE Global Nostro Balance Proofing', 'Excel', 'System Scheduler', '03/01/2021 07:40:09', '03/01/2021 07:40:09', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '03/01/2021 07:40:07', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(713, 3327101, '2021-04-10 15:09:12.617114', 'Rwanda Nostro Balance Proofing', 'Excel', 'System Scheduler', '02/26/2021 12:53:02', '02/26/2021 12:53:02', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '02/26/2021 12:53:00', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(714, 3327095, '2021-04-10 15:09:41.756214', 'KE Global Nostro Balance Proofing', 'Excel', 'System Scheduler', '02/26/2021 12:51:55', '02/26/2021 12:51:55', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '02/26/2021 12:51:53', 'Complete', 'a52659655d1a78d391fd4942cb7ae4dd'),
	(715, 3691971, '2021-04-12 16:32:53.107229', 'KE Utilities Finacle Data Source Status', 'Excel', 'System Scheduler', '04/12/2021 02:46:06', '04/12/2021 02:46:06', 'Use the Matching Data Source Template to create a report on matching data from a Data Source level. Select a Data Source to add the Data Source\'s columns to the Report Fields. Add fields to report on Data Source data and include properties of the Match Sets associated with the Data Source.', '04/12/2021 02:46:03', 'Complete', 'd9794b09fbc840cd656858494ea97433'),
	(716, 3713095, '2021-04-13 22:17:26.705275', 'KE Utilities Finacle Data Source Status', 'Excel', 'System Scheduler', '04/13/2021 07:26:46', '04/13/2021 07:26:46', 'Use the Matching Data Source Template to create a report on matching data from a Data Source level. Select a Data Source to add the Data Source\'s columns to the Report Fields. Add fields to report on Data Source data and include properties of the Match Sets associated with the Data Source.', '04/13/2021 07:26:43', 'Complete', '54f9b71e9524633a0699fe269c677856'),
	(717, 3712776, '2021-04-13 22:17:55.904576', 'Kenya Utilities Balance Proofing Report', 'Excel', 'System Scheduler', '04/13/2021 07:10:21', '04/13/2021 07:10:21', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/13/2021 07:10:16', 'Complete', '54f9b71e9524633a0699fe269c677856'),
	(718, 3712773, '2021-04-13 22:18:30.413032', 'KE Utilities Finacle Data Source Status', 'Excel', 'System Scheduler', '04/13/2021 07:10:09', '04/13/2021 07:10:09', 'Use the Matching Data Source Template to create a report on matching data from a Data Source level. Select a Data Source to add the Data Source\'s columns to the Report Fields. Add fields to report on Data Source data and include properties of the Match Sets associated with the Data Source.', '04/13/2021 07:10:06', 'Complete', '54f9b71e9524633a0699fe269c677856'),
	(719, 3709933, '2021-04-13 22:51:23.455696', 'KE Utilities Finacle Data Source Status', 'Excel', 'System Scheduler', '04/13/2021 04:23:07', '04/13/2021 04:23:07', 'Use the Matching Data Source Template to create a report on matching data from a Data Source level. Select a Data Source to add the Data Source\'s columns to the Report Fields. Add fields to report on Data Source data and include properties of the Match Sets associated with the Data Source.', '04/13/2021 04:23:04', 'Complete', '4a3db633d9e9d94e9f10c8efff17b579'),
	(720, 3709700, '2021-04-13 22:51:56.975366', 'KE Utilities Finacle Data Source Status', 'Excel', 'System Scheduler', '04/13/2021 04:08:02', '04/13/2021 04:08:02', 'Use the Matching Data Source Template to create a report on matching data from a Data Source level. Select a Data Source to add the Data Source\'s columns to the Report Fields. Add fields to report on Data Source data and include properties of the Match Sets associated with the Data Source.', '04/13/2021 04:07:58', 'Complete', '4a3db633d9e9d94e9f10c8efff17b579'),
	(721, 3709621, '2021-04-13 22:52:31.660386', 'KE Utilities Finacle Data Source Status', 'Excel', 'System Scheduler', '04/13/2021 04:03:48', '04/13/2021 04:03:48', 'Use the Matching Data Source Template to create a report on matching data from a Data Source level. Select a Data Source to add the Data Source\'s columns to the Report Fields. Add fields to report on Data Source data and include properties of the Match Sets associated with the Data Source.', '04/13/2021 04:03:45', 'Complete', '4a3db633d9e9d94e9f10c8efff17b579'),
	(722, 3698261, '2021-04-13 22:54:21.964512', 'User Access', 'Excel', 'System Scheduler', '04/12/2021 08:13:15', '04/12/2021 08:13:15', 'User Access - List of users showing their current authorized Roles by module, Entity hierarchy and Team assignments with the date and user name of who made the last changes to the user\'s status.', '04/12/2021 08:13:13', 'Complete', '2e80cad2e5a4379b6e00984dcf60cd0d'),
	(723, 3720648, '2021-04-14 11:22:17.337829', 'RWANDA_ABC_MTN PUSH PULL OPEN ITEMS REPORT', 'Excel', 'System Scheduler', '04/13/2021 23:24:03', '04/13/2021 23:24:03', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/13/2021 23:24:01', 'Complete', '9f6e0655ca9df90d1ae75021ec16ba09'),
	(724, 3571515, '2021-04-14 11:22:50.557505', 'Tanzania Mobile Banking Open Items Daily Report', 'Excel', 'System Scheduler', '03/25/2021 05:13:23', '03/25/2021 05:13:23', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '03/25/2021 05:13:21', 'Complete', '9f6e0655ca9df90d1ae75021ec16ba09'),
	(725, 3755514, '2021-04-16 14:09:38.030334', 'User Role Assignment', 'Excel', 'System Scheduler', '04/16/2021 00:02:48', '04/16/2021 00:02:48', 'Certification Status By People - Summary report showing number of accounts by Person and Role in each reconciliation status (Prepared, Approved, Reviewed, Auto-Certified, Not Prepared) along with the number of Not Assigned. This report has a filter on Assignment Roles.', '04/16/2021 00:02:45', 'Complete', '90f87b9cb0acd68c38039c7a94ab94c7'),
	(726, 3755482, '2021-04-16 14:10:07.327007', 'User Access', 'Excel', 'System Scheduler', '04/16/2021 00:00:59', '04/16/2021 00:00:59', 'User Access - List of users showing their current authorized Roles by module, Entity hierarchy and Team assignments with the date and user name of who made the last changes to the user\'s status.', '04/16/2021 00:00:57', 'Complete', '90f87b9cb0acd68c38039c7a94ab94c7'),
	(727, 3806376, '2021-04-21 12:53:25.102694', 'Rwanda Suspense Unmatched Report', 'Excel', 'System Scheduler', '04/21/2021 02:43:02', '04/21/2021 02:43:02', 'The Detailed Unmatched Transaction Aging report will provide the Aging Categories of a Data Source\'s transactions, based on the age calculated using the selected Transaction Date of a Data Source.', '04/21/2021 02:43:00', 'Complete', '55b64cfac2a2ffa733e0cc0fcf65fd3e'),
	(728, 3796254, '2021-04-21 12:55:00.185910', 'Rwanda Suspense Unmatched Report', 'Excel', 'System Scheduler', '04/20/2021 07:31:40', '04/20/2021 07:31:40', 'The Detailed Unmatched Transaction Aging report will provide the Aging Categories of a Data Source\'s transactions, based on the age calculated using the selected Transaction Date of a Data Source.', '04/20/2021 07:31:38', 'Complete', '55b64cfac2a2ffa733e0cc0fcf65fd3e'),
	(729, 3806798, '2021-04-21 15:02:13.647485', 'Rwanda Suspense Unmatched Report', 'Excel', 'System Scheduler', '04/21/2021 03:16:40', '04/21/2021 03:16:40', 'The Detailed Unmatched Transaction Aging report will provide the Aging Categories of a Data Source\'s transactions, based on the age calculated using the selected Transaction Date of a Data Source.', '04/21/2021 03:16:38', 'Complete', 'b344d353363713de5ab9bc077173a271'),
	(730, 3811213, '2021-04-23 12:46:15.779222', 'KE MOBILE MONEY FLOAT OPEN ITEMS REPORT', 'Excel', 'System Scheduler', '04/21/2021 08:39:18', '04/21/2021 08:39:18', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/21/2021 08:39:16', 'Complete', '08ff8d36fd46a9290457911ea3ae3146'),
	(731, 3810291, '2021-04-23 12:46:24.541886', 'KE_MOBILE MONEY FLOAT BALANCE PROOFING REPORT', 'Excel', 'System Scheduler', '04/21/2021 07:24:45', '04/21/2021 07:24:45', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '04/21/2021 07:24:43', 'Complete', '08ff8d36fd46a9290457911ea3ae3146'),
	(732, 3912893, '2021-05-05 22:08:44.682720', 'Kenya Branch Suspense Balance Proofing', 'Excel', 'System Scheduler', '05/05/2021 09:13:51', '05/05/2021 09:13:51', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '05/05/2021 09:13:48', 'Complete', '8369e16a9c6f9e2b3709a7975d0bf297'),
	(733, 3917494, '2021-05-07 13:53:51.404802', 'Kenya Branch Suspense Balance Proofing', 'Excel', 'System Scheduler', '05/06/2021 01:24:17', '05/06/2021 01:24:17', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '05/06/2021 01:24:15', 'Complete', 'f14b7f0757dffc3892d3c1edcc40b257'),
	(734, 3917493, '2021-05-07 13:54:02.713086', 'Kenya Branch Suspense Balance Proofing', 'Excel', 'System Scheduler', '05/06/2021 01:24:09', '05/06/2021 01:24:09', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '05/06/2021 01:24:07', 'Complete', 'f14b7f0757dffc3892d3c1edcc40b257'),
	(735, 3917489, '2021-05-07 13:54:31.440923', 'Kenya Branch Suspense Open Items Daily Report', 'Excel', 'System Scheduler', '05/06/2021 01:24:00', '05/06/2021 01:24:00', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '05/06/2021 01:23:54', 'Complete', 'f14b7f0757dffc3892d3c1edcc40b257'),
	(736, 3959047, '2021-05-11 11:58:22.787961', 'Tanzania USD Clearing Suspense Settlement balance proofing report ', 'Excel', 'System Scheduler', '05/11/2021 01:58:19', '05/11/2021 01:58:19', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '05/11/2021 01:58:17', 'Complete', '2aa003c1c35020cfb45c268fc5bd0c78'),
	(737, 3959488, '2021-05-11 12:24:50.606681', 'Tanzania USD Clearing Suspense Settlement balance proofing report - Comment', 'Excel', 'System Scheduler', '05/11/2021 02:16:56', '05/11/2021 02:16:56', 'Summary showing the status of each reconciliation, Unidentified Difference, and category totals.', '05/11/2021 02:16:53', 'Complete', 'cd4fbb2817dc8bb5844a1b3233454c65'),
	(738, 4003510, '2021-05-14 12:10:52.828689', 'Rwanda_Suspense_Central Operations_Open Items Report', 'Excel', 'System Scheduler', '05/14/2021 02:11:48', '05/14/2021 02:11:48', 'All Supporting Items for the specified period end date for all Item Classes (Required Adjustment, List Component, Timing Item).', '05/14/2021 02:11:46', 'Complete', 'dfc00a38a10066062e7e858b15da21f4');
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

-- Dumping data for table fileetl.uploadedfiles: ~33 rows (approximately)
/*!40000 ALTER TABLE `uploadedfiles` DISABLE KEYS */;
REPLACE INTO `uploadedfiles` (`Id`, `Name`, `Md5`, `UploadedDate`, `Size`, `IsProduction`, `FilePath`, `MtAccountNo`, `MtStatementNo`, `MtSequenceNo`, `ProcessFor62F`, `Converted`, `ConvertedBy`, `Failed`) VALUES
	('08d916b1-747f-4ba7-8ac4-6c4ef93cf389', 'I & M Data April 2021 3rd Party.xlsx', '6dd9420029fa180bccb3feb57a846570', '2021-05-14 11:22:46.887466', 1805253, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imke\\Utilities\\OMNI_SETT\\I & M Data April 2021 3rd Party.xlsx', '', '', '', 0, 0, NULL, 0),
	('08d916b1-74a0-48f0-8d5e-fe0c01e2269b', 'TPS_VTU_MTN_07MAY2021.csv', 'e0284573c26fcc3ec2fecd102b9c3fdc', '2021-05-14 11:22:47.132923', 1241, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imrw\\MB\\MTN_VTU_AIRTIME\\PORTAL\\TPS_VTU_MTN_07MAY2021.csv', '', '', '', 0, 1, 'MTNRwandaBalanceExtractor', 0),
	('08d916b1-74a6-407a-8ad6-205f4a65850e', 'EndOfDayBalance_20210422.txt', '72ca827fac84d45f3212c823dcb80322', '2021-05-14 11:22:47.168597', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210422.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74ac-47ab-80ed-fe836a771b65', 'EndOfDayBalance_20210423.txt', 'c897442cbffbe88da5f5d85288c4cf97', '2021-05-14 11:22:47.211795', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210423.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74c8-48d9-8249-11adc500e754', 'EndOfDayBalance_20210424.txt', 'dae959725c82fd863372026b717d4e37', '2021-05-14 11:22:47.395776', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210424.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74cc-4bda-8e40-88566b9074d2', 'EndOfDayBalance_20210425.txt', 'b31f6653a344b081ac85fc9c85c51d3d', '2021-05-14 11:22:47.423157', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210425.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74d1-4039-827f-7f13f7f309b5', 'EndOfDayBalance_20210426.txt', '729918a300556c69d7af3f29e9a138fb', '2021-05-14 11:22:47.451238', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210426.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74d5-4a31-85cf-922fc8a9ed7a', 'EndOfDayBalance_20210427.txt', '207e808f23f1bb66d14a29537b2042a0', '2021-05-14 11:22:47.481546', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210427.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74da-42bc-8ebe-4203e3d70911', 'EndOfDayBalance_20210428.txt', '9f1537919abcad0a76e563a87a359113', '2021-05-14 11:22:47.511255', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210428.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74de-457d-8548-9b76e1f5cd32', 'EndOfDayBalance_20210429.txt', '2a77413c1ccc295e89552105f565b761', '2021-05-14 11:22:47.538598', 44, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210429.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74e2-43db-8a7d-77a58bcbf329', 'EndOfDayBalance_20210502.txt', 'b2a0b05b53340521a1d6b2a47574212a', '2021-05-14 11:22:47.564128', 44, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210502.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74eb-43e8-82f0-24ba8067beb0', 'EndOfDayBalance_20210503.txt', 'e1962da6fdd24a270f27f740b19619ce', '2021-05-14 11:22:47.622721', 44, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210503.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74f0-4027-8037-956a3fe3a4da', 'EndOfDayBalance_20210506.txt', 'fed0f82d00da77a2a073f59d4249f223', '2021-05-14 11:22:47.654351', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210506.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74f4-48ae-87e5-605e12b22837', 'EndOfDayBalance_20210507.txt', 'fabbb9205256b500f24da666ca3698f7', '2021-05-14 11:22:47.684035', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210507.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74f9-43d5-8461-81abfc8bf445', 'EndOfDayBalance_20210508.txt', 'ac88570454117f2b4ed2f7511e3d43c8', '2021-05-14 11:22:47.714863', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210508.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-74fe-4aa2-81e5-396969fee33a', 'EndOfDayBalance_20210509.txt', '05e50d99b3bb474a925e42d42150a101', '2021-05-14 11:22:47.750396', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210509.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-7503-4e1c-8866-521033b42afa', 'EndOfDayBalance_20210510.txt', '60f55a4cc80bc3505b44610d8c1b724e', '2021-05-14 11:22:47.784600', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210510.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-750b-4482-8586-50f9e779e703', 'EndOfDayBalance_20210511.txt', '92b82534970b120eb2188c17a72c2672', '2021-05-14 11:22:47.833111', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210511.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-7511-46ad-8460-8a933510ee90', 'EndOfDayBalance_20210512.txt', '92ac9438f97adb5e52a79d67843d5920', '2021-05-14 11:22:47.873244', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210512.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b1-7519-40a6-837e-e8227313db47', 'EndOfDayBalance_20210513.txt', '57d43d959e5258fdd9da54d7a1a13f2e', '2021-05-14 11:22:47.923212', 43, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\imtz\\MB\\SPENN\\CONTROL\\ENDOFDAYBALANCE\\EndOfDayBalance_20210513.txt', '', '', '', 0, 1, 'SpennControlExtractor', 0),
	('08d916b2-4be7-42fa-8b23-e312367f1f06', 'MultiCurr_2021_05_14_ance_20210422_SPEN_CTRL_TZ.txt', '458db080eed8e3242d3212a0a7090e2b', '2021-05-14 11:28:48.293615', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210422_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4bed-4413-857c-1b8bfb89a7dc', 'MultiCurr_2021_05_14_ance_20210423_SPEN_CTRL_TZ.txt', '71627843516d8c29e4123c791356023a', '2021-05-14 11:28:48.345864', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210423_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4bf1-446c-8867-c7f519518091', 'MultiCurr_2021_05_14_ance_20210427_SPEN_CTRL_TZ.txt', '8005dd960196d3d2de97ff7c5014f0e5', '2021-05-14 11:28:48.372784', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210427_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4bf4-4241-86ce-840168dcb066', 'MultiCurr_2021_05_14_ance_20210428_SPEN_CTRL_TZ.txt', 'e1176baa48f24e96bd37195cfbb5e0c0', '2021-05-14 11:28:48.391636', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210428_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4bf6-4db9-880c-49c6d38a2e73', 'MultiCurr_2021_05_14_ance_20210429_SPEN_CTRL_TZ.txt', 'a4dbf8778beff17f5ae9483b31e744c9', '2021-05-14 11:28:48.409437', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210429_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4bf9-4766-8e90-7114d9f1f51f', 'MultiCurr_2021_05_14_ance_20210502_SPEN_CTRL_TZ.txt', 'b182db3adec674e7d9cc73431ff2c7e3', '2021-05-14 11:28:48.426492', 89, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210502_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4bfc-4843-87df-9bf36ed07a64', 'MultiCurr_2021_05_14_ance_20210506_SPEN_CTRL_TZ.txt', '715625013cee70ea0578bdb9137a6706', '2021-05-14 11:28:48.446518', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210506_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4bff-47d9-86d4-52e9345755a6', 'MultiCurr_2021_05_14_ance_20210507_SPEN_CTRL_TZ.txt', '86fd78010f09096f2449b8b9b2a1b1d3', '2021-05-14 11:28:48.466009', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210507_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4c03-498e-8b82-79e4899e00d7', 'MultiCurr_2021_05_14_ance_20210510_SPEN_CTRL_TZ.txt', '45cf6761fff5024ecfdb86b5e9247256', '2021-05-14 11:28:48.492781', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210510_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4c07-4304-8721-d4c81d7e8eeb', 'MultiCurr_2021_05_14_ance_20210511_SPEN_CTRL_TZ.txt', '4e5c4dd427f155cb71cbcd2495a283af', '2021-05-14 11:28:48.516293', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210511_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4c0a-4961-8fcc-0b4317473d86', 'MultiCurr_2021_05_14_ance_20210512_SPEN_CTRL_TZ.txt', '011e959d21e84ea6505adcc11945f0a1', '2021-05-14 11:28:48.538681', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210512_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4c0d-476f-8579-d73daf760272', 'MultiCurr_2021_05_14_ance_20210513_SPEN_CTRL_TZ.txt', 'b5a80a417c77dd6931c7fdfbd143edf5', '2021-05-14 11:28:48.557573', 88, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_ance_20210513_SPEN_CTRL_TZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b2-4c10-48c9-8b62-fcce00f40a7f', 'MultiCurr_2021_05_14_MTN_07MAY2021_MTN_IMTZ.txt', 'd103f116c4ead34f80c2de09e383f12a', '2021-05-14 11:28:48.577792', 82, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\MultiCurr_2021_05_14_MTN_07MAY2021_MTN_IMTZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916b5-a62f-4a0d-8499-71bd5b073f1b', 'BR_SUS_BALANCE_14May2021.CSV', '98fd43ca20c95b0087f6771c9241558c', '2021-05-14 11:52:48.266402', 4394, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\BR_SUS_BALANCE_14May2021.CSV', '', '', '', 0, 0, NULL, 0),
	('08d916b5-a632-4be6-8605-844f6f474b6e', 'FCO_SUS_BALANCE_14May2021.CSV', 'e21d94d497186c92b075205e3ee8238f', '2021-05-14 11:52:48.286971', 5693, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\FCO_SUS_BALANCE_14May2021.CSV', '', '', '', 0, 0, NULL, 0),
	('08d916b7-95fc-4c6a-875b-16461b4fe3a2', 'GLAccounts_20210513_SUS_IMTZ.txt', '45a818af55fbf460e77bbabfbaeec8f7', '2021-05-14 12:06:40.055519', 9424, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\GLAccounts_20210513_SUS_IMTZ.txt', '', '', '', 0, 0, NULL, 0),
	('08d916ba-01c1-4ba9-808d-b4f1e8faafd4', 'GLAccounts_20210513_174_SUS_IMTZ.txt', '0cfd9d658b1fc78b13a0221a943fd9e5', '2021-05-14 12:23:59.861169', 11714, 1, 'C:\\Users\\Yida\\Downloads\\jobrunner\\940\\GLAccounts_20210513_174_SUS_IMTZ.txt', '', '', '', 0, 0, NULL, 0);
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
