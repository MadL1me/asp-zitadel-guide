# Asp Zitadel Guide

## Сетап настроек Zitadel

Запускаем Zitadel с помощью команды в корне проекта:

```sh
docker compose up -d
```

Заходим в UI Цитадели http://localhost:8080/.
Если мы заходим в первый раз, то нас могут попросить поменять пароль. По умолчанию креды следующие:

- Логин: zitadel-admin@zitadel.localhost
- Пароль: Password1!


После замены базового пароля, мы увидим основную админку цитадели:

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/dd1ae20f-c7bc-4768-a948-7b099eda8d4c)

### Этап 1 - Регистрация проекта

В первую очередь, с нашей стороны должен быть заведен проект к которому и будет выдаваться доступ. Такая настройка должна быть сделана единожды.

Во вкладке с проектами заводим новый проект:

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/09e5b033-08c3-484f-92c6-9411b7ecb094)

Вводим название проекта, и проваливаемся в его настройки. В данном примере, я назвал проект “Test”:

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/9eb9ff1a-a041-4f72-9f73-f6f70047c5d2)

В рамках каждого проекта должны быть заведены “приложения” - Applications. 
Applications могут быть разными - начиная приложениями с фронтендом, заканчивая сугубо API.
В данном примере, для удобства, мы настроим и проверим работу по формату M2M (Machine-to-machine) с созданием PAN ключа. Кликаем New, и выбираем API: 

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/341990cd-558c-427f-a920-da4acade4e4b)

Жмем далее и выбираем тип аутентификации -  Private JWT или Basic. Оба варианта предусматривают `client credentails flow`, 
разница лишь в том что для подписи токенов при Basic настройке цитадель будет использовать свой внутренний единый ключ, 
вместо того чтобы давать приватный ключ подписи клиентам. Для примера возьмем Basic, т.к в нашем случае, само наше приложение это и есть доверенный ресурс: 

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/249ef108-1b72-4efe-b7ef-5e59c7ae2bda)

После создания Application'а, нам выдадут следующие параметры:

- ***ClientID***.

- ***ClientSecret***. 

Оба секрета необходимо сохранить, и в последствии добавить в секреты сервис (в `appsettings.Json -> ZitadelConfiguration`):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ZitadelConfiguration": {
    "ClientId": "266918751616754691@zitadelexample",
    "ClientSecret": "qFfxhq3083rgEnxoPCCqYEcJUaTl1kgeUv322oRnaizqCLAMMljNMHOeKtXialgf",
    "ZitadelHostUrl": "http://localhost:8080/"
  }
}
```

Серверное приложение в последствии будет использовать эти секреты для интроспекта токенов и запросам к ZitadelAPI:

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/9a20e3d7-d99a-40d5-ab6b-b6e8c8aeb096)

> Важная помарка - если разрабатываешь локально, то надо данные значения добавить в appsettings.Development.json

После создания проекта и приложения, для проекта нужно завести роли, по которым будет проходить Challenge например на определенные API endpoint'ы. 
Заходим в вкладку Roles:

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/6d15443c-9ebc-4475-87e1-1c911b101583)

Можем сразу создать одну или несколько ролей. По умолчанию в C# проекте этого репозитория настроена ручка на роль `test`. Создадим ее:

<img width="1338" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/222d47b6-c100-4a31-ac05-aed15dd4b5d9">

Роль можно увидеть во вкладке `Roles` проекта:

<img width="1276" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/193637eb-5186-4841-8c17-d5599d909362">

### Этап 2 - Регистрация пользователя

В данном примере, будут создаваться сервисные учетки. Как правило, если проект B2B, то каждый мерчант/теннант/клиент - это определенный ***“Service User”***
(сервисная учетка, которая будет участвовать в m2m аутентификации). Создать таких пользователей можно на верхней вкладке ***Users → Service Users***:

![image](https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/4a6ce7fc-371c-4d60-8064-0b55e2ef6832)

Кликаем `+ New` и создаем нового пользователя:

<img width="863" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/f0728771-c711-4101-a7aa-bb47a358daca">

Затем, этому пользователю можно назначить роль в вкладке `Authorizations` внутри проекта. Вот пример назначенной роли для нового созданного сервисного пользователя:

<img width="1007" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/982811a5-fb41-4be9-9ce3-2de7e640893c">

### Этап 3 - Ключи доступа

В Zitadel есть большое кол-во способов авторизироваться как `Service User` - но для примера возьмем самый простой - Personal Access Token (PAN). Чтобы его создать, надо зайти в `профиль 
сервисной учетки -> Personal Access Tokens -> + New`:

<img width="1363" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/237d74d4-3588-4674-ab02-e1b1777e75ae">

Создаем токен. Ему можно указать время жизни, при желании. Сохраняем его к себе: 

<img width="509" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/77ea66e3-e8ec-4b1c-87d9-37ec983d7144">

### Этап 4 - Использование ключа

Запускаем dotnet приложение, заходим в swagger (скорее всего можно найти здесь: http://localhost:5090/swagger/index.html)

В Authorize сваггера вставляем токен (без добавления перед ним Bearer)

<img width="1465" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/d7b26d0e-7c90-40a9-b991-90a09b2ec61e">

Затем, если все настроено верно, можем прожимать ручки: 

<img width="1410" alt="image" src="https://github.com/MadL1me/asp-zitadel-guide/assets/46647517/cc82c535-70eb-4619-9eca-d1d38ebe7016">









