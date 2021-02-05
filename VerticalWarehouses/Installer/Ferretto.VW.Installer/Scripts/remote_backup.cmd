net use z: /delete
net use z: \\192.168.137.1\c$ fergrp_2012 /user:wmsadmin /persistent:no
if not exist z:\backup\MAS (md z:\backup\MAS)
xcopy "e:\database\MachineAutomationService.Primary.db" z:\backup\MAS\MachineAutomationService.Primary.db* /y
net use z: /delete
