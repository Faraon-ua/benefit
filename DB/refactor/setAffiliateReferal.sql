select concat(
"Update ApplicationUsers set ReferalId = (Select Id from ApplicationUsers where Email = '", 
(Select email from benefitcompany.oc_affiliate where affiliate_id = bu.parent),
"') where email = '",
bu.email,
"';"
)
from benefitcompany.oc_affiliate bu
limit 10000
INTO OUTFILE 'd:/Backups/Benefit/results/usersUpdateReferal.sql';

