netsh interface ipv4 set address name="Ethernet-Machine" static $(Install:Parameter:PpcIpAddress) 255.255.255.0 store=persistent
