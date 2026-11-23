# CustomChannel

> **Статус:** структурный скелетон. Бизнес-логики, кода и запускаемого приложения пока нет.

Решение на .NET 10 (LTS), построенное по Onion Architecture.

## Структура каталогов

```
CustomChannel.slnx
Directory.Build.props        # общие настройки сборки (net10.0, Nullable, ImplicitUsings)
.editorconfig
.gitignore
src/
  CustomChannel.Domain/          # ядро: сущности, доменные правила
  CustomChannel.Application/     # сценарии использования, интерфейсы (порты)
  CustomChannel.Infrastructure/  # реализации портов: Pub/Sub, PostgreSQL, Cloud Storage
  CustomChannel.Worker/          # точка входа, composition root, appsettings.json
tests/
  CustomChannel.UnitTests/
  CustomChannel.IntegrationTests/
```

## Слои Onion Architecture

Зависимости направлены только внутрь:

```
Worker ──► Infrastructure ──► Application ──► Domain
   └──────────────────────────►┘
```

| Слой | Назначение | Ссылается на |
|------|-----------|--------------|
| **Domain** | Центр архитектуры: сущности, value objects, доменные правила. Не зависит ни от чего. | — |
| **Application** | Сценарии использования и интерфейсы, которые нужны приложению от внешнего мира. | Domain |
| **Infrastructure** | Конкретные реализации интерфейсов: работа с Pub/Sub, PostgreSQL, Cloud Storage. | Application, Domain |
| **Worker** | Внешний слой: запуск процесса, конфигурация и сборка зависимостей (DI). | Application, Infrastructure |

Тестовые проекты пока пустые и не имеют ссылок на другие проекты.

## Локальные настройки и GCP credentials

Шаблон настроек: `src/CustomChannel.Worker/appsettings.json`. В нём только пустые значения
(GCP Project ID, Pub/Sub Topic/Subscription ID, Cloud Storage bucket, строка подключения PostgreSQL).
**Реальные значения в этот файл не вписывайте.**

Позже задавайте их так:

- **Локальные значения:** `appsettings.Local.json` рядом с `appsettings.json` или
  [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
  (`dotnet user-secrets`). Эти файлы исключены через `.gitignore`.
- **Переменные окружения:** например `GoogleCloud__ProjectId`, `ConnectionStrings__PostgreSql`.
  Файлы `.env` игнорируются Git; при необходимости можно добавить `.env.example` без секретов.
- **GCP credentials:** локально используйте Application Default Credentials
  (`gcloud auth application-default login`) либо переменную окружения
  `GOOGLE_APPLICATION_CREDENTIALS` с путём к ключу **вне репозитория**. В облаке используйте
  привязанный service account без файла ключа.

Никогда не коммитьте пароли, токены, service account JSON и `.env` с секретами.
