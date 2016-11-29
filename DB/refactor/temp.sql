select ru.id_user,
((SELECT sum(amount) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and order_id = 0)
+ (SELECT sum(ct.amount) FROM benefitcompany.oc_customer_transaction ct where cu.customer_id = ct.customer_id)
) as Bonuses,

(
ifnull((SELECT sum(amount) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and  date_added > '2016-11-01' and order_id != 0), 0) 
+
ifnull((SELECT sum(paybonus) FROM recard.payment where id_user = ru.id_user and datecheck > '2016-11-01'), 0)
) as CurrentBonuses ,

0 as HangingBonuses,

(SELECT sum(amount) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and order_id = 0 and amount > 0)
 as TotalBonuses,

(
ifnull((SELECT sum(paybals) FROM recard.payment where id_user = ru.id_user and status = 1 and datecheck>='2016-10-01' and datecheck<'2016-11-01'), 0) + 
ifnull((SELECT sum(balls) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and date_added>'2016-10-01' and date_added<'2016-11-01' and order_id>0),0)
) as HandlingPointsAccount,

(
ifnull((SELECT sum(paybals) FROM recard.payment where id_user = ru.id_user and status = 1 and datecheck>'2016-11-01'),0) 
+ 
ifnull((SELECT sum(balls) FROM benefitcompany.oc_affiliate_transaction where affiliate_id = bu.affiliate_id and date_added>'2016-11-01' and order_id>0), 0)
) as PointsAccount

from benefitcompany.oc_affiliate bu
left join benefitcompany.oc_customer cu on cu.email = bu.email
left join recard.reuser ru on bu.db_card_id = ru.id_user
left join recard.cardbase cb on ru.username = cb.cardcode
where 
bu.affiliate_id = 6454
group by bu.code
limit 10000