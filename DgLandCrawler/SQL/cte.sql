
USE DGProduct;

WITH CTE AS (
    SELECT 
        gs.GoogleId, 
        gs.Supplier, 
        dg.Name, 
        gs.BaseUrl,
        TRY_CAST(LTRIM(RTRIM(dg.RegularPrice)) AS DECIMAL(18, 2)) AS DGRegularPrice, 
        TRY_CAST(LTRIM(RTRIM(gs.Price)) AS DECIMAL(18, 2)) AS GooglePrice, 
        TRY_CAST(LTRIM(RTRIM(dg.RegularPrice)) AS DECIMAL(18, 2)) - TRY_CAST(LTRIM(RTRIM(gs.Price)) AS DECIMAL(18, 2)) AS PriceDifference
    FROM 
        DGProducts AS dg
    JOIN 
        GoogleSearchResults AS gs 
    ON 
        dg.Id = gs.DGProductId
		Where gs.Price <> N'0'
)
SELECT * FROM CTE
WHERE PriceDifference > 0;