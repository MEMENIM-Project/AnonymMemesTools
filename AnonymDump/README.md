# <div align="center">**CLI Tool for dumping Anonym database**</div>

### How to get started

1) Run AnonymDump.exe and close it
2) Open generated AppSettings.config file
3) Configure the ConnectionString (DatabaseConnectionString setting) to connect to your MySql database
4) Configure number of connections in the pool (DatabaseConnectionsNumber setting) *(Set larger values to increased perfomance and stability)* **(with values >100, there may be problems)**
5) [Download](#download-links) and import the sql files into your database to create the tables and structure (with some data) need for the dump
6) Run AnonymDump.exe again
7) Enter 'Users' or 'Posts' to select a dump of users or posts
8) Take your tea and wait (If you chose a dump of users - you will need a LOT of tea)


### How to get ready-to-use dumped database

1) [Download](#download-links) ready-to-use dumped database (with xampp) archive
2) Move the 'xampp' folder from the archive to the '&#95;tools' folder (located in the project root folder) **(If you use the release and not the source code, you can move it to any folder)**
3) Run setup_xampp.bat from '&#95;tools\xampp' folder and wait for the operations to complete
4) Run xampp-control.exe from '&#95;tools\xampp' folder
5) Start 'Apache' and 'MySQL' modules and wait a few seconds
6) Open [PhpMyAdmin](http://localhost/phpmyadmin/index.php)
7) ??? PROFIT!!!


## <div align="center">**Settings**</div>

*Run AnonymDump.exe once and close it to create default settings files*


### AppSettings.config (ini file)

#### Database section
- *DatabaseConnectionString* - Connection string for MySql database provider
- *DatabaseConnectionsNumber* - Number of connections to MySql database provider **(with values >100, there may be problems)**
- *DatabaseCommandTimeout* - Default request timeout for MySql database provider

#### Users section
- *UsersOffset* - Id from which the users dump will start **(>0)**

#### Posts section
- *PostsOffset* - Id from which the posts dump will start **(>0)**

#### Comments section
- *CommentsCountPerTime* - Number of comments uploaded per request **(preferably at least >=20)**

#### Log section
- *LogRetentionDaysPeriod* - Number of days that the log files are stored (i.e., before they are deleted) **('-1' - without deletion, '0' - last log file only)**


## <div align="center">**Download links**</div>

#### Sql files for database (with schemas and some data)

- *Separated files (export by tables)*
    - [YandexDisk](https://disk.yandex.ru/d/GUtxNbjHoNc_2Q)
    - [GoogleDrive](https://drive.google.com/file/d/1XAY57pf7SD_toe7GZXVBSLdbnGXoBoGO/view?usp=sharing)

***OR***

- *Single file (export database)*
    - [YandexDisk](https://disk.yandex.ru/d/lT6EgGvyg03TOg)
    - [GoogleDrive](https://drive.google.com/file/d/1vEoZbCWMirVXj5sfekZ8MPK4bFCCeZL2/view?usp=sharing)


#### Ready-to-use dumped database

- *Single archive (with xampp)*
    - Version 3 (***06.01.2022***)
        - [GoogleDrive](https://drive.google.com/file/d/1xg87l9npBtQgquvWRDsm9hgY93TL9-_h/view?usp=sharing)
    <br>
    
    - <details>
        <summary><b>Older versions</b> <i>(click to expand)</i></summary>
        <br>
    
        - Version 2 (***08.05.2021***)
            - [YandexDisk](https://disk.yandex.ru/d/ZHhbJYKh5GogJA)
            - [GoogleDrive](https://drive.google.com/file/d/1WQ2iCPonhEg7Wmb8BBORtMOMz4UzuamR/view?usp=sharing)
    
        - Version 1 (***18.03.2021***)
            - [YandexDisk](https://disk.yandex.ru/d/DYrC3PiWwlE27A)
            - [GoogleDrive](https://drive.google.com/file/d/1r5MdxPaKWBJcbJm03xPdXmrihMC4HV6k/view?usp=sharing)
        
      </details>


