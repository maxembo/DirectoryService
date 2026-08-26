# DirectoryService

## Docker Compose

Для восстановления приватных пакетов `SharedService.*` Docker-сборке нужны имя пользователя
GitHub и PAT classic с минимальным scope `read:packages`.

1. Добавьте локальные переменные в `.env`:

   ```dotenv
   NUGET_USERNAME=<github-username>
   CORS_ALLOWED_ORIGINS=http://localhost:3000
   ```

2. Создайте файл `.secrets/nuget_password`. Он должен содержать только значение PAT, без имени
   переменной, кавычек или префикса `NUGET_PASSWORD=`:

   ```text
   <github-pat-classic>
   ```

3. Запустите сборку:

   ```shell
   docker compose build directory_service
   ```

Файлы `.env` и `.secrets/` исключены из Git. Не добавляйте в них значения, предназначенные для
публикации или совместного использования.
