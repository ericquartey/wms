net use z: \\192.168.137.1\c$ fergrp_2012 /user:wmsadmin /persistent:no
md z:\backup
copy e:\database\*.* z:\backup /y
net use z: /delete
