USE [master]
GO
/****** Object:  Database [Neo_Order]    Script Date: 28/2/2022 5:01:50 pm ******/
CREATE DATABASE [Neo_Order]
GO
ALTER DATABASE [Neo_Order] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Neo_Order].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Neo_Order] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Neo_Order] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Neo_Order] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Neo_Order] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Neo_Order] SET ARITHABORT OFF 
GO
ALTER DATABASE [Neo_Order] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Neo_Order] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Neo_Order] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Neo_Order] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Neo_Order] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Neo_Order] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Neo_Order] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Neo_Order] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Neo_Order] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Neo_Order] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Neo_Order] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Neo_Order] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Neo_Order] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Neo_Order] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Neo_Order] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Neo_Order] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Neo_Order] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Neo_Order] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Neo_Order] SET  MULTI_USER 
GO
ALTER DATABASE [Neo_Order] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Neo_Order] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Neo_Order] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Neo_Order] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Neo_Order] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Neo_Order] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [Neo_Order] SET QUERY_STORE = OFF
GO
USE [Neo_Order]
GO
/****** Object:  Table [dbo].[tbl_orders]    Script Date: 28/2/2022 5:01:50 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_orders](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[OrderStatusID] [int] NULL,
	[ProductList] [nvarchar](max) NULL,
	[DateCreated] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[GetAllOrderItem]    Script Date: 28/2/2022 5:01:50 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAllOrderItem]
        AS
        BEGIN
            SET NOCOUNT ON;
            select * from tbl_orders
        END
GO
/****** Object:  StoredProcedure [dbo].[InsertOrderItem]    Script Date: 28/2/2022 5:01:50 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ================================================================
-- Developer:	Wilson Yao Weilin
-- Version:     Intial Version
-- Description:	Create Order for Neo Commerces
-- ================================================================
CREATE PROCEDURE [dbo].[InsertOrderItem]
(
	@OrderID            INT            = NULL OUTPUT,
	@OrderStatusID      INT            = NULL,
	@ProductList		NVARCHAR(MAX)  = NULL
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO tbl_orders
	(
		OrderStatusID,
		ProductList,
		DateCreated
	)
	VALUES
	(
		@OrderStatusID,
		@ProductList,
		GETDATE()
	)

	SET @OrderID = SCOPE_IDENTITY()

END
GO
USE [master]
GO
ALTER DATABASE [Neo_Order] SET  READ_WRITE 
GO
