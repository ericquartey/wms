net use z: /delete
net use z: \\192.168.137.1\backup fergrp_2012 /user:wmsadmin /persistent:no
if not exist z:\MAS (md z:\MAS)
xcopy "e:\database\MachineAutomationService.Primary.db" z:\MAS\MachineAutomationService.Primary.db* /y
net use z: /delete
