set @sku := 100000;

select distinct concat(
"Insert into Products values('", 
pr.product_id, #Id
"','",
REPLACE(prd.name, "'", "''"), #Name
"','",
url.keyword, #UrlName
"',",
(@sku := @sku + 1),#SKU
",'",
REPLACE(prd.description, "'", "''"), #Description
"',",
pr.price, #Price
",",
pr.quantity, #Amount
",",
1, #IsActive
",'",
NOW(), #LastModified
"',",
'null', #LastModifiedBy
",",
'(select top(1) Id from Categories)', #CategoryId
",'",
cat.seller_id, #SellerId
"',",
"(select top(1) Id from Currencies where Name='UAH' and Provider='PrivatBank')", #CurrencyId
");"
)
from oc_product pr
LEFT JOIN oc_product_description prd ON prd.product_id = pr.product_id 
left join oc_url_alias url on url.query = concat('product_id=',pr.product_id)
left join benefitcompany.oc_product_to_category prtc on pr.product_id = prtc.product_id
left join benefitcompany.oc_category cat on prtc.category_id = cat.category_id
where cat.seller_id != 5030 
and prd.language_id = 3
#and pr.product_id = 13843
and cat.seller_id != 0
group by cat.seller_id,pr.product_id
order by cat.seller_id
INTO OUTFILE 'e:/Backups/benefit/results/products.sql';