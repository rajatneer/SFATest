ALTER USER 'root'@'localhost' IDENTIFIED BY 'Root@12345!';
CREATE DATABASE IF NOT EXISTS sfa_app_dev;
CREATE USER IF NOT EXISTS 'sfa_user'@'localhost' IDENTIFIED BY 'SfaUser@123!';
GRANT ALL PRIVILEGES ON sfa_app_dev.* TO 'sfa_user'@'localhost';
FLUSH PRIVILEGES;
