
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE or ALTER PROCEDURE GetUpdatedCrawledPriceList
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	WITH RankedResults AS (
		SELECT 
			dg.DgLandId, 
			dg.Category,
			gs.CreationTime, 
			dg.Name, 
			gs.Title,
			gs.BaseUrl,
			gs.Supplier,
			gs.Price,
			dg.RegularPrice,
			dg.SalePrice
		FROM GoogleSearchResults AS gs
		JOIN DGProducts AS dg ON gs.DGProductId = dg.Id
	)
	SELECT *
	FROM RankedResults
	order by CreationTime DESC;

END
GO
