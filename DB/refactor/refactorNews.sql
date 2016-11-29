select concat(
"Insert into InfoPages values('", 
(SELECT UUID()), #Id
"','",
ndescr.name, #Name
"','",
url.keyword, #UrlName
"','",
REPLACE(ndescr.description, "'", "''"), #[Content]
"','",
news.image, #[ImageUrl]
"',",
1,#IsActive
",",
1,#IsNews
",",
news.sort_order,
",'",
news.date_added,#[CreatedOn]
"','",
NOW(),#LastModified
"',",
'null'
");"
)
FROM benefitcompany.oc_record news, benefitcompany.oc_record_description ndescr, benefitcompany.oc_url_alias_blog url
where news.record_id = ndescr.record_id
and url.query = concat('record_id=', news.record_id)
and url.language_id = 3
and ndescr.language_id = 3
INTO OUTFILE 'e:/Backups/benefit/results/news.sql';