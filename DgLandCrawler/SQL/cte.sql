
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

WITH RankedResults AS (
    SELECT 
		dg.Id,
		gs.GoogleId,
        dg.DgLandId, 
        gs.CreationTime, 
        dg.Name, 
        gs.Title,
        gs.BaseUrl,
        gs.Supplier,
        gs.Price,
        dg.RegularPrice,
        dg.SalePrice,
        ROW_NUMBER() OVER (
            PARTITION BY gs.BaseUrl 
            ORDER BY gs.CreationTime DESC
        ) AS rn
    FROM GoogleSearchResults AS gs
    INNER JOIN DGProducts AS dg ON gs.DGProductId = dg.Id
    WHERE dg.RegularPrice IS NOT NULL AND dg.RegularPrice != 0
)
SELECT *
FROM RankedResults
WHERE rn = 1 order by CreationTime DESC;