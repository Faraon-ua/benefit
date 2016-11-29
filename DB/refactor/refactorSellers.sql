select distinct concat(
"Insert into Sellers values('", 
sel.seller_id, #Id
"','",
REPLACE(sel.nickname, "'", "''"), #Name
"','",
REPLACE(catdesc.description, "'", "''"), #Description
"','",
url.keyword, #UrlName
"','",
"Меню", #CatalogButtonName
"',",
1, #IsActive
",",
1, #IsBenefitCardActive
",",
1, #HasEcommerce
",",
10, #TotalDiscount
",",
10, #UserDiscount,
",'",
date_created, #RegisteredOn
"','",
NOW(), #LastModified
"',",
'null', #ModifiedBy
",",
'(select top(1) Id from ApplicationUsers)', #OwnerId
",",
'null', #BenefitCardReferal,
",",
'null', #SiteReferal
");"
)
from oc_ms_seller sel, oc_category cat, oc_category_description catdesc, oc_url_alias url where 
sel.seller_id = cat.seller_id and
cat.category_id = catdesc.category_id and
url.query = concat('category_id=', cat.category_id) and
cat.parent_id in (SELECT category_id FROM benefitcompany.oc_category where parent_id = 0) and
catdesc.language_id = 3 
group by sel.nickname
having count(sel.nickname) < 5
order by sel.nickname
INTO OUTFILE 'e:/Backups/benefit/results/sellers.sql';