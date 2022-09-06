
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/14/2021 13:57:49
-- Generated from EDMX file: G:\najez_latest_11_11_2021\najez_latest\Almanea\AlmaneaDb.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [NajezDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Category_Category]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Category] DROP CONSTRAINT [FK_Category_Category];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblAdditionalServices_dbo_tblOrders_OrderId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderAdditionalServices] DROP CONSTRAINT [FK_dbo_tblAdditionalServices_dbo_tblOrders_OrderId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblAdminUsers_dbo_tblUserGroupCompanies_UserGroupId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblAdminUsers] DROP CONSTRAINT [FK_dbo_tblAdminUsers_dbo_tblUserGroupCompanies_UserGroupId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblLocations_dbo_tblAdminUsers_UserId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblLocations] DROP CONSTRAINT [FK_dbo_tblLocations_dbo_tblAdminUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrderAddtionalWork_dbo_tblAdditionalWork_AdditionalWorkId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderAdditionalWork] DROP CONSTRAINT [FK_dbo_tblOrderAddtionalWork_dbo_tblAdditionalWork_AdditionalWorkId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrderAddtionalWork_dbo_tblAdminUsers_UserId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderAdditionalWork] DROP CONSTRAINT [FK_dbo_tblOrderAddtionalWork_dbo_tblAdminUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrderAddtionalWork_dbo_tblOrders_OrderId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderAdditionalWork] DROP CONSTRAINT [FK_dbo_tblOrderAddtionalWork_dbo_tblOrders_OrderId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrderHistories_dbo_tblAdminUsers_UserId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderHistories] DROP CONSTRAINT [FK_dbo_tblOrderHistories_dbo_tblAdminUsers_UserId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrderHistories_dbo_tblOrders_OrderId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderHistories] DROP CONSTRAINT [FK_dbo_tblOrderHistories_dbo_tblOrders_OrderId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrders_dbo_tblLocations_LocationId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrders] DROP CONSTRAINT [FK_dbo_tblOrders_dbo_tblLocations_LocationId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrderServices_dbo_tblOrders_OrderId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderServices] DROP CONSTRAINT [FK_dbo_tblOrderServices_dbo_tblOrders_OrderId];
GO
IF OBJECT_ID(N'[dbo].[FK_dbo_tblOrderServices_dbo_tblServices_ServiceId]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderServices] DROP CONSTRAINT [FK_dbo_tblOrderServices_dbo_tblServices_ServiceId];
GO
IF OBJECT_ID(N'[dbo].[FK_Delivery_tblAdminUsers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblDeliveries] DROP CONSTRAINT [FK_Delivery_tblAdminUsers];
GO
IF OBJECT_ID(N'[dbo].[FK_Delivery_tblOrders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblDeliveries] DROP CONSTRAINT [FK_Delivery_tblOrders];
GO
IF OBJECT_ID(N'[HangFire].[FK_HangFire_JobParameter_Job]', 'F') IS NOT NULL
    ALTER TABLE [HangFire].[JobParameter] DROP CONSTRAINT [FK_HangFire_JobParameter_Job];
GO
IF OBJECT_ID(N'[HangFire].[FK_HangFire_State_Job]', 'F') IS NOT NULL
    ALTER TABLE [HangFire].[State] DROP CONSTRAINT [FK_HangFire_State_Job];
GO
IF OBJECT_ID(N'[dbo].[FK_OrderDisplay_tblOrders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrderDisplay] DROP CONSTRAINT [FK_OrderDisplay_tblOrders];
GO
IF OBJECT_ID(N'[dbo].[FK_tblAdditionalWork_Category]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblAdditionalWork] DROP CONSTRAINT [FK_tblAdditionalWork_Category];
GO
IF OBJECT_ID(N'[dbo].[FK_tblComplainHistory_tblAdminUsers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblComplainHistory] DROP CONSTRAINT [FK_tblComplainHistory_tblAdminUsers];
GO
IF OBJECT_ID(N'[dbo].[FK_tblDeliveryServices_tblDeliveries]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblDeliveryServices] DROP CONSTRAINT [FK_tblDeliveryServices_tblDeliveries];
GO
IF OBJECT_ID(N'[dbo].[FK_tblDeliveryServices_tblServices]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblDeliveryServices] DROP CONSTRAINT [FK_tblDeliveryServices_tblServices];
GO
IF OBJECT_ID(N'[dbo].[FK_tblLaborInactive_tblAdminUsers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblLaborInactive] DROP CONSTRAINT [FK_tblLaborInactive_tblAdminUsers];
GO
IF OBJECT_ID(N'[dbo].[FK_tblLaborInactive_tblLaborInactive]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblLaborInactive] DROP CONSTRAINT [FK_tblLaborInactive_tblLaborInactive];
GO
IF OBJECT_ID(N'[dbo].[FK_tblMultipleComplains_tblComplainType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblMultipleComplains] DROP CONSTRAINT [FK_tblMultipleComplains_tblComplainType];
GO
IF OBJECT_ID(N'[dbo].[FK_tblMultipleComplains_tblMultipleComplains]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblMultipleComplains] DROP CONSTRAINT [FK_tblMultipleComplains_tblMultipleComplains];
GO
IF OBJECT_ID(N'[dbo].[FK_tblMultipleComplains_tblOrderComplains]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblMultipleComplains] DROP CONSTRAINT [FK_tblMultipleComplains_tblOrderComplains];
GO
IF OBJECT_ID(N'[dbo].[FK_tblOrderAdditionalServices_tblAdditionalWork]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderAdditionalServices] DROP CONSTRAINT [FK_tblOrderAdditionalServices_tblAdditionalWork];
GO
IF OBJECT_ID(N'[dbo].[FK_tblOrderComplains_tblOrders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderComplains] DROP CONSTRAINT [FK_tblOrderComplains_tblOrders];
GO
IF OBJECT_ID(N'[dbo].[FK_tblOrderReleases_tblOrders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblOrderReleases] DROP CONSTRAINT [FK_tblOrderReleases_tblOrders];
GO
IF OBJECT_ID(N'[dbo].[FK_tblProviderTimeSlot_tblAdminUsers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblProviderTimeSlot] DROP CONSTRAINT [FK_tblProviderTimeSlot_tblAdminUsers];
GO
IF OBJECT_ID(N'[dbo].[FK_tblProviderTimeSlot_tblUserGroupCompanies]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblProviderTimeSlot] DROP CONSTRAINT [FK_tblProviderTimeSlot_tblUserGroupCompanies];
GO
IF OBJECT_ID(N'[dbo].[FK_tblProviderWorkinHour_tblUserGroupCompanies]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblProviderWorkinHour] DROP CONSTRAINT [FK_tblProviderWorkinHour_tblUserGroupCompanies];
GO
IF OBJECT_ID(N'[dbo].[FK_tblServiceMapper_tblServices]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblServiceMapper] DROP CONSTRAINT [FK_tblServiceMapper_tblServices];
GO
IF OBJECT_ID(N'[dbo].[FK_tblTeamCapacity]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblTeamCapacity] DROP CONSTRAINT [FK_tblTeamCapacity];
GO
IF OBJECT_ID(N'[dbo].[FK_tblTeamCapacityCalculation_tblOrders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblTeamCapacityCalculation] DROP CONSTRAINT [FK_tblTeamCapacityCalculation_tblOrders];
GO
IF OBJECT_ID(N'[dbo].[FK_tblTeamCapacityCalculation_tblUserGroupCompanies]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tblTeamCapacityCalculation] DROP CONSTRAINT [FK_tblTeamCapacityCalculation_tblUserGroupCompanies];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[__MigrationHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[__MigrationHistory];
GO
IF OBJECT_ID(N'[dbo].[Category]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Category];
GO
IF OBJECT_ID(N'[dbo].[OrderDisplay]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OrderDisplay];
GO
IF OBJECT_ID(N'[dbo].[tblAccountType]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblAccountType];
GO
IF OBJECT_ID(N'[dbo].[tblAdditionalWork]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblAdditionalWork];
GO
IF OBJECT_ID(N'[dbo].[tblAdminUsers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblAdminUsers];
GO
IF OBJECT_ID(N'[dbo].[tblComplainHistory]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblComplainHistory];
GO
IF OBJECT_ID(N'[dbo].[tblComplainType]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblComplainType];
GO
IF OBJECT_ID(N'[dbo].[tblDeliveries]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblDeliveries];
GO
IF OBJECT_ID(N'[dbo].[tblDeliveryServices]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblDeliveryServices];
GO
IF OBJECT_ID(N'[dbo].[tblDirection]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblDirection];
GO
IF OBJECT_ID(N'[dbo].[tblEmails]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblEmails];
GO
IF OBJECT_ID(N'[dbo].[tblLaborInactive]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLaborInactive];
GO
IF OBJECT_ID(N'[dbo].[tblLocations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblLocations];
GO
IF OBJECT_ID(N'[dbo].[tblMultipleComplains]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblMultipleComplains];
GO
IF OBJECT_ID(N'[dbo].[tblOrderAdditionalServices]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrderAdditionalServices];
GO
IF OBJECT_ID(N'[dbo].[tblOrderAdditionalWork]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrderAdditionalWork];
GO
IF OBJECT_ID(N'[dbo].[tblOrderComplains]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrderComplains];
GO
IF OBJECT_ID(N'[dbo].[tblOrderHistories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrderHistories];
GO
IF OBJECT_ID(N'[dbo].[tblOrderReleases]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrderReleases];
GO
IF OBJECT_ID(N'[dbo].[tblOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrders];
GO
IF OBJECT_ID(N'[dbo].[tblOrderServices]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrderServices];
GO
IF OBJECT_ID(N'[dbo].[tblOrderUserLink]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblOrderUserLink];
GO
IF OBJECT_ID(N'[dbo].[tblProviderSettingMapper]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblProviderSettingMapper];
GO
IF OBJECT_ID(N'[dbo].[tblProviderTimeSlot]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblProviderTimeSlot];
GO
IF OBJECT_ID(N'[dbo].[tblProviderWorkinHour]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblProviderWorkinHour];
GO
IF OBJECT_ID(N'[dbo].[tblPushNotification]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblPushNotification];
GO
IF OBJECT_ID(N'[dbo].[tblResetOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblResetOrders];
GO
IF OBJECT_ID(N'[dbo].[tblServiceMapper]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblServiceMapper];
GO
IF OBJECT_ID(N'[dbo].[tblServices]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblServices];
GO
IF OBJECT_ID(N'[dbo].[tblSettings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblSettings];
GO
IF OBJECT_ID(N'[dbo].[tblSMS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblSMS];
GO
IF OBJECT_ID(N'[dbo].[tblTeamCapacity]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblTeamCapacity];
GO
IF OBJECT_ID(N'[dbo].[tblTeamCapacityCalculation]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblTeamCapacityCalculation];
GO
IF OBJECT_ID(N'[dbo].[tblUnit]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblUnit];
GO
IF OBJECT_ID(N'[dbo].[tblUserGroupCompanies]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tblUserGroupCompanies];
GO
IF OBJECT_ID(N'[HangFire].[AggregatedCounter]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[AggregatedCounter];
GO
IF OBJECT_ID(N'[HangFire].[Hash]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[Hash];
GO
IF OBJECT_ID(N'[HangFire].[Job]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[Job];
GO
IF OBJECT_ID(N'[HangFire].[JobParameter]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[JobParameter];
GO
IF OBJECT_ID(N'[HangFire].[JobQueue]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[JobQueue];
GO
IF OBJECT_ID(N'[HangFire].[List]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[List];
GO
IF OBJECT_ID(N'[HangFire].[Schema]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[Schema];
GO
IF OBJECT_ID(N'[HangFire].[Server]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[Server];
GO
IF OBJECT_ID(N'[HangFire].[Set]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[Set];
GO
IF OBJECT_ID(N'[HangFire].[State]', 'U') IS NOT NULL
    DROP TABLE [HangFire].[State];
GO
IF OBJECT_ID(N'[AlmaneaDbModelStoreContainer].[Counter]', 'U') IS NOT NULL
    DROP TABLE [AlmaneaDbModelStoreContainer].[Counter];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'C__MigrationHistory'
CREATE TABLE [dbo].[C__MigrationHistory] (
    [MigrationId] nvarchar(150)  NOT NULL,
    [ContextKey] nvarchar(300)  NOT NULL,
    [Model] varbinary(max)  NOT NULL,
    [ProductVersion] nvarchar(32)  NOT NULL
);
GO

-- Creating table 'tblAccountTypes'
CREATE TABLE [dbo].[tblAccountTypes] (
    [AccountTypeId] int IDENTITY(1,1) NOT NULL,
    [GroupTypeId] int  NULL,
    [TitleEN] nvarchar(50)  NULL,
    [TitleAR] nvarchar(50)  NULL,
    [IsActive] bit  NULL
);
GO

-- Creating table 'tblAdditionalWorks'
CREATE TABLE [dbo].[tblAdditionalWorks] (
    [AdditionalWorkId] int IDENTITY(1,1) NOT NULL,
    [AdditionalWorkNameEN] nvarchar(500)  NULL,
    [AdditionalWorkNameAR] nvarchar(500)  NULL,
    [Price] decimal(18,2)  NOT NULL,
    [UserGroupId] int  NULL,
    [CategoryrId] smallint  NULL
);
GO

-- Creating table 'tblAdminUsers'
CREATE TABLE [dbo].[tblAdminUsers] (
    [UserId] int IDENTITY(1,1) NOT NULL,
    [FirstName] nvarchar(500)  NULL,
    [LastName] nvarchar(500)  NULL,
    [Email] nvarchar(500)  NULL,
    [MobileNo] nvarchar(50)  NULL,
    [Status] bit  NOT NULL,
    [UserGroupId] int  NULL,
    [UserGroupTypeId] tinyint  NULL,
    [IqaamaNo] nvarchar(50)  NULL,
    [AccountTypeId] int  NOT NULL,
    [ProfilePic] nvarchar(50)  NULL,
    [AddedDate] datetime  NOT NULL,
    [AddedBy] int  NOT NULL,
    [DeviceToken] varchar(6000)  NULL,
    [LabourIsDriver] bit  NOT NULL,
    [DeviceType] varchar(10)  NULL,
    [LastLoggedIn] datetime  NULL,
    [IsLogin] bit  NOT NULL,
    [EncryptedPassword] nvarchar(max)  NULL
);
GO

-- Creating table 'tblComplainHistories'
CREATE TABLE [dbo].[tblComplainHistories] (
    [HistoryId] int IDENTITY(1,1) NOT NULL,
    [ComplainId] int  NULL,
    [Comments] nvarchar(max)  NULL,
    [StatusId] tinyint  NULL,
    [UpdateOn] datetime  NOT NULL,
    [UpdatedBy] int  NULL
);
GO

-- Creating table 'tblComplainTypes'
CREATE TABLE [dbo].[tblComplainTypes] (
    [ComplainTypeId] int IDENTITY(1,1) NOT NULL,
    [UserGroupTypeId] tinyint  NULL,
    [TitleEN] nvarchar(250)  NULL,
    [TitleAR] nvarchar(250)  NULL,
    [IsActive] bit  NULL,
    [ComplainCategoryId] int  NOT NULL
);
GO

-- Creating table 'tblDeliveries'
CREATE TABLE [dbo].[tblDeliveries] (
    [DeliveryId] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NOT NULL,
    [DriverId] int  NOT NULL,
    [PickupDate] datetime  NOT NULL
);
GO

-- Creating table 'tblDeliveryServices'
CREATE TABLE [dbo].[tblDeliveryServices] (
    [DevlieryServiceId] int IDENTITY(1,1) NOT NULL,
    [DeliveryId] int  NOT NULL,
    [ServiceId] int  NOT NULL,
    [Quantity] int  NOT NULL
);
GO

-- Creating table 'tblDirections'
CREATE TABLE [dbo].[tblDirections] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [DirectionName] nvarchar(250)  NOT NULL,
    [DirectionNameAr] nvarchar(250)  NOT NULL
);
GO

-- Creating table 'tblEmails'
CREATE TABLE [dbo].[tblEmails] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [KeyName] nvarchar(max)  NULL,
    [EmailTextEN] nvarchar(max)  NULL,
    [EmailTextAR] nvarchar(max)  NULL,
    [SubjectEN] nvarchar(500)  NULL,
    [SubjectAR] nvarchar(500)  NULL
);
GO

-- Creating table 'tblLocations'
CREATE TABLE [dbo].[tblLocations] (
    [LocationId] int IDENTITY(1,1) NOT NULL,
    [LocationNameEN] nvarchar(500)  NULL,
    [LocationNameAR] nvarchar(500)  NULL,
    [Status] bit  NOT NULL,
    [AddedDate] datetime  NOT NULL,
    [UserId] int  NOT NULL,
    [Direction] int  NULL,
    [UserGroupId] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'tblMultipleComplains'
CREATE TABLE [dbo].[tblMultipleComplains] (
    [IIdentity] bigint IDENTITY(1,1) NOT NULL,
    [Complainid] int  NOT NULL,
    [ComplainTypeId] int  NOT NULL
);
GO

-- Creating table 'tblOrderAdditionalServices'
CREATE TABLE [dbo].[tblOrderAdditionalServices] (
    [AdditionalServiceId] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NOT NULL,
    [UserId] int  NOT NULL,
    [Quantity] int  NOT NULL,
    [Price] decimal(18,2)  NOT NULL,
    [SpareParts] decimal(18,2)  NOT NULL,
    [AddedOn] datetime  NOT NULL,
    [Status] bit  NOT NULL,
    [AdditionalWorkId] int  NOT NULL
);
GO

-- Creating table 'tblOrderAdditionalWorks'
CREATE TABLE [dbo].[tblOrderAdditionalWorks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NOT NULL,
    [AdditionalWorkId] int  NOT NULL,
    [LabourId] int  NOT NULL,
    [Quantity] int  NOT NULL,
    [Price] int  NOT NULL,
    [Total] int  NOT NULL
);
GO

-- Creating table 'tblOrderComplains'
CREATE TABLE [dbo].[tblOrderComplains] (
    [ComplainId] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NULL,
    [ComplainTypeId] int  NULL,
    [Subject] nvarchar(500)  NULL,
    [Comments] nvarchar(max)  NULL,
    [StatusId] tinyint  NOT NULL,
    [AddedOn] datetime  NOT NULL,
    [Response] nvarchar(max)  NULL,
    [UpdateOn] datetime  NULL,
    [ComplainBy] tinyint  NULL,
    [AddedBy] int  NULL
);
GO

-- Creating table 'tblOrderHistories'
CREATE TABLE [dbo].[tblOrderHistories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NOT NULL,
    [ActivityDate] datetime  NOT NULL,
    [Status] tinyint  NOT NULL,
    [UserId] int  NOT NULL,
    [Comments] nvarchar(2500)  NULL,
    [FileAttachment] nvarchar(max)  NULL,
    [ServiceProviderId] int  NULL,
    [CommentsReschedule] nvarchar(max)  NULL,
    [LabourId] int  NOT NULL,
    [DriverId] int  NULL
);
GO

-- Creating table 'tblOrderReleases'
CREATE TABLE [dbo].[tblOrderReleases] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NOT NULL,
    [ActivityDate] datetime  NOT NULL,
    [Status] tinyint  NOT NULL,
    [UserId] int  NOT NULL,
    [Comments] nvarchar(2500)  NULL,
    [FileAttachment] nvarchar(max)  NULL
);
GO

-- Creating table 'tblOrders'
CREATE TABLE [dbo].[tblOrders] (
    [OrderId] int IDENTITY(1,1) NOT NULL,
    [SellerName] nvarchar(500)  NULL,
    [SellerContact] nvarchar(50)  NULL,
    [InvoiceNo] nvarchar(150)  NULL,
    [CustomerName] nvarchar(500)  NULL,
    [CustomerContact] nvarchar(50)  NULL,
    [AlternateMobile] nvarchar(50)  NULL,
    [LocationId] int  NOT NULL,
    [InstallDate] datetime  NULL,
    [PrefferTime] tinyint  NOT NULL,
    [Status] tinyint  NOT NULL,
    [AddedDate] datetime  NOT NULL,
    [SupplierId] int  NOT NULL,
    [AddedBy] int  NOT NULL,
    [ReservedProvider] int  NOT NULL,
    [ReservedBy] int  NULL,
    [PreferDate] tinyint  NOT NULL,
    [PrefferHr] int  NULL,
    [PrefferMeridian] tinyint  NULL,
    [CustomerCode] nvarchar(50)  NOT NULL,
    [IsOnEdit] bit  NOT NULL,
    [CustomerSignOff] nvarchar(max)  NULL,
    [SmsInArabic] bit  NULL,
    [ServiceTotal] decimal(18,2)  NULL,
    [ServiceVat] decimal(18,2)  NULL,
    [AdditionalTotal] decimal(18,2)  NULL,
    [AdditionalVat] decimal(18,2)  NULL,
    [TotalAmount] decimal(18,2)  NOT NULL,
    [ReportAdmin] bit  NULL,
    [Comments] nvarchar(max)  NULL,
    [AssignedTo] char(10)  NULL,
    [LabourId] int  NOT NULL,
    [DriverId] int  NULL,
    [InvoiceImages] varchar(6000)  NULL,
    [DeliveryStatus] varchar(20)  NULL,
    [CustomerLngLat] varchar(100)  NULL,
    [WhyPartial] varchar(50)  NULL
);
GO

-- Creating table 'tblOrderServices'
CREATE TABLE [dbo].[tblOrderServices] (
    [OrderServiceId] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NOT NULL,
    [ServiceId] int  NOT NULL,
    [Quantity] int  NOT NULL,
    [IsActive] bit  NOT NULL,
    [IsAdditional] int  NULL,
    [Unit] int  NOT NULL
);
GO

-- Creating table 'tblOrderUserLinks'
CREATE TABLE [dbo].[tblOrderUserLinks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NULL,
    [ExpireOn] datetime  NULL,
    [AddedOn] datetime  NULL,
    [IsActive] bit  NULL
);
GO

-- Creating table 'tblResetOrders'
CREATE TABLE [dbo].[tblResetOrders] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NOT NULL,
    [InstallDate] datetime  NULL,
    [PrefferTime] tinyint  NULL,
    [PreferDate] tinyint  NULL,
    [PrefferHr] int  NULL,
    [PrefferMeridian] tinyint  NULL
);
GO

-- Creating table 'tblServiceMappers'
CREATE TABLE [dbo].[tblServiceMappers] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [ServiceId] int  NULL,
    [ServiceProviderId] int  NULL,
    [SupplierId] int  NULL,
    [Estimated] nvarchar(50)  NULL,
    [WorkingHours] int  NULL,
    [IsWorking] bit  NOT NULL,
    [ServiceAcceptStatus] bit  NOT NULL
);
GO

-- Creating table 'tblServices'
CREATE TABLE [dbo].[tblServices] (
    [ServiceId] int IDENTITY(1,1) NOT NULL,
    [ServiceNameEN] nvarchar(500)  NULL,
    [ServiceNameAR] nvarchar(500)  NULL,
    [UnitPrice] decimal(18,2)  NOT NULL,
    [Status] bit  NOT NULL,
    [AddedDate] datetime  NOT NULL,
    [UserId] int  NOT NULL,
    [ServiceProviderId] nvarchar(max)  NULL,
    [SupplierId] nvarchar(max)  NULL,
    [IsDisplay] bit  NOT NULL,
    [CategoryId] smallint  NULL,
    [Estimatetime] nvarchar(50)  NULL
);
GO

-- Creating table 'tblSettings'
CREATE TABLE [dbo].[tblSettings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [KeyName] nvarchar(max)  NULL,
    [KeyValue] nvarchar(max)  NULL,
    [Version] float  NULL,
    [ProviderId] int  NOT NULL
);
GO

-- Creating table 'tblSMS'
CREATE TABLE [dbo].[tblSMS] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [KeyName] nvarchar(max)  NULL,
    [SMSTextEN] nvarchar(max)  NULL,
    [SMSTextAR] nvarchar(max)  NULL
);
GO

-- Creating table 'tblUnits'
CREATE TABLE [dbo].[tblUnits] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [Unit] int  NOT NULL
);
GO

-- Creating table 'tblUserGroupCompanies'
CREATE TABLE [dbo].[tblUserGroupCompanies] (
    [UserGroupId] int IDENTITY(1,1) NOT NULL,
    [CompanyNameEN] nvarchar(500)  NOT NULL,
    [CompanyNameAR] nvarchar(500)  NOT NULL,
    [UserGroupTypeId] tinyint  NOT NULL,
    [Telephone] nvarchar(50)  NULL,
    [Fax] nvarchar(50)  NULL,
    [Email] nvarchar(500)  NULL,
    [CompanyLogo] nvarchar(50)  NULL,
    [Contract] decimal(18,2)  NULL,
    [Status] bit  NOT NULL,
    [AddedDate] datetime  NOT NULL,
    [AddedBy] int  NOT NULL,
    [IsInternal] bit  NOT NULL
);
GO

-- Creating table 'AggregatedCounters'
CREATE TABLE [dbo].[AggregatedCounters] (
    [Key] nvarchar(100)  NOT NULL,
    [Value] bigint  NOT NULL,
    [ExpireAt] datetime  NULL
);
GO

-- Creating table 'Hashes'
CREATE TABLE [dbo].[Hashes] (
    [Key] nvarchar(100)  NOT NULL,
    [Field] nvarchar(100)  NOT NULL,
    [Value] nvarchar(max)  NULL,
    [ExpireAt] datetime  NULL
);
GO

-- Creating table 'Jobs'
CREATE TABLE [dbo].[Jobs] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [StateId] bigint  NULL,
    [StateName] nvarchar(20)  NULL,
    [InvocationData] nvarchar(max)  NOT NULL,
    [Arguments] nvarchar(max)  NOT NULL,
    [CreatedAt] datetime  NOT NULL,
    [ExpireAt] datetime  NULL
);
GO

-- Creating table 'JobParameters'
CREATE TABLE [dbo].[JobParameters] (
    [JobId] bigint  NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [Value] nvarchar(max)  NULL
);
GO

-- Creating table 'JobQueues'
CREATE TABLE [dbo].[JobQueues] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [JobId] bigint  NOT NULL,
    [Queue] nvarchar(50)  NOT NULL,
    [FetchedAt] datetime  NULL
);
GO

-- Creating table 'Lists'
CREATE TABLE [dbo].[Lists] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [Key] nvarchar(100)  NOT NULL,
    [Value] nvarchar(max)  NULL,
    [ExpireAt] datetime  NULL
);
GO

-- Creating table 'Schemata'
CREATE TABLE [dbo].[Schemata] (
    [Version] int  NOT NULL
);
GO

-- Creating table 'Servers'
CREATE TABLE [dbo].[Servers] (
    [Id] nvarchar(100)  NOT NULL,
    [Data] nvarchar(max)  NULL,
    [LastHeartbeat] datetime  NOT NULL
);
GO

-- Creating table 'Sets'
CREATE TABLE [dbo].[Sets] (
    [Key] nvarchar(100)  NOT NULL,
    [Score] float  NOT NULL,
    [Value] nvarchar(256)  NOT NULL,
    [ExpireAt] datetime  NULL
);
GO

-- Creating table 'States'
CREATE TABLE [dbo].[States] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [JobId] bigint  NOT NULL,
    [Name] nvarchar(20)  NOT NULL,
    [Reason] nvarchar(100)  NULL,
    [CreatedAt] datetime  NOT NULL,
    [Data] nvarchar(max)  NULL
);
GO

-- Creating table 'Counters'
CREATE TABLE [dbo].[Counters] (
    [Key] nvarchar(100)  NOT NULL,
    [Value] int  NOT NULL,
    [ExpireAt] datetime  NULL
);
GO

-- Creating table 'tblPushNotifications'
CREATE TABLE [dbo].[tblPushNotifications] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [KeyName] nvarchar(max)  NULL,
    [PushNotificationTextEN] nvarchar(max)  NULL,
    [PushNotificationTextAR] nvarchar(max)  NULL
);
GO

-- Creating table 'Categories'
CREATE TABLE [dbo].[Categories] (
    [Id] smallint IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(150)  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [NameEn] nvarchar(150)  NOT NULL,
    [DescriptionEn] nvarchar(max)  NULL,
    [ParentId] smallint  NULL,
    [HasChild] bit  NOT NULL,
    [Thumb] nvarchar(150)  NULL,
    [Picture] nvarchar(150)  NULL,
    [ThumbEn] nvarchar(150)  NULL,
    [PictureEn] nvarchar(150)  NULL,
    [Active] bit  NOT NULL,
    [SortOrder] smallint  NOT NULL,
    [UserGroupId] int  NULL
);
GO

-- Creating table 'OrderDisplays'
CREATE TABLE [dbo].[OrderDisplays] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [OrderId] int  NULL,
    [ProviderId] int  NULL,
    [AjentId] int  NULL,
    [IsMessasgeSent] bit  NOT NULL,
    [IsOrderReserved] bit  NOT NULL,
    [Datetime] datetime  NULL,
    [ReservedBy] int  NULL
);
GO

-- Creating table 'tblProviderSettingMappers'
CREATE TABLE [dbo].[tblProviderSettingMappers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ServiceProviderId] int  NULL,
    [SupplierId] int  NULL,
    [IsInternal] bit  NULL
);
GO

-- Creating table 'tblTeamCapacities'
CREATE TABLE [dbo].[tblTeamCapacities] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ServiceProviderId] int  NULL,
    [DailyCapacity] int  NULL,
    [ConsumedCapacity] int  NULL,
    [Updatedate] datetime  NULL,
    [CurrentCapacity] int  NULL,
    [Supplier] int  NULL
);
GO

-- Creating table 'tblProviderWorkinHours'
CREATE TABLE [dbo].[tblProviderWorkinHours] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ServiceProviderId] int  NULL,
    [WorkingHours] int  NULL
);
GO

-- Creating table 'tblTeamCapacityCalculations'
CREATE TABLE [dbo].[tblTeamCapacityCalculations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NULL,
    [InstallDate] datetime  NULL,
    [ServiceProviderId] int  NULL,
    [DailyCapacity] int  NULL,
    [ConsumedCapacity] int  NULL,
    [CurrentCapacity] int  NULL,
    [CapcityPercentage] decimal(18,0)  NULL,
    [Updatedate] datetime  NULL
);
GO

-- Creating table 'tblLaborInactives'
CREATE TABLE [dbo].[tblLaborInactives] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ProviderId] int  NOT NULL,
    [LabourId] int  NULL,
    [InactiveDates] nvarchar(max)  NULL,
    [UpdateDate] datetime  NULL
);
GO

-- Creating table 'tblProviderTimeSlots'
CREATE TABLE [dbo].[tblProviderTimeSlots] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderId] int  NULL,
    [InstallDate] datetime  NULL,
    [ServiceProviderId] int  NULL,
    [Prefer] int  NULL,
    [BookSlot] int  NULL,
    [LabourId] int  NULL,
    [Updatedate] datetime  NULL,
    [StartHour] int  NULL,
    [EndHour] int  NULL,
    [TotalConsumedHour] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [MigrationId], [ContextKey] in table 'C__MigrationHistory'
ALTER TABLE [dbo].[C__MigrationHistory]
ADD CONSTRAINT [PK_C__MigrationHistory]
    PRIMARY KEY CLUSTERED ([MigrationId], [ContextKey] ASC);
GO

-- Creating primary key on [AccountTypeId] in table 'tblAccountTypes'
ALTER TABLE [dbo].[tblAccountTypes]
ADD CONSTRAINT [PK_tblAccountTypes]
    PRIMARY KEY CLUSTERED ([AccountTypeId] ASC);
GO

-- Creating primary key on [AdditionalWorkId] in table 'tblAdditionalWorks'
ALTER TABLE [dbo].[tblAdditionalWorks]
ADD CONSTRAINT [PK_tblAdditionalWorks]
    PRIMARY KEY CLUSTERED ([AdditionalWorkId] ASC);
GO

-- Creating primary key on [UserId] in table 'tblAdminUsers'
ALTER TABLE [dbo].[tblAdminUsers]
ADD CONSTRAINT [PK_tblAdminUsers]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [HistoryId] in table 'tblComplainHistories'
ALTER TABLE [dbo].[tblComplainHistories]
ADD CONSTRAINT [PK_tblComplainHistories]
    PRIMARY KEY CLUSTERED ([HistoryId] ASC);
GO

-- Creating primary key on [ComplainTypeId] in table 'tblComplainTypes'
ALTER TABLE [dbo].[tblComplainTypes]
ADD CONSTRAINT [PK_tblComplainTypes]
    PRIMARY KEY CLUSTERED ([ComplainTypeId] ASC);
GO

-- Creating primary key on [DeliveryId] in table 'tblDeliveries'
ALTER TABLE [dbo].[tblDeliveries]
ADD CONSTRAINT [PK_tblDeliveries]
    PRIMARY KEY CLUSTERED ([DeliveryId] ASC);
GO

-- Creating primary key on [DevlieryServiceId] in table 'tblDeliveryServices'
ALTER TABLE [dbo].[tblDeliveryServices]
ADD CONSTRAINT [PK_tblDeliveryServices]
    PRIMARY KEY CLUSTERED ([DevlieryServiceId] ASC);
GO

-- Creating primary key on [Id] in table 'tblDirections'
ALTER TABLE [dbo].[tblDirections]
ADD CONSTRAINT [PK_tblDirections]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblEmails'
ALTER TABLE [dbo].[tblEmails]
ADD CONSTRAINT [PK_tblEmails]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [LocationId] in table 'tblLocations'
ALTER TABLE [dbo].[tblLocations]
ADD CONSTRAINT [PK_tblLocations]
    PRIMARY KEY CLUSTERED ([LocationId] ASC);
GO

-- Creating primary key on [IIdentity] in table 'tblMultipleComplains'
ALTER TABLE [dbo].[tblMultipleComplains]
ADD CONSTRAINT [PK_tblMultipleComplains]
    PRIMARY KEY CLUSTERED ([IIdentity] ASC);
GO

-- Creating primary key on [AdditionalServiceId] in table 'tblOrderAdditionalServices'
ALTER TABLE [dbo].[tblOrderAdditionalServices]
ADD CONSTRAINT [PK_tblOrderAdditionalServices]
    PRIMARY KEY CLUSTERED ([AdditionalServiceId] ASC);
GO

-- Creating primary key on [Id] in table 'tblOrderAdditionalWorks'
ALTER TABLE [dbo].[tblOrderAdditionalWorks]
ADD CONSTRAINT [PK_tblOrderAdditionalWorks]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [ComplainId] in table 'tblOrderComplains'
ALTER TABLE [dbo].[tblOrderComplains]
ADD CONSTRAINT [PK_tblOrderComplains]
    PRIMARY KEY CLUSTERED ([ComplainId] ASC);
GO

-- Creating primary key on [Id] in table 'tblOrderHistories'
ALTER TABLE [dbo].[tblOrderHistories]
ADD CONSTRAINT [PK_tblOrderHistories]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblOrderReleases'
ALTER TABLE [dbo].[tblOrderReleases]
ADD CONSTRAINT [PK_tblOrderReleases]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [OrderId] in table 'tblOrders'
ALTER TABLE [dbo].[tblOrders]
ADD CONSTRAINT [PK_tblOrders]
    PRIMARY KEY CLUSTERED ([OrderId] ASC);
GO

-- Creating primary key on [OrderServiceId] in table 'tblOrderServices'
ALTER TABLE [dbo].[tblOrderServices]
ADD CONSTRAINT [PK_tblOrderServices]
    PRIMARY KEY CLUSTERED ([OrderServiceId] ASC);
GO

-- Creating primary key on [Id] in table 'tblOrderUserLinks'
ALTER TABLE [dbo].[tblOrderUserLinks]
ADD CONSTRAINT [PK_tblOrderUserLinks]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblResetOrders'
ALTER TABLE [dbo].[tblResetOrders]
ADD CONSTRAINT [PK_tblResetOrders]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblServiceMappers'
ALTER TABLE [dbo].[tblServiceMappers]
ADD CONSTRAINT [PK_tblServiceMappers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [ServiceId] in table 'tblServices'
ALTER TABLE [dbo].[tblServices]
ADD CONSTRAINT [PK_tblServices]
    PRIMARY KEY CLUSTERED ([ServiceId] ASC);
GO

-- Creating primary key on [Id] in table 'tblSettings'
ALTER TABLE [dbo].[tblSettings]
ADD CONSTRAINT [PK_tblSettings]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblSMS'
ALTER TABLE [dbo].[tblSMS]
ADD CONSTRAINT [PK_tblSMS]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblUnits'
ALTER TABLE [dbo].[tblUnits]
ADD CONSTRAINT [PK_tblUnits]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [UserGroupId] in table 'tblUserGroupCompanies'
ALTER TABLE [dbo].[tblUserGroupCompanies]
ADD CONSTRAINT [PK_tblUserGroupCompanies]
    PRIMARY KEY CLUSTERED ([UserGroupId] ASC);
GO

-- Creating primary key on [Key] in table 'AggregatedCounters'
ALTER TABLE [dbo].[AggregatedCounters]
ADD CONSTRAINT [PK_AggregatedCounters]
    PRIMARY KEY CLUSTERED ([Key] ASC);
GO

-- Creating primary key on [Key], [Field] in table 'Hashes'
ALTER TABLE [dbo].[Hashes]
ADD CONSTRAINT [PK_Hashes]
    PRIMARY KEY CLUSTERED ([Key], [Field] ASC);
GO

-- Creating primary key on [Id] in table 'Jobs'
ALTER TABLE [dbo].[Jobs]
ADD CONSTRAINT [PK_Jobs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [JobId], [Name] in table 'JobParameters'
ALTER TABLE [dbo].[JobParameters]
ADD CONSTRAINT [PK_JobParameters]
    PRIMARY KEY CLUSTERED ([JobId], [Name] ASC);
GO

-- Creating primary key on [Id], [Queue] in table 'JobQueues'
ALTER TABLE [dbo].[JobQueues]
ADD CONSTRAINT [PK_JobQueues]
    PRIMARY KEY CLUSTERED ([Id], [Queue] ASC);
GO

-- Creating primary key on [Id], [Key] in table 'Lists'
ALTER TABLE [dbo].[Lists]
ADD CONSTRAINT [PK_Lists]
    PRIMARY KEY CLUSTERED ([Id], [Key] ASC);
GO

-- Creating primary key on [Version] in table 'Schemata'
ALTER TABLE [dbo].[Schemata]
ADD CONSTRAINT [PK_Schemata]
    PRIMARY KEY CLUSTERED ([Version] ASC);
GO

-- Creating primary key on [Id] in table 'Servers'
ALTER TABLE [dbo].[Servers]
ADD CONSTRAINT [PK_Servers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Key], [Value] in table 'Sets'
ALTER TABLE [dbo].[Sets]
ADD CONSTRAINT [PK_Sets]
    PRIMARY KEY CLUSTERED ([Key], [Value] ASC);
GO

-- Creating primary key on [Id], [JobId] in table 'States'
ALTER TABLE [dbo].[States]
ADD CONSTRAINT [PK_States]
    PRIMARY KEY CLUSTERED ([Id], [JobId] ASC);
GO

-- Creating primary key on [Key], [Value] in table 'Counters'
ALTER TABLE [dbo].[Counters]
ADD CONSTRAINT [PK_Counters]
    PRIMARY KEY CLUSTERED ([Key], [Value] ASC);
GO

-- Creating primary key on [Id] in table 'tblPushNotifications'
ALTER TABLE [dbo].[tblPushNotifications]
ADD CONSTRAINT [PK_tblPushNotifications]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OrderDisplays'
ALTER TABLE [dbo].[OrderDisplays]
ADD CONSTRAINT [PK_OrderDisplays]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblProviderSettingMappers'
ALTER TABLE [dbo].[tblProviderSettingMappers]
ADD CONSTRAINT [PK_tblProviderSettingMappers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblTeamCapacities'
ALTER TABLE [dbo].[tblTeamCapacities]
ADD CONSTRAINT [PK_tblTeamCapacities]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblProviderWorkinHours'
ALTER TABLE [dbo].[tblProviderWorkinHours]
ADD CONSTRAINT [PK_tblProviderWorkinHours]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblTeamCapacityCalculations'
ALTER TABLE [dbo].[tblTeamCapacityCalculations]
ADD CONSTRAINT [PK_tblTeamCapacityCalculations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblLaborInactives'
ALTER TABLE [dbo].[tblLaborInactives]
ADD CONSTRAINT [PK_tblLaborInactives]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tblProviderTimeSlots'
ALTER TABLE [dbo].[tblProviderTimeSlots]
ADD CONSTRAINT [PK_tblProviderTimeSlots]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [AdditionalWorkId] in table 'tblOrderAdditionalWorks'
ALTER TABLE [dbo].[tblOrderAdditionalWorks]
ADD CONSTRAINT [FK_dbo_tblOrderAddtionalWork_dbo_tblAdditionalWork_AdditionalWorkId]
    FOREIGN KEY ([AdditionalWorkId])
    REFERENCES [dbo].[tblAdditionalWorks]
        ([AdditionalWorkId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrderAddtionalWork_dbo_tblAdditionalWork_AdditionalWorkId'
CREATE INDEX [IX_FK_dbo_tblOrderAddtionalWork_dbo_tblAdditionalWork_AdditionalWorkId]
ON [dbo].[tblOrderAdditionalWorks]
    ([AdditionalWorkId]);
GO

-- Creating foreign key on [AdditionalWorkId] in table 'tblOrderAdditionalServices'
ALTER TABLE [dbo].[tblOrderAdditionalServices]
ADD CONSTRAINT [FK_tblOrderAdditionalServices_tblAdditionalWork]
    FOREIGN KEY ([AdditionalWorkId])
    REFERENCES [dbo].[tblAdditionalWorks]
        ([AdditionalWorkId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblOrderAdditionalServices_tblAdditionalWork'
CREATE INDEX [IX_FK_tblOrderAdditionalServices_tblAdditionalWork]
ON [dbo].[tblOrderAdditionalServices]
    ([AdditionalWorkId]);
GO

-- Creating foreign key on [UserGroupId] in table 'tblAdminUsers'
ALTER TABLE [dbo].[tblAdminUsers]
ADD CONSTRAINT [FK_dbo_tblAdminUsers_dbo_tblUserGroupCompanies_UserGroupId]
    FOREIGN KEY ([UserGroupId])
    REFERENCES [dbo].[tblUserGroupCompanies]
        ([UserGroupId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblAdminUsers_dbo_tblUserGroupCompanies_UserGroupId'
CREATE INDEX [IX_FK_dbo_tblAdminUsers_dbo_tblUserGroupCompanies_UserGroupId]
ON [dbo].[tblAdminUsers]
    ([UserGroupId]);
GO

-- Creating foreign key on [UserId] in table 'tblLocations'
ALTER TABLE [dbo].[tblLocations]
ADD CONSTRAINT [FK_dbo_tblLocations_dbo_tblAdminUsers_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblLocations_dbo_tblAdminUsers_UserId'
CREATE INDEX [IX_FK_dbo_tblLocations_dbo_tblAdminUsers_UserId]
ON [dbo].[tblLocations]
    ([UserId]);
GO

-- Creating foreign key on [LabourId] in table 'tblOrderAdditionalWorks'
ALTER TABLE [dbo].[tblOrderAdditionalWorks]
ADD CONSTRAINT [FK_dbo_tblOrderAddtionalWork_dbo_tblAdminUsers_UserId]
    FOREIGN KEY ([LabourId])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrderAddtionalWork_dbo_tblAdminUsers_UserId'
CREATE INDEX [IX_FK_dbo_tblOrderAddtionalWork_dbo_tblAdminUsers_UserId]
ON [dbo].[tblOrderAdditionalWorks]
    ([LabourId]);
GO

-- Creating foreign key on [UserId] in table 'tblOrderHistories'
ALTER TABLE [dbo].[tblOrderHistories]
ADD CONSTRAINT [FK_dbo_tblOrderHistories_dbo_tblAdminUsers_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrderHistories_dbo_tblAdminUsers_UserId'
CREATE INDEX [IX_FK_dbo_tblOrderHistories_dbo_tblAdminUsers_UserId]
ON [dbo].[tblOrderHistories]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'tblServices'
ALTER TABLE [dbo].[tblServices]
ADD CONSTRAINT [FK_dbo_tblServices_dbo_tblAdminUsers_UserId]
    FOREIGN KEY ([UserId])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblServices_dbo_tblAdminUsers_UserId'
CREATE INDEX [IX_FK_dbo_tblServices_dbo_tblAdminUsers_UserId]
ON [dbo].[tblServices]
    ([UserId]);
GO

-- Creating foreign key on [DriverId] in table 'tblDeliveries'
ALTER TABLE [dbo].[tblDeliveries]
ADD CONSTRAINT [FK_Delivery_tblAdminUsers]
    FOREIGN KEY ([DriverId])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Delivery_tblAdminUsers'
CREATE INDEX [IX_FK_Delivery_tblAdminUsers]
ON [dbo].[tblDeliveries]
    ([DriverId]);
GO

-- Creating foreign key on [UpdatedBy] in table 'tblComplainHistories'
ALTER TABLE [dbo].[tblComplainHistories]
ADD CONSTRAINT [FK_tblComplainHistory_tblAdminUsers]
    FOREIGN KEY ([UpdatedBy])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblComplainHistory_tblAdminUsers'
CREATE INDEX [IX_FK_tblComplainHistory_tblAdminUsers]
ON [dbo].[tblComplainHistories]
    ([UpdatedBy]);
GO

-- Creating foreign key on [ComplainTypeId] in table 'tblMultipleComplains'
ALTER TABLE [dbo].[tblMultipleComplains]
ADD CONSTRAINT [FK_tblMultipleComplains_tblComplainType]
    FOREIGN KEY ([ComplainTypeId])
    REFERENCES [dbo].[tblComplainTypes]
        ([ComplainTypeId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblMultipleComplains_tblComplainType'
CREATE INDEX [IX_FK_tblMultipleComplains_tblComplainType]
ON [dbo].[tblMultipleComplains]
    ([ComplainTypeId]);
GO

-- Creating foreign key on [OrderId] in table 'tblDeliveries'
ALTER TABLE [dbo].[tblDeliveries]
ADD CONSTRAINT [FK_Delivery_tblOrders]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Delivery_tblOrders'
CREATE INDEX [IX_FK_Delivery_tblOrders]
ON [dbo].[tblDeliveries]
    ([OrderId]);
GO

-- Creating foreign key on [DeliveryId] in table 'tblDeliveryServices'
ALTER TABLE [dbo].[tblDeliveryServices]
ADD CONSTRAINT [FK_tblDeliveryServices_tblDeliveries]
    FOREIGN KEY ([DeliveryId])
    REFERENCES [dbo].[tblDeliveries]
        ([DeliveryId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblDeliveryServices_tblDeliveries'
CREATE INDEX [IX_FK_tblDeliveryServices_tblDeliveries]
ON [dbo].[tblDeliveryServices]
    ([DeliveryId]);
GO

-- Creating foreign key on [ServiceId] in table 'tblDeliveryServices'
ALTER TABLE [dbo].[tblDeliveryServices]
ADD CONSTRAINT [FK_tblDeliveryServices_tblServices]
    FOREIGN KEY ([ServiceId])
    REFERENCES [dbo].[tblServices]
        ([ServiceId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblDeliveryServices_tblServices'
CREATE INDEX [IX_FK_tblDeliveryServices_tblServices]
ON [dbo].[tblDeliveryServices]
    ([ServiceId]);
GO

-- Creating foreign key on [LocationId] in table 'tblOrders'
ALTER TABLE [dbo].[tblOrders]
ADD CONSTRAINT [FK_dbo_tblOrders_dbo_tblLocations_LocationId]
    FOREIGN KEY ([LocationId])
    REFERENCES [dbo].[tblLocations]
        ([LocationId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrders_dbo_tblLocations_LocationId'
CREATE INDEX [IX_FK_dbo_tblOrders_dbo_tblLocations_LocationId]
ON [dbo].[tblOrders]
    ([LocationId]);
GO

-- Creating foreign key on [IIdentity] in table 'tblMultipleComplains'
ALTER TABLE [dbo].[tblMultipleComplains]
ADD CONSTRAINT [FK_tblMultipleComplains_tblMultipleComplains]
    FOREIGN KEY ([IIdentity])
    REFERENCES [dbo].[tblMultipleComplains]
        ([IIdentity])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Complainid] in table 'tblMultipleComplains'
ALTER TABLE [dbo].[tblMultipleComplains]
ADD CONSTRAINT [FK_tblMultipleComplains_tblOrderComplains]
    FOREIGN KEY ([Complainid])
    REFERENCES [dbo].[tblOrderComplains]
        ([ComplainId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblMultipleComplains_tblOrderComplains'
CREATE INDEX [IX_FK_tblMultipleComplains_tblOrderComplains]
ON [dbo].[tblMultipleComplains]
    ([Complainid]);
GO

-- Creating foreign key on [OrderId] in table 'tblOrderAdditionalServices'
ALTER TABLE [dbo].[tblOrderAdditionalServices]
ADD CONSTRAINT [FK_dbo_tblAdditionalServices_dbo_tblOrders_OrderId]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblAdditionalServices_dbo_tblOrders_OrderId'
CREATE INDEX [IX_FK_dbo_tblAdditionalServices_dbo_tblOrders_OrderId]
ON [dbo].[tblOrderAdditionalServices]
    ([OrderId]);
GO

-- Creating foreign key on [OrderId] in table 'tblOrderAdditionalWorks'
ALTER TABLE [dbo].[tblOrderAdditionalWorks]
ADD CONSTRAINT [FK_dbo_tblOrderAddtionalWork_dbo_tblOrders_OrderId]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrderAddtionalWork_dbo_tblOrders_OrderId'
CREATE INDEX [IX_FK_dbo_tblOrderAddtionalWork_dbo_tblOrders_OrderId]
ON [dbo].[tblOrderAdditionalWorks]
    ([OrderId]);
GO

-- Creating foreign key on [OrderId] in table 'tblOrderComplains'
ALTER TABLE [dbo].[tblOrderComplains]
ADD CONSTRAINT [FK_tblOrderComplains_tblOrders]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblOrderComplains_tblOrders'
CREATE INDEX [IX_FK_tblOrderComplains_tblOrders]
ON [dbo].[tblOrderComplains]
    ([OrderId]);
GO

-- Creating foreign key on [OrderId] in table 'tblOrderHistories'
ALTER TABLE [dbo].[tblOrderHistories]
ADD CONSTRAINT [FK_dbo_tblOrderHistories_dbo_tblOrders_OrderId]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrderHistories_dbo_tblOrders_OrderId'
CREATE INDEX [IX_FK_dbo_tblOrderHistories_dbo_tblOrders_OrderId]
ON [dbo].[tblOrderHistories]
    ([OrderId]);
GO

-- Creating foreign key on [OrderId] in table 'tblOrderReleases'
ALTER TABLE [dbo].[tblOrderReleases]
ADD CONSTRAINT [FK_tblOrderReleases_tblOrders]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblOrderReleases_tblOrders'
CREATE INDEX [IX_FK_tblOrderReleases_tblOrders]
ON [dbo].[tblOrderReleases]
    ([OrderId]);
GO

-- Creating foreign key on [OrderId] in table 'tblOrderServices'
ALTER TABLE [dbo].[tblOrderServices]
ADD CONSTRAINT [FK_dbo_tblOrderServices_dbo_tblOrders_OrderId]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrderServices_dbo_tblOrders_OrderId'
CREATE INDEX [IX_FK_dbo_tblOrderServices_dbo_tblOrders_OrderId]
ON [dbo].[tblOrderServices]
    ([OrderId]);
GO

-- Creating foreign key on [ServiceId] in table 'tblOrderServices'
ALTER TABLE [dbo].[tblOrderServices]
ADD CONSTRAINT [FK_dbo_tblOrderServices_dbo_tblServices_ServiceId]
    FOREIGN KEY ([ServiceId])
    REFERENCES [dbo].[tblServices]
        ([ServiceId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_tblOrderServices_dbo_tblServices_ServiceId'
CREATE INDEX [IX_FK_dbo_tblOrderServices_dbo_tblServices_ServiceId]
ON [dbo].[tblOrderServices]
    ([ServiceId]);
GO

-- Creating foreign key on [ServiceId] in table 'tblServiceMappers'
ALTER TABLE [dbo].[tblServiceMappers]
ADD CONSTRAINT [FK_tblServiceMapper_tblServices]
    FOREIGN KEY ([ServiceId])
    REFERENCES [dbo].[tblServices]
        ([ServiceId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblServiceMapper_tblServices'
CREATE INDEX [IX_FK_tblServiceMapper_tblServices]
ON [dbo].[tblServiceMappers]
    ([ServiceId]);
GO

-- Creating foreign key on [JobId] in table 'JobParameters'
ALTER TABLE [dbo].[JobParameters]
ADD CONSTRAINT [FK_HangFire_JobParameter_Job]
    FOREIGN KEY ([JobId])
    REFERENCES [dbo].[Jobs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [JobId] in table 'States'
ALTER TABLE [dbo].[States]
ADD CONSTRAINT [FK_HangFire_State_Job]
    FOREIGN KEY ([JobId])
    REFERENCES [dbo].[Jobs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_HangFire_State_Job'
CREATE INDEX [IX_FK_HangFire_State_Job]
ON [dbo].[States]
    ([JobId]);
GO

-- Creating foreign key on [ParentId] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [FK_Category_Category]
    FOREIGN KEY ([ParentId])
    REFERENCES [dbo].[Categories]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Category_Category'
CREATE INDEX [IX_FK_Category_Category]
ON [dbo].[Categories]
    ([ParentId]);
GO

-- Creating foreign key on [CategoryId] in table 'tblServices'
ALTER TABLE [dbo].[tblServices]
ADD CONSTRAINT [FK_tblServices_Category]
    FOREIGN KEY ([CategoryId])
    REFERENCES [dbo].[Categories]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblServices_Category'
CREATE INDEX [IX_FK_tblServices_Category]
ON [dbo].[tblServices]
    ([CategoryId]);
GO

-- Creating foreign key on [OrderId] in table 'OrderDisplays'
ALTER TABLE [dbo].[OrderDisplays]
ADD CONSTRAINT [FK_OrderDisplay_tblOrders]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OrderDisplay_tblOrders'
CREATE INDEX [IX_FK_OrderDisplay_tblOrders]
ON [dbo].[OrderDisplays]
    ([OrderId]);
GO

-- Creating foreign key on [ServiceProviderId] in table 'tblTeamCapacities'
ALTER TABLE [dbo].[tblTeamCapacities]
ADD CONSTRAINT [FK_tblTeamCapacity]
    FOREIGN KEY ([ServiceProviderId])
    REFERENCES [dbo].[tblUserGroupCompanies]
        ([UserGroupId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblTeamCapacity'
CREATE INDEX [IX_FK_tblTeamCapacity]
ON [dbo].[tblTeamCapacities]
    ([ServiceProviderId]);
GO

-- Creating foreign key on [ServiceProviderId] in table 'tblProviderWorkinHours'
ALTER TABLE [dbo].[tblProviderWorkinHours]
ADD CONSTRAINT [FK_tblProviderWorkinHour_tblUserGroupCompanies]
    FOREIGN KEY ([ServiceProviderId])
    REFERENCES [dbo].[tblUserGroupCompanies]
        ([UserGroupId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblProviderWorkinHour_tblUserGroupCompanies'
CREATE INDEX [IX_FK_tblProviderWorkinHour_tblUserGroupCompanies]
ON [dbo].[tblProviderWorkinHours]
    ([ServiceProviderId]);
GO

-- Creating foreign key on [OrderId] in table 'tblTeamCapacityCalculations'
ALTER TABLE [dbo].[tblTeamCapacityCalculations]
ADD CONSTRAINT [FK_tblTeamCapacityCalculation_tblOrders]
    FOREIGN KEY ([OrderId])
    REFERENCES [dbo].[tblOrders]
        ([OrderId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblTeamCapacityCalculation_tblOrders'
CREATE INDEX [IX_FK_tblTeamCapacityCalculation_tblOrders]
ON [dbo].[tblTeamCapacityCalculations]
    ([OrderId]);
GO

-- Creating foreign key on [ServiceProviderId] in table 'tblTeamCapacityCalculations'
ALTER TABLE [dbo].[tblTeamCapacityCalculations]
ADD CONSTRAINT [FK_tblTeamCapacityCalculation_tblUserGroupCompanies]
    FOREIGN KEY ([ServiceProviderId])
    REFERENCES [dbo].[tblUserGroupCompanies]
        ([UserGroupId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblTeamCapacityCalculation_tblUserGroupCompanies'
CREATE INDEX [IX_FK_tblTeamCapacityCalculation_tblUserGroupCompanies]
ON [dbo].[tblTeamCapacityCalculations]
    ([ServiceProviderId]);
GO

-- Creating foreign key on [CategoryrId] in table 'tblAdditionalWorks'
ALTER TABLE [dbo].[tblAdditionalWorks]
ADD CONSTRAINT [FK_tblAdditionalWork_Category]
    FOREIGN KEY ([CategoryrId])
    REFERENCES [dbo].[Categories]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblAdditionalWork_Category'
CREATE INDEX [IX_FK_tblAdditionalWork_Category]
ON [dbo].[tblAdditionalWorks]
    ([CategoryrId]);
GO

-- Creating foreign key on [LabourId] in table 'tblLaborInactives'
ALTER TABLE [dbo].[tblLaborInactives]
ADD CONSTRAINT [FK_tblLaborInactive_tblAdminUsers]
    FOREIGN KEY ([LabourId])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblLaborInactive_tblAdminUsers'
CREATE INDEX [IX_FK_tblLaborInactive_tblAdminUsers]
ON [dbo].[tblLaborInactives]
    ([LabourId]);
GO

-- Creating foreign key on [ProviderId] in table 'tblLaborInactives'
ALTER TABLE [dbo].[tblLaborInactives]
ADD CONSTRAINT [FK_tblLaborInactive_tblLaborInactive]
    FOREIGN KEY ([ProviderId])
    REFERENCES [dbo].[tblUserGroupCompanies]
        ([UserGroupId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblLaborInactive_tblLaborInactive'
CREATE INDEX [IX_FK_tblLaborInactive_tblLaborInactive]
ON [dbo].[tblLaborInactives]
    ([ProviderId]);
GO

-- Creating foreign key on [LabourId] in table 'tblProviderTimeSlots'
ALTER TABLE [dbo].[tblProviderTimeSlots]
ADD CONSTRAINT [FK_tblProviderTimeSlot_tblAdminUsers]
    FOREIGN KEY ([LabourId])
    REFERENCES [dbo].[tblAdminUsers]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblProviderTimeSlot_tblAdminUsers'
CREATE INDEX [IX_FK_tblProviderTimeSlot_tblAdminUsers]
ON [dbo].[tblProviderTimeSlots]
    ([LabourId]);
GO

-- Creating foreign key on [ServiceProviderId] in table 'tblProviderTimeSlots'
ALTER TABLE [dbo].[tblProviderTimeSlots]
ADD CONSTRAINT [FK_tblProviderTimeSlot_tblUserGroupCompanies]
    FOREIGN KEY ([ServiceProviderId])
    REFERENCES [dbo].[tblUserGroupCompanies]
        ([UserGroupId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblProviderTimeSlot_tblUserGroupCompanies'
CREATE INDEX [IX_FK_tblProviderTimeSlot_tblUserGroupCompanies]
ON [dbo].[tblProviderTimeSlots]
    ([ServiceProviderId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------