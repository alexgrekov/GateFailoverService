#Сервис обеспечения отказоустойчивости шлюза

##Описание
Задача сервиса состоит в том, чтобы гарантировать поддержание в рабочем состоянии одной
включенной виртуальной машины с CloudHostedRouter на одном из трех нод Hyper-V.
Сделано для возможности обеспечения отказоустойчивости без домена Windows, а значит 
удешевлением и упрощением инфраструктуры датацентра.
Это должно быть устойчиво к Split-Brain и к отказу одного из трех хостов (впоследствии 
может быть масштабировано до отказа двух хостов из пяти и далее).

##Требования

Хост серверы должны быть настроены следующим образом:
1. Открытый порт Firewall TCP/9999 (настраивается) - для коммуникаций кластера.
2. Готовая к запуску виртуальная машина с именем, указаным в конфигурационном файле.
3. Крайне рекомендуется использование частной сети для коммуникации кластера в целях безопасности.

##Логика работы

Один рабочий инстанс пытается отключить виртуальную машину, предполагая, что он отрезан от остальных
нод кластера из-за отказа сети.
Два и более инстансов сервиса, если их количество больше половины от всех узлов кластера, образуют
кворум, и пытаются запустить виртуальную машину на одном из них, если она еще не запущена.
Критерием является наибольший объем свободной оперативной памяти.

##Структура сервиса

* C:\GFS\GateFailoverService.exe					-- исполняемый файл службы
* C:\GFS\GateFailoverService.conf				-- конфигурационный файл
* C:\GFS\GateFailoverService.exe.config			-- system config
* C:\GFS\GateFailoverService.log 				-- стандартный лог-файл
 
 
Структура конфига
```
# Local IP of node:
[localIp]=194.87.99.235 

# LogFile name:
[logFileName]=C:\GFS\GateFailoverService.log

# IPs of all nodes in cluster, splitted by ',' without spaces:
[nodes]=194.87.99.235,194.87.103.145,176.119.156.173

# Port, that will be used to inter-cluster communication, need to be opened in Firewall:
[incomeTcpPort]=9999
```

##Тестовое окружение
1. Создать 3 виртуальные машины с Windows Server 2016
2. Включить у них вложенную виртуализацию
3. Выполнить код подготовки:
```
Rename-Computer TEST1
winrm quickconfig -Force
Enable-PSRemoting -Force
Enable-WSManCredSSP -Role Server -Force

$Password = Read-Host -AsSecureString
New-LocalUser "root" -Password $Password -FullName "root" -Description "root" -AccountNeverExpires -UserMayNotChangePassword
Add-LocalGroupMember -Group 'Administrators' -Member "root"
Set-LocalUser -Name "root" -PasswordNeverExpires 1

Install-WindowsFeature -Name Hyper-V -IncludeManagementTools -Restart

New-NetFirewallRule -Name "GateFailoverService" -Description "GateFailoverService cluster ring sync accept" -DisplayName "GateFailoverService" -Enabled True -Profile Public -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9999
```

4. Разместить файлы сервиса и создать сам сервис, разместить виртуальные машины.
