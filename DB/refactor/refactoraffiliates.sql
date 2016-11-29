update benefitcompany.oc_affiliate aff set aff.email = concat('[copy]', email)
where aff.affiliate_id in
(select aff_id from
(select affiliate_id aff_id from benefitcompany.oc_affiliate
group by email
having count(email)>1) as sub);

update benefitcompany.oc_affiliate aff set aff.telephone = concat('[copy]', telephone)
where aff.affiliate_id in
(select aff_id from
(select affiliate_id aff_id from benefitcompany.oc_affiliate
group by telephone
having count(telephone)>1) as sub);

update benefitcompany.oc_affiliate aff set aff.telephone = concat('[copy2]', telephone)
where aff.affiliate_id in
(select aff_id from
(select affiliate_id aff_id from benefitcompany.oc_affiliate
group by telephone
having count(telephone)>1) as sub);

SET @maxcode := (select max(code) from benefitcompany.oc_affiliate);

select concat(
"Insert into ApplicationUsers values('", 
(SELECT UUID()), #Id 1
"',",
1, #IsActive 2
",'",
REPLACE(concat(bu.firstname, ' ', bu.lastname), "'", "''"), #FullName 3
"','",
SUBSTRING(IFNULL(ru.username,''),1,10), #CardNumber 4
"',",
IFNULL(NULLIF(bu.code, ''), @maxcode := @maxcode + 1), #ExternalNumber 5
",",
'null', #BusinessLevel 6
",",
'null' #Status 7
",",
'null', #ReferalId !!!!!!!! 8
",'",
bu.date_added, #RegisteredOn 9
"',",
#CurrentBonusAccount 10
(
ifnull((SELECT sum(amount) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and  date_added > '2016-11-01' and order_id != 0), 0) 
+
ifnull((SELECT sum(paybonus) FROM recard.payment where id_user = ru.id_user and datecheck > '2016-11-01'), 0)
),
",'",
bu.email, #Email 11
"',",
1, #EmailConfirmed 12
",",
'null' #PasswordHash 13
",",
'null' #SecurityStamp 14
",'",
SUBSTRING(bu.telephone,1,15), #PhoneNumber 15
"',",
'1', #PhoneNumberConfirmed 16
",",
0, #[TwoFactorEnabled] 17
",",
'null',#[LockoutEndDateUtc] 18
",",
0,#[LockoutEnabled] 19
",",
0,#[AccessFailedCount] 20
",'",
bu.email, #Username 21
"','",
IFNULL(cb.nfccode, ''), #[NFCCardNumber] 22
"',",
'null', #FinancialPassword 23
",",
400000, #RegionId 24
",",
'null', #Address 25
",",
#BonusAccount 26
IFNULL((SELECT sum(amount) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and order_id = 0), 0) +  
IFNULL((SELECT sum(ct.amount) FROM benefitcompany.oc_customer_transaction ct where cu.customer_id = ct.customer_id), 0),
",",
#[TotalBonusAccount] 27
IFNULL((SELECT sum(amount) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and order_id = 0 and amount > 0), 0),
",",
#[HangingBonusAccount] 28
0, 
",",
#[PointsAccount] 29
(
ifnull((SELECT sum(paybals) FROM recard.payment where id_user = ru.id_user and status = 1 and datecheck>'2016-11-01'),0) 
+ 
ifnull((SELECT sum(balls) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and date_added>'2016-11-01' and order_id>0), 0)
),
",",
#[HangingPointsAccount] 30
(
ifnull((SELECT sum(paybals) FROM recard.payment where id_user = ru.id_user and status = 1 and datecheck>='2016-10-01' and datecheck<'2016-11-01'), 0) + 
ifnull((SELECT sum(balls) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and date_added>'2016-10-01' and date_added<'2016-11-01' and order_id>0),0)
),
");" 
)
from benefitcompany.oc_affiliate bu
left join benefitcompany.oc_customer cu on cu.email = bu.email
left join recard.reuser ru on bu.db_card_id = ru.id_user
left join recard.cardbase cb on ru.username = cb.cardcode
group by bu.code
limit 10000
INTO OUTFILE 'd:/Backups/Benefit/results/users.sql';

