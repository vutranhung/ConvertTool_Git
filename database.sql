USE [master]
GO
/****** Object:  Database [PruData]    Script Date: 2/10/2020 3:03:38 PM ******/
CREATE DATABASE [PruData]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'PruData', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLSERVER2014\MSSQL\DATA\PruData.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'PruData_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLSERVER2014\MSSQL\DATA\PruData_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [PruData] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [PruData].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [PruData] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [PruData] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [PruData] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [PruData] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [PruData] SET ARITHABORT OFF 
GO
ALTER DATABASE [PruData] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [PruData] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [PruData] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [PruData] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [PruData] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [PruData] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [PruData] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [PruData] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [PruData] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [PruData] SET  DISABLE_BROKER 
GO
ALTER DATABASE [PruData] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [PruData] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [PruData] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [PruData] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [PruData] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [PruData] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [PruData] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [PruData] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [PruData] SET  MULTI_USER 
GO
ALTER DATABASE [PruData] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [PruData] SET DB_CHAINING OFF 
GO
ALTER DATABASE [PruData] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [PruData] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [PruData] SET DELAYED_DURABILITY = DISABLED 
GO
USE [PruData]
GO
/****** Object:  Table [dbo].[Beneficiary]    Script Date: 2/10/2020 3:03:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Beneficiary](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ConstractID] [int] NOT NULL,
	[CustomerID] [int] NOT NULL,
	[ConstractCode] [nvarchar](50) NULL,
	[CustomerCode] [nvarchar](50) NULL,
	[Relation] [nvarchar](50) NULL,
 CONSTRAINT [PK_Beneficiary] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Contract]    Script Date: 2/10/2020 3:03:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Contract](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[ProductName] [nvarchar](150) NULL,
	[InsuranceFee] [decimal](18, 0) NULL,
	[Status] [nvarchar](50) NULL,
	[EffectiveDate] [datetime] NULL,
	[NextPaymentDate] [datetime] NULL,
	[LatestPaymentDate] [datetime] NULL,
	[DueDate] [datetime] NULL,
	[ValueRefundedToAdvance] [decimal](18, 0) NULL,
	[AdvancePayment] [decimal](18, 0) NULL,
	[RTBT] [decimal](18, 0) NULL,
	[QLDK] [decimal](18, 0) NULL,
	[HuyHD] [decimal](18, 0) NULL,
	[TVVCode] [nvarchar](50) NULL,
	[TVV] [nvarchar](200) NULL,
	[DataComplete] [char](1) NOT NULL,
	[DiaChiHienTai] [nvarchar](500) NULL,
	[TelMobile] [nvarchar](50) NULL,
	[TelCompany] [nvarchar](50) NULL,
	[TelHome] [nvarchar](50) NULL,
 CONSTRAINT [PK_Contract] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 2/10/2020 3:03:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](20) NOT NULL,
	[CMND] [nvarchar](50) NULL,
	[Name] [nvarchar](150) NULL,
	[DOB] [datetime] NULL,
	[City] [nvarchar](50) NULL,
	[District] [nvarchar](50) NULL,
	[Address] [nvarchar](500) NULL,
	[Phone] [nvarchar](50) NULL,
	[TelCompany] [nvarchar](50) NULL,
	[TelHome] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[InsuredPerson]    Script Date: 2/10/2020 3:03:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InsuredPerson](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ContractID] [int] NOT NULL,
	[CustomerID] [int] NOT NULL,
	[Relation] [nvarchar](50) NULL,
	[TenSanPham] [nvarchar](100) NULL,
	[Loai] [nvarchar](50) NULL,
	[Fee] [decimal](18, 0) NULL,
	[NgayDaoHan] [datetime] NULL,
	[DongPhiDenNgay] [datetime] NULL,
	[TinhTrang] [nvarchar](100) NULL,
 CONSTRAINT [PK_InsuredPerson] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Y: Complete; N: Not' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Contract', @level2type=N'COLUMN',@level2name=N'DataComplete'
GO
USE [master]
GO
ALTER DATABASE [PruData] SET  READ_WRITE 
GO
