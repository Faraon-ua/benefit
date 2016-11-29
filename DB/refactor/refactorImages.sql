select concat(
"Insert into Images values('", 
(SELECT UUID()), #Id
"','",
image, #[ImageUrl]
"',",
ImageType, #[ImageType]
",",
SellerId, #SellerId
",",
ProductId,#IFNULL(ProductId, `null`), #ProductId
");"
)
from(
select seller_id as SellerId, 'null' as ProductId, image, 0 as 'Order', 0 as ImageType from oc_category 
where image !='' 
and seller_id!=0 
and parent_id in (SELECT category_id FROM benefitcompany.oc_category where parent_id= 0 and status = 1) 
group by seller_id
union 
select seller_id as SellerId, 'null', image, sort, 1 from oc_seller_images
union 
select null, product_id as ProductId, image, sort_order, 2 from oc_product_image 
) as images
INTO OUTFILE 'e:/Backups/benefit/results/images.sql';