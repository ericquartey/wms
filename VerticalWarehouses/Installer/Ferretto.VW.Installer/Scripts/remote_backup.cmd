net use z: /delete
net use z: \\192.168.137.1\c$ fergrp_2012 /user:wmsadmin /persistent:no
if not exist z:\backup\MAS (md z:\backup\MAS)
copy e:\database\MachineAutomationService.Primary.db z:\backup\MAS /y
net use z: /delete
