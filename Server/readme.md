# GPNBot net core 3.1 api

Создаем ВМ и настраиваем доступ:

    1. Создать файл для ssh (задать название файла (и путь) и passphrase):
    gpnbot_rsa

    ssh-keygen -t rsa -b 2048

    2. Приватный ключ должен быть импортирован, иначе команда 
    ssh -i "путь.к.паблик.ключу" пользователь@0.0.0.0 
    вернет ошибку.

    запуск полнофункционального агента в винде:
      ssh-agent bash
    добавление сертификата:
      ssh-add "C:\Users\<user>\.ssh\unipro_rsa"

    Проверка списка ключей, которые доступны для ssh:

    список "отпечатков"
      ssh-add -l
      // пример: 2048 SHA256:Qn+/x7vlgBtWF+HmuDUJHi3BDhZ0OlOvvl/kvNz9ORg C:\Users\a.chukhonin\.ssh\id_rsa (RSA)

    3. Создать ВМ Ubuntu 18.04
    пользователь gpnadmin
    ключ gpnbot_rsa.pub


Настраиваем ВМ для netcore + webserver
    
    /*
    https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#1804-
    https://www.c-sharpcorner.com/article/how-to-deploy-net-core-application-on-linux/
    https://www.nginx.com/blog/tutorial-proxy-net-core-kestrel-nginx-plus/
    https://docs.microsoft.com/ru-ru/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-3.1
    */
    4. netcore

    wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    sudo dpkg -i packages-microsoft-prod.deb

    sudo apt-get update; \
      sudo apt-get install -y apt-transport-https && \
      sudo apt-get update && \
      sudo apt-get install -y dotnet-sdk-3.1

    /*
    sudo apt-get update; \
      sudo apt-get install -y apt-transport-https && \
      sudo apt-get update && \
      sudo apt-get install -y aspnetcore-runtime-3.1
    */
    deploy : use FileZilla

    5.1. apache2 (go to 5.2. Nginx)

    sudo apt-get install apache2
    sudo a2enmod proxy proxy_http proxy_html proxy_wstunnel
    sudo a2enmod rewrite

    /* 
    edit new conf file: 
    replace {endpoint} with a path
    */

    sudo nano /etc/apache2/conf-enabled/netcore.conf

    <VirtualHost *:80>  
       ServerName www.DOMAIN.COM  
       ProxyPreserveHost On  
       ProxyPass /{endpoint}/ http://127.0.0.1:5000/  
       ProxyPassReverse /{endpoint}/ http://127.0.0.1:5000/  
       RewriteEngine on  
       RewriteCond %{HTTP:UPGRADE} ^WebSocket$ [NC]  
       RewriteCond %{HTTP:CONNECTION} Upgrade$ [NC]  
       RewriteRule /{endpoint}/(.*) ws://127.0.0.1:5000/$1 [P]  
       ErrorLog /var/log/apache2/netcore-error.log  
       CustomLog /var/log/apache2/netcore-access.log common  
    </VirtualHost>

    5.2. Nginx

    /*
    stop Apache

    sudo service httpd stop
    sudo systemctl disable httpd
    */

    sudo apt-get install -y nginx
    sudo openssl req -x509 -subj /CN=localhost -days 365 -set_serial 2 -newkey rsa:4096 -keyout /etc/nginx/cert.key -nodes -out /etc/nginx/cert.pem

    sudo nano /etc/nginx/sites-enabled/default

    server {
        listen 80 default_server;
        listen [::]:80 default_server;
        return 301 https://$host$request_uri;
    }

    server {
        listen 443 ssl http2 default_server;
        listen [::]:443 ssl http2 default_server;

        ssl_certificate /etc/nginx/cert.pem;
        ssl_certificate_key /etc/nginx/cert.key;

        location / {
            proxy_pass http://dotnet;
            proxy_set_header Host $host;
        }
    }

    upstream dotnet {
        zone dotnet 64k;
        server 127.0.0.1:5000;
    }

    опционально ssl letsencrypt:

    sudo apt install snapd
    sudo snap install core
    sudo snap refresh core
    sudo snap install --classic certbot

    sudo ln -s /snap/bin/certbot /usr/bin/certbot 

    sudo certbot --nginx

Настройка SQL

    6. MariaDB
    sudo apt-get update; \
      sudo apt-get install -y mariadb-server mariadb-client

    sudo mysql_secure_installation
    root password = passphrase (п.1)
    sudo mysql -u root

    Create user and DB:

    CREATE USER 'gpnadmin'@'%' IDENTIFIED BY '8FxLWcquHb';
    GRANT ALL PRIVILEGES ON *.* TO 'gpnadmin'@'%' WITH GRANT OPTION;
    FLUSH PRIVILEGES;

    CREATE DATABASE store;
    USE store;
    CREATE TABLE `messages` (
      `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
      `jsonData` longtext DEFAULT NULL CHECK (json_valid(`jsonData`)),
      `userGuid` varchar(36) NOT NULL,
      `created` timestamp NOT NULL DEFAULT current_timestamp(),
      `serviceGuid` varchar(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
      `iid` bigint(20) unsigned DEFAULT NULL,
      `isMy` tinyint(4) NOT NULL DEFAULT 0,
      PRIMARY KEY (`id`),
      KEY `USER_IDX` (`userGuid`,`created`,`serviceGuid`)
    );
    CREATE TABLE `ailog` (
      `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
      `messageId` bigint(20) unsigned NOT NULL,
      `userMeasuring` varchar(255) DEFAULT NULL,
      PRIMARY KEY (`id`),
      KEY `message_IDX` (`messageId`)
    );

    CREATE DATABASE apilog;
    USE apilog;
    CREATE TABLE `web` (
      `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
      `data` longtext DEFAULT NULL,
      `created` datetime NOT NULL DEFAULT current_timestamp(),
      `level` varchar(15) NOT NULL,
      `address` varchar(255) NOT NULL,
      PRIMARY KEY (`id`)
    );

Service

    sudo nano /etc/systemd/system/gpnbot.service

    [Unit]
    Description=gpnbot

    [Service]
    WorkingDirectory=/home/gpnadmin/netcore
    ExecStart=/usr/bin/dotnet /home/gpnadmin/netcore/GPNBot.API.dll
    Restart=always
    RestartSec=30
    SyslogIdentifier=gpnbot
    User=gpnadmin
    Environment=ASPNETCORE_ENVIRONMENT=Production
    Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

    [Install]
    WantedBy=multi-user.target

    sudo systemctl enable gpnbot
    sudo systemctl start gpnbot
    sudo systemctl status gpnbot
